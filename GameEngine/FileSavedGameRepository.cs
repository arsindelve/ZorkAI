using System.Text.Json;
using Model.Interface;

namespace GameEngine;

/// <summary>
///     File-backed <see cref="ISavedGameRepository" /> for self-hosted/offline play (issue #383):
///     the drop-in replacement for <c>DynamoDbSavedGameRepository</c>, so the game backends can serve
///     save/restore with no AWS at all.
///     <para>
///     The DynamoDB table is keyed by the pair (id, session_id), and the only query is "every save for
///     this session" over the session_id index. That maps directly onto a directory per session:
///     <c>&lt;root&gt;/&lt;table&gt;/&lt;session&gt;/&lt;id&gt;.savedgame</c>, one JSON document each.
///     Listing a session's saves is then a directory enumeration rather than a scan, and the (id,
///     session_id) lookup is a single path.
///     </para>
/// </summary>
public class FileSavedGameRepository : ISavedGameRepository
{
    private readonly string _baseDirectory;
    private readonly Func<Guid> _newId;
    private readonly Func<DateTime> _utcNow;

    /// <param name="baseDirectory">
    ///     Root folder for saved games. Defaults to the ZORKAI_SAVE_DIR environment variable, falling
    ///     back to ~/.zorkai — the same root <see cref="FileSessionRepository" /> uses.
    /// </param>
    /// <param name="newId">Id generator for new saves. Injectable so tests are deterministic.</param>
    /// <param name="utcNow">Clock for the save timestamp. Injectable so tests are deterministic.</param>
    public FileSavedGameRepository(string? baseDirectory = null, Func<Guid>? newId = null,
        Func<DateTime>? utcNow = null)
    {
        _baseDirectory = SelfHostedStorage.ResolveBaseDirectory(baseDirectory);
        _newId = newId ?? Guid.NewGuid;
        _utcNow = utcNow ?? (() => DateTime.UtcNow);
    }

    /// <summary>
    ///     Returns the saved game's data, or null when there is no such save.
    ///     <para>
    ///     The DynamoDB implementation indexes straight into <c>response.Item["gameData"]</c> and throws
    ///     if the row is missing. Returning null instead is deliberate: every controller already guards
    ///     with <c>string.IsNullOrEmpty(gameData)</c> and raises its own "had empty game data" error, so
    ///     a missing save produces that clear message rather than a key-not-found from the storage layer.
    ///     </para>
    /// </summary>
    public async Task<string?> GetSavedGame(string id, string sessionId, string tableName)
    {
        var path = SaveFile(id, sessionId, tableName);
        if (!File.Exists(path))
            return null;

        var document = Deserialize(await File.ReadAllTextAsync(path));
        return document?.GameData;
    }

    public async Task<string> SaveGame(string? id, string clientId, string name, string gameData, string tableName)
    {
        // Matches DynamoDbSavedGameRepository: a null id means "new save", anything else overwrites.
        // Note the parameter is clientId here and sessionId everywhere else — the DynamoDB row stores
        // it in the single session_id attribute, so the two names address the same thing.
        id ??= _newId().ToString();

        var path = SaveFile(id, clientId, tableName);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        var document = new SavedGameDocument(id, name, clientId, gameData, _utcNow().Ticks);
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(document));

        return id;
    }

    /// <summary>
    ///     Every save belonging to a session. Order is not specified — the controllers sort by
    ///     <c>SavedOn</c> themselves, exactly as they must with DynamoDB's unordered query results.
    /// </summary>
    public async Task<List<(string Id, string Name, DateTime SavedOn)>> GetSavedGames(string sessionId,
        string tableName)
    {
        var directory = SessionDirectory(sessionId, tableName);
        if (!Directory.Exists(directory))
            return [];

        var saves = new List<(string, string, DateTime)>();

        foreach (var file in Directory.EnumerateFiles(directory, "*.savedgame"))
        {
            // A file we cannot use is skipped rather than thrown over: one corrupted save must not
            // make the whole save list unreadable and lock the player out of the others. That means
            // more than catching JsonException — a truncated or hand-edited file can parse as valid
            // JSON and still carry out-of-range ticks or a missing name, and `new DateTime(ticks)`
            // would throw straight out of this listing.
            if (TryReadSave(await File.ReadAllTextAsync(file), out var save))
                saves.Add(save);
        }

        return saves;
    }

    public Task DeleteSavedGameAsync(string id, string sessionId, string tableName)
    {
        var path = SaveFile(id, sessionId, tableName);
        if (File.Exists(path))
            File.Delete(path);

        return Task.CompletedTask;
    }

    private string SessionDirectory(string sessionId, string tableName)
    {
        return Path.Combine(_baseDirectory, SelfHostedStorage.Sanitize(tableName),
            SelfHostedStorage.Sanitize(sessionId));
    }

    private string SaveFile(string id, string sessionId, string tableName)
    {
        return Path.Combine(SessionDirectory(sessionId, tableName),
            SelfHostedStorage.Sanitize(id) + ".savedgame");
    }

    private static SavedGameDocument? Deserialize(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<SavedGameDocument>(json);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    ///     Reads one save file into a listing entry, or reports false for anything unusable. Every
    ///     field is treated as untrusted: the file may have been truncated mid-write or edited by hand.
    /// </summary>
    private static bool TryReadSave(string json, out (string Id, string Name, DateTime SavedOn) save)
    {
        save = default;

        var document = Deserialize(json);
        if (document is null || string.IsNullOrEmpty(document.Id))
            return false;

        // DateTime rejects anything outside its range, so a garbage tick count would otherwise throw
        // ArgumentOutOfRangeException out of GetSavedGames and take the whole listing with it.
        if (document.DateTicks < DateTime.MinValue.Ticks || document.DateTicks > DateTime.MaxValue.Ticks)
            return false;

        save = (document.Id, document.Name ?? string.Empty, new DateTime(document.DateTicks));
        return true;
    }

    /// <summary>
    ///     On-disk shape of one save. Ticks rather than a formatted date so the round trip is exact and
    ///     culture-independent, mirroring how the DynamoDB row stores its "date" attribute.
    /// </summary>
    public sealed record SavedGameDocument(
        string Id,
        string Name,
        string SessionId,
        string GameData,
        long DateTicks);
}

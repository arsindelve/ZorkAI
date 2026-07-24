using Model.Interface;

namespace GameEngine;

/// <summary>
///     File-backed <see cref="ISessionRepository" /> for self-hosted/offline play (issue #383):
///     the console's drop-in replacement for the DynamoDB repository, keeping every game state on
///     local disk so no AWS access is ever needed. Layout mirrors the DynamoDB shape: one directory
///     per "table", one file per session, plus an append-only step log alongside it.
/// </summary>
public class FileSessionRepository : ISessionRepository
{
    private readonly string _baseDirectory;

    /// <param name="baseDirectory">
    ///     Root folder for saved sessions. Defaults to the ZORKAI_SAVE_DIR environment variable,
    ///     falling back to ~/.zorkai.
    /// </param>
    public FileSessionRepository(string? baseDirectory = null)
    {
        _baseDirectory = baseDirectory
                         ?? Environment.GetEnvironmentVariable("ZORKAI_SAVE_DIR")
                         ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".zorkai");
    }

    public async Task<string?> GetSessionState(string sessionId, string tableName)
    {
        var path = SessionFile(sessionId, tableName);
        if (!File.Exists(path))
            return null;

        return await File.ReadAllTextAsync(path);
    }

    public async Task WriteSessionState(string sessionId, string gameData, string tableName)
    {
        var path = SessionFile(sessionId, tableName);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllTextAsync(path, gameData);
    }

    public async Task WriteSessionStep(string sessionId, long turnIndex, string input, string output,
        string tableName)
    {
        var path = StepsFile(sessionId, tableName);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.AppendAllTextAsync(path,
            $"[{turnIndex}] > {input}{Environment.NewLine}{output}{Environment.NewLine}{Environment.NewLine}");
    }

    public async Task<string> GetSessionStepsAsText(string sessionId, string tableName)
    {
        var path = StepsFile(sessionId, tableName);
        if (!File.Exists(path))
            return string.Empty;

        return await File.ReadAllTextAsync(path);
    }

    private string SessionFile(string sessionId, string tableName)
    {
        return Path.Combine(_baseDirectory, Sanitize(tableName), Sanitize(sessionId) + ".session");
    }

    private string StepsFile(string sessionId, string tableName)
    {
        return Path.Combine(_baseDirectory, Sanitize(tableName), Sanitize(sessionId) + ".steps.log");
    }

    // The Windows-invalid set, applied on every platform so a save directory is portable and a
    // session id can never smuggle in a path separator (Linux only forbids '/' and NUL natively).
    private static readonly char[] HostileChars =
        Path.GetInvalidFileNameChars().Union(['/', '\\', ':', '*', '?', '"', '<', '>', '|']).ToArray();

    /// <summary>
    ///     Makes an arbitrary session/table identifier safe to use as a file name. Public and static
    ///     so it is unit-testable directly.
    /// </summary>
    public static string Sanitize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "_";

        var chars = value.Select(c => HostileChars.Contains(c) ? '_' : c).ToArray();
        return new string(chars);
    }
}

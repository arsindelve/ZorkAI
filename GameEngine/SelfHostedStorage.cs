namespace GameEngine;

/// <summary>
///     Filesystem conventions shared by the self-hosted (issue #383) repositories:
///     <see cref="FileSessionRepository" /> and <see cref="FileSavedGameRepository" />.
///     Both write under one root and both have to turn arbitrary session/table identifiers into
///     safe file names, so the rules live here rather than being restated — and drifting apart — in
///     each repository.
/// </summary>
public static class SelfHostedStorage
{
    /// <summary>Environment variable that relocates the whole self-hosted save tree.</summary>
    public const string SaveDirectoryVariable = "ZORKAI_SAVE_DIR";

    // The Windows-invalid set, applied on every platform so a save directory is portable and a
    // session id can never smuggle in a path separator (Linux only forbids '/' and NUL natively).
    private static readonly char[] HostileChars =
        Path.GetInvalidFileNameChars().Union(['/', '\\', ':', '*', '?', '"', '<', '>', '|']).ToArray();

    /// <summary>
    ///     Resolves the root folder for self-hosted state: an explicit directory if the caller supplied
    ///     one, else <see cref="SaveDirectoryVariable" />, else ~/.zorkai.
    /// </summary>
    public static string ResolveBaseDirectory(string? explicitDirectory)
    {
        return explicitDirectory
               ?? Environment.GetEnvironmentVariable(SaveDirectoryVariable)
               ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".zorkai");
    }

    /// <summary>
    ///     Makes an arbitrary session/table/save identifier safe to use as a file or directory name.
    /// </summary>
    public static string Sanitize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "_";

        var chars = value.Select(c => HostileChars.Contains(c) ? '_' : c).ToArray();
        return new string(chars);
    }
}

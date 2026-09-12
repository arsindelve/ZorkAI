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
    ///     <para>
    ///     Neutralizing the all-dots segments matters because <see cref="FileSavedGameRepository" />
    ///     uses a sanitized session id as a <i>directory</i>, and session and client ids arrive
    ///     straight off the wire in every controller. Replacing only the invalid characters left
    ///     <c>".."</c> intact, so a save posted with that client id resolved one level up and wrote,
    ///     read and deleted outside its table directory.
    ///     </para>
    /// </summary>
    public static string Sanitize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "_";

        var chars = value.Select(c => HostileChars.Contains(c) ? '_' : c).ToArray();

        // "." and ".." (and longer runs) are valid file names character-wise but navigate the tree.
        // Only a segment that is *entirely* dots traverses, so "save.1" and "my.session" survive.
        if (chars.All(c => c == '.'))
            return new string('_', chars.Length);

        return new string(chars);
    }
}

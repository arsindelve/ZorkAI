namespace ZorkConsole;

/// <summary>
///     Validates and normalizes the command-line argument the console needs to start a game. The console
///     requires a single argument naming the game to launch; without it (or with an unrecognized name)
///     we hand back clear feedback instead of letting the program crash on <c>args[0]</c> (an empty
///     <c>args</c> used to throw <see cref="IndexOutOfRangeException"/>) or throw an uncaught exception on
///     an unknown game.
/// </summary>
public static class GameArgumentResolver
{
    /// <summary>The games the console knows how to launch. Keep in sync with the switch in Program.cs.</summary>
    public static readonly string[] SupportedGames = ["ZorkOne", "Planetfall", "Stationfall", "EscapeRoom"];

    /// <summary>
    ///     Resolves the requested game from the process arguments. Returns a canonical game name when the
    ///     first argument matches a supported game (case-insensitively, trimmed); otherwise returns
    ///     feedback that tells the user exactly what to supply.
    /// </summary>
    public static GameArgument Resolve(string[]? args)
    {
        if (args is null || args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
            return GameArgument.Invalid($"No game was specified. {UsageMessage}");

        var requested = args[0].Trim();

        var match = SupportedGames.FirstOrDefault(
            game => game.Equals(requested, StringComparison.OrdinalIgnoreCase));

        return match is null
            ? GameArgument.Invalid($"'{requested}' is not a game I recognize. {UsageMessage}")
            : GameArgument.Valid(match);
    }

    /// <summary>Human-readable reminder of how to launch the console, listing the supported games.</summary>
    public static string UsageMessage =>
        "Usage: dotnet run --project Console <game>. " +
        $"Supported games: {string.Join(", ", SupportedGames)}.";
}

/// <summary>The outcome of resolving the console's game argument: either a canonical game name or user feedback.</summary>
public record GameArgument(string? GameName, string? Feedback)
{
    public bool IsValid => GameName is not null;

    public static GameArgument Valid(string gameName) => new(gameName, null);

    public static GameArgument Invalid(string feedback) => new(null, feedback);
}
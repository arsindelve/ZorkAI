namespace Model.Interface;

/// <summary>
///     Extension point for game-specific god-mode subcommands. The engine's god-mode processor knows
///     nothing about individual games' characters or mechanics; a game context implements this to
///     handle its own toggles (e.g. Planetfall suppressing its companion's random wandering) without
///     leaking game-specific concepts into the engine.
/// </summary>
public interface IGodModeCommandHandler
{
    /// <summary>
    ///     Attempts to handle a god-mode command the engine itself didn't recognize. Returns the
    ///     confirmation message if this game handled the command, or null to fall through to the
    ///     engine's generic "Invalid use of God mode" error.
    /// </summary>
    string? HandleGodModeCommand(string input);
}

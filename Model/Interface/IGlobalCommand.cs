using Model.AIGeneration;

namespace Model.Interface;

/// <summary>
///     Represents a global command that can be processed by the game engine.
/// </summary>
public interface IGlobalCommand
{
    /// <summary>
    ///     Represents a method for processing global commands in the game engine.
    /// </summary>
    /// <param name="input">The input string for the command.</param>
    /// <param name="context">The game <see cref="IContext" /> in which the command is being processed.</param>
    /// <param name="client"></param>
    /// <param name="runtime"></param>
    /// <returns>The output string result of the command processing.</returns>
    /// <remarks>
    ///     This method is called to process global commands in the game engine.
    ///     The method takes an optional input string and the game context as parameters.
    ///     It returns a string representing the output of the command processing.
    ///     The input string may be null or empty if no input is provided by the user.
    ///     The <see cref="IContext" /> parameter provides access to game-related information and functionality.
    /// </remarks>
    Task<string> Process(string? input, IContext context, IGenerationClient client, Runtime runtime);
}

public interface ISystemCommand : IGlobalCommand;

/// <summary>
///     Marks a global command as a "free" meta/informational action - e.g. look, inventory, score,
///     current time - that must not consume the player's own turn. The engine skips
///     <see cref="IContext.ProcessBeginningOfTurn" /> and <see cref="IContext.ProcessEndOfTurn" /> for
///     these, so checking your status can never itself advance <c>Context.Moves</c>, trigger
///     survival-clock escalation (hunger/sleep in Planetfall), or tick the survival clock (issue
///     #354). Unlike a true system command (<see cref="ISystemCommand" />), the rest of the world
///     still gets a turn - actor processing (chase scenes, countdown timers, NPCs, ...) still runs,
///     since the world doesn't pause just because the player glanced at their status.
/// </summary>
public interface IFreeGlobalCommand : IGlobalCommand;
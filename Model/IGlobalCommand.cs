using OpenAI;

namespace Model;

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
    /// <returns>The output string result of the command processing.</returns>
    /// <remarks>
    ///     This method is called to process global commands in the game engine.
    ///     The method takes an optional input string and the game context as parameters.
    ///     It returns a string representing the output of the command processing.
    ///     The input string may be null or empty if no input is provided by the user.
    ///     The <see cref="IContext" /> parameter provides access to game-related information and functionality.
    /// </remarks>
    Task<string> Process(string? input, IContext context, IGenerationClient client);
}
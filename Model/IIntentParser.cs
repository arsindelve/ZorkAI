using Model.Intent;

namespace Model;

/// <summary>
/// Interface for a parser that can take user's input and determine their intent. Do they want to move? Quit
/// Pick something up? The parser will hopefully tell us. 
/// </summary>
public interface IIntentParser
{
    /// <summary>
    /// Determines the type of intent based on the input and session ID.
    /// </summary>
    /// <param name="input">The user input.</param>
    /// <param name="sessionId">The session ID uniquely identifies this session and adventurer to the parser. </param>
    /// <returns>The intent type as an instance of IntentBase.</returns>
    Task<IntentBase> DetermineIntentType(string input, string sessionId);
}
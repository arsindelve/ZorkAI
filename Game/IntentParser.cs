using Game.StaticCommand;
using Lex;

namespace Game;

/// <summary>
///     This class will take the response from the text parser and try to determine which category
///     of "intent" their input represents. Do they want to interact with something, move somewhere, etc.
/// </summary>
public class IntentParser : IIntentParser
{
    private readonly IAIParser _parser;

    /// <summary>
    /// Constructor for unit testing 
    /// </summary>
    /// <param name="parser"></param>
    public IntentParser(IAIParser parser)
    {
        _parser = parser;
    }

    public IntentParser()
    {
        _parser = new LexParser();
    }

    /// <summary>
    ///     Determines the type of intent based on the input and session ID.
    /// </summary>
    /// <param name="input">The user input.</param>
    /// <param name="sessionId">The unique session ID.</param>
    /// <returns>The determined intent type.</returns>
    public async Task<IntentBase> DetermineIntentType(string input, string sessionId)
    {
        if (DirectionParser.IsDirection(input, out var moveTo))
            return new MoveIntent { Direction = moveTo };

        if (GlobalCommandFactory.GetGlobalCommands(input) is { } command)
            return new GlobalCommandIntent { Command = command };

        // At this point, we don't know the user's intent without asking the
        // AI parsing engine, so let's do that. 
        
        return await _parser.AskTheAIParser(input, sessionId);
    }
}
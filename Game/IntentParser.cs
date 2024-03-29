using Game.StaticCommand;
using Lex;
using Model.AIParsing;

namespace Game;

/// <summary>
///     This class will take the response from the text parser and try to determine which category
///     of "intent" their input represents. Do they want to interact with something, move somewhere, etc.
/// </summary>
public class IntentParser : IIntentParser
{
    private readonly IGlobalCommandFactory _defaultGlobalCommandFactory = new GlobalCommandFactory();
    private readonly IGlobalCommandFactory _gameSpecificCommandFactory;
    private readonly IAIParser _parser;

    /// <summary>
    ///     Constructor for unit testing
    /// </summary>
    /// <param name="parser"></param>
    /// <param name="gameSpecificCommandFactory"></param>
    public IntentParser(IAIParser parser, IGlobalCommandFactory gameSpecificCommandFactory)
    {
        _parser = parser;
        _gameSpecificCommandFactory = gameSpecificCommandFactory;
    }

    public IntentParser(IGlobalCommandFactory gameSpecificCommandFactory)
    {
        _gameSpecificCommandFactory = gameSpecificCommandFactory;
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

        if (_defaultGlobalCommandFactory.GetGlobalCommands(input) is { } globalCommand)
            return new GlobalCommandIntent { Command = globalCommand };

        if (_gameSpecificCommandFactory.GetGlobalCommands(input) is { } gameSpecificGlobalCommand)
            return new GlobalCommandIntent { Command = gameSpecificGlobalCommand };

        // At this point, we don't know the user's intent without asking the
        // AI parsing engine, so let's do that. 

        return await _parser.AskTheAIParser(input, sessionId);
    }
}
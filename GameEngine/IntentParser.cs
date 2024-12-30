using Bedrock;
using CloudWatch;
using CloudWatch.Model;
using GameEngine.StaticCommand;
using Microsoft.Extensions.Logging;
using Model.AIParsing;
using Model.Interface;
using Model.Movement;
using Newtonsoft.Json;

namespace GameEngine;

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

    public IntentParser(IGlobalCommandFactory gameSpecificCommandFactory, ILogger? logger = null)
    {
        _gameSpecificCommandFactory = gameSpecificCommandFactory;
        _parser = new OpenAIParser(logger);
    }

    public IntentBase? DetermineSystemIntentType(string? input)
    {
        if (_defaultGlobalCommandFactory.GetSystemCommands(input) is { } systemCommand)
            return new SystemCommandIntent { Command = systemCommand };

        return null;
    }

    public IntentBase? DetermineGlobalIntentType(string? input)
    {
        // Cardinal movement. 
        if (DirectionParser.IsDirection(input, out var moveTo))
            return new MoveIntent { Direction = moveTo };

        // Common to all Infocom games
        if (_defaultGlobalCommandFactory.GetGlobalCommands(input) is { } globalCommand)
            return new GlobalCommandIntent { Command = globalCommand };

        // Specific to this game. 
        if (_gameSpecificCommandFactory.GetGlobalCommands(input) is { } gameSpecificGlobalCommand)
            return new GlobalCommandIntent { Command = gameSpecificGlobalCommand };

        return null;
    }

    /// <summary>
    ///     Determines the type of intent based on the input and session ID.
    /// </summary>
    /// <param name="input">The user input.</param>
    /// <param name="locationDescription">
    ///     This can be instrumental in determining what the user wants to do. If they
    ///     say "follow the path", we need the location description to tell us which way the path goes.
    /// </param>
    /// <param name="sessionId">The unique session ID.</param>
    /// <returns>The determined intent type.</returns>
    public virtual async Task<IntentBase> DetermineComplexIntentType(string? input, string locationDescription,
        string sessionId)
    {
        // At this point, we don't know the user's intent without asking the
        // AI parsing engine, so let's do that. 
        var response = await _parser.AskTheAIParser(input!, locationDescription, sessionId);

        if (!string.IsNullOrEmpty(input))
            await Logger?.WriteLogEvents(new GenerationLog
            {
                SystemPrompt = string.Empty,
                Temperature = 0,
                LanguageModel = _parser.LanguageModel,
                UserPrompt = input,
                Response = JsonConvert.SerializeObject(response),
                TurnCorrelationId = TurnCorrelationId.ToString()
            })!;

        return response;
    }

    public Guid? TurnCorrelationId { get; set; }

    public ICloudWatchLogger<GenerationLog>? Logger { get; set; }
}
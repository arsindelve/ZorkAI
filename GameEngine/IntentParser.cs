using System.Diagnostics;
using CloudWatch;
using CloudWatch.Model;
using GameEngine.StaticCommand;
using Microsoft.Extensions.Logging;
using Model.AIParsing;
using Model.Interface;
using Model.Movement;
using Newtonsoft.Json;
using ZorkAI.OpenAI;

namespace GameEngine;

/// <summary>
/// This class will take the response from the text parser and try to determine which category
/// of "intent" their input represents. Do they want to interact with something, move somewhere, etc.
/// </summary>
public class IntentParser : IIntentParser
{
    private readonly IGlobalCommandFactory _defaultGlobalCommandFactory = new GlobalCommandFactory();
    private readonly IGlobalCommandFactory _gameSpecificCommandFactory;
    private readonly ILogger? _logger;
    private readonly IAIParser _parser;
    private readonly IPronounResolver _pronounResolver;

    // Layer 1 of the parser plan. Null in unit tests (the mock-AI constructor below leaves it unset, so
    // those tests exercise the AI path exactly as before); the production constructor builds it from the
    // game name so the deterministic pass runs ahead of the AI in real games.
    private readonly DeterministicParser? _deterministicParser;

    /// <summary>
    ///     Constructor for unit testing
    /// </summary>
    /// <param name="parser"></param>
    /// <param name="gameSpecificCommandFactory"></param>
    public IntentParser(IAIParser parser, IGlobalCommandFactory gameSpecificCommandFactory)
    {
        _parser = parser;
        _gameSpecificCommandFactory = gameSpecificCommandFactory;
        _pronounResolver = new PronounResolver(); // No logger per user preference
    }

    /// <summary>
    ///     Constructor for unit testing the deterministic-first path: inject a mock AI parser AND enable the
    ///     deterministic pass for a given game.
    /// </summary>
    public IntentParser(IAIParser parser, IGlobalCommandFactory gameSpecificCommandFactory, string gameName)
    {
        _parser = parser;
        _gameSpecificCommandFactory = gameSpecificCommandFactory;
        _pronounResolver = new PronounResolver();
        _deterministicParser = new DeterministicParser(gameName);
    }

    public IntentParser(IGlobalCommandFactory gameSpecificCommandFactory, ILogger? logger = null,
        string? gameName = null)
    {
        _gameSpecificCommandFactory = gameSpecificCommandFactory;
        _logger = logger;
        _parser = new OpenAIParser(logger);
        _pronounResolver = new PronounResolver(logger);
        if (!string.IsNullOrWhiteSpace(gameName))
            _deterministicParser = new DeterministicParser(gameName);
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
        // Layer 1 of the parser plan: try the deterministic, vocabulary-driven parser first. On a confident
        // parse we skip the AI entirely — instant, free, and reproducible. It is conservative by design and
        // returns null for anything it isn't sure about, in which case we fall through to the AI parser.
        // The hit/miss log lets us measure how much of real play the deterministic pass already covers.
        if (_deterministicParser is not null && !string.IsNullOrWhiteSpace(input))
        {
            var deterministic = _deterministicParser.Parse(input);
            if (deterministic is not null)
            {
                _logger?.LogInformation("Parser: deterministic HIT for '{Input}' -> {Intent}", input,
                    deterministic.GetType().Name);
                return deterministic;
            }

            _logger?.LogInformation("Parser: deterministic MISS for '{Input}' -> falling back to AI", input);
        }

        // Otherwise we don't know the user's intent without asking the AI parsing engine, so let's do that.
        var response = await _parser.AskTheAIParser(input!, locationDescription, sessionId);

        if (!string.IsNullOrEmpty(input) && Logger != null)
            await Logger.WriteLogEvents(new GenerationLog
            {
                SystemPrompt = string.Empty,
                Temperature = 0,
                LanguageModel = _parser.LanguageModel,
                UserPrompt = input,
                Response = JsonConvert.SerializeObject(response),
                TurnCorrelationId = TurnCorrelationId?.ToString() ?? string.Empty
            });

        Debug.Assert(response != null, nameof(response) + " != null");
        return response;
    }

    public virtual async Task<string?> ResolvePronounsAsync(string input, string? lastInput, string? lastResponse)
    {
        string? result = await _pronounResolver.ResolvePronouns(input, lastInput, lastResponse);
        if (result != null && !result.Equals(input, StringComparison.OrdinalIgnoreCase))
        {
            _logger?.LogDebug($"Pronoun resolution: '{input}' -> '{result}' (lastInput: '{lastInput}', lastResponse: '{lastResponse}')");
        }
        return result;
    }

    public Guid? TurnCorrelationId { get; set; }

    public ICloudWatchLogger<GenerationLog>? Logger { get; set; }
}
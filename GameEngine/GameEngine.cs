using System.Reflection;
using ChatLambda;
using CloudWatch;
using CloudWatch.Model;
using GameEngine.IntentEngine;
using GameEngine.Item;
using GameEngine.Item.ItemProcessor;
using GameEngine.Location;
using GameEngine.StaticCommand;
using GameEngine.StaticCommand.Implementation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Model.AIGeneration;
using Model.AIGeneration.Requests;
using Model.Interface;
using Model.Item;
using Model.Movement;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Utilities;

namespace GameEngine;

/// <summary>
///     The engine takes the input, figures our what to do with it, adjusts the state of the game
///     as required, and returns the output. It has a single method for this: <see cref="GetResponse" />"
/// </summary>
/// <remarks>
///     Most of the work of figuring out what to do is deferred to the specific location we're in,
///     or the objects available in the locations, which are matched to the "noun" identified by the parser.
///     The state of the game is preserved in two places: the <see cref="Context" />, which is created by and
///     owned by the engine, and the repository, which contains all the items and locations in the game
///     (and their corresponding state). The <see cref="Repository" /> is static, self-contained and not owned by
///     anyone.
/// </remarks>
public class GameEngine<TInfocomGame, TContext> : IGameEngine
    where TInfocomGame : IInfocomGame, new()
    where TContext : IContext, new()
{
    private readonly IItemProcessorFactory _itemProcessorFactory;
    private readonly AgainProcessor _againProcessor = new();
    private readonly LimitedStack<(string, string, bool)> _inputOutputs = new();
    private readonly ItProcessor _itProcessor = new();
    private readonly ILogger<GameEngine<TInfocomGame, TContext>>? _logger;
    private readonly IIntentParser _parser;
    private readonly ISecretsManager _secretsManager;
    private readonly string _sessionId = Guid.NewGuid().ToString();
    private readonly Guid _turnCorrelationId = Guid.NewGuid();
    private readonly OpenAITakeAndDropListParser _openAITakeAndDropListParser;
    private readonly IParseConversation _parseConversation;
    private string? _currentInput;
    private TInfocomGame _gameInstance;
    private bool _lastResponseWasGenerated;
    private IStatefulProcessor? _processorInProgress;
    private ICloudWatchLogger<TurnLog>? _turnLogger;
    internal TContext Context;
    
    [ActivatorUtilitiesConstructor]
    public GameEngine(
        ILogger<GameEngine<TInfocomGame, TContext>> logger,
        ISecretsManager secretsManager,
        IParseConversation parseConversation)
    {
        _logger = logger;
        _secretsManager = secretsManager;
        _parseConversation = parseConversation;
        _gameInstance = new TInfocomGame();
        Context = new TContext
        {
            Engine = this,
            Game = _gameInstance,
            Verbosity = Verbosity.Brief
        };

        Context.CurrentLocation.Init();
        Context.Init();
        _gameInstance.Init(Context);

        Runtime = Runtime.Web;
        IntroText = $"""
                     {_gameInstance.StartText}
                     {Context.CurrentLocation.GetDescription(Context)}
                     """;

        GenerationClient = new ChatGPTClient(_logger);
        GenerationClient.OnGenerate += () => _lastResponseWasGenerated = true;

        _openAITakeAndDropListParser = new OpenAITakeAndDropListParser(logger);
        _itemProcessorFactory = new ItemProcessorFactory(_openAITakeAndDropListParser);
        _parser = new IntentParser(_gameInstance.GetGlobalCommandFactory(), _logger);
        Inventory = [];
    }

    /// <summary>
    ///     Constructor for unit test dependency injection.
    /// </summary>
    /// <param name="itemProcessorFactory"></param>
    /// <param name="parser"></param>
    /// <param name="generationClient"></param>
    /// <param name="secretsManager"></param>
    /// <param name="turnLogger"></param>
    public GameEngine(
        IItemProcessorFactory itemProcessorFactory,
        IIntentParser parser,
        IGenerationClient generationClient,
        ISecretsManager secretsManager,
        ICloudWatchLogger<TurnLog> turnLogger,
        IParseConversation parseConversation)
    {
        Repository.Reset();

        Context = new TContext { Engine = this };
        Inventory = [];
        IntroText = string.Empty;
        _parser = parser;
        GenerationClient = generationClient;
        _secretsManager = secretsManager;
        _parseConversation = parseConversation;
        _gameInstance = (TInfocomGame)Context.Game;
        _gameInstance.Init(Context);
        _itemProcessorFactory = itemProcessorFactory;
        _turnLogger = turnLogger;
        _openAITakeAndDropListParser = null!;
    }

    public int Score => Context.Score;

    public Runtime Runtime { get; set; }

    public string SessionTableName => _gameInstance.SessionTableName;

    public string IntroText { get; }

    public IGenerationClient GenerationClient { get; }

    public string LocationName => Context.CurrentLocation.Name;

    public string? PreviousLocationName { get; private set; }

    public Direction? LastMovementDirection => Context.LastMovementDirection;

    public string LocationDescription => Context.CurrentLocation.GetDescriptionForGeneration(Context);

    public int Moves => Context.Moves;

    public int CurrentTime => Context is ITimeBasedContext tc ? tc.CurrentTime : 0;

    public List<Direction> Exits => Context.CurrentLocation.Exits(Context);

    /// <summary>
    ///     Parse the input, determine the user's <see cref="IntentBase" /> and allow the
    ///     implementation of that specific intent to determine what to do next.
    /// </summary>
    /// <param name="playerInput">The text typed by the adventurer</param>
    /// <returns>The output which we need to display to the adventurer</returns>
    public async Task<string?> GetResponse(string? playerInput)
    {
        _currentInput = playerInput;

        // 1. ------- Processor in Progress -
        // See if we have something already running like a save, quit, etc.
        // and see if it has any output.  Does not count as a turn. No actor or turn processing. 
        var (returnProcessorInProgressOutput, processorInProgressOutput) =
            await RunProcessorInProgress(playerInput);

        if (returnProcessorInProgressOutput)
            return PostProcessing(processorInProgressOutput!);

        // 2. -------  Empty command. Does not count as a turn. No actor or turn processing. 
        if (string.IsNullOrEmpty(playerInput))
            return PostProcessing(await GetGeneratedNoCommandResponse());

        PreviousLocationName = LocationName;

        // 3. ------- System, or "meta" commands - like save, restore, quit, verbose etc. Does not count as a turn. No actor or turn processing. 
        var systemCommand = _parser.DetermineSystemIntentType(playerInput);
        if (systemCommand is GlobalCommandIntent global)
        {
            var globalResult = await ProcessGlobalCommandIntent(global);
            return PostProcessing(globalResult);
        }

        // Everything below here counts as a turn. Pre-process the turn. 
        // See if the context needs to notify us of anything. Are we sleepy? Hungry?
        var contextPrepend = Context.ProcessBeginningOfTurn();

        // See if the user typed "again" or some variation.
        // if so, we'll replace the input with their previous input.
        (_currentInput, var returnResponseFromAgainProcessor) = _againProcessor.Process(
            _currentInput!,
            Context
        );
        if (returnResponseFromAgainProcessor)
            return PostProcessing(_currentInput);

        // 4. ------- Location specific raw commands 
        // Check if the location has an interaction with the raw, unparsed input. 
        // Some locations have a special interaction to raw input that does not fit 
        // the traditional sentence parsing. Sometimes that is a single verb with no 
        // noun like "jump" or "pray" or "echo", or some other specific phrase
        // that does not lend itself well to parsing. 
        var singleVerbResult = await Context.CurrentLocation.RespondToSpecificLocationInteraction(
            playerInput,
            Context,
            GenerationClient
        );
        if (singleVerbResult.InteractionHappened)
            return await ProcessActorsAndContextEndOfTurn(contextPrepend, singleVerbResult.InteractionMessage);

        // 5. ------- Global commands - these work always, everywhere: like look, inventory, wait and cardinal directions. These DO count as a turn, 
        // We must process actors afterwards 
        var simpleIntent = _parser.DetermineGlobalIntentType(playerInput);
        if (simpleIntent is not null)
        {
            var resultMessage = simpleIntent switch
            {
                GlobalCommandIntent intent => await ProcessGlobalCommandIntent(intent),
                MoveIntent moveInteraction => (await new MoveEngine().Process(moveInteraction, Context,
                    GenerationClient)).ResultMessage,
                _ => null
            };

            return await ProcessActorsAndContextEndOfTurn(contextPrepend, resultMessage);
        }

        // Is the player talking to someone?
        Console.WriteLine($"[GAME ENGINE DEBUG] About to check for conversation with input: '{_currentInput}'");
        var conversation = await CheckForConversation(_currentInput);
        if (conversation is not null)
        {
            Console.WriteLine($"[GAME ENGINE DEBUG] Conversation detected, returning response: '{conversation}'");
            return await ProcessActorsAndContextEndOfTurn(contextPrepend, conversation);
        }
        Console.WriteLine("[GAME ENGINE DEBUG] No conversation detected, continuing with normal processing");

        // 6. ------- Complex parsed commands. These require a parser to break them down into their noun(s) and verb.

        // if the user referenced an object using "it", let's see if we can handle that.
        var (requiresClarification, replacedInput) = _itProcessor.Check(_currentInput, Context);
        if (requiresClarification)
        {
            _processorInProgress = _itProcessor;
            return PostProcessing(replacedInput);
        }

        // Replace the "it" with the correct noun, if applicable. 
        _currentInput = replacedInput;

        var parsedResult = await _parser.DetermineComplexIntentType(
            _currentInput,
            Context.CurrentLocation.GetDescription(Context),
            _sessionId
        );

        var complexIntentResult = await ProcessComplexIntent(parsedResult);

        // Put it all together for return. 
        return await ProcessActorsAndContextEndOfTurn(contextPrepend, complexIntentResult.ResultMessage);
    }

    public List<string> Inventory { get; set; }

    public IContext RestoreGame(string data)
    {
        var deserializeObject = JsonConvert.DeserializeObject<SavedGame<TContext>>(
            data,
            JsonSettings()
        );
        var allItems = deserializeObject?.AllItems ?? throw new ArgumentException();
        var allLocations = deserializeObject.AllLocations ?? throw new ArgumentException();

        Repository.Restore(allItems, allLocations);

        Context = deserializeObject.Context ?? throw new ArgumentException();
        Context.Engine = this;
        Context.Game = new TInfocomGame();

        return Context;
    }

    public string SaveGame()
    {
        var savedGame = Repository.Save<TContext>();
        savedGame.Context = Context;
        return JsonConvert.SerializeObject(savedGame, JsonSettings());
    }

    private async Task<string?> CheckForConversation(string input)
    {
        Console.WriteLine($"[CONVERSATION DEBUG] Checking input: '{input}'");
        
        // Collect all talkable entities
        var talkables = new List<ICanBeTalkedTo>();
        talkables.AddRange(Context.Items.OfType<ICanBeTalkedTo>());

        if (Context.CurrentLocation is ICanContainItems container)
        {
            talkables.AddRange(container.Items.OfType<ICanBeTalkedTo>());
        }

        Console.WriteLine($"[CONVERSATION DEBUG] Found {talkables.Count} talkable entities in total");
        foreach (var talkable in talkables)
        {
            if (talkable is IItem item)
            {
                Console.WriteLine($"[CONVERSATION DEBUG] - {item.Name} (nouns: {string.Join(", ", item.NounsForMatching)})");
            }
            else
            {
                Console.WriteLine($"[CONVERSATION DEBUG] - {talkable.GetType().Name} (not an IItem)");
            }
        }

        if (talkables.Count == 0)
        {
            Console.WriteLine("[CONVERSATION DEBUG] No talkable entities found, returning null");
            return null;
        }

        // Check if input contains any character nouns (exact match only)
        var inputLower = input.ToLowerInvariant();
        Console.WriteLine($"[CONVERSATION DEBUG] Input lowercased: '{inputLower}'");
        
        var targetCharacter = talkables
            .OfType<IItem>()
            .FirstOrDefault(item => item.NounsForMatching
                .Any(noun => {
                    var nounLower = noun.ToLowerInvariant();
                    var contains = inputLower.Contains(nounLower);
                    Console.WriteLine($"[CONVERSATION DEBUG] Checking noun '{nounLower}' in '{inputLower}': {contains}");
                    return contains;
                })) as ICanBeTalkedTo;

        if (targetCharacter == null)
        {
            Console.WriteLine("[CONVERSATION DEBUG] No matching character found in input, returning null");
            return null;
        }

        Console.WriteLine($"[CONVERSATION DEBUG] Found target character: {(targetCharacter as IItem)?.Name ?? targetCharacter.GetType().Name}");

        // Use ParseConversation to determine if this is actually communication
        Console.WriteLine($"[CONVERSATION DEBUG] Calling ParseConversation.ParseAsync with input: '{input}'");
        var parseResult = await _parseConversation.ParseAsync(input);
        Console.WriteLine($"[CONVERSATION DEBUG] ParseConversation result - isNo: {parseResult.isNo}, response: '{parseResult.response}'");
        
        // If ParseConversation says "No", continue with normal processing
        if (parseResult.isNo)
        {
            Console.WriteLine("[CONVERSATION DEBUG] ParseConversation returned 'No', continuing with normal processing");
            return null;
        }

        // Send the rewritten message to the character
        Console.WriteLine($"[CONVERSATION DEBUG] Sending rewritten message '{parseResult.response}' to character");
        var result = await targetCharacter.OnBeingTalkedTo(parseResult.response, Context, GenerationClient);
        Console.WriteLine($"[CONVERSATION DEBUG] Character response: '{result}'");
        return result;
    }

    public async Task InitializeEngine()
    {
        try
        {
            _turnLogger = await CloudWatchLoggerFactory.Get<TurnLog>(_gameInstance.GameName, "Turns", _turnCorrelationId);

            GenerationClient.TurnCorrelationId = _turnCorrelationId;
            GenerationClient.CloudWatchLogger =
                await CloudWatchLoggerFactory.Get<GenerationLog>(_gameInstance.GameName, "ResponseGeneration",
                    _turnCorrelationId);

            _parser.TurnCorrelationId = _turnCorrelationId;
            _parser.Logger =
                await CloudWatchLoggerFactory.Get<GenerationLog>(_gameInstance.GameName, "InputParsing",
                    _turnCorrelationId);

            GenerationClient.SystemPrompt = await _secretsManager.GetSecret(
                _gameInstance.SystemPromptSecretKey
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private string FormatResult(string? contextPrepend, string? mainBody, string? actorResult, string? contextAppend)
    {
        var sb = new StringBuilder();

        if (!string.IsNullOrEmpty(contextPrepend))
            sb.AppendLine("\n" + contextPrepend.TrimEnd());

        if (!string.IsNullOrEmpty(mainBody))
            sb.AppendLine(mainBody.TrimEnd());

        if (!string.IsNullOrEmpty(actorResult))
            sb.AppendLine("\n" + actorResult.TrimEnd());

        if (!string.IsNullOrEmpty(contextAppend))
            sb.AppendLine("\n" + contextAppend.TrimEnd());

        return sb.ToString();
    }

    private async Task<(InteractionResult? resultObject, string? ResultMessage)> ProcessComplexIntent(
        IntentBase parsedResult)
    {
        _logger?.LogDebug($"Input was parsed as {parsedResult.GetType().Name}");

        // TODO: why does this return an interaction result and a result message? This feels vestigial. 
        var complexIntentResult = parsedResult switch
        {
            GlobalCommandIntent intent => (null, await ProcessGlobalCommandIntent(intent)),

            NullIntent => (
                null,
                await GetGeneratedNoOpResponse(_currentInput!, GenerationClient, Context)
            ),

            PromptIntent => (null, parsedResult.Message),

            EnterSubLocationIntent subLocationIntent => await new EnterSubLocationEngine().Process(
                subLocationIntent,
                Context,
                GenerationClient
            ),

            MoveIntent moveInteraction => await new MoveEngine().Process(moveInteraction, Context,
                GenerationClient),

            ExitSubLocationIntent exitSubLocationIntent =>
                await new ExitSubLocationEngine().Process(
                    exitSubLocationIntent,
                    Context,
                    GenerationClient
                ),

            TakeIntent takeIntent => await new TakeOrDropInteractionProcessor(_openAITakeAndDropListParser).Process(
                takeIntent, Context, GenerationClient),
            
            DropIntent dropIntent => await new TakeOrDropInteractionProcessor(_openAITakeAndDropListParser).Process(
                dropIntent, Context, GenerationClient),
            
            SimpleIntent simpleInteraction => await new SimpleInteractionEngine(_itemProcessorFactory).Process(
                simpleInteraction,
                Context,
                GenerationClient
            ),

            MultiNounIntent multiInteraction => await new MultiNounEngine().Process(
                multiInteraction,
                Context,
                GenerationClient
            ),

            _ => (null, await GetGeneratedNoOpResponse(_currentInput!, GenerationClient, Context))
        };

        if (complexIntentResult.resultObject is DisambiguationInteractionResult complexResult)
            _processorInProgress = new DisambiguationProcessor(complexResult);

        return complexIntentResult;
    }

    private async Task<string> ProcessActorsAndContextEndOfTurn(string? contextPrepend, string? turnResult)
    {
        var actors = await ProcessActors();
        var contextAppend = Context.ProcessEndOfTurn();
        Inventory = Context.Items.Select(s => s.Name).ToList();
        return PostProcessing(FormatResult(contextPrepend, turnResult, actors, contextAppend));
    }

    private string PostProcessing(string finalResult)
    {
        _logger?.LogDebug($"Items in inventory: {Context.LogItems()}");
        _logger?.LogDebug($"Items in location: {Context.CurrentLocation.LogItems()}");
        _logger?.LogDebug($"Moves: {Context.Moves}");

        if (!string.IsNullOrEmpty(_currentInput))
            _turnLogger?.WriteLogEvents(new TurnLog
            {
                SessionId = _sessionId,
                Location = Context.CurrentLocation.Name,
                Score = Context.Score,
                Moves = Context.Moves,
                Input = _currentInput,
                Response = finalResult.Trim()
            });

        if (!string.IsNullOrEmpty(finalResult))
        {
            _inputOutputs.Push((_currentInput!, finalResult, _lastResponseWasGenerated));
            GenerationClient.LastFiveInputOutputs = _inputOutputs.GetAll();
        }

        _lastResponseWasGenerated = false;
        return finalResult.TrimEnd() + Environment.NewLine;
    }

    private async Task<string> ProcessGlobalCommandIntent(GlobalCommandIntent intent)
    {
        var intentResponse = await intent.Command.Process(
            _currentInput,
            Context,
            GenerationClient,
            Runtime
        );
        if (intent.Command is IStatefulProcessor { Completed: false } statefulProcessor)
            _processorInProgress = statefulProcessor;

        intentResponse += Environment.NewLine;
        return intentResponse;
    }

    private async Task<string> ProcessActors()
    {
        var actorResults = string.Empty;
        foreach (var actor in Context.Actors.ToList())
        {
            _logger?.LogDebug($"Processing actor: {actor.GetType()}");
            var task = await actor.Act(Context, GenerationClient);
            actorResults += $"{task} ";
        }

        return actorResults.Trim();
    }

    private async Task<(bool, string?)> RunProcessorInProgress(string? playerInput)
    {
        string? processorInProgressOutput = null;
        var immediatelyReturn = false;

        // When this is not null, it means we have another processor in progress.
        // Defer all execution to that processor until it's complete.
        if (_processorInProgress == null)
            return (immediatelyReturn, processorInProgressOutput);

        processorInProgressOutput = await _processorInProgress.Process(
            playerInput,
            Context,
            GenerationClient,
            Runtime
        );

        // The processor is done. Clear it, and see what we want to do with the output.
        if (_processorInProgress.Completed)
        {
            var continueProcessingThisInput = _processorInProgress.ContinueProcessing;
            _processorInProgress = null;

            // Does the processor want us to return what it outputted?....
            if (!continueProcessingThisInput)
                immediatelyReturn = true;

            // ....or does it want to push that output through for further processing?
            _currentInput = processorInProgressOutput;
        }
        // Return the output and keep processing? Or are we done here yet.
        else
        {
            immediatelyReturn = true;
        }

        return (immediatelyReturn, processorInProgressOutput);
    }

    private static async Task<string> GetGeneratedNoOpResponse(
        string input,
        IGenerationClient generationClient,
        IContext context
        )
    {
        var request = new CommandHasNoEffectOperationRequest(
            context.CurrentLocation.GetDescriptionForGeneration(context),
            input
        );
        var result = await generationClient.GenerateNarration(request, context.SystemPromptAddendum);
        return result + Environment.NewLine;
    }


    private async Task<string> GetGeneratedNoCommandResponse()
    {
        var request = new EmptyRequest();
        var result = await GenerationClient.GenerateNarration(request, String.Empty);
        return result;
    }

    private static JsonSerializerSettings JsonSettings()
    {
        return new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            TypeNameHandling = TypeNameHandling.All,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            ContractResolver = new DoNotSerializeReadOnlyPropertiesResolver(),
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
        };
    }
}

/// <summary>
/// A custom resolver used to modify the serialization behavior of JSON objects
/// to exclude read-only properties. Extends <see cref="DefaultContractResolver" />
/// and provides a mechanism to exclude non-writable properties from being serialized.
/// </summary>
/// <remarks>
/// This class is useful for scenarios where serialization of read-only properties
/// is not desired, such as when saving the state of objects or generating JSON
/// responses to ensure only editable or writable properties are included.
/// This is achieved by overriding the <see cref="CreateProperty" /> method and adjusting
/// the serialization behavior based on property writability.
/// </remarks>
public class DoNotSerializeReadOnlyPropertiesResolver : DefaultContractResolver
{
    protected override JsonProperty CreateProperty(
        MemberInfo member,
        MemberSerialization memberSerialization
        )
    {
        var property = base.CreateProperty(member, memberSerialization);

        if (!property.Writable) property.ShouldSerialize = _ => false;

        return property;
    }
}
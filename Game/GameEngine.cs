using Game.IntentEngine;
using Game.StaticCommand;
using Game.StaticCommand.Implementation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Model.AIGeneration;
using Model.AIGeneration.Requests;
using Model.Interface;
using Newtonsoft.Json;
using Utilities;

namespace Game;

/// <summary>
///     The engine takes the input, figures our what to do with it, adjusts the state of the game
///     as required, and returns the output. It has a single method <see cref="GetResponse" />"
/// </summary>
/// <remarks>
///     Most of the work of figuring out what to do is deferred to the specific location we're in,
///     or the objects available in the locations, which are matched to the "noun" identified by the parser.
///     The state of the game is preserved in two places: the <see cref="Context" />, which is created by and
///     owned by the engine, and the repository, which contains all the items and locations in the game
///     (and their corresponding state). The <see cref="Repository" /> is static, self-contained and not owned by
///     anyone.
/// </remarks>
public class GameEngine<TInfocomGame, TContext> : IGameEngine where TInfocomGame : IInfocomGame, new()
    where TContext : IContext, new()
{
    private readonly AgainProcessor _againProcessor = new();

    private readonly IGenerationClient _generator;
    private readonly LimitedStack<(string, string, bool)> _inputOutputs = new();
    private readonly ItProcessor _itProcessor = new();
    private readonly ILogger<GameEngine<TInfocomGame, TContext>>? _logger;
    private readonly IIntentParser _parser;
    private readonly string _sessionId = Guid.NewGuid().ToString();

    private string? _currentInput;
    private bool _lastResponseWasGenerated;
    private IStatefulProcessor? _processorInProgress;
    internal TContext Context;

    [ActivatorUtilitiesConstructor]
    public GameEngine(ILogger<GameEngine<TInfocomGame, TContext>> logger)
    {
        _logger = logger;
        var gameInstance = new TInfocomGame();
        Context = new TContext
        {
            Engine = this,
            Game = gameInstance
        };

        Context.CurrentLocation.Init();
        Runtime = Runtime.Web;
        IntroText = $"""
                     {gameInstance.StartText}
                     {Context.CurrentLocation.Description}
                     """;

        _generator = new ChatGPTClient(_logger);
        _parser = new IntentParser(gameInstance.GetGlobalCommandFactory(), _logger);
        _generator.OnGenerate += () => _lastResponseWasGenerated = true;
    }

    /// <summary>
    ///     Constructor for unit test dependency injection.
    /// </summary>
    /// <param name="parser"></param>
    /// <param name="generationClient"></param>
    public GameEngine(IIntentParser parser, IGenerationClient generationClient)
    {
        Repository.Reset();

        Context = new TContext
        {
            Engine = this
        };

        IntroText = string.Empty;
        _parser = parser;
        _generator = generationClient;
    }

    public string IntroText { get; }

    public string LocationName => Context.CurrentLocation.Name;

    public IContext RestoreGame(string data)
    {
        var deserializeObject = JsonConvert.DeserializeObject<SavedGame<TContext>>(data, JsonSettings());
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

    public int Moves => Context.Moves;

    /// <summary>
    ///     Parse the input, determine the user's <see cref="IntentBase" /> and allow the
    ///     implementation of that specific intent to determine what to do next.
    /// </summary>
    /// <param name="playerInput">The text typed by the adventurer</param>
    /// <returns>The output which we need to display to the adventurer</returns>
    public async Task<string?> GetResponse(string? playerInput)
    {
        _currentInput = playerInput;

        // See if we have something already running like a save, quit, etc..
        // and see if it has any output. 
        var (returnProcessorInProgressOutput, processorInProgressOutput) = await RunProcessorInProgress();

        if (returnProcessorInProgressOutput)
            return PostProcessing(processorInProgressOutput!);

        if (string.IsNullOrEmpty(_currentInput))
            return PostProcessing(await GetGeneratedNoCommandResponse());

        // if the user referenced an object using "it", let's see 
        // if we can handle that. 
        var (requiresClarification, replacedInput) = _itProcessor.Check(_currentInput, Context);
        if (requiresClarification)
        {
            _processorInProgress = _itProcessor;
            return PostProcessing(replacedInput);
        }

        _currentInput = replacedInput;

        // See if the user typed "again" or some variation.
        // if so, we'll replace the input with their previous input. 
        (_currentInput, var returnResponseFromAgainProcessor) =
            _againProcessor.Process(_currentInput, Context);

        if (returnResponseFromAgainProcessor)
            return PostProcessing(_currentInput);

        // See if the context needs to notify us of anything. Are we sleepy? Hungry?
        var turnCounterResponse = Context.ProcessTurnCounter();

        // ----------------------------------------------------------------------------
        // We're done now doing pre-processing, we're ready to actually look at what the
        // user wrote and do something with it. 

        // Does the location have a special interaction to input such as "jump" or "pray"? 
        var singleVerbResult = Context.CurrentLocation.RespondToSpecificLocationInteraction(_currentInput, Context);
        if (singleVerbResult.InteractionHappened)
            return PostProcessing(singleVerbResult.InteractionMessage);

        var parsedResult =
            await _parser.DetermineIntentType(_currentInput, Context.CurrentLocation.Description, _sessionId);
        
        _logger?.LogDebug($"Input was parsed as {parsedResult.GetType().Name}");

        var intentResult = parsedResult switch
        {
            GlobalCommandIntent intent =>
                await ProcessGlobalCommandIntent(intent),

            NullIntent =>
                await GetGeneratedNoOpResponse(_currentInput, _generator, Context),

            PromptIntent => parsedResult.Message,
            
            EnterSubLocationIntent subLocationIntent =>
                await new EnterSubLocationEngine().Process(subLocationIntent, Context, _generator),
            
            ExitSubLocationIntent exitSubLocationIntent =>
                await new ExitSubLocationEngine().Process(exitSubLocationIntent, Context, _generator),

            MoveIntent moveInteraction =>
                await new MoveEngine().Process(moveInteraction, Context, _generator),

            SimpleIntent simpleInteraction =>
                await new SimpleInteractionEngine().Process(simpleInteraction, Context, _generator),

            MultiNounIntent multiInteraction =>
                await new MultiNounEngine().Process(multiInteraction, Context, _generator),

            _ => await GetGeneratedNoOpResponse(_currentInput, _generator, Context)
        };

        // "Actors" are things that can occur each turn. Examples are the troll
        // attacking, the maintenance room flooding, Floyd mumbling. 
        var actorResults = ProcessActors();
        return PostProcessing(turnCounterResponse + intentResult?.Trim() + actorResults);
    }

    public int Score => Context.Score;

    public Runtime Runtime { get; set; }

    private string PostProcessing(string finalResult)
    {
        Context.Moves++;

        if (!string.IsNullOrEmpty(finalResult))
        {
            _inputOutputs.Push((_currentInput!, finalResult, _lastResponseWasGenerated));
            _generator.LastFiveInputOutputs = _inputOutputs.GetAll();
        }

        _lastResponseWasGenerated = false;
        return finalResult.Trim() + Environment.NewLine;
    }

    private async Task<string> ProcessGlobalCommandIntent(GlobalCommandIntent intent)
    {
        var intentResponse = await intent.Command.Process(_currentInput, Context, _generator, Runtime);
        if (intent.Command is IStatefulProcessor { Completed: false } statefulProcessor)
            _processorInProgress = statefulProcessor;

        intentResponse += Environment.NewLine;
        return intentResponse;
    }

    private string ProcessActors()
    {
        var actorResults = string.Empty;
        foreach (var actor in Context.Actors.ToList())
            actorResults += $"{actor.Act(Context, _generator)} ";

        return actorResults;
    }

    private async Task<(bool, string?)> RunProcessorInProgress()
    {
        string? processorInProgressOutput = null;
        var immediatelyReturn = false;

        // When this is not null, it means we have another processor in progress.
        // Defer all execution to that processor until it's complete. 
        if (_processorInProgress == null)
            return (immediatelyReturn, processorInProgressOutput);

        processorInProgressOutput = await _processorInProgress.Process(_currentInput, Context, _generator, Runtime);

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

    private static async Task<string> GetGeneratedNoOpResponse(string input, IGenerationClient generationClient,
        IContext context)
    {
        var request =
            new CommandHasNoEffectOperationRequest(context.CurrentLocation.DescriptionForGeneration, input);
        var result = await generationClient.CompleteChat(request);
        return result + Environment.NewLine;
    }

    private async Task<string> GetGeneratedNoCommandResponse()
    {
        var request = new EmptyRequest();
        var result = await _generator.CompleteChat(request);
        return result;
    }

    private static JsonSerializerSettings JsonSettings()
    {
        return new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            TypeNameHandling = TypeNameHandling.All,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects
        };
    }
}
using System.Diagnostics;
using Game.IntentEngine;
using Game.StaticCommand;
using Game.StaticCommand.Implementation;
using Newtonsoft.Json;

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
public class GameEngine<T> : IGameEngine where T : IInfocomGame, new()
{
    private readonly IGenerationClient _generator;
    private readonly ItProcessor _itProcessor;
    private readonly IIntentParser _parser;

    private readonly string _sessionId = Guid.NewGuid().ToString();

    public readonly string IntroText;
    private IStatefulProcessor? _processorInProgress;
    internal Context<T> Context;

    public GameEngine()
    {
        var gameType = new T();
        Context = new Context<T>(this, gameType);

        IntroText = $"""
                     {gameType.StartText}
                     {Context.CurrentLocation.Description}
                     """;

        _generator = new Client();
        _parser = new IntentParser();
        _itProcessor = new ItProcessor();
    }

    /// <summary>
    ///     Constructor for unit test dependency injection.
    /// </summary>
    /// <param name="parser"></param>
    /// <param name="generationClient"></param>
    public GameEngine(IIntentParser parser, IGenerationClient generationClient)
    {
        var gameType = new T();
        Context = new Context<T>(this, gameType);
        IntroText = string.Empty;
        _parser = parser;
        _generator = generationClient;
        _itProcessor = new ItProcessor();
    }

    public IContext RestoreGame(string data)
    {
        var deserializeObject = JsonConvert.DeserializeObject<SavedGame<T>>(data, JsonSettings());
        var allItems = deserializeObject?.AllItems ?? throw new ArgumentException();
        var allLocations = deserializeObject.AllLocations ?? throw new ArgumentException();

        Repository.Restore(allItems, allLocations);

        Context = deserializeObject.Context ?? throw new ArgumentException();
        Context.Engine = this;
        Context.Game = new T();

        return Context;
    }

    public string SaveGame()
    {
        var savedGame = Repository.Save<T>();
        savedGame.Context = Context;
        return JsonConvert.SerializeObject(savedGame, JsonSettings());
    }

    /// <summary>
    ///     Parse the input, determine the user's <see cref="IntentBase" /> and allow the
    ///     implementation of that specific intent to determine what to do next.
    /// </summary>
    /// <param name="input">The text typed by the adventurer</param>
    /// <returns>The output which we need to display to the adventurer</returns>
    public async Task<string?> GetResponse(string? input)
    {
        string? processorOutput;

        // TODO: Refactor this. It's ugly. 

        // When this is not null, it means we have another processor in progress.
        // Defer all execution to that processor until it's complete. 
        if (_processorInProgress != null)
        {
            processorOutput = await _processorInProgress.Process(input, Context, _generator);

            // The processor is done. Clear it, and see what we want to do with the output. 
            if (_processorInProgress.Completed)
            {
                var continueProcessingThisInput = _processorInProgress.ContinueProcessing;
                _processorInProgress = null;

                // Does the processor want us to return what it outputted?....
                if (!continueProcessingThisInput)
                    return processorOutput + Environment.NewLine;

                // ....or does it want to push that output through for further processing? 
                input = processorOutput;
            }
            // Return the output and keep processing. We're not done here yet.  
            else
            {
                return processorOutput + Environment.NewLine;
            }
        }

        if (string.IsNullOrEmpty(input))
            return await GetGeneratedNoCommandResponse();

        Context.IncreaseMoves();

        // Does the location have a special interaction to input such as "jump" or "pray"? 
        var singleVerbResult = Context.CurrentLocation.RespondToSpecificLocationInteraction(input, Context);
        if (singleVerbResult.InteractionHappened)
            return singleVerbResult.InteractionMessage;

        // if the user referenced an object using "it", let's see 
        // if we can handle that. 
        var result = _itProcessor.Check(input, Context);
        if (result.RequiresClarification)
        {
            _processorInProgress = _itProcessor;
            return result.Output;
        }

        input = result.Output;

        var parsedResult = await _parser.DetermineIntentType(input, _sessionId);
        Debug.WriteLine($"Input was parsed as {parsedResult.GetType().Name}");

        switch (parsedResult)
        {
            case GlobalCommandIntent intent:
                processorOutput = await intent.Command.Process(input, Context, _generator);
                if (intent.Command is IStatefulProcessor { Completed: false } statefulProcessor)
                    _processorInProgress = statefulProcessor;
                return processorOutput + Environment.NewLine;

            case NullIntent:
                processorOutput = await GetGeneratedNoOpResponse(input, _generator, Context);
                break;

            case PromptIntent:
                processorOutput = parsedResult.Message;
                break;

            case MoveIntent moveInteraction:
                processorOutput = await new MoveEngine().Process(moveInteraction, Context, _generator);
                break;

            case SimpleIntent simpleInteraction:
                processorOutput = await new SimpleInteractionEngine().Process(simpleInteraction, Context, _generator);
                break;

            case MultiNounIntent multiInteraction:
                processorOutput = await new MultiNounEngine().Process(multiInteraction, Context, _generator);
                break;

            default:
                processorOutput = await GetGeneratedNoOpResponse(input, _generator, Context);
                break;
        }

        return processorOutput?.Trim() + Environment.NewLine;
    }

    private static async Task<string> GetGeneratedNoOpResponse(string input, IGenerationClient generationClient,
        Context<T> context)
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
            TypeNameHandling = TypeNameHandling.All,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects
        };
    }
}
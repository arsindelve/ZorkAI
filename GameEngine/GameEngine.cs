using System.Reflection;
using System.Text.RegularExpressions;
using ChatLambda;
using CloudWatch;
using CloudWatch.Model;
using GameEngine.IntentEngine;
using GameEngine.Item;
using GameEngine.Item.ItemProcessor;
using GameEngine.StaticCommand;
using GameEngine.StaticCommand.Implementation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Model.AIGeneration;
using Model.AIGeneration.Requests;
using Model.Interface;
using Model.Movement;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Utilities;
using ZorkAI.OpenAI;

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
    where TContext : class, IContext, new()
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
    private readonly ConversationHandler _conversationHandler;
    private string? _currentInput;
    private TInfocomGame _gameInstance;
    private bool _lastResponseWasGenerated;
    private IStatefulProcessor? _processorInProgress;
    private ICloudWatchLogger<TurnLog>? _turnLogger;
    public TContext Context { get; private set; }

    /// <summary>
    ///     The single guaranteed static fallback sentence for the engine error safety net (issue #271).
    ///     This is the one acceptable canned string: it is returned only when the AI narrator itself
    ///     cannot be reached (generation disabled, or it threw while producing the "oops" narration), so
    ///     that turn processing still returns a graceful, in-character 200 body instead of a hard failure.
    /// </summary>
    public const string EngineErrorFallbackMessage =
        "Something goes wrong, and for a moment the world seems to flicker. Perhaps try that a different way.";

    /// <summary>
    ///     The flat list of held item names, used by the web clients to render the player's hands.
    ///     Derived live from <see cref="Context" />.Items rather than stored, so the projection can
    ///     never drift from the authoritative game state. A stored copy previously went stale on the
    ///     GET no-turn rehydrate path (RestoreGame did not refresh it), reporting empty hands on
    ///     reconnect/refresh even while items were held (issue #230).
    /// </summary>
    public List<string> Inventory => Context.Items.Select(s => s.Name).ToList();

    /// <summary>
    ///     Explicit interface implementation to satisfy IGameEngine.Context requirement.
    /// </summary>
    IContext IGameEngine.Context => Context;
    
    [ActivatorUtilitiesConstructor]
    public GameEngine(
        ILogger<GameEngine<TInfocomGame, TContext>> logger,
        ISecretsManager secretsManager,
        IParseConversation parseConversation)
    {
        _logger = logger;
        _secretsManager = secretsManager;
        parseConversation.Logger = logger;
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
        _conversationHandler = new ConversationHandler(_logger, parseConversation, GenerationClient,
            _gameInstance.TalkableCharacterTypes);
    }

    /// <summary>
    ///     Constructor for unit test dependency injection.
    /// </summary>
    /// <param name="itemProcessorFactory"></param>
    /// <param name="parser"></param>
    /// <param name="generationClient"></param>
    /// <param name="secretsManager"></param>
    /// <param name="turnLogger"></param>
    /// <param name="parseConversation"></param>
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
        IntroText = string.Empty;
        _parser = parser;
        GenerationClient = generationClient;
        _secretsManager = secretsManager;
        var parseConversation1 = parseConversation;
        _gameInstance = (TInfocomGame)Context.Game;
        _gameInstance.Init(Context);
        _itemProcessorFactory = itemProcessorFactory;
        _turnLogger = turnLogger;
        _openAITakeAndDropListParser = null!;
        _conversationHandler = new ConversationHandler(null, parseConversation1, GenerationClient,
            _gameInstance.TalkableCharacterTypes);
    }

    public int Score => Context.Score;

    public Runtime Runtime { get; set; }

    private bool _noGeneratedResponses;
    public bool NoGeneratedResponses
    {
        get => _noGeneratedResponses;
        set
        {
            _noGeneratedResponses = value;
            GenerationClient.IsDisabled = value;
        }
    }

    public string SessionTableName => _gameInstance.SessionTableName;

    public string IntroText { get; }

    public IGenerationClient GenerationClient { get; }

    public string LocationName => Context.CurrentLocation.Name;

    // Projection of Context.PreviousLocationName (mirrors LastMovementDirection below). It must
    // live on Context, not as an engine-only field, so it serializes/restores — otherwise the GET
    // no-turn rehydrate path leaves it null while POST returns it, losing the "came from" location
    // on reconnect (issue #250).
    public string? PreviousLocationName => Context.PreviousLocationName;

    public Direction? LastMovementDirection => Context.LastMovementDirection;

    public string LocationDescription => Context.CurrentLocation.GetDescriptionForGeneration(Context);

    public int Moves => Context.Moves;

    public int CurrentTime => Context is ITimeBasedContext tc ? tc.CurrentTime : 0;

    public List<Direction> Exits => Context.CurrentLocation.Exits(Context);

    /// <summary>
    ///     Parse the input, determine the user's <see cref="IntentBase" /> and allow the
    ///     implementation of that specific intent to determine what to do next.
    ///     Supports multiple sentences separated by periods (e.g., "take lamp. go north").
    /// </summary>
    /// <param name="playerInput">The text typed by the adventurer</param>
    /// <returns>The output which we need to display to the adventurer</returns>
    public async Task<string?> GetResponse(string? playerInput)
    {
        // Engine-wide safety net (issue #271): no matter what throws deep in turn processing, the
        // player must never see a raw HTTP 500 / empty body. Every turn entry point funnels through
        // GetResponse — POST, the GET reconnect "look", and restoreGame — so this single wrap covers
        // them all across Zork, Planetfall, and EscapeRoom. Specific bugs that throw still get their
        // own root-cause fixes; this only guarantees the *next* unknown crash degrades into a
        // graceful, logged, in-character "oops" rather than breaking the transport to the client.
        try
        {
            _currentInput = playerInput;

            // Check for multi-sentence input
            var sentences = SentenceSplitter.Split(playerInput);

            if (sentences.Count == 0)
            {
                // Empty input (like "...") - treat as empty command
                return await ProcessSingleSentence(null);
            }

            if (sentences.Count > 1)
            {
                return await ProcessMultipleSentences(sentences);
            }

            // Single sentence processing
            return await ProcessSingleSentence(playerInput);
        }
        catch (Exception ex)
        {
            // Intentionally broad — this deliberately includes OperationCanceledException /
            // TaskCanceledException, which is also how HttpClient timeouts (e.g. the OpenAI
            // generation call) surface. Those are genuine "something went wrong deep in the engine"
            // failures that must degrade gracefully. GetResponse has no CancellationToken to tell a
            // real client-cancel apart from a timeout, so filtering cancellation out here would let
            // timeouts regress straight back into the HTTP 500s this net exists to prevent.
            return await HandleUnexpectedEngineError(ex);
        }
    }

    /// <summary>
    ///     Converts an unhandled turn-processing exception into a graceful, in-character narrator
    ///     "oops" response (issue #271). The exception is logged loudly — via the engine logger and,
    ///     best-effort, to CloudWatch with the turn correlation id — so the underlying bug is still
    ///     discoverable; the safety net must never <em>hide</em> a bug. The player gets a normal-shaped
    ///     200 turn response and can keep playing.
    /// </summary>
    private async Task<string> HandleUnexpectedEngineError(Exception ex)
    {
        // Abandon any half-finished stateful interaction (disambiguation, save/quit confirmation,
        // "it" clarification). A crash can occur *after* _processorInProgress was assigned but before
        // the turn completed; if we leave it set, the NEXT turn's input is fed into that orphaned
        // processor instead of being parsed normally — soft-locking the player in a long-lived engine
        // (e.g. console runtime). Clearing it keeps the "player can keep playing" guarantee (issue #271).
        _processorInProgress = null;

        _logger?.LogError(ex,
            "Unhandled exception during turn processing for input '{Input}'. TurnCorrelationId: {TurnCorrelationId}",
            _currentInput, _turnCorrelationId);

        // Best-effort structured log with the turn correlation id so the swallowed exception stays
        // discoverable. Logging must never be allowed to defeat the safety net it supports.
        try
        {
            _turnLogger?.WriteLogEvents(new TurnLog
            {
                SessionId = _sessionId,
                Location = Context.CurrentLocation.Name,
                Score = Context.Score,
                Moves = Context.Moves,
                Input = _currentInput ?? string.Empty,
                Response = $"ENGINE ERROR ({_turnCorrelationId}): {ex}"
            });
        }
        catch (Exception loggingEx)
        {
            _logger?.LogError(loggingEx, "Failed to write engine-error turn log.");
        }

        // Prefer an AI-generated, in-character acknowledgement (consistent with the engine's other
        // deflection narration). Only fall back to the canned sentence when the narrator itself is
        // unavailable — i.e. generation is disabled, or the AI client is the very thing that failed.
        if (!NoGeneratedResponses)
        {
            try
            {
                var narration = await GenerationClient.GenerateNarration(
                    new EngineErrorRequest(), Context.SystemPromptAddendum);

                if (!string.IsNullOrWhiteSpace(narration))
                    return SafePostProcess(narration);
            }
            catch (Exception generationEx)
            {
                _logger?.LogError(generationEx,
                    "Engine-error narration generation itself failed; using static fallback.");
            }
        }

        return SafePostProcess(EngineErrorFallbackMessage);
    }

    /// <summary>
    ///     Runs the normal <see cref="PostProcessing" /> pipeline but, as the safety net's last line of
    ///     defense, guarantees a non-empty 200 body even if post-processing itself throws.
    /// </summary>
    private string SafePostProcess(string message)
    {
        try
        {
            return PostProcessing(message);
        }
        catch (Exception postEx)
        {
            _logger?.LogError(postEx, "Post-processing failed inside the engine-error safety net.");
            return message + Environment.NewLine;
        }
    }

    /// <summary>
    ///     Processes multiple sentences sequentially, maintaining game state between each command.
    /// </summary>
    private async Task<string> ProcessMultipleSentences(List<string> sentences)
    {
        var responses = new List<string>();

        foreach (var sentence in sentences)
        {
            _logger?.LogDebug($"Processing sentence: {sentence}");

            var response = await ProcessSingleSentence(sentence);

            if (!string.IsNullOrWhiteSpace(response))
            {
                responses.Add(response.TrimEnd());
            }

            // Check if a processor needs user input (like save, quit, disambiguation)
            if (_processorInProgress == null) 
                continue;
            
            _logger?.LogDebug($"Processor in progress ({_processorInProgress.GetType().Name}) - stopping multi-sentence processing");
            break;
        }

        return string.Join(Environment.NewLine + Environment.NewLine, responses);
    }

    /// <summary>
    ///     Rewrites a leading "look at &lt;noun&gt;" into the canonical "examine &lt;noun&gt;" so it routes
    ///     through the examine-with-noun path instead of collapsing to the bare-room LOOK command
    ///     (issues #312 / #283). Only the exact "look at " prefix followed by a noun is rewritten — bare
    ///     "look"/"look around" and other "look &lt;preposition&gt;" phrases (e.g. "look under the rug",
    ///     "look in the box") are intentionally left alone.
    /// </summary>
    internal static string? NormalizeLookAt(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        const string prefix = "look at ";
        var trimmed = input.TrimStart();
        if (!trimmed.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return input;

        var noun = trimmed[prefix.Length..].Trim();
        return string.IsNullOrEmpty(noun) ? input : "examine " + noun;
    }

    /// <summary>
    ///     Processes a single sentence/command through the game engine.
    /// </summary>
    private async Task<string?> ProcessSingleSentence(string? playerInput)
    {
        _currentInput = playerInput;

        // 1. ------- Processor in Progress -
        // See if we have something already running like a save, quit, etc.
        // and see if it has any output.  Does not count as a turn. No actor or turn processing.
        var (returnProcessorInProgressOutput, processorInProgressOutput) =
            await RunProcessorInProgress(playerInput);

        if (returnProcessorInProgressOutput)
            return PostProcessing(processorInProgressOutput!);

        // After disambiguation resolution, _currentInput may have been updated to the clarified command
        // Use it for all subsequent processing
        playerInput = _currentInput;

        // 2. -------  Empty command. Does not count as a turn. No actor or turn processing.
        if (string.IsNullOrEmpty(playerInput))
            return PostProcessing(await GetGeneratedNoCommandResponse());

        // 2b. ------- "look at <noun>" is an examine synonym, not the bare-room LOOK command. The AI
        // parser (rule (f) in the system prompt) collapses "look at X" to a noun-less look intent for
        // single-word nouns, re-describing the room instead of the object (issues #312 / #283). Rewrite
        // it to the canonical "examine <noun>" here so it routes through the examine-with-noun path,
        // while leaving the bare forms ("look", "look around") and other "look <prep>" phrases
        // ("look under the rug", "look in the box") untouched. Note: because this runs before pronoun
        // resolution and the LastInput capture below, a subsequent "again"/"g" replays "look at X" as
        // "examine X" — harmless, since both produce the same examination output.
        playerInput = NormalizeLookAt(playerInput);
        _currentInput = playerInput;

        Context.PreviousLocationName = LocationName;

        // 3. ------- System, or "meta" commands - like save, restore, quit, verbose etc. Does not count as a turn. No actor or turn processing.
        var systemCommand = _parser.DetermineSystemIntentType(playerInput);
        if (systemCommand is GlobalCommandIntent global)
        {
            var globalResult = await ProcessGlobalCommandIntent(global);
            return PostProcessing(globalResult);
        }

        // 3b. ------- Context-level command override. Lets a game-specific context fully handle the
        // raw command before normal parsing — used by Zork's spirit/DEAD state, which overrides most
        // verbs with canned ghost responses while letting movement and resurrection fall through by
        // returning null (issue #17). Does not count as a turn; no actor or end-of-turn processing.
        var contextOverride = Context.InterceptPlayerCommand(playerInput);
        if (contextOverride is not null)
            return PostProcessing(contextOverride);

        // Everything below here counts as a turn. Pre-process the turn.
        // See if the context needs to notify us of anything. Are we sleepy? Hungry?
        var contextPrepend = Context.ProcessBeginningOfTurn();

        // Check if player died during beginning-of-turn processing (e.g., hunger death)
        if (Context.PendingDeath is not null)
        {
            var deathResult = Context.PendingDeath;
            var deathMessage = deathResult.InteractionMessage;
            RestartAfterDeath(deathResult.DeathCount);
            return PostProcessing(deathMessage + Context.CurrentLocation.GetDescription(Context));
        }

        // Issue #355: a scheduled event (e.g. Planetfall's forced sleep) consumed this turn during
        // ProcessBeginningOfTurn, mutating state (dropping carried items, changing location) against
        // wherever the player was BEFORE their own command ran. Executing that command now - most
        // dangerously a movement command - would change CurrentLocation again, leaving the narration
        // and any side effects of the event stranded against a location the response never mentions
        // again. Short-circuit here, mirroring the PendingDeath check above: report wherever the
        // player actually is instead of running their command, which is deferred to next turn. Still
        // routed through ProcessActorsAndContextEndOfTurn so the clock ticks and actors act, exactly
        // as they would for any other turn.
        if (Context.TurnConsumedByForcedEvent)
        {
            Context.TurnConsumedByForcedEvent = false;
            return await ProcessActorsAndContextEndOfTurn(
                contextPrepend, Context.CurrentLocation.GetDescription(Context));
        }

        // See if the user typed "again" or some variation.
        // if so, we'll replace the input with their previous input.
        (_currentInput, var returnResponseFromAgainProcessor) = _againProcessor.Process(
            _currentInput!,
            Context
        );
        if (returnResponseFromAgainProcessor)
            return PostProcessing(_currentInput);

        // Resolve pronouns from recent player input and game response (BEFORE ItProcessor), UNLESS a
        // just-completed move left the deterministic engine holding a still-carried antecedent for
        // this pronoun. After a move, LastInput is the movement command ("north") and LastResponse is
        // the destination room's description, so the AI resolver would re-bind "it"/"them" to a noun
        // in the NEW room and lose the carried-item antecedent that MoveEngine deliberately preserved
        // across the move (issues #248 / #275). In that case we defer to the deterministic ItProcessor
        // below, which resolves the pronoun from the preserved LastNoun/LastNouns.
        if (!MoveJustClobberedPronounContext(_currentInput!, Context)
            && (!string.IsNullOrEmpty(Context.LastInput) || !string.IsNullOrEmpty(Context.LastResponse)))
        {
            var resolved = await _parser.ResolvePronounsAsync(_currentInput!, Context.LastInput, Context.LastResponse);
            if (resolved != null && !resolved.Equals(_currentInput, StringComparison.OrdinalIgnoreCase)
                && !ResolverConflatedSingularItWithASet(_currentInput!, resolved))
            {
                _currentInput = resolved;
            }
        }

        // Track player input for pronoun resolution (AFTER resolution, so next command can use this as context)
        // Store the RESOLVED input so subsequent pronoun resolution has the actual noun, not the pronoun
        if (!string.IsNullOrWhiteSpace(_currentInput))
            Context.LastInput = _currentInput;

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
        _logger?.LogDebug($"[GAME ENGINE DEBUG] About to check for conversation with input: '{_currentInput}'");
        var conversation = await _conversationHandler.CheckForConversation(_currentInput, Context);
        if (conversation is not null)
        {
            _logger?.LogDebug($"[GAME ENGINE DEBUG] Conversation detected, returning response: '{conversation}'");
            return await ProcessActorsAndContextEndOfTurn(contextPrepend, conversation);
        }
        _logger?.LogDebug("[GAME ENGINE DEBUG] No conversation detected, continuing with normal processing");

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

    /// <summary>
    ///     True when the immediately preceding command was a movement and the player's current command
    ///     uses "it"/"them" with a still-carried antecedent that <see cref="MoveEngine" /> preserved
    ///     across that move (issue #248). When this holds we must NOT run the AI pronoun resolver: right
    ///     after a move its only context is the movement command (LastInput) and the destination room's
    ///     description (LastResponse), so it re-binds the pronoun to a noun in the NEW room and the
    ///     carried-item antecedent is lost (issue #275 — "the invisible gangway"). The deterministic
    ///     <see cref="ItProcessor" /> downstream resolves the pronoun from LastNoun/LastNouns instead.
    ///
    ///     The check is deliberately scoped to the post-move case so the AI resolver keeps its value for
    ///     every other turn — semantic rewrites like "put it on" -> "wear X", resolving from response
    ///     narration, the other pronouns (him/her/that/...), and so on.
    /// </summary>
    private static bool MoveJustClobberedPronounContext(string input, IContext context)
    {
        // "the move is the trigger": only defer to the preserved antecedent when the previous command
        // (now sitting in LastInput) was itself a movement. Any non-move command refreshes LastInput
        // with a real noun phrase, which is exactly what the AI resolver needs to work correctly.
        if (!DirectionParser.IsDirection(context.LastInput, out _))
            return false;

        // Only "it"/"them" are resolved deterministically from LastNoun/LastNouns; for any other
        // pronoun (him, her, that, ...) the AI resolver is the only thing that can help, so let it run.
        if (Regex.IsMatch(input, @"\bit\b", RegexOptions.IgnoreCase))
            return !string.IsNullOrEmpty(context.LastNoun) &&
                   context.HasMatchingNoun(context.LastNoun).HasItem;

        if (Regex.IsMatch(input, @"\bthem\b", RegexOptions.IgnoreCase))
            return context.LastNouns.Any(noun => context.HasMatchingNoun(noun).HasItem);

        return false;
    }

    /// <summary>
    ///     True when the AI pronoun resolver rewrote a SINGULAR "it" into a multi-object noun phrase,
    ///     e.g. "drop it" -&gt; "drop rope and knife" (issue #341). This happens after a multi-object
    ///     action like "take all": the resolver's only context is the previous command/response naming
    ///     every object that was handled, so it conflates "it" with the whole set instead of the one
    ///     object the player actually meant. "them" is the collection pronoun (issue #248) and is not
    ///     affected - this guard only fires when the ORIGINAL command used singular "it". When it fires,
    ///     the rewrite is discarded and the deterministic <see cref="ItProcessor" /> downstream resolves
    ///     "it" from the single last-handled antecedent instead.
    /// </summary>
    private static bool ResolverConflatedSingularItWithASet(string original, string resolved)
    {
        if (!Regex.IsMatch(original, @"\bit\b", RegexOptions.IgnoreCase))
            return false;

        // A correct singular rewrite swaps "it" for exactly one noun phrase. A rewrite that introduces
        // a conjunction the original didn't already have means the resolver named more than one object.
        return Regex.IsMatch(resolved, @"\band\b", RegexOptions.IgnoreCase)
               && !Regex.IsMatch(original, @"\band\b", RegexOptions.IgnoreCase);
    }

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

    /// <summary>
    ///     Restarts the game after player death, preserving only the death counter.
    ///     Uses the same code path as starting a fresh game to ensure complete state reset.
    /// </summary>
    /// <param name="deathCount">The death counter to preserve across restarts.</param>
    private void RestartAfterDeath(int deathCount)
    {
        _logger?.LogInformation($"Restarting game after death #{deathCount}");

        // Reset repository (clears all items/locations)
        Repository.Reset();

        // Create fresh game instance and context
        _gameInstance = new TInfocomGame();
        Context = new TContext
        {
            Engine = this,
            Game = _gameInstance,
            Verbosity = Verbosity.Brief
        };

        // Re-initialize location and game
        Context.CurrentLocation.Init();
        Context.Init();
        _gameInstance.Init(Context);

        // Restore cross-death state via virtual method (game-agnostic)
        Context.SetDeathCount(deathCount);
    }

    /// <summary>
    /// Saves the current game state, including game context and other relevant metadata,
    /// and returns it as a serialized JSON string.
    /// </summary>
    /// <returns>
    /// A JSON string representing the serialized state of the game, including type
    /// and reference metadata for restoration purposes.
    /// </returns>
    public string SaveGame()
    {
        var savedGame = Repository.Save<TContext>();
        savedGame.Context = Context;
        return JsonConvert.SerializeObject(savedGame, JsonSettings());
    }

    public async Task<string> GenerateSaveGameNarration()
    {
        // Ask the context if it wants to provide a custom save game request (e.g., Floyd in Planetfall)
        // If not, use the default AfterSaveGameRequest
        var saveRequest = Context.GetSaveGameRequest(LocationDescription)
                          ?? new AfterSaveGameRequest(LocationDescription);

        return await GenerationClient.GenerateNarration(saveRequest, string.Empty);
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
            sb.AppendLine(contextPrepend.TrimEnd());

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

            InventoryIntent => (null, await new InventoryProcessor().Process("", Context, GenerationClient, Runtime.Unknown)),
            
            LookIntent => (null, await new LookProcessor().Process("", Context, GenerationClient, Runtime.Unknown)),
            
            PromptIntent => (null, parsedResult.Message),

            // #256: the player ran multiple commands together on one line with no periods.
            // We don't execute them; the narrator asks them to separate commands with periods.
            MultipleCommandsIntent => (
                null,
                await GetGeneratedMultipleCommandsResponse(_currentInput!, GenerationClient, Context)
            ),

            EnterSubLocationIntent subLocationIntent => await new EnterSubLocationEngine().Process(
                subLocationIntent,
                Context,
                GenerationClient
            ),

            MoveIntent moveInteraction => await new MoveEngine().Process(moveInteraction, Context,
                GenerationClient),

            // Issue #268: "go to / walk to / enter <named room>" — resolve the name against the
            // current room's exits and walk there (one hop), or ask which one / refuse.
            GoToDestinationIntent goToIntent => await new DestinationNavigationEngine().Process(goToIntent,
                Context, GenerationClient),

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
        // Check if player died during the turn (e.g., from location hazards, items, etc.)
        if (Context.PendingDeath is not null)
        {
            var deathResult = Context.PendingDeath;
            // Include any context and turn result before the death message (e.g., "The door opens.")
            // The death message is already in turnResult since it was returned from the interaction
            var preDeathOutput = FormatResult(contextPrepend, turnResult, null, null);
            RestartAfterDeath(deathResult.DeathCount);
            return PostProcessing(preDeathOutput + "\n" + Context.CurrentLocation.GetDescription(Context));
        }

        // Skip actor processing when disambiguation question is being asked
        // The player hasn't completed a real action yet - just asking for clarification
        // Note: When disambiguation is resolved (player answers), the clarified command IS a real action
        var actors = _processorInProgress is DisambiguationProcessor
            ? string.Empty
            : await ProcessActors();

        // Check if player died during actor processing (e.g., Bio Lock mutations)
        if (Context.PendingDeath is not null)
        {
            var deathResult = Context.PendingDeath;
            // The death message is already included in actors output (from BioLockStateMachineManager etc.)
            // We just need to format everything before death, restart, and append the new location
            var preDeathOutput = FormatResult(contextPrepend, turnResult, actors, null);
            RestartAfterDeath(deathResult.DeathCount);
            return PostProcessing(preDeathOutput + "\n" + Context.CurrentLocation.GetDescription(Context));
        }

        var contextAppend = Context.ProcessEndOfTurn();
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

        // Store response for pronoun resolution
        var trimmedResult = finalResult.TrimEnd();
        if (!string.IsNullOrWhiteSpace(trimmedResult))
            Context.LastResponse = trimmedResult;

        return trimmedResult + Environment.NewLine;
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

            // Stop processing actors if death occurred - subsequent actors should not run
            if (Context.PendingDeath is not null)
            {
                _logger?.LogDebug("Death occurred during actor processing, stopping remaining actors");
                break;
            }
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


    private static async Task<string> GetGeneratedMultipleCommandsResponse(
        string input,
        IGenerationClient generationClient,
        IContext context
        )
    {
        var request = new MultipleCommandsRequest(
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
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            // Replace collections during deserialization instead of adding to them.
            // This prevents items from accumulating in Items lists across session restores.
            ObjectCreationHandling = ObjectCreationHandling.Replace
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
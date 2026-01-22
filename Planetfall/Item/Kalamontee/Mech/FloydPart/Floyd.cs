using ChatLambda;
using GameEngine.IntentEngine;
using Model.AIGeneration;
using Newtonsoft.Json;
using Planetfall.Item.Kalamontee.Admin;
using Planetfall.Item.Lawanda;
using Planetfall.Location;
using Utilities;

namespace Planetfall.Item.Kalamontee.Mech.FloydPart;

public class Floyd : QuirkyCompanion, IAmANamedPerson, ICanHoldItems, ICanBeGivenThings, ICanBeTalkedTo
{
    private readonly GiveSomethingToSomeoneDecisionEngine<Floyd> _giveHimSomethingEngine = new();
    private readonly FloydLocationBehaviors _locationBehaviors;
    private readonly FloydPowerManager _powerManager;
    private readonly FloydInventoryManager _inventoryManager;
    private readonly FloydSocialResponses _socialResponses;
    private readonly FloydMovementManager _movementManager;

    public Floyd()
    {
        _locationBehaviors = new FloydLocationBehaviors(this);
        _powerManager = new FloydPowerManager(this);
        _inventoryManager = new FloydInventoryManager(this);
        _socialResponses = new FloydSocialResponses(this);
        _movementManager = new FloydMovementManager(this);
    }

    // This is the thing that he is holding, literally in his hand. 
    [UsedImplicitly] public IItem? ItemBeingHeld { get; set; }
       
    [UsedImplicitly] public bool HasDied { get; set; }
    
    [UsedImplicitly] [JsonIgnore] public IRandomChooser Chooser { get; set; } = new RandomChooser();

    [UsedImplicitly] [JsonIgnore] public IChatWithFloyd ChatWithFloyd { get; set; } = new ChatWithFloyd(null);

    [UsedImplicitly] public bool IsOn { get; set; }

    [UsedImplicitly] public bool IsOffWandering { get; set; }

    [UsedImplicitly] public int WanderingTurnsRemaining { get; set; }

    [UsedImplicitly] public bool HasEverBeenOn { get; set; }

    [UsedImplicitly] public bool HasEverGoneThroughTheLittleDoor { get; set; }

    [UsedImplicitly] public bool HasGottenTheFromitzBoard { get; set; }

    // When you initially turn on Floyd, nothing happens for 3 turns. This delay never happens
    // again if you turn him on/off another time.
    [UsedImplicitly] public int TurnOnCountdown { get; set; } = 3;

    /// <summary>
    /// Checks if Floyd is both turned on and in the same location as the player.
    /// </summary>
    /// <param name="context">The game context containing the player's current location.</param>
    /// <returns>True if Floyd is on and in the player's current location; otherwise, false.</returns>
    public bool IsHereAndIsOn(IContext context)
    {
        return IsOn && IsInTheRoom(context);
    }

    /// <summary>
    /// Call this from anywhere to queue Floyd to comment on a player action.
    /// The comment will be generated automatically during Floyd's Act() phase.
    /// Each prompt can only be used once per game - repeat calls with the same prompt are ignored.
    /// </summary>
    /// <param name="prompt">The AI prompt describing what Floyd should say</param>
    /// <param name="context">Current game context</param>
    public void CommentOnAction(string prompt, IContext context)
    {
        // Must be on and in the same location as player
        if (!IsHereAndIsOn(context))
            return;

        if (context is not PlanetfallContext planetfallContext)
            return;

        // Only one action comment per turn
        if (planetfallContext.PendingFloydActionCommentPrompt != null)
            return;

        // Don't repeat prompts that have already been used
        if (planetfallContext.UsedFloydActionCommentPrompts.Contains(prompt))
            return;

        // Store the prompt for Act() to process and mark as used
        planetfallContext.PendingFloydActionCommentPrompt = prompt;
        planetfallContext.UsedFloydActionCommentPrompts.Add(prompt);
    }

    /// <summary>
    /// Call this from anywhere to prevent Floyd from acting or talking this turn.
    /// Useful when something important is happening and Floyd's chatter would be disruptive.
    /// </summary>
    /// <param name="context">Current game context</param>
    public void SkipActingThisTurn(IContext context)
    {
        if (context is PlanetfallContext pfc)
            pfc.FloydShouldNotActThisTurn = true;
    }

    public override string[] NounsForMatching => ["floyd", "robot", "B-19-7", "multi-purpose robot"];

    public override string? CannotBeTakenDescription => IsOn
        ? FloydConstants.TakeFloyd
        : null;

    protected override string SystemPrompt => FloydPrompts.SystemPrompt;

    /// <summary>
    /// Gets the examination description for Floyd based on his current state (dead, on, or off).
    /// </summary>
    public string ExaminationDescription =>
        HasDied ? FloydConstants.ExaminationDead :
        IsOn
            ? FloydConstants.ExaminationOn
            : FloydConstants.ExaminationOff;

    /// <summary>
    /// Handles when an item is offered to Floyd. Floyd can accept and hold items.
    /// </summary>
    /// <param name="item">The item being offered to Floyd.</param>
    /// <param name="context">The current game context.</param>
    /// <returns>An InteractionResult indicating whether Floyd accepted the item.</returns>
    public InteractionResult OfferThisThing(IItem item, IContext context)
    {
        // Special handling for Lazarus's breastplate
        if (item is MedicalRobotBreastPlate)
        {
            // Remove breastplate from player's inventory and drop it in current location
            context.RemoveItem(item);
            context.CurrentLocation.ItemPlacedHere(item);

            // Floyd becomes upset and wanders off
            StartWandering(context);

            return new PositiveInteractionResult(FloydConstants.GivenLazarusBreastplate);
        }

        return _inventoryManager.OfferItem(item, context);
    }

    /// <summary>
    /// Handles conversation with Floyd using AI-generated responses based on his personality.
    /// </summary>
    /// <param name="text">The text spoken to Floyd.</param>
    /// <param name="context">The current game context.</param>
    /// <param name="client">The AI generation client for creating responses.</param>
    /// <returns>Floyd's response to the conversation, or an error message if he's off or the AI service fails.</returns>
    public async Task<string> OnBeingTalkedTo(string text, IContext context, IGenerationClient client)
    {
        if (!IsOn)
            return "The robot doesn't respond - it appears to be turned off.";

        try
        {
            CompanionResponse response = await ChatWithFloyd.AskFloydAsync(text);

            // Add the response to Floyd's conversation history for continuity
            LastTurnsOutput.Push(response.Message);

            var specificInteraction = _locationBehaviors.HandleSpecificInteraction(response, context);

            if (!string.IsNullOrEmpty(specificInteraction))
                return specificInteraction;

            return response.Message;
        }
        catch (Exception)
        {
            // Fallback to a generic Floyd response if the service fails
            return "Floyd tilts his head and makes some mechanical whirring sounds, but doesn't seem to understand.";
        }
    }

    private bool IsInTheRoom(IContext context)
    {
        return CurrentLocation == context.CurrentLocation;
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        if (HasDied)
            return "Your former companion, Floyd, is lying on the ground in a pool of oil. ";

        return HasEverBeenOn
            ? "There is a multiple purpose robot here. " + _inventoryManager.DescribeItemBeingHeld(currentLocation)
            : "Only one robot, about four feet high, looks even remotely close to being in working order. ";
    }

    public override void Init()
    {
        StartWithItemInside<LowerElevatorAccessCard>();
    }

    protected override string PreparePrompt(string userPrompt, ILocation? currentLocation)
    {
        // The base class now handles removing the companion's description from the room description
        return userPrompt;
    }

    public override async Task<InteractionResult?> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (action.Match(["turn on", "activate", "start"], NounsForMatching)) return _powerManager.Activate(context);
        if (action.Match(["turn off", "deactivate", "stop"], NounsForMatching)) return _powerManager.Deactivate(context);
        
        if (HasDied)
            return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
        
        if (ItemBeingHeld is not null)
        {
            var result = await ItemBeingHeld.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
            if (result is not null)
                return result;
        }

        var socialResult = _socialResponses.HandleSocialInteraction(action);
        if (socialResult is not null)
            return socialResult;

        // Floyd can be "opened" but in no other way behaves like a typical open/close container. For him,
        // "open" is a synonym for "search"
        if (action.Match(["open", "search"], NounsForMatching)) return _inventoryManager.SearchFloyd(context);

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }

    public override async Task<string> Act(IContext context, IGenerationClient client)
    {
        if (HasDied)
            return string.Empty;

        // Check if Floyd should skip acting this turn
        if (context is PlanetfallContext { FloydShouldNotActThisTurn: true })
            return string.Empty;

        // Check for pending action comment - this takes priority over random behavior
        if (context is PlanetfallContext { PendingFloydActionCommentPrompt: not null } pfContext)
        {
            var prompt = pfContext.PendingFloydActionCommentPrompt;
            pfContext.PendingFloydActionCommentPrompt = null; // Clear after use
            return await GenerateCompanionSpeech(context, client, prompt);
        }

        var countdownResult = _powerManager.HandleTurnOnCountdown(context);
        if (countdownResult != null)
            return countdownResult;

        if (!IsOn)
            return string.Empty;

        // Handle wandering countdown - if Floyd is off wandering, decrement turns and check for return
        var wanderingCountdownResult = await _movementManager.HandleWanderingCountdown(context, client);
        if (wanderingCountdownResult != null)
            return wanderingCountdownResult;

        var followResult = await _movementManager.HandleFollowingPlayer(context, client);
        if (!string.IsNullOrEmpty(followResult))
            return followResult;

        // Spontaneous wandering trigger - 1 in 20 chance per turn
        var spontaneousWanderingResult = await _movementManager.HandleSpontaneousWandering(context, client);
        if (spontaneousWanderingResult != null)
            return spontaneousWanderingResult;

        if (context.CurrentLocation is IFloydDoesNotTalkHere)
            return string.Empty;

        return await PerformRandomAction(context, client);
    }

    private async Task<string> PerformRandomAction(IContext context, IGenerationClient client)
    {
        // 1 in 12 chance of any output (~8.3%)
        if (Chooser.RollDice(12) != 1)
            return string.Empty;

        // Pick which prompt (each has 1/72 chance overall, ~1.4%)
        var promptRoll = Chooser.RollDice(6);
        return promptRoll switch
        {
            1 => await GenerateCompanionSpeech(context, client, FloydPrompts.DoSomethingSmall),
            2 => await GenerateCompanionSpeech(context, client, FloydPrompts.NonSequiturDialog),
            3 => await GenerateCompanionSpeech(context, client, FloydPrompts.NonSequiturReflection),
            4 => await GenerateCompanionSpeech(context, client, FloydPrompts.HappySayAndDoSomething),
            5 => await GenerateCompanionSpeech(context, client, FloydPrompts.MelancholyNonSequitur),
            _ => FloydConstants.RandomActions.GetRandomElement()
        };
    }

    public override async Task<InteractionResult?> RespondToMultiNounInteraction(MultiNounIntent action,
        IContext context)
    {
        if (!IsOn || HasDied)
            return await base.RespondToMultiNounInteraction(action, context);

        var result = _giveHimSomethingEngine.AreWeGivingSomethingToSomeone(action, this, context);

        if (result is not null)
            return result;

        return await base.RespondToMultiNounInteraction(action, context);
    }

    /// <summary>
    /// Everytime Floyd sees you successfully swipe a card, there is a chance he will show you his own.
    /// </summary>
    /// <param name="context">The current game context.</param>
    /// <returns>A message if Floyd offers his lower elevator access card, or null if he doesn't.</returns>
    internal string? OffersLowerElevatorCard(IContext context)
    {
        return _inventoryManager.OfferLowerElevatorCard(context, Chooser);
    }

    /// <summary>
    /// Makes Floyd start wandering for a random number of turns (1-5).
    /// Floyd will be removed from his current location and will return to the player later.
    /// </summary>
    /// <param name="context">The current game context.</param>
    public void StartWandering(IContext context)
    {
        _movementManager.StartWandering(context);
    }

    /// <summary>
    /// Callback invoked when Floyd is taken by the player. Clears the item Floyd is currently holding.
    /// </summary>
    [UsedImplicitly]
    public void BeingTakenCallback()
    {
        ItemBeingHeld = null;
    }
}

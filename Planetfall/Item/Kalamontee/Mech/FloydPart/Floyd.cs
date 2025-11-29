using ChatLambda;
using GameEngine.IntentEngine;
using Model.AIGeneration;
using Newtonsoft.Json;
using Planetfall.Item.Kalamontee.Admin;
using Planetfall.Location;
using Planetfall.Location.Lawanda.Lab;
using Utilities;

namespace Planetfall.Item.Kalamontee.Mech.FloydPart;

public class Floyd : QuirkyCompanion, IAmANamedPerson, ICanHoldItems, ICanBeGivenThings, ICanBeTalkedTo
{
    private readonly GiveSomethingToSomeoneDecisionEngine<Floyd> _giveHimSomethingEngine = new();
    private readonly FloydLocationBehaviors _locationBehaviors;
    private readonly FloydPowerManager _powerManager;
    private readonly FloydInventoryManager _inventoryManager;
    private readonly FloydSocialResponses _socialResponses;

    public Floyd()
    {
        _locationBehaviors = new FloydLocationBehaviors(this);
        _powerManager = new FloydPowerManager(this);
        _inventoryManager = new FloydInventoryManager(this);
        _socialResponses = new FloydSocialResponses(this);
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

        var countdownResult = _powerManager.HandleTurnOnCountdown(context);
        if (countdownResult != null)
            return countdownResult;

        if (!IsOn)
            return string.Empty;

        // Handle wandering countdown - if Floyd is off wandering, decrement turns and check for return
        if (IsOffWandering && WanderingTurnsRemaining > 0)
        {
            WanderingTurnsRemaining--;

            if (WanderingTurnsRemaining == 0)
            {
                // Floyd returns to the player
                IsOffWandering = false;
                CurrentLocation = context.CurrentLocation as ICanContainItems;
                context.CurrentLocation.ItemPlacedHere(this);
                return FloydConstants.ReturnMessages.GetRandomElement();
            }

            return string.Empty; // Still wandering
        }

        var followResult = HandleFollowingPlayer(context);
        if (!string.IsNullOrEmpty(followResult))
            return followResult;

        // Spontaneous wandering trigger - 1 in 20 chance per turn
        if (!IsOffWandering && IsInTheRoom(context) && context.CurrentLocation is not IFloydDoesNotTalkHere)
        {
            BioLockEast bioLockEast = Repository.GetLocation<BioLockEast>();

            // Don't wander if fighting in the bio lab
            if (!bioLockEast.StateMachine.IsFloydInLabFighting && Chooser.RollDice(20) == 1)
            {
                IsOffWandering = true;
                WanderingTurnsRemaining = Chooser.RollDice(5); // 1-5 turns
                (context.CurrentLocation as ICanContainItems)?.RemoveItem(this);
                CurrentLocation = null; // Floyd is not in any location while wandering
                return FloydConstants.GoingExploring;
            }
        }

        if (context.CurrentLocation is IFloydDoesNotTalkHere)
            return string.Empty;

        return await PerformRandomAction(context, client);
    }

    private string HandleFollowingPlayer(IContext context)
    {
        // Check if Floyd is in the Bio Lab fighting - if so, don't follow
        BioLockEast bioLockEast = Repository.GetLocation<BioLockEast>();

        if (bioLockEast.StateMachine.IsFloydInLabFighting)
            return string.Empty; // Floyd is busy fighting in the lab

        // Don't follow if already wandering
        if (IsOffWandering)
            return string.Empty;

        if (!IsInTheRoom(context))
        {
            // Random chance to not follow (1 in 5 chance)
            if (Chooser.RollDice(5) == 1)
            {
                IsOffWandering = true;
                WanderingTurnsRemaining = Chooser.RollDice(5); // 1-5 turns
                CurrentLocation = null; // Floyd is not in any location while wandering
                return string.Empty; // No message - player just doesn't see "Floyd follows you"
            }

            // Normal follow behavior
            context.CurrentLocation.ItemPlacedHere(this);
            return "Floyd follows you. ";
        }

        return string.Empty;
    }

    private async Task<string> PerformRandomAction(IContext context, IGenerationClient client)
    {
        // Check if Floyd is in the Bio Lab fighting - if so, don't perform random actions
        var bioLockEast = Repository.GetLocation<BioLockEast>();

        if (bioLockEast.StateMachine.IsFloydInLabFighting)
            return string.Empty; // Floyd is busy fighting in the lab

        // Randomly, Floyd will say or do something (or possibly nothing) based on one of the
        // prompts below - or he might do one of the things from the original game.
        var action = Chooser.RollDice(15) switch
        {
            <= 3 => (Func<Task<string>>)(async () =>
                await GenerateCompanionSpeech(context, client, FloydPrompts.HappySayAndDoSomething)),
            <= 4 => (Func<Task<string>>)(async () =>
                await GenerateCompanionSpeech(context, client, FloydPrompts.HappyDoSomething)),
            <= 5 => (Func<Task<string>>)(async () =>
                await GenerateCompanionSpeech(context, client, FloydPrompts.HappySayAndDoSomething)),
            <= 6 => (Func<Task<string>>)(async () =>
                await GenerateCompanionSpeech(context, client, FloydPrompts.HappyDoSomething)),
            <= 7 => (Func<Task<string>>)(async () =>
                await GenerateCompanionSpeech(context, client, FloydPrompts.HappySayAndDoSomething)),
            <= 8 => (Func<Task<string>>)(() => Task.FromResult(FloydConstants.RandomActions.GetRandomElement())),

            _ => (Func<Task<string>>)(async () => await Task.FromResult(string.Empty))
        };

        var chosenAction = action.Invoke();
        return await chosenAction;
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
        if (!IsOn || HasDied)
            return; // Can't wander if not on or dead

        IsOffWandering = true;
        WanderingTurnsRemaining = Chooser.RollDice(5); // 1-5 turns

        // Remove Floyd from current location
        (CurrentLocation as ICanContainItems)?.RemoveItem(this);
        CurrentLocation = null; // Floyd is not in any location while wandering
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

// TODO: Floyd gives you a nudge with his foot and giggles. "You sure look silly sleeping on the floor," he says.
// TODO: Floyd bounces impatiently at the foot of the bed. "About time you woke up, you lazy bones! Let's explore around some more!"
// TODO: Floyd says "Floyd going exploring. See you later." He glides out of the room.
// TODO: Floyd rushes into the room and barrels into you. "Oops, sorry," he says. "Floyd not looking at where he was going to."
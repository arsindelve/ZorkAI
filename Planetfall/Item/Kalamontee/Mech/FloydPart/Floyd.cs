using ChatLambda;
using GameEngine.IntentEngine;
using Model.AIGeneration;
using Newtonsoft.Json;
using Planetfall.Item.Kalamontee.Admin;
using Planetfall.Item.Lawanda;
using Planetfall.Location;
using Planetfall.Location.Lawanda;

namespace Planetfall.Item.Kalamontee.Mech.FloydPart;

public class Floyd : QuirkyCompanion, IAmANamedPerson, ICanHoldItems, ICanBeGivenThings, ICanBeTalkedTo, IDoNotAppearInItemLists
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

    // Tracks whether the one-time +2 award for turning Floyd on has already been granted. We can't
    // key this off HasEverBeenOn: that flag only flips when Floyd actually wakes (after a 3-turn
    // countdown), so during the countdown every repeated "activate floyd" would otherwise re-award
    // the points (score 2 -> 4 -> 6). This dedicated guard makes the award strictly once.
    [UsedImplicitly] public bool HasAwardedActivationPoints { get; set; }

    [UsedImplicitly] public bool HasEverGoneThroughTheLittleDoor { get; set; }

    [UsedImplicitly] public bool HasGottenTheFromitzBoard { get; set; }

    // CARD-REVEALED in the original (compone.zil:2039, comptwo/globals). One-time flag, set by EITHER
    // path: the player showing Floyd a lower-elevator card ("I've got one just like that!", the SHOW
    // branch) or Floyd spontaneously revealing his own (FloydInventoryManager.OfferLowerElevatorCard,
    // #222). Shared so the two paths never double-reveal.
    [UsedImplicitly] public bool HasRevealedLowerElevatorCard { get; set; }

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
    /// <returns>True if the comment was queued; false if it was a no-op (Floyd absent/off, already
    /// commenting this turn, or this prompt was already used). Callers that need to know whether the
    /// queue took — e.g. to decide between an empty response and a fallback line — can branch on this.</returns>
    public bool CommentOnAction(string prompt, IContext context)
    {
        // Must be on and in the same location as player
        if (!IsHereAndIsOn(context))
            return false;

        if (context is not PlanetfallContext planetfallContext)
            return false;

        // Only one action comment per turn
        if (planetfallContext.PendingFloydActionCommentPrompt != null)
            return false;

        // Don't repeat prompts that have already been used
        if (planetfallContext.UsedFloydActionCommentPrompts.Contains(prompt))
            return false;

        // Store the prompt for Act() to process and mark as used
        planetfallContext.PendingFloydActionCommentPrompt = prompt;
        planetfallContext.UsedFloydActionCommentPrompts.Add(prompt);
        return true;
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

        // Weighted bucket pick. We lean hardest on the canonical static actions (the original 19 -
        // Floyd's eccentric heart) and keep melancholy a thin slice so he never reads as mopey. Each
        // AI bucket gets a random seed (see FloydPrompts.Seeds) so identical room+context still varies.
        return Chooser.RollDice(100) switch
        {
            <= 35 => Chooser.Choose(FloydConstants.RandomActions.ToList()), // 35% canonical static actions
            <= 53 => await Speak(FloydPrompts.NonSequiturDialog, "non_sequitur_dialog"), // 18%
            <= 68 => await Speak(FloydPrompts.DoSomethingSmall, "do_something_small"), // 15%
            <= 80 => await Speak(FloydPrompts.HappySayAndDoSomething, "happy_say_and_do"), // 12%
            <= 92 => await Speak(FloydPrompts.NonSequiturReflection, "non_sequitur_reflection"), // 12%
            _ => await Speak(FloydPrompts.MelancholyNonSequitur, "melancholy") // 8%
        };

        Task<string> Speak(string prompt, string seedKey) =>
            GenerateCompanionSpeech(context, client, prompt, Chooser.Choose(FloydPrompts.Seeds[seedKey].ToList()));
    }

    public override async Task<InteractionResult?> RespondToMultiNounInteraction(MultiNounIntent action,
        IContext context)
    {
        if (!IsOn || HasDied)
            return await base.RespondToMultiNounInteraction(action, context);

        // SHOW is handled before GIVE: in the original, "show <x> to floyd" drives several reactions
        // (FLOYD-F SHOW branch, compone.zil:2022-2047) that GIVE does not — the printout/computer-concern
        // gate chief among them. SHOW keeps the object in your hand; GIVE transfers it.
        var showResult = RespondToShow(action, context);
        if (showResult is not null)
            return showResult;

        var result = _giveHimSomethingEngine.AreWeGivingSomethingToSomeone(action, this, context);

        if (result is not null)
            return result;

        return await base.RespondToMultiNounInteraction(action, context);
    }

    /// <summary>
    ///     Handles "show &lt;x&gt; to floyd". Ports the reactions from the original FLOYD-F SHOW branch
    ///     (<c>compone.zil:2022-2047</c>) whose objects exist in this port, in the ZIL's evaluation order.
    ///     Returns null when this isn't a show-to-Floyd intent (or the item can't be resolved) so the
    ///     caller falls through to the give engine / base.
    /// </summary>
    private InteractionResult? RespondToShow(MultiNounIntent action, IContext context)
    {
        if (!action.MatchVerb(Verbs.ShowVerbs))
            return null;

        // Floyd can be either noun: "show x to floyd" or "show floyd the x".
        string shownNoun;
        if (action.MatchNounOne(NounsForMatching))
            shownNoun = action.NounTwo;
        else if (action.MatchNounTwo(NounsForMatching))
            shownNoun = action.NounOne;
        else
            return null;

        var shown = Repository.GetItemInScope(shownNoun, context);
        if (shown is null)
            return null;

        // ZIL syntax is SHOW OBJECT (HAVE) ... (syntax.zil:450) — the shown item must be held.
        if (!context.Items.Contains(shown))
            return new PositiveInteractionResult($"You don't have the {shown.NounsForMatching[0]}! ");

        // Branch order mirrors the ZIL exactly; the printout and lower-card branches are one-shot.

        // 1. Printout, first time -> Floyd's "computer is broken" concern. Sets the SAME flag the
        //    Computer-Room visit sets (COMPUTER-FLAG / ComputerRoom.FloydHasExpressedConcern), which
        //    gates the bio-lab card foray. Once concerned, the printout falls through to the default.
        if (shown is ComputerOutput)
        {
            var computerRoom = Repository.GetLocation<ComputerRoom>();
            if (!computerRoom.FloydHasExpressedConcern)
            {
                computerRoom.FloydHasExpressedConcern = true;
                return new PositiveInteractionResult(FloydConstants.ComputerBrokenFromPrintout);
            }
        }

        // 2. The four "blue" cards. Enumerate explicitly: IdCard isn't an AccessCard at all, and the
        //    teleport/mini/lower AccessCards must NOT match here — so `is AccessCard` would be wrong.
        if (shown is IdCard or ShuttleAccessCard or KitchenAccessCard or UpperElevatorAccessCard)
            return new PositiveInteractionResult(FloydConstants.CardsUsuallyBlue);

        // 3. Lower-elevator card, first time -> recognition. Shares the revealed flag with the reveal
        //    daemon (FloydInventoryManager.OfferLowerElevatorCard, #222) so the paths don't double-reveal.
        if (shown is LowerElevatorAccessCard && !HasRevealedLowerElevatorCard)
        {
            HasRevealedLowerElevatorCard = true;
            return new PositiveInteractionResult(FloydConstants.LowerCardJustLikeThat);
        }

        // 4. Default — anything else (and the printout/lower-card once their one-shot flags are set).
        //    The original's fixed "Can you play any games with it?" is replaced by an in-character LLM
        //    reaction. RespondToMultiNounInteraction has no IGenerationClient to generate inline, so we
        //    queue it via the standard CommentOnAction path (Floyd's Act() renders it this turn) and
        //    return an empty body. If the comment can't be queued — Floyd already reacted to this object,
        //    already spoke this turn, or isn't a turn actor right now — fall back to the canned line so
        //    the player never gets a blank response.
        // Only queue when Floyd will actually act this turn (he renders the comment in Act()); the bool
        // result tells us the queue took (vs. one of CommentOnAction's silent no-ops), so we know whether
        // to return the empty body or fall back to the canned line.
        if (context.Actors.Contains(this)
            && CommentOnAction(FloydPrompts.ShownAnObject(shown.NounsForMatching[0]), context))
            return new PositiveInteractionResult("");

        return new PositiveInteractionResult(string.Format(FloydConstants.ShowDefaultFormat,
            shown.NounsForMatching[0]));
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

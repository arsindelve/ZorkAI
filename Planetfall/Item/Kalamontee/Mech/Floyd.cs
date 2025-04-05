using GameEngine.IntentEngine;
using Model.AIGeneration;
using Newtonsoft.Json;
using Planetfall.Item.Kalamontee.Admin;
using Utilities;

namespace Planetfall.Item.Kalamontee.Mech;

public class Floyd : QuirkyCompanion, IAmANamedPerson, ICanHoldItems, ICanBeGivenThings
{
    private readonly GiveSomethingToSomeoneDecisionEngine<Floyd> _giveHimSomethingEngine = new();

    // This is the thing that he is holding, literally in his hand. 
    [UsedImplicitly] public IItem? ItemBeingHeld { get; set; }

    [UsedImplicitly][JsonIgnore] public IRandomChooser Chooser { get; set; } = new RandomChooser();

    [UsedImplicitly] public bool IsOn { get; set; }

    [UsedImplicitly] public bool IsOffWandering { get; set; }

    [UsedImplicitly] public bool HasEverBeenOn { get; set; }

    private bool IsInTheRoom(IContext context)
    {
        return CurrentLocation == context.CurrentLocation;
    }

    // When you initially turn on Floyd, nothing happens for 3 turns. This delay never happens
    // again if you turn him on/off another time. 
    // ReSharper disable once MemberCanBePrivate.Global
    public int TurnOnCountdown { get; set; } = 3;


    // TODO: If you wander away before he wakes: The robot you were fiddling with in the Robot Shop bounds into the room. "Hi!" he says, with a wide and friendly smile. "You turn Floyd on? Be Floyd's friend, yes?"

    public override string[] NounsForMatching => ["floyd", "robot", "B-19-7", "multi-purpose robot"];

    public override string? CannotBeTakenDescription => IsOn
        ? FloydConstants.TakeFloyd
        : null;

    protected override string SystemPrompt => FloydPrompts.SystemPrompt;

    public string ExaminationDescription =>
        IsOn
            ? "From its design, the robot seems to be of the multi-purpose sort. It is slightly cross-eyed, and its " +
              "mechanical mouth forms a lopsided grin. "
            : "The deactivated robot is leaning against the wall, its head lolling to the side. It is short, and seems " +
              "to be equipped for general-purpose work. It has apparently been turned off. ";

    public InteractionResult OfferThisThing(IItem item, IContext context)
    {
        if (ItemBeingHeld != null)
        {
            // It ends up on the floor. 
            context.Drop(item);
            return new PositiveInteractionResult($"Floyd examines the {item.Name}, shrugs, and drops it.");
        }

        item.CurrentLocation = this;
        context.RemoveItem(item);
        ItemBeingHeld = item;
        item.OnBeingTakenCallback = _ => ItemBeingHeld = null;

        return new PositiveInteractionResult(FloydConstants.ThanksYouForGivingItem);
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return HasEverBeenOn
            ? "There is a multiple purpose robot here. " + DescribeItemHeIsHolding(currentLocation)
            : "Only one robot, about four feet high, looks even remotely close to being in working order. ";
    }

    private string DescribeItemHeIsHolding(ILocation? currentLocation)
    {
        if (ItemBeingHeld is null)
            return "";

        return "\nThe multiple purpose robot is holding: \n\t " + ItemBeingHeld.GenericDescription(currentLocation);
    }

    public override void Init()
    {
        StartWithItemInside<LowerElevatorAccessCard>();
    }

    protected override string PreparePrompt(string userPrompt, ILocation? currentLocation)
    {
        // If we don't do this, Floyd will believe, from the location description, that there is another robot in the room
        // when in fact it is him! 
        return userPrompt.Replace(GenericDescription(currentLocation), string.Empty);
    }

    public override async Task<InteractionResult?> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (ItemBeingHeld is not null)
        {
            var result = await ItemBeingHeld.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
            if (result is not null)
                return result;
        }            
            
        if (IsOn && action.Match(["play"], NounsForMatching))
            return new PositiveInteractionResult(FloydConstants.Play);

        if (IsOn && action.Match(["kiss"], NounsForMatching))
            return new PositiveInteractionResult(FloydConstants.Kiss);

        if (IsOn && action.Match(["kick"], NounsForMatching))
            return new PositiveInteractionResult(FloydConstants.Kick);

        if (IsOn && action.Match(Verbs.KillVerbs, NounsForMatching))
            return new PositiveInteractionResult(FloydConstants.Kill);

        // Floyd can be "opened" but in no other way behaves like a typical open/close container. For him,
        // "open" is a synonym for "search"
        if (action.Match(["open", "search"], NounsForMatching)) return SearchHim(context);
        if (action.Match(["turn off", "deactivate", "stop"], NounsForMatching)) return TurnHimOff(context);
        if (action.Match(["turn on", "activate", "start"], NounsForMatching)) return ActivateHim(context);

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }

    private InteractionResult SearchHim(IContext context)
    {
        if (IsOn)
            return new PositiveInteractionResult(FloydConstants.TickleFloyd);

        if (HasItem<LowerElevatorAccessCard>())
        {
            context.ItemPlacedHere<LowerElevatorAccessCard>();
            return new PositiveInteractionResult(FloydConstants.FindAndTakeLowerCard);
        }

        return new PositiveInteractionResult(
            "Your search discovers nothing in the robot's compartments except a single crayon " +
            "which you leave where you found it. ");
    }

    private InteractionResult ActivateHim(IContext context)
    {
        if (IsOn)
            return new PositiveInteractionResult("He's already been activated. ");

        context.RegisterActor(this);

        if (TurnOnCountdown > 0)
            return new PositiveInteractionResult("Nothing happens. ");

        IsOn = true;
        HasEverBeenOn = true;

        return new PositiveInteractionResult(FloydConstants.MadAfterTurnOffAndBackOn);
    }

    private InteractionResult TurnHimOff(IContext context)
    {
        if (!IsOn)
            return new PositiveInteractionResult("The robot doesn't seem to be on. ");

        context.RemoveActor(this);

        IsOn = false;
        return new PositiveInteractionResult(FloydConstants.TurnOffBetrayal);
    }

    public override async Task<string> Act(IContext context, IGenerationClient client)
    {
        if (TurnOnCountdown > 0)
        {
            if (TurnOnCountdown == 1)
            {
                IsOn = true;
                HasEverBeenOn = true;
                TurnOnCountdown = 0;
                return FloydConstants.ComesAlive;
            }

            TurnOnCountdown--;
            return string.Empty;
        }

        if (!IsOn)
            return string.Empty;

        // Should he follow us?
        if (!IsOffWandering && !IsInTheRoom(context))
        {
            context.CurrentLocation.ItemPlacedHere(this);
            return "Floyd follows you. ";
        }

        // Randomly, Floyd will say or do something (or possibly nothing) based on one of the
        // prompts below - or he might do one of the things from the original game. 
        var action = Chooser.RollDice(10) switch
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
        if (!IsOn)
            return await base.RespondToMultiNounInteraction(action, context);

        var result = _giveHimSomethingEngine.AreWeGivingSomethingToSomeone(action, this, context);

        if (result is not null)
            return result;

        return await base.RespondToMultiNounInteraction(action, context);
    }

    /// <summary>
    /// Everytime Floyd sees you successfully swipe a card, there is a chance he will show you his own. 
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    internal string? OffersLowerElevatorCard(IContext context)
    {
        if (!IsInTheRoom(context) || 
            !IsOn || 
            !Items.Any() || 
            !Chooser.RollDiceSuccess(3))
            return null;

        // Remove it from inside, put it in his hand. 
        Items.Clear();
        ItemBeingHeld = Repository.GetItem<LowerElevatorAccessCard>();

        return "\n\nFloyd claps his hands with excitement. \"Those cards are really neat, huh? Floyd has one for " +
               "himself--see?\" He reaches behind one of his panels and retrieves a magnetic-striped card. He waves it exuberantly in the air. ";
    }
}

// TODO: Floyd gives you a nudge with his foot and giggles. "You sure look silly sleeping on the floor," he says.
// TODO: Floyd bounces impatiently at the foot of the bed. "About time you woke up, you lazy bones! Let's explore around some more!"
// TODO: Floyd says "Floyd going exploring. See you later." He glides out of the room.
// TODO: Floyd rushes into the room and barrels into you. "Oops, sorry," he says. "Floyd not looking at where he was going to."
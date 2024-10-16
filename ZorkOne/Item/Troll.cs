using GameEngine.IntentEngine;
using GameEngine.Item;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;
using ZorkOne.ActorInteraction;

namespace ZorkOne.Item;

public class Troll : ContainerBase, ICanBeExamined, ITurnBasedActor, ICanBeAttacked, ICanBeGivenThings
{
    private readonly GiveSomethingToSomeoneDecisionEngine<Troll> _giveHimSomethingEngine = new();
    private readonly TrollCombatEngine _trollAttackEngine = new();

    public bool IsUnconscious { get; set; }

    public bool IsStunned { get; set; }

    public bool IsDead { get; set; }

    public override string[] NounsForMatching => ["troll", "monster"];

    public override string CannotBeTakenDescription => IsUnconscious
        ? ""
        : "The troll spits in your face, grunting \"Better luck next time\" in a rather barbarous accent. ";

    public string ExaminationDescription => IsUnconscious
        ? "An unconscious troll is sprawled on the floor. All passages out of the room are open. "
        : !HasItem<BloodyAxe>()
            ? ""
            : "A nasty-looking troll, brandishing a bloody axe, blocks all passages out of the room. ";

    InteractionResult ICanBeGivenThings.OfferThisThing(IItem item, IContext context)
    {
        return item switch
        {
            BloodyAxe => OfferTheAxe(item, context),
            _ => OfferOtherItem(item, context)
        };
    }

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        if (IsDead || IsUnconscious)
            return Task.FromResult(string.Empty);

        if (!HasItem<BloodyAxe>())
            return
                Task.FromResult(
                    "\nThe troll, disarmed, cowers in terror, pleading for his life in the guttural tongue of the trolls. ");

        return Task.FromResult(_trollAttackEngine.Attack(context));
    }

    private InteractionResult OfferOtherItem(IItem item, IContext context)
    {
        context.Drop(item);
        return new PositiveInteractionResult(
            $"The troll, who is not overly proud, graciously accepts the " +
            $"gift and, being for the moment sated, throws it back. Fortunately, " +
            $"the troll has poor control, and the {item.NounsForMatching[0]} falls " +
            $"to the floor. He does not look pleased. ");
    }

    private InteractionResult OfferTheAxe(IItem item, IContext context)
    {
        item.CurrentLocation = this;
        Items.Add(item);
        context.RemoveItem(item);

        return new PositiveInteractionResult("The troll scratches his head in confusion, then takes the axe. ");
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return ExaminationDescription;
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return ExaminationDescription;
    }

    public override void Init()
    {
        StartWithItemInside<BloodyAxe>();
    }

    public override InteractionResult RespondToMultiNounInteraction(MultiNounIntent action, IContext context)
    {
        var result = _giveHimSomethingEngine.AreWeGivingSomethingToSomeone(action, this, context);

        if (result is not null)
            return result;

        return base.RespondToMultiNounInteraction(action, context);
    }
}

// https://github.com/PDP-10/zork/blob/134871aea9cab2cf91982efad038091d416dddf8/dung.mud#L904
// https://github.com/PDP-10/zork/blob/134871aea9cab2cf91982efad038091d416dddf8/dung.mud#L1119
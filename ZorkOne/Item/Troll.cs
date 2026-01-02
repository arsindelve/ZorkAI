using GameEngine;
using GameEngine.IntentEngine;
using GameEngine.Item;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;
using ZorkOne.ActorInteraction;

namespace ZorkOne.Item;

public class Troll : ContainerBase, ICanBeExamined, ITurnBasedActor, ICanBeAttacked, ICanBeGivenThings, ICanHoldItems
{
    private readonly GiveSomethingToSomeoneDecisionEngine<Troll> _giveHimSomethingEngine = new();

    internal TrollCombatEngine TrollAttackEngine { get; set; } = new();

    public bool IsUnconscious { get; set; }

    public bool IsStunned { get; set; }

    public bool IsDead { get; set; }

    public IItem? ItemBeingHeld { get; set; }

    public override string[] NounsForMatching => ["troll", "monster", "creature"];

    public override string CannotBeTakenDescription => IsUnconscious
        ? ""
        : "The troll spits in your face, grunting \"Better luck next time\" in a rather barbarous accent. ";

    public string ExaminationDescription => IsUnconscious && !IsDead
        ? "An unconscious troll is sprawled on the floor. All passages out of the room are open. "
        : ItemBeingHeld is not BloodyAxe
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

        if (ItemBeingHeld is not BloodyAxe)
            return
                Task.FromResult(
                    "\nThe troll, disarmed, cowers in terror, pleading for his life in the guttural tongue of the trolls. ");

        if (IsStunned)
        {
            IsStunned = false;
            return Task.FromResult("The troll slowly regains his feet. ");
        }

        return Task.FromResult(TrollAttackEngine.Attack(context));
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
        ItemBeingHeld = item;
        item.CurrentLocation = this;
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
        var axe = Repository.GetItem<BloodyAxe>();
        ItemBeingHeld = axe;
        axe.CurrentLocation = this;
    }

    public override async Task<InteractionResult?> RespondToMultiNounInteraction(MultiNounIntent action,
        IContext context)
    {
        var result = _giveHimSomethingEngine.AreWeGivingSomethingToSomeone(action, this, context);

        if (result is not null)
            return result;

        return await base.RespondToMultiNounInteraction(action, context);
    }
}

// https://github.com/PDP-10/zork/blob/134871aea9cab2cf91982efad038091d416dddf8/dung.mud#L904
// https://github.com/PDP-10/zork/blob/134871aea9cab2cf91982efad038091d416dddf8/dung.mud#L1119
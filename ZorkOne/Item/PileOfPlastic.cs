using Model.AIGeneration;
using Model.Intent;
using Model.Interface;

namespace ZorkOne.Item;

public class PileOfPlastic : ContainerBase, ICanBeTakenAndDropped, ICanHoldItems
{
    private List<IItem> _sharpItems =
    [
        Repository.GetItem<Sword>(),
        Repository.GetItem<Knife>(),
        Repository.GetItem<Trident>(),
        Repository.GetItem<Sceptre>()
    ];

    public bool IsInflated { get; set; }

    public override string[] NounsForMatching => ["pile", "pile of plastic", "plastic", "boat", "raft"];

    public override string InInventoryDescription => IsInflated ? "A magic boat here. " : "A pile of plastic";

    public override int Size => 14;

    public string OnTheGroundDescription => IsInflated
        ? "There is a magic boat here. " + (Items.Any() ? ItemListDescription("magic boat") : "")
        : "There is a folded pile of plastic here which has a small valve attached. ";

    public override string NeverPickedUpDescription => OnTheGroundDescription;

    public override bool IsTransparent => true;

    public override void Init()
    {
        // Starts empty 
    }

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client)
    {
        if (action.Match(["inflate", "blow up", "blow"], NounsForMatching))
        {
            if (context.HasItem<PileOfPlastic>())
                return new PositiveInteractionResult("The boat must be on the ground to be inflated. ");

            return new PositiveInteractionResult("You don't have enough lung power to inflate it. ");
        }

        if (action.Match(["deflate"], NounsForMatching))
        {
            if (context.HasItem<PileOfPlastic>())
                return new PositiveInteractionResult("The boat must be on the ground to be deflated. ");

            IsInflated = false;
            return new PositiveInteractionResult("The boat deflates. ");
        }

        return base.RespondToSimpleInteraction(action, context, client);
    }

    public override InteractionResult RespondToMultiNounInteraction(MultiNounIntent action, IContext context)
    {
        string[] verbs = ["inflate", "blow", "blow up", "use"];
        string[] prepositions = ["with", "to", "on", "using"];

        if (!action.MatchNounOne(NounsForMatching))
            return base.RespondToMultiNounInteraction(action, context);

        if (context.HasItem<PileOfPlastic>())
            return new PositiveInteractionResult("The boat must be on the ground to be inflated. ");

        if (!action.MatchNounTwo(Repository.GetItem<AirPump>().NounsForMatching))
            return base.RespondToMultiNounInteraction(action, context);

        if (!verbs.Contains(action.Verb.ToLowerInvariant().Trim()))
            return base.RespondToMultiNounInteraction(action, context);

        if (!prepositions.Contains(action.Preposition.ToLowerInvariant().Trim()))
            return base.RespondToMultiNounInteraction(action, context);

        if (!context.HasItem<AirPump>())
            return new PositiveInteractionResult("You don't have the air pump.");

        if (IsInflated)
            return new PositiveInteractionResult("Inflating it further would probably burst it. ");

        IsInflated = true;
        ItemPlacedHere(Repository.GetItem<Label>());
        return new PositiveInteractionResult(
            "The boat inflates and appears seaworthy. " + (Repository.GetItem<Label>().CurrentLocation == this
                ? "\nA tan label is lying inside the boat. "
                : ""));
    }
}
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;

namespace ZorkOne.Item;

public class PileOfPlastic : ItemBase, ICanBeTakenAndDropped, ISubLocation
{
    public bool IsInflated { get; set; }

    public override string[] NounsForMatching => ["pile", "pile of plastic", "plastic", "boat", "raft"];

    public override string InInventoryDescription => "A pile of plastic";

    public override int Size => 14;

    public string OnTheGroundDescription => "There is a folded pile of plastic here which has a small valve attached. ";

    public override string NeverPickedUpDescription => OnTheGroundDescription;

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client)
    {
        if (action.Match(["inflate", "blow up", "blow"], NounsForMatching))
        {
            if (context.HasItem<PileOfPlastic>())
                return new PositiveInteractionResult("The boat must be on the ground to be inflated. ");

            return new PositiveInteractionResult("You don't have enough lung power to inflate it. ");
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
        return new PositiveInteractionResult(
            "The boat inflates and appears seaworthy. A tan label is lying inside the boat. ");
    }

    public string GetIn(IContext context)
    {
        // TODO: Sharp items
        context.CurrentLocation.SubLocation = this;
        return "You are now in the magic boat. ";
    }

    public string GetOut(IContext context)
    {
        // TODO: You realize that getting out here would be fatal.
        context.CurrentLocation.SubLocation = null;
        return "You are on your own feet again. ";
    }

    public string LocationDescription => ", in the magic boat";
}
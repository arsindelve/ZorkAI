using Model.AIGeneration;
using Model.Intent;

namespace ZorkOne.Item;

public class PileOfLeaves : ItemBase, ICanBeTakenAndDropped, ICanBeExamined
{
    public override string[] NounsForMatching => ["leaves", "pile", "pile of leaves"];

    public override string InInventoryDescription => "A pile of leaves";

    public string ExaminationDescription => "There's nothing special about the pile of leaves. ";

    public string OnTheGroundDescription => "On the ground is a pile of leaves. ";

    public override string NeverPickedUpDescription => OnTheGroundDescription;

    public override string OnBeingTaken(IContext context)
    {
        Repository.GetLocation<Clearing>().ItemPlacedHere(Repository.GetItem<Grating>());
        return "In disturbing the pile of leaves, a grating is revealed. ";
    }

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client)
    {
        if (action.Verb.ToLowerInvariant().Trim() == "count")
            if (action.Noun?.ToLowerInvariant().Trim() == "leaves")
                return new PositiveInteractionResult("There are 69,105 leaves here. ");

        if (action.Verb.ToLowerInvariant().Trim() == "move")
            if (action.Noun?.ToLowerInvariant().Trim() == "leaves")
            {
                HasEverBeenPickedUp = true;
                Repository.GetLocation<Clearing>().ItemPlacedHere(Repository.GetItem<Grating>());
                return new PositiveInteractionResult("In disturbing the pile of leaves, a grating is revealed. ");
            }

        return base.RespondToSimpleInteraction(action, context, client);
    }
}
using Model.AIGeneration;
using Model.Intent;
using ZorkOne.Location;

namespace ZorkOne.Item;

public class BrassBell : ItemBase, ICanBeExamined, ICanBeTakenAndDropped, ITurnBasedActor
{
    public bool BellIsRedHot { get; set; }

    public int BellHotCounter { get; set; }

    public override string[] NounsForMatching => ["bell", "brass bell"];

    public override string InInventoryDescription => "A brass bell";

    public override string? CannotBeTakenDescription =>
        BellIsRedHot ? "The bell is very hot and cannot be taken. " : null;

    public string ExaminationDescription => "There is nothing special about the " + (BellIsRedHot ? "red hot " : "") + "brass bell.";

    public string OnTheGroundDescription =>
        BellIsRedHot ? "On the ground is a red hot brass bell." : "There is a brass bell here.";

    public override string NeverPickedUpDescription => OnTheGroundDescription;

    public string Act(IContext context, IGenerationClient client)
    {
        if (!BellIsRedHot)
        {
            context.RemoveActor(this);
            return "";
        }

        BellHotCounter--;

        if (BellHotCounter == 0)
        {
            BellIsRedHot = false;
            if (context.CurrentLocation == Repository.GetLocation<EntranceToHades>())
                return "The bell appears to have cooled down.";
        }

        return "";
    }

    public string BecomesRedHot(IContext context)
    {
        context.Drop(this);
        context.RegisterActor(this);
        BellIsRedHot = true;
        BellHotCounter = 7;
        return "The bell suddenly becomes red hot and falls to the ground. ";
    }

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client)
    {
        if (action.Verb.ToLowerInvariant() != "ring")
            return base.RespondToSimpleInteraction(action, context, client);

        if (!action.MatchNoun(NounsForMatching))
            return base.RespondToSimpleInteraction(action, context, client);

        return new PositiveInteractionResult("Ding, dong");
    }
}
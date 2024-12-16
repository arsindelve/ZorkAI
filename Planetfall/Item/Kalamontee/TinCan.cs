using Model.AIGeneration;

namespace Planetfall.Item.Kalamontee;

public class TinCan : ItemBase, ICanBeTakenAndDropped, ICanBeExamined
{
    public override string[] NounsForMatching => ["can", "tin can", "Spam and Egz"];

    public string ExaminationDescription =>
        "This is a rather normal tin can. It is large and is labelled \"Spam and Egz.\"";

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a large tin can, labelled \"Spam and Egz,\" sitting here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return
            "On a small shelf is a large, unopened tin can. It has a plain white label which reads \"Spam and Egz.\"";
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A tin can";
    }

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client)
    {
        if (action.Match(["open"], NounsForMatching))
            return new PositiveInteractionResult(
                "You certainly can't open it with your hands, and you don't seem to have found a can opener yet. ");

        return base.RespondToSimpleInteraction(action, context, client);
    }
}
using Model.AIGeneration;
using Planetfall.Location.Kalamontee.Admin;

namespace Planetfall.Item.Kalamontee;

public class Ladder : ItemBase, ICanBeTakenAndDropped, ICanBeExamined
{
    [UsedImplicitly]
    public bool IsAcrossRift { get; set; }

    [UsedImplicitly]
    public bool IsExtended { get; set; }

    public override string[] NounsForMatching => ["ladder"];

    public override string? CannotBeTakenDescription =>
        IsExtended ? "You can't possibly carry the ladder while it's extended. " : null;

    public string ExaminationDescription =>
        "It is a heavy-duty ladder built of sturdy aluminum tubing. " + (IsExtended
            ? "It is currently collapsed and is around " +
              "two-and-a-half meters long, but if extended would obviously be much longer. "
            : "It is currently extended to its full length of about 8 meters, but could be collapsed to a shorter length for easier carrying. ");

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return IsAcrossRift ? "" : "There is a large aluminum ladder here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return "A heavy-duty extendable ladder is leaning against the rear wall. ";
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A ladder";
    }

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client)
    {
        if (action.Match(["extend", "expand", "deploy", "pull out", "stretch", "open", "lengthen"], NounsForMatching))
            return ExtendTheLadder(context);

        if (action.Match(["retract", "collapse", "shorten", "close", "contract"], NounsForMatching))
            return CollapseTheLadder();

        return base.RespondToSimpleInteraction(action, context, client);
    }

    private InteractionResult CollapseTheLadder()
    {
        if (!IsExtended)
            return new PositiveInteractionResult("The ladder is already in its collapsed state. ");

        if (IsAcrossRift)
        {
            IsAcrossRift = false;
            Repository.GetLocation<AdminCorridor>().RemoveItem(this);
            Repository.GetLocation<AdminCorridorNorth>().RemoveItem(this);
            CurrentLocation = null;
            return new PositiveInteractionResult("As the ladder shortens it plunges into the rift. ");
        }

        IsExtended = false;
        return new PositiveInteractionResult("The ladder collapses to a length of around two-and-a-half meters. ");
    }

    private InteractionResult ExtendTheLadder(IContext context)
    {
        if (IsExtended)
            return new PositiveInteractionResult("The ladder is already extended. ");

        if (context.HasItem<Ladder>())
            return new PositiveInteractionResult(
                "You couldn't possibly extend the ladder while you're holding it. ");

        IsExtended = true;
        return new PositiveInteractionResult("The ladder extends to a length of around eight meters. ");
    }
}
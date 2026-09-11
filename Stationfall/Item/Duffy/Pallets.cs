using Model.AIGeneration;

namespace Stationfall.Item.Duffy;

/// <summary>
///     The endless pallets of boxed forms filling the Forms Storage Room (ship.zil:178-186). They
///     cannot be taken, opened to any profit, or counted — they exist to establish that the Patrol's
///     true business is paperwork.
/// </summary>
public class Pallets : ItemBase, ICanBeExamined, ICanBeRead
{
    public override string[] NounsForMatching => ["pallets", "pallet", "boxes", "box", "pallets of boxes"];

    // Far too heavy to be carried; the original tells you to fetch a forklift.
    public override int Size => 500;

    public string ExaminationDescription =>
        "Row upon row of pallets, stacked to the ceiling with identical boxes. Each box has something " +
        "stamped on the side. ";

    public string ReadDescription =>
        "The nearer boxes are labelled with the names of forms: a form for disbursing form pallets, a " +
        "form for reporting the loss of a form pallet label, and a form for releasing that report. ";

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return string.Empty;
    }

    public override async Task<InteractionResult?> RespondToSimpleInteraction(SimpleIntent action,
        IContext context, IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (!action.MatchNounAndAdjective(NounsForMatching))
            return new NoNounMatchInteractionResult();

        // Opening a box is a joke with a long windup, and the length is the joke (ship.zil PALLETS-F).
        if (action.MatchVerb(["open", "look in", "look inside", "search", "empty", "unseal"]))
            return new PositiveInteractionResult(
                "You work a box open. Inside are forms" + string.Concat(Enumerable.Repeat(" and forms", 50)) +
                ". Horrified, you reseal the box. ");

        if (action.MatchVerb(["close", "shut", "seal"]))
            return new PositiveInteractionResult("The boxes are already sealed. ");

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "Pallets of boxes";
    }
}

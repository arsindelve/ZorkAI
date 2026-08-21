using Model.AIGeneration;

namespace Stationfall.Item.Duffy;

/// <summary>
///     A plaid Thermos bottle from the survival kit (ship.zil:1280-1286), holding a serving of soup.
///     Its real importance is far downstream: sealed inside it, the station's FREZONE explosive
///     sublimes four times more slowly (interrupts.zil:361).
/// </summary>
public class Thermos : OpenAndCloseContainerBase, ICanBeTakenAndDropped, ICanBeExamined
{
    public override string[] NounsForMatching => ["thermos", "bottle", "thermos bottle"];

    public override int Size => 4;

    protected override int SpaceForItems => 4;

    public string ExaminationDescription =>
        "A battered insulated bottle, plaid, with little cartoon robots printed all over it. " +
        (IsOpen ? ItemListDescription("Thermos", null) : "It is closed. ");

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "A plaid Thermos bottle is here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return OnTheGroundDescription(currentLocation);
    }

    public override async Task<InteractionResult?> RespondToSimpleInteraction(SimpleIntent action,
        IContext context, IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        // NB: no early return when the noun isn't ours. ContainerBase's implementation offers the
        // action to the items INSIDE this one before considering itself, so returning NoNounMatch here
        // would cut the soup off from every verb it has - it would simply stop existing to the parser.
        if (action.MatchNounAndAdjective(NounsForMatching))
        {
            var soup = Repository.GetItem<BlueSoup>();

            // Emptying the Thermos means emptying what is in it, which is worth doing deliberately:
            // the soup is gone for good, and the bottle matters later.
            if (action.MatchVerb(["pour", "empty", "pour out", "dump"]) && Items.Contains(soup))
            {
                RemoveItem(soup);
                soup.CurrentLocation = null;

                return new PositiveInteractionResult(
                    "You pour the soup out. It spreads across the deck in a wide blue disc, and the " +
                    "smell of blueberries fills the place. ");
            }

            if (action.MatchVerb(["look in", "look inside", "reach in", "reach into"]))
                return new PositiveInteractionResult(
                    Items.Contains(soup) ? soup.ExaminationDescription : "The Thermos is empty. ");
        }

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }

    /// <summary>
    ///     Nothing goes into the Thermos. Far downstream the bottle matters for what it can be made to
    ///     hold, so the refusal is worded to leave that possibility open rather than to close the idea
    ///     off entirely.
    /// </summary>
    public override async Task<InteractionResult?> RespondToMultiNounInteraction(MultiNounIntent action,
        IContext context)
    {
        var isPuttingSomethingIn =
            action.MatchVerb(["put", "place", "insert", "drop"]) &&
            action.MatchNounTwo(NounsForMatching) &&
            action.Preposition is "in" or "into" or "inside";

        if (isPuttingSomethingIn)
            return new PositiveInteractionResult(
                $"The neck of the Thermos is too narrow for the {action.NounOne}. ");

        return await base.RespondToMultiNounInteraction(action, context);
    }

    public override void Init()
    {
        ItemPlacedHere<BlueSoup>();
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A Thermos bottle";
    }
}

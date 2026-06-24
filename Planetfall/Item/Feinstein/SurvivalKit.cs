using Model.AIGeneration;
using Utilities;

namespace Planetfall.Item.Feinstein;

public class SurvivalKit : OpenAndCloseContainerBase, ICanBeTakenAndDropped, ICanBeExamined
{
    public override string[] NounsForMatching => ["survival kit", "kit", "survival"];

    public string ExaminationDescription => IsOpen
        ? ItemListDescription("survival kit", null)
        : "The survival kit is closed. ";

    public override string NowOpen(ILocation currentLocation)
    {
        if (!Items.Any())
            return "Opened. ";

        var gooDescriptions = Items.Select(item => "blob of " + item.NounsForMatching[0]).ToList();
        return $"Opening the survival kit reveals {gooDescriptions.SingleLineListWithAnd()}. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return OnTheGroundDescription(currentLocation);
    }

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a survival kit here. ";
    }

    public override void Init()
    {
        ItemPlacedHere<RedGoo>();
        ItemPlacedHere<BrownGoo>();
        ItemPlacedHere<GreenGoo>();
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return !IsOpen ? "A survival kit" : $"A survival kit\n{ItemListDescription("survival kit", null)}";
    }

    public override async Task<InteractionResult?> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        // The original FOOD-KIT-F routine gives state-specific flavor for EMPTY, and it is a
        // pure misdirect - EMPTY never removes the goo. Closed -> "The kit is closed!"; open with
        // goo -> the goo-sticks message; open and empty -> the default V-EMPTY message.
        // See planetfall-source/globals.zil:1022-1029 and verbs.zil:1718.
        if (action.Match(["empty"], NounsForMatching))
        {
            if (!IsOpen)
                return new PositiveInteractionResult("The kit is closed! ");

            return new PositiveInteractionResult(Items.Any()
                ? "The goo, being gooey, sticks to the inside of the kit. You would probably " +
                  "have to shake the kit to get the goo out. "
                : "There's nothing in the survival kit. ");
        }

        // V-SHAKE special-cases the food kit: if any goo is inside, shaking flings it out and
        // destroys whatever goo it holds - the player's only food source. The original checks only
        // whether the goo is IN the kit (not OPENBIT), so a closed kit is emptied just the same.
        // With no goo left, fall through to the default shake handling. See
        // planetfall-source/verbs.zil:1167.
        if (action.Match(["shake"], NounsForMatching))
        {
            var goo = Items.OfType<GooBase>().ToList();
            if (goo.Count > 0)
            {
                foreach (var blob in goo)
                    RemoveItem(blob);

                return new PositiveInteractionResult("Colored goo flies all over everything. Yechh! ");
            }
        }

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }
}

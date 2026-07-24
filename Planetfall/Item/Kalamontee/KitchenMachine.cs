using Model.AIGeneration;

namespace Planetfall.Item.Kalamontee;

internal class KitchenMachine : ContainerBase, ICanBeExamined
{
    public override string[] NounsForMatching => ["dispenser unit", "kitchen machine", "machine", "niche", "dispenser"];

    public override Type[] CanOnlyHoldTheseTypes => [typeof(Canteen)];

    public override bool IsTransparent => true;

    public override string CanOnlyHoldTheseTypesErrorMessage(string nameOfItemWeTriedToPlaceHere) => "It doesn't fit in the niche. ";

    public string ExaminationDescription =>
        "This wall-mounted unit contains an octagonal niche beneath a spout. Above the spout is a button. " +
        "The machine is labelled\n\"Hii Prooteen Likwid Dispensur.\" ";

    public override string GenericDescription(ILocation? currentLocation)
    {
        return Items.Any() ? base.ItemListDescription("dispenser unit", currentLocation) : "";
    }

    public override void Init()
    {
    }

    public override async Task<InteractionResult?> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (action.Match(Verbs.PushVerbs, ["button"]))
        {
            var canteen = Repository.GetItem<Canteen>();

            // Niche is empty.
            if (!Items.Any())
                return new PositiveInteractionResult("A thick, brownish liquid pours from the spout and " +
                                                     "splashes to the floor, where it quickly evaporates. ");

            if (canteen.IsOpen)
            {
                if (canteen.Items.Any())
                {
                    var uniform = Repository.GetItem<PatrolUniform>();
                    var isOn = uniform is { CurrentLocation: IContext, BeingWorn: true };
                    return new PositiveInteractionResult(
                        $"The brown liquid splashes over the mouth of the already-filled canteen, creating a mess{(isOn ? " and staining your uniform" : "")}. ");
                }

                canteen.ItemPlacedHere(Repository.GetItem<ProteinLiquid>());
                return new PositiveInteractionResult("The canteen fills almost to the brim with a brown liquid. ");
            }

            return new PositiveInteractionResult("A thick, brown liquid spills over the closed canteen, dribbles " +
                                                 "down the side of the machine, and forms a puddle on the floor which quickly dries up. ");
        }

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }

    public override async Task<InteractionResult?> RespondToMultiNounInteraction(MultiNounIntent action,
        IContext context)
    {
        // Issue #424: the machine describes its niche as "beneath a spout" and the sibling Machine Shop
        // dispenser accepts "put flask under spout", but here "spout" was never a matching noun and
        // PutProcessor only understands in/into/onto - never "under" - so "put canteen under spout"
        // silently no-oped to the narrator. Route the "under spout" phrasing to the same niche placement
        // as "put canteen in niche", applying the same guards (in that order) PutProcessor applies there.
        string[] verbs = ["put", "place", "move", "shove", "jam", "push"];
        string[] receiverNouns = [.. NounsForMatching, "spout"];

        if (action.MatchVerb(verbs) && action.MatchNounTwo(receiverNouns) &&
            action.MatchPreposition(["under", "underneath"]))
        {
            // The parser hands us this phrasing even when nothing by that name is in scope. Fall through
            // to the narrator rather than asserting the player doesn't have it - this mirrors the Machine
            // Shop's NoNounMatch fall-through and the in-niche path's scope-aware missing-noun response.
            var itemToPlace = Repository.GetItemInScope(action.NounOne, context);
            if (itemToPlace is null)
                return await base.RespondToMultiNounInteraction(action, context);

            // You can't place what you aren't holding - the same guard PutProcessor applies to "in niche".
            // A canteen already resting in the niche has its CurrentLocation set to this machine (not the
            // player), so this also blocks re-placing it - matching the in-niche path exactly.
            if (itemToPlace.CurrentLocation is not IContext)
                return new PositiveInteractionResult($"You don't have the {itemToPlace.NounsForMatching.First()}. ");

            // Only a canteen fits; give a wrong-type item the same refusal as the in-niche path.
            if (!CanOnlyHoldTheseTypes.Any(type => type.IsInstanceOfType(itemToPlace)))
                return new PositiveInteractionResult(CanOnlyHoldTheseTypesErrorMessage(itemToPlace.Name));

            ItemPlacedHere(itemToPlace);
            return new PositiveInteractionResult(ItemPlacedHereResult(itemToPlace, context));
        }

        return await base.RespondToMultiNounInteraction(action, context);
    }

    public override string ItemPlacedHereResult(IItem item, IContext context)
    {
        return
            "The canteen fits snugly into the octagonal niche, its mouth resting just below the spout of the machine. ";
    }
}
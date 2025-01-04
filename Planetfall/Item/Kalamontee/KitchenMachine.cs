using Model.AIGeneration;

namespace Planetfall.Item.Kalamontee;

internal class KitchenMachine : ContainerBase, ICanBeExamined
{
    public override string[] NounsForMatching => ["dispenser unit", "kitchen machine", "machine", "niche", "dispenser"];

    public override Type[] CanOnlyHoldTheseTypes => [typeof(Canteen)];

    public override bool IsTransparent => true;

    public override string CanOnlyHoldTheseTypesErrorMessage => "It doesn't fit in the niche. ";

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

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client)
    {
        if (action.Match(["push", "press"], ["button"]))
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
                    var uniform = Repository.GetItem<Uniform>();
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


        return base.RespondToSimpleInteraction(action, context, client);
    }

    // TODO: put canteen under spout. 

    public override string ItemPlacedHereResult(IItem item, IContext context)
    {
        return
            "The canteen fits snugly into the octagonal niche, its mouth resting just below the spout of the machine. ";
    }
}
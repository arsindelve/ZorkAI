using GameEngine.Location;
using Planetfall.Command;
using Planetfall.Item.Lawanda.LabOffice;

namespace Planetfall.Location.Lawanda.LabOffice;

internal class LabOffice : LocationBase
{
    public override string Name => "Lab Office";

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is the office for storing files on Bio Lab experiments. A large and messy desk is surrounded by " +
            "locked files. A small booth lies to the south. A closed door to the west is labelled \"Biioo Lab.\" " +
            "You realize with shock and horror that the only way out is through the mutant-infested Bio Lab. " +
            "\nOn the wall are three buttons: a white button labelled \"Lab Liits On\", a black button labelled " +
            "\"Lab Liits Of\", and a red button labelled \"Eemurjensee Sistum.\" ";
    }

    public override void Init()
    {
        StartWithItem<LabDesk>();
        StartWithItem<Memo>();
        StartWithItem<WhiteButton>();
        StartWithItem<BlackButton>();
        StartWithItem<RedButton>();
        StartWithItem<OfficeDoor>();
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.E, Go<AuxiliaryBooth>() },
            {
                Direction.W,
                new MovementParameters
                {
                    CanGo = _ => Repository.GetItem<OfficeDoor>().IsOpen,
                    CustomFailureMessage = "The office door is closed. ",
                    Location = GetLocation<BioLabLocation>()
                }
            }
        };
    }

    public override string BeforeEnterLocation(IContext context, ILocation previousLocation)
    {
        // Death check: entering without gas mask while fungicide is active
        var fungicideTimer = Repository.GetItem<FungicideTimer>();
        var gasMask = Repository.GetItem<GasMask>();

        if (previousLocation is BioLabLocation && fungicideTimer.IsActive &&
            (!context.Items.Contains(gasMask) || !gasMask.BeingWorn))
        {
            return new DeathProcessor().Process(
                "As you enter the office from the Bio Lab, you inhale the toxic fungicide mist. " +
                "Your lungs burn and you collapse, unable to breathe. ",
                context).InteractionMessage;
        }

        return base.BeforeEnterLocation(context, previousLocation);
    }
}

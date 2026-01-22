using GameEngine.Location;
using Planetfall.Command;
using Planetfall.Item.Lawanda.LabOffice;

namespace Planetfall.Location.Lawanda.LabOffice;

internal class LabOfficeLocation : LocationBase
{
    public override string Name => "Lab Office";

    protected override string GetContextBasedDescription(IContext context)
    {
        var door = Repository.GetItem<OfficeDoor>();
        var deskDescription = Repository.GetItem<LabDesk>().NeverPickedUpDescription(this);

        return
            "This small office appears to be a control room for the Bio Lab. " +
            $"On the wall are three buttons labeled LIGHT, DARK, and FUNGICIDE. {deskDescription}" +
            $"A door leads west{(door.IsOpen ? " (open)" : " (closed)")}. The exit is east. ";
    }

    public override void Init()
    {
        StartWithItem<LabDesk>();
        StartWithItem<Memo>();
        StartWithItem<LightButton>();
        StartWithItem<DarkButton>();
        StartWithItem<FungicideButton>();
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

    public override string? BeforeEnterLocation(IContext context, ILocation previousLocation)
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

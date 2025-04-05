namespace Planetfall.Item.Kalamontee.Admin;

internal class UpperElevatorAccessSlot : SlotBase<UpperElevatorAccessCard, UpperElevatorAccessSlot>
{
    public override string[] NounsForMatching => ["upper elevator slot", "upper elevator access slot", "slot", "upper elevator card slot", "upper slot"];
    
    protected override InteractionResult OnSuccessfulSlide(IContext context, string? afterMessage)
    {
        var elevator = Repository.GetLocation<UpperElevator>();
        elevator.IsEnabled = true;
        context.RegisterActor(elevator);
        return new PositiveInteractionResult("A recorded voice chimes \"Elevator enabled.\"" + afterMessage);
    }
}
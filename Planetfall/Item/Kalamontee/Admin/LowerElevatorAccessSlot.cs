namespace Planetfall.Item.Kalamontee.Admin;

internal class LowerElevatorAccessSlot : SlotBase<LowerElevatorAccessCard, LowerElevatorAccessSlot>
{
    public override string[] NounsForMatching => ["lower elevator slot", "lower elevator access slot", "slot", "lower elevator card slot", "lower slot"];
    
    protected override InteractionResult OnSuccessfulSlide(IContext context, string? afterMessage)
    {
        var elevator = Repository.GetLocation<LowerElevator>();
        elevator.IsEnabled = true;
        elevator.TurnsSinceEnabled = 0;
        context.RegisterActor(elevator);
        return new PositiveInteractionResult("A recorded voice chimes \"Elevator enabled.\"");
    }
}
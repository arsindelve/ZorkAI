namespace Planetfall.Item.Kalamontee.Admin;

internal class KitchenSlot : SlotBase<KitchenAccessCard, KitchenSlot>
{
    public override string[] NounsForMatching => ["kitchen slot", "slot", "kitchen card slot"];

    protected override InteractionResult OnSuccessfulSlide(IContext context, string? afterMessage)
    {
        var door = Repository.GetItem<KitchenDoor>();
        door.IsOpen = true;
        context.RegisterActor(door);
        return new PositiveInteractionResult(door.NowOpen(context.CurrentLocation) + afterMessage);
    }
}
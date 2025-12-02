using Planetfall.Item.Kalamontee.Admin;
using Planetfall.Item.Lawanda.Lab;
using Planetfall.Location.Lawanda;

namespace Planetfall.Item.Lawanda;

internal class MiniaturizationSlot : SlotBase<MiniaturizationAccessCard, MiniaturizationSlot>
{
    public override string[] NounsForMatching { get; }
        = ["miniaturization slot", "slot", "miniaturization card slot"];

    protected override InteractionResult OnSuccessfulSlide(IContext context, string? afterMessage)
    {
        Repository.GetLocation<MiniaturizationBooth>().IsEnabled = true;
        return new PositiveInteractionResult(
            "A melodic high-pitched voice says \"Miniaturization and teleportation booth activated. Please type in damaged sector number.\"" + afterMessage);
    }
}

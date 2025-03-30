using Planetfall.Item.Kalamontee.Admin;
using Planetfall.Location;

namespace Planetfall.Item;

/// <summary>
/// Represents a slot that enables teleportation functionality when a valid teleportation access card is slid through it.
/// The slot is specifically designed to enable locations of type <typeparamref name="TBooth"/>.
/// </summary>
/// <typeparam name="TBooth">
/// The type of the booth location that this teleportation slot interacts with.
/// It must inherit from <see cref="BoothBase"/> and implement <see cref="ILocation"/>.
/// </typeparam>
/// <remarks>
/// The slot activates the specified booth when a successful interaction with a teleportation access card occurs.
/// </remarks>
internal class TeleportationSlot<TBooth> : SlotBase<TeleportationAccessCard, TeleportationSlot<TBooth>>
    where TBooth : BoothBase, ILocation, new()
{
    public override string[] NounsForMatching { get; }
    = ["teleportation slot", "slot", "teleportation card slot"];
    
    protected override InteractionResult OnSuccessfulSlide(IContext context)
    {
        Repository.GetLocation<TBooth>().IsEnabled = true;
        return new PositiveInteractionResult("Nothing happens for a moment. Then a light flashes \"Redee.\"");
    }
}
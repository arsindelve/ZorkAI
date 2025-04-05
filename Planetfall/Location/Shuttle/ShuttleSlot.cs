using Planetfall.Item.Kalamontee.Admin;

namespace Planetfall.Location.Shuttle;

/// <summary>
/// Represents a slot designed to activate shuttle controls upon successful interaction.
/// This class is a specialized implementation of the <see cref="SlotBase{TAccessCard,TAccessSlot}"/>
/// for shuttle systems within a specific context.
/// A ShuttleSlot corresponds to a location where a specific type of access card (e.g., <see cref="ShuttleAccessCard"/>)
/// can be inserted to activate or interact with shuttle controls of type <typeparamref name="TShuttle"/>.
/// </summary>
/// <typeparam name="TShuttle">
/// The type of the shuttle control this slot interacts with. It must be a class derived from
/// <see cref="ShuttleControl"/> and provide a parameterless constructor.
/// </typeparam>
internal class ShuttleSlot<TShuttle> : SlotBase<ShuttleAccessCard, ShuttleSlot<TShuttle>>
    where TShuttle : class, IShuttleControl, new()
{
    public override string[] NounsForMatching => ["shuttle slot", "slot", "shuttle control slot"];

    protected override InteractionResult OnSuccessfulSlide(IContext context, string? afterSlideMessage)
    {
        return Repository.GetLocation<TShuttle>().Activate(context);
    }
}
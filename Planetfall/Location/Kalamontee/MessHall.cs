using GameEngine.Location;
using Model.Movement;
using Planetfall.Item.Kalamontee;

namespace Planetfall.Location.Kalamontee;

internal class MessHall : LocationBase
{
    protected override Dictionary<Direction, MovementParameters> Map { get; }

    protected override string ContextBasedDescription =>
        "This is a large hall lined with tables and benches. An opening to the north leads back to the corridor. " +
        "A door to the south is closed. Next to the door is a small slot. ";

    public override string Name => "Mess Hall";

    public override void Init()
    {
        StartWithItem<Canteen>();
    }
}
using Model.Movement;

namespace ZorkOne.Location;

public class SandyBeach : BaseLocation
{
    protected override Dictionary<Direction, MovementParameters> Map { get; }

    protected override string ContextBasedDescription =>
        "You are on a large sandy beach on the east shore of the river, which is flowing quickly by. " +
        "A path runs beside the river to the south here, and a passage is partially buried in sand to the northeast. ";

    public override string Name => "Sandy Beach";
    
    public override void Init()
    {
       StartWithItem<Shovel>(this);
    }
}
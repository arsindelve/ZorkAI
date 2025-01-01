using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

public class AtlantisRoom : DarkLocation
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context) =>
        new()
        {
            { Direction.S, new MovementParameters { Location = GetLocation<ReservoirNorth>() } },
            { Direction.Up, new MovementParameters { Location = GetLocation<CaveNorth>() } }
        };

    protected override string GetContextBasedDescription(IContext context) =>
        "This is an ancient room, long under water. There is an exit to the south and a staircase leading up. ";

    public override string Name => "Atlantis Room";

    public override void Init()
    {
        StartWithItem<Trident>();
    }
}
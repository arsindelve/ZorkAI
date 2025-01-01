using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

internal class NarrowPassage : DarkLocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context) =>
        new()
        {
            { Direction.N, new MovementParameters { Location = GetLocation<RoundRoom>() } },
            { Direction.S, new MovementParameters { Location = GetLocation<MirrorRoomSouth>() } }
        };

    protected override string GetContextBasedDescription(IContext context) =>
        "This is a long and narrow corridor where a long north-south passageway briefly narrows even further. ";

    public override string Name => "Narrow Passage";
}
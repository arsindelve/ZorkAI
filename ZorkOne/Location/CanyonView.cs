using Model.Movement;
using ZorkOne.Location.ForestLocation;

namespace ZorkOne.Location;

public class CanyonView : LocationWithNoStartingItems
{
    public override string Name => "Canyon View";

    protected override string ContextBasedDescription =>
        "You are at the top of the Great Canyon on its west wall. From here there is a marvelous " +
        "view of the canyon and parts of the Frigid River upstream. Across the canyon, the walls of the " +
        "White Cliffs join the mighty ramparts of the Flathead Mountains to the east. Following the Canyon " +
        "upstream to the north, Aragain Falls may be seen, complete with rainbow. The mighty Frigid River " +
        "flows out from a great dark cavern. To the west and south can be seen an immense forest, stretching for " +
        "miles around. A path leads northwest. It is possible to climb down into the canyon from here.";

    protected override Dictionary<Direction, MovementParameters> Map => new()
    {
        {
            Direction.NW, new MovementParameters { Location = GetLocation<ClearingBehindHouse>() }
        },
        {
            Direction.Down, new MovementParameters { Location = GetLocation<RockyLedge>() }
        },
        {
            Direction.E, new MovementParameters { Location = GetLocation<RockyLedge>() }
        },
        {
            Direction.W, new MovementParameters { Location = GetLocation<ForestThree>() }
        }
    };
}
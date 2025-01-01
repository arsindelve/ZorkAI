using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location.CoalMineLocation;

internal class TimberRoom : DarkLocation
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context) =>
        new()
        {
            {
                Direction.E, new MovementParameters { Location = GetLocation<LadderBottom>() }
            },
            {
                Direction.W, new MovementParameters { Location = GetLocation<DraftyRoom>(), WeightLimit = 2 }
            }
        };

    protected override string GetContextBasedDescription(IContext context) =>
        "This is a long and narrow passage, which is cluttered with broken timbers. A wide passage comes from " +
        "the east and turns at the west end of the room into a very narrow passageway. From the west comes a strong draft.";

    public override string Name => "Timber Room";

    public override void Init()
    {
        StartWithItem<Timber>();
    }
}
using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

public class DeepCanyon : DarkLocation
{
    public override string Name => "Deep Canyon";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.NW, new MovementParameters { Location = GetLocation<ReservoirSouth>() } },
            { Direction.SW, new MovementParameters { Location = GetLocation<NorthSouthPassage>() } },
            { Direction.E, new MovementParameters { Location = GetLocation<Dam>() } },
            { Direction.Down, new MovementParameters { Location = GetLocation<LoudRoom>() } }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "You are on the south edge of a deep canyon. Passages lead off to the east, " +
               "northwest and southwest. A stairway leads down. You can hear the sound of flowing water from below.";
    }

    public override void Init()
    {
    }
}
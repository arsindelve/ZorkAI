using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

public class NorthSouthPassage : DarkLocation
{
    // Room names are serialized as area keys, so keep formatting whitespace out of the name.
    public override string Name => "North-South Passage";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.S, new MovementParameters { Location = GetLocation<RoundRoom>() } },
            { Direction.N, new MovementParameters { Location = GetLocation<Chasm>() } },
            { Direction.NE, new MovementParameters { Location = GetLocation<DeepCanyon>() } }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "This is a high north-south passage, which forks to the northeast. ";
    }

    public override void Init()
    {
    }
}

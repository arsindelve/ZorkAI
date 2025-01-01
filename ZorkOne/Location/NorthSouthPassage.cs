using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

public class NorthSouthPassage : DarkLocation
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context) =>
        new()
        {
            { Direction.S, new MovementParameters { Location = GetLocation<RoundRoom>() } },
            { Direction.N, new MovementParameters { Location = GetLocation<Chasm>() } },
            { Direction.NE, new MovementParameters { Location = GetLocation<DeepCanyon>() } }
        };

    protected override string GetContextBasedDescription(IContext context) =>
        "This is a high north-south passage, which forks to the northeast. ";

    public override string Name => "North-South Passage\n";

    public override void Init()
    {
    }
}
using GameEngine.Location;
using Model.Movement;
using ZorkOne.Location.ForestLocation;
using ZorkOne.Location.MazeLocation;

namespace ZorkOne.Location;

public class Clearing : LocationBase
{
    protected override Dictionary<Direction, MovementParameters> Map => new()
    {
        {
            Direction.S, new MovementParameters { Location = GetLocation<ForestPath>() }
        },
        {
            Direction.E, new MovementParameters { Location = GetLocation<ForestTwo>() }
        },
        {
            Direction.W, new MovementParameters { Location = GetLocation<ForestOne>() }
        },
        {
            Direction.N,
            new MovementParameters
                { CanGo = _ => false, CustomFailureMessage = "The forest becomes impenetrable to the north." }
        },
        {
            Direction.Down,
            new MovementParameters
            {
                CanGo = _ => false, CustomFailureMessage = "The grating is closed. ",
                Location = GetLocation<GratingRoom>()
            }
        }
    };

    public override string Name => "Clearing";

    protected override string GetContextBasedDescription() =>
        "You are in a clearing, with a forest surrounding you on all sides. A path leads south. ";

    public override void Init()
    {
        StartWithItem<PileOfLeaves>();
    }
}
using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

public class DampCave : DarkLocation
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context) =>
        new()
        {
            { Direction.W, new MovementParameters { Location = GetLocation<LoudRoom>() } },
            { Direction.E, new MovementParameters { Location = GetLocation<WhiteCliffsBeachNorth>() } },
            {
                Direction.S,
                new MovementParameters
                    { CanGo = _ => false, CustomFailureMessage = "It is too narrow for most insects. " }
            }
        };

    protected override string GetContextBasedDescription(IContext context) =>
        "This cave has exits to the west and east, and narrows to a crack toward the south. The earth is particularly damp here. ";

    public override string Name => "Damp Cave";

    public override void Init()
    {
    }
}
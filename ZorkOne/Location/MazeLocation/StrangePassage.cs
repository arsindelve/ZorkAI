using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location.MazeLocation;

public class StrangePassage : DarkLocationWithNoStartingItems
{
    public override string Name => "Strange Passage ";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.W, Go<CyclopsRoom>() },
            { Direction.E, Go<LivingRoom>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "This is a long passage. To the west is one entrance. On the east there is an old wooden door, with " +
               "a large opening in it (about cyclops sized). ";
    }
}
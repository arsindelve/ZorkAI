using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location.CoalMineLocation;

public class SlideRoom : LocationWithNoStartingItems
{
    public override string Name => "Slide Room";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.E, new MovementParameters { Location = GetLocation<ColdPassage>() } },
            { Direction.Down, new MovementParameters { Location = GetLocation<Cellar>() } },
            { Direction.N, new MovementParameters { Location = GetLocation<MineEntrance>() } }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "This is a small chamber, which appears to have been part of a coal mine. On the south wall of the " +
               "chamber the letters \"Granite Wall\" are etched in the rock. To the east is a long passage, and " +
               "there is a steep metal slide twisting downward. To the north is a small opening.";
    }
}
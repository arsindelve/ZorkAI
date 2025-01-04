using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

public class Attic : DarkLocation
{
    public override string Name => "Attic";

    protected override string GetContextBasedDescription(IContext context)
    {
        return "This is the attic. The only exit is a stairway leading down. ";
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.Down, new MovementParameters { Location = GetLocation<Kitchen>() } }
        };
    }

    public override void Init()
    {
        StartWithItem<Rope>();
        StartWithItem<NastyKnife>();
    }
}
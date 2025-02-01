using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

public class EgyptianRoom : DarkLocation, IThiefMayVisit
{
    public override string Name => "Egyptian Room";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.W, new MovementParameters { Location = GetLocation<Temple>() } }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "This is a room which looks like an Egyptian tomb. There is an ascending staircase to the west. ";
    }

    public override void Init()
    {
        StartWithItem<Coffin>();
    }
}
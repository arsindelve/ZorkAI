using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

public class EngravingsCave : DarkLocation, IThiefMayVisit
{
    public override string Name => "Engravings Cave";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.NW, new MovementParameters { Location = GetLocation<RoundRoom>() } },
            { Direction.E, new MovementParameters { Location = GetLocation<DomeRoom>() } }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "You have entered a low cave with passages leading northwest and east.\n\nThere are old engravings on the walls here.";
    }

    public override void Init()
    {
        StartWithItem<Engravings>();
    }
}
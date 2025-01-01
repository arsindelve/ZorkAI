using GameEngine.Location;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

public class EngravingsCave : DarkLocation
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context) =>
        new()
        {
            { Direction.NW, new MovementParameters { Location = GetLocation<RoundRoom>() } },
            { Direction.E, new MovementParameters { Location = GetLocation<DomeRoom>() } }
        };

    public override string Name => "Engravings Cave";

    protected override string GetContextBasedDescription(IContext context) =>
        "You have entered a low cave with passages leading northwest and east.\n\nThere are old engravings on the walls here.";

    public override void Init()
    {
        StartWithItem<Engravings>();
    }
}
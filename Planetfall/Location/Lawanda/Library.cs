using GameEngine.Location;
using Model.Movement;
using Planetfall.Item.Lawanda;
using Planetfall.Item.Lawanda.Library;

namespace Planetfall.Location.Lawanda;

public class Library : LocationBase
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
        =>
            new()
            {
                { Direction.Down, Go<LibraryLobby>() },
                { Direction.E, Go<LibraryLobby>() }
            };
    

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is a large carpeted room with a desk and many small tables. The sole exit is down a few steps to the east. ";
    }

    public override string Name => "Library";
    
    public override void Init()
    {
        StartWithItem<GreenSpool>();
    }
}
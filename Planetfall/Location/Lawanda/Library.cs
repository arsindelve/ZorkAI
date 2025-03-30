using GameEngine.Location;
using Planetfall.Item.Lawanda.Library;

namespace Planetfall.Location.Lawanda;

public class Library : LocationBase
{
    public override string Name => "Library";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.Down, Go<LibraryLobby>() },
            { Direction.E, Go<LibraryLobby>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is a large carpeted room with a desk and many small tables. The sole exit is down a few steps to the east. ";
    }

    public override void Init()
    {
        StartWithItem<GreenSpool>();
    }
}
using GameEngine.Location;
using Model.Movement;
using Planetfall.Item.Lawanda.Library.Computer;

namespace Planetfall.Location.Lawanda;

public class LibraryLobby : LocationBase
{
    public override string Name => "Library Lobby";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.Up, Go<Library>() },
            { Direction.W, Go<Library>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is a carpeted room, thick with dust, with exits to the north and south. To the west, " +
            "up a few steps, is a wide doorway. A small booth lies to the east. ";
    }

    public override void Init()
    {
        StartWithItem<ComputerTerminal>();
    }
}
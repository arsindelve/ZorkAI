using GameEngine.Location;
using Model.Movement;

namespace Planetfall.Location.Lawanda;

public class LibraryLobby : LocationBase
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        throw new NotImplementedException();
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is a carpeted room, thick with dust, with exits to the north and south. To the west, up a few steps, is a wide doorway. A small booth lies to the east. ";
    }

    public override string Name => "Library Lobby";
    
    public override void Init()
    {
       
    }
}
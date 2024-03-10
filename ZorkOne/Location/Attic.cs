using Game;
using Game.Location;
using Model;
using ZorkOne.Item;

namespace ZorkOne.Location;

public class Attic : DarkLocation
{
    protected override string Name => "Attic";

    protected override string ContextBasedDescription => "This is the attic. The only exit is a stairway leading down. ";

    public Attic()
    {
        StartWithItem(Repository.GetItem<Rope>(), this);
    }
    
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.Down, new MovementParameters { Location = GetLocation<Kitchen>() } }
        };
}
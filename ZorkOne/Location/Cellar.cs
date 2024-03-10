using Game.Location;
using Model;

namespace ZorkOne.Location;

public class Cellar : DarkLocation
{
    protected override Dictionary<Direction, MovementParameters> Map { get; }
    
    protected override string Name => "Cellar";
    
    protected override string ContextBasedDescription => "oh";
}
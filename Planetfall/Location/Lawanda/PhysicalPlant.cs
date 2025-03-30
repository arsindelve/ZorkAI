using GameEngine.Location;

namespace Planetfall.Location.Lawanda;

internal class PhysicalPlant : LocationWithNoStartingItems
{
    public override string Name => "Physical Plant";
    
    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.W, Go<SystemsCorridorEast>() }
           
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "This is an enormous room full of environmental control equipment presumably intended to heat and " +
               "ventilate the Lawanda Complex. Oddly, although the Lawanda Complex is slightly smaller than its " +
               "counterpart, this plant is much larger than the one in the Kalamontee Complex. The only exit is westward. ";
    }
}
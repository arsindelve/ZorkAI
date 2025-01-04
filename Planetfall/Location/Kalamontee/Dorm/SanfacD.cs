using GameEngine.Location;
using Model.Movement;

namespace Planetfall.Location.Kalamontee.Dorm;

internal class SanfacD : LocationWithNoStartingItems
{
    public override string Name => "Sanfac D";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.S, Go<DormD>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "This must be the sanitary facility for the adjacent dormitory. " +
               "The fixtures are dry and dusty, the room dead and deserted. You marvel at how " +
               "little the millenia and cultural gulfs have changed toilet bowl design. The only exit is south. ";
    }
}
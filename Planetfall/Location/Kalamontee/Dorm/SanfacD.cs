using GameEngine.Location;
using Model.Movement;

namespace Planetfall.Location.Kalamontee.Dorm;

internal class SanfacD : LocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map(IContext context) =>
        new()
        {
            { Direction.S, Go<DormD>() }
        };

    protected override string GetContextBasedDescription(IContext context) =>
        "This must be the sanitary facility for the adjacent dormitory. " +
        "The fixtures are dry and dusty, the room dead and deserted. You marvel at how " +
        "little the millenia and cultural gulfs have changed toilet bowl design. The only exit is south. ";

    public override string Name => "Sanfac D";
}
using GameEngine.Location;
using Model.Movement;

namespace Planetfall.Location.Kalamontee.Dorm;

internal class SanfacA : LocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.N, Go<DormA>() }
        };

    protected override string GetContextBasedDescription() =>
        "This must be the sanitary facility for the adjacent dormitory. " +
        "The fixtures are dry and dusty, the room dead and deserted. You marvel at how " +
        "little the millenia and cultural gulfs have changed toilet bowl design. The only exit is north. ";

    public override string Name => "Sanfac A";
}
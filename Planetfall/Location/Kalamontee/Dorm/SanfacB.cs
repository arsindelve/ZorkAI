using GameEngine.Location;
using Model.Movement;

namespace Planetfall.Location.Kalamontee.Dorm;

internal class SanfacB : LocationWithNoStartingItems
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.S, Go<DormB>() }
        };

    protected override string ContextBasedDescription =>
        "This must be the sanitary facility for the adjacent dormitory. " +
        "The fixtures are dry and dusty, the room dead and deserted. You marvel at how " +
        "little the millenia and cultural gulfs have changed toilet bowl design. The only exit is south. ";

    public override string Name => "Sanfac B";
}
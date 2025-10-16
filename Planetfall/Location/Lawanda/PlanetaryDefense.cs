using GameEngine.Location;
using Planetfall.Item.Lawanda;
using Planetfall.Item.Lawanda.PlanetaryDefense;
using Planetfall.Location.Kalamontee.Admin;

namespace Planetfall.Location.Lawanda;

internal class PlanetaryDefense : LocationBase
{
    public override string Name => "Planetary Defense";

    [UsedImplicitly]
    public bool Fixed { get; set; }

    public override void Init()
    {
        StartWithItem<FromitzAccessPanel>();
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.S, Go<SystemsCorridor>() }
        };
    }

    internal void ItIsFixed(IContext context)
    {
        Fixed = true;
        Repository.GetLocation<SystemsMonitors>().MarkPlanetaryDefenseFixed();
        context.AddPoints(6);
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This room is filled with a dazzling array of lights and controls. " + (!Fixed
                ? "One light, blinking quickly, catches " +
                  "your eye. It reads \"Surkit Boord Faalyur. WORNEENG: xis boord kuntroolz xe diskriminaashun\nsurkits.\" "
                : "") + "There is a small access panel on one wall which is closed. ";
    }
}
namespace Planetfall.Location.Kalamontee.Dorm;

/// <summary>
/// Base class for the four dormitory sanitary facilities (Sanfac A-D), which share the same prose
/// about dry, dusty fixtures. The fixtures are declared as scenery here so all four inherit it
/// (issue #315). Sanfac E and F live elsewhere and have their own prose (F explicitly has no
/// fixtures), so they are not part of this hierarchy.
/// </summary>
internal abstract class SanfacBase : LocationWithNoStartingItems
{
    protected override IReadOnlyList<SceneryItem> Scenery =>
    [
        new(["fixtures", "fixture"], "The fixtures are bone dry and thick with dust. ",
            "The fixtures are plumbed into the wall. ")
    ];
}

using GameEngine.Location;

namespace ZorkOne.Location.ForestLocation;

/// <summary>
/// Base class for the plain forest rooms (Forest One through Four), which all describe trees all
/// around. The trees are declared as scenery here so every forest room inherits it (issue #315).
/// Forest Path is intentionally excluded: it has its own examine handling for the large climbable
/// tree that leads Up a Tree.
/// </summary>
public abstract class ForestBase : LocationWithNoStartingItems
{
    protected override IReadOnlyList<SceneryItem> Scenery =>
    [
        new(["trees", "tree", "forest"],
            "Forest trees stand all around you. ",
            "The trees are far too firmly rooted to take. ")
    ];
}

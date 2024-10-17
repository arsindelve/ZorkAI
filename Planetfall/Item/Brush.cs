using GameEngine.Item;
using Model.Item;

namespace Planetfall.Item;

public class Brush : ItemBase, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching =>
    [
        "brush", "scrub brush", "multi-purpose scrub brush", "Patrol-issue self-contained multi-purpose scrub brush"
    ];

    public string OnTheGroundDescription => "There is a Patrol-issue self-contained multi-purpose scrub brush here.";

    public override string NeverPickedUpDescription => OnTheGroundDescription;

    public override string InInventoryDescription => "A Patrol-issue self-contained multi-purpose scrub brush ";
}
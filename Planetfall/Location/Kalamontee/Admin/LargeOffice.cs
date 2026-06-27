using GameEngine.Location;
using Planetfall.Item.Kalamontee.Admin;
using Planetfall.Item.Kalamontee.Mech.FloydPart;

namespace Planetfall.Location.Kalamontee.Admin;

internal class LargeOffice : FloydSpecialInteractionLocation
{
    public override string Name => "Large Office";

    public override string[] NounsForMatching => ["study"];

    public override string FloydPrompt => FloydPrompts.LargeOfficeWindow;

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.E, Go<SmallOffice>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "This is a large, plush office. The far wall is one large picture window, scratched but unbroken, " +
               "offering a view of this installation and the ocean beyond. In front of the window is a wide wooden desk. " +
               "The only exit is east. ";
    }

    protected override IReadOnlyList<SceneryItem> Scenery =>
    [
        new(["ocean"], "The ocean stretches out past the installation, all the way to the horizon. ",
            "The ocean is well out of your reach. ")
    ];

    public override void Init()
    {
        StartWithItem<LargeDesk>();
    }
}
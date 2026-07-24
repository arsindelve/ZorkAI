using Planetfall.Item.Kalamontee.Mech;
using Planetfall.Location.Lawanda;

namespace Planetfall.Item.Lawanda;

public class LargeMetalCube : OpenAndCloseContainerBase
{
    public override string[] NounsForMatching => ["large metal cube", "metal cube", "cube", "lid"];

    // Examine is inherited from OpenAndCloseContainerBase (issue #398): open -> lists the fused
    // bedistor inside; closed -> "The large metal cube is closed." The old override reported only
    // "is open" and hid the contents.
    public override Type[] CanOnlyHoldTheseTypes => [typeof(BedistorBase)];

    // Issue #462: the cube is a SINGLE bedistor socket. ContainerBase.SpaceForItems defaults to 2
    // and each bedistor is Size 1, so without this override the cube silently admitted a good
    // bedistor ALONGSIDE the shipped fused one (1 + 1 <= 2) - "fixing" Course Control with the
    // broken part still socketed and skipping the pliers-removal sub-puzzle. One slot forces the
    // correct swap (pry the fused bedistor out first, then socket the good one). Mirrors the laser
    // depression (#437) and FromitzAccessPanel capping.
    protected override int SpaceForItems => 1;

    // With the socket full, refuse the extra bedistor by asserting the occupancy (mirrors the laser,
    // issue #437). This is safe because every bedistor is Size 1 and the cube only holds bedistors:
    // HaveRoomForItem can only fail here when one is already in the socket. The type check runs
    // before the room check (PutProcessor, issue #417), so a non-bedistor still gets the type
    // refusal below.
    public override string NoRoomMessage =>
        "There's already a bedistor in the socket. ";

    // The original's "put" handler ends with a refusal naming the item (CUBE-F, comptwo.zil:583).
    // Without this the type rejection has no message and falls through to the AI narrator, which
    // answers a deterministic refusal with a generated one (and a blank line if generation fails).
    public override string CanOnlyHoldTheseTypesErrorMessage(string nameOfItemWeTriedToPlace) =>
        $"The {nameOfItemWeTriedToPlace} doesn't fit. ";

    public override int Size => 1;

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return "In one corner is a large metal cube whose lid is open. ";
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        if (IsOpen && Items.Any())
            return $"In one corner is a large metal cube whose lid is open. \n{ItemListDescription("large metal cube", currentLocation)}";

        return $"In one corner is a large metal cube whose lid is {(IsOpen ? "open" : "closed")}. ";
    }

    public override string NowOpen(ILocation currentLocation)
    {
        if (!Items.Any())
            return "The lid swings open. ";

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("The lid swings open.");
        sb.AppendLine("The large metal cube contains:");
        foreach (var item in Items)
        {
            sb.AppendLine($"  {item.GenericDescription(currentLocation)}");
        }

        return sb.ToString().TrimEnd();
    }
    
    public override string ItemPlacedHereResult(IItem item, IContext context)
    {
        if (item is not GoodBedistor) return base.ItemPlacedHereResult(item, context);
        
        string response = "";
        response += "Done. The warning lights go out and another light goes on. ";
        Repository.GetLocation<CourseControl>().ItIsFixed(context);
        return response;
    }

    public override void Init()
    {
        IsOpen = false;
        StartWithItemInside<FusedBedistor>();
    }
}

using Planetfall.Item.Kalamontee.Mech;
using Planetfall.Location.Lawanda;

namespace Planetfall.Item.Lawanda;

public class LargeMetalCube : OpenAndCloseContainerBase, ICanBeExamined
{
    public override string[] NounsForMatching => ["large metal cube", "metal cube", "cube", "lid"];

    public string ExaminationDescription => $"The large metal cube is {(IsOpen ? "open" : "closed")}. ";
    
    public override Type[] CanOnlyHoldTheseTypes => [typeof(BedistorBase)];

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

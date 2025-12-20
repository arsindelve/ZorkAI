namespace Planetfall.Item.Lawanda;

public class LargeMetalCube : OpenAndCloseContainerBase, ICanBeExamined
{
    public override string[] NounsForMatching => ["large metal cube", "metal cube", "cube", "lid"];

    public string ExaminationDescription => $"The large metal cube is {(IsOpen ? "open" : "closed")}. ";

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

    public override void Init()
    {
        IsOpen = false;
        StartWithItemInside<FusedBedistor>();
    }
}

namespace Planetfall.Item.Kalamontee.Mech;

public abstract class BatteryBase : ItemBase
{
    public override int Size => 1;
    
    public override string[] NounsForPreciseMatching => NounsForMatching.Except(["battery"]).ToArray();
    
    public abstract int ChargesRemaining { get; set; }
}
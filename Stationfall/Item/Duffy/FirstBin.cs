namespace Stationfall.Item.Duffy;

/// <summary>The leftmost bin in the Robot Pool. See <see cref="RobotBin" />.</summary>
public class FirstBin : RobotBin
{
    protected override int BinNumber => 1;

    public override string[] NounsForMatching => ["first bin", "bin one", "bin 1", "one bin"];
}

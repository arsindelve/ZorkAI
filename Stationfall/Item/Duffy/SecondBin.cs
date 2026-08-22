namespace Stationfall.Item.Duffy;

/// <summary>The middle bin in the Robot Pool. See <see cref="RobotBin" />.</summary>
public class SecondBin : RobotBin
{
    protected override int BinNumber => 2;

    public override string[] NounsForMatching => ["second bin", "bin two", "bin 2", "two bin"];
}

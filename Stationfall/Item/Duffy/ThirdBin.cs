namespace Stationfall.Item.Duffy;

/// <summary>The rightmost bin in the Robot Pool. See <see cref="RobotBin" />.</summary>
public class ThirdBin : RobotBin
{
    protected override int BinNumber => 3;

    public override string[] NounsForMatching => ["third bin", "bin three", "bin 3", "three bin"];
}

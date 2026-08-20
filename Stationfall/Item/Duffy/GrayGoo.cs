namespace Stationfall.Item.Duffy;

/// <summary>
///     One of the two ration blobs in the survival kit (ship.zil:1377-1384).
/// </summary>
public class GrayGoo : GooBase
{
    public override string[] NounsForMatching => ["gray goo", "grey goo", "gray blob", "custard"];

    protected override string OfficialName => "Ramosian tree-mold custard";

    protected override string Color => "gray";
}

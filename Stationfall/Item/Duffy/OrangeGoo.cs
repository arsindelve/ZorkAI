namespace Stationfall.Item.Duffy;

/// <summary>
///     The other ration blob in the survival kit (ship.zil:1386-1393).
/// </summary>
public class OrangeGoo : GooBase
{
    public override string[] NounsForMatching => ["orange goo", "orange blob", "yogurt"];

    protected override string OfficialName => "apricot yogurt";

    protected override string Color => "orange";
}

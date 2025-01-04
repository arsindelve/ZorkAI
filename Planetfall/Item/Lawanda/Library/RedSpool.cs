namespace Planetfall.Item.Lawanda.Library;

public class RedSpool : SpoolBase
{
    public override string[] NounsForMatching => ["red spool", "spool", "red"];

    public override string Contents =>
        "\"Xe jestaashun peereeid uv Xe Dizeez, folooweeng ekspoozur, vaareez treemenduslee frum pursin " +
        "tuu pursin, raanjeeng frum wun daa tuu sevrul rootaashunz. Wuns xe furst simptumz ar shoon, " +
        "dex alwaaz okurz in aat tuu ten daaz.\nXe priimeree simptum iz aa hii feevur. Xe sekunderee " +
        "simptum iz aa sharp inkrees in xe amownt uv sleep needid eec niit.\"\nThe rest of the " +
        "information is about symptoms which can be detected only by using complicated medical procedures.";

    public override string ExaminationDescription => "The spool is labelled \"Simptumz uv Xe Dizeez.\"";

    public override string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a red spool here. ";
    }
    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return OnTheGroundDescription(currentLocation);
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A red spool. ";
    }
}
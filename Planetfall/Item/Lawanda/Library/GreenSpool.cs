namespace Planetfall.Item.Lawanda.Library;

public class GreenSpool : SpoolBase
{
    public override string[] NounsForMatching => ["green spool", "spool", "green"];

    public override string Contents =>
        "\"Oonlee peepul wix propur traaneeng shud piilot xe helikopturz. Reekwiird ekwipmint inkluudz aa " +
        "Helikoptur Akses Kard and aa Kuntrool Panul Kee. Xeez kan bee obtaand frum Tranzportaashun " +
        "Stoorij.\"\nThe rest is all very technical. ";

    public override string ExaminationDescription => "The spool is labelled \"Helikoptur Opuraateeng Manyuuwul.\" ";

    public override string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a green spool here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return OnTheGroundDescription(currentLocation);
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A green spool. ";
    }
}
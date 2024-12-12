namespace Planetfall.Item.Feinstein;

public class Chronometer : ItemBase, ICanBeTakenAndDropped, ICanBeExamined, ICanBeRead
{

    internal int CurrentTime { get; set; } = 4501;

    public string ExaminationDescription => $"It is a standard wrist chronometer with a digital display. According to the chronometer, the current time is {CurrentTime}. The back is engraved with the message \"Good luck in the Patrol! Love, Mom and Dad.\"";

    public override string[] NounsForMatching => ["chronometer", "watch", "wrist-watch"];

    public string ReadDescription => ExaminationDescription;

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a chronometer here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return OnTheGroundDescription(currentLocation);
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A chronometer";
    }
}

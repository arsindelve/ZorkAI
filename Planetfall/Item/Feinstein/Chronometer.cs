namespace Planetfall.Item.Feinstein;

public class Chronometer : ItemBase, ICanBeTakenAndDropped, ICanBeExamined, ICanBeRead, IAmClothing
{
    // Start time of the game. 
    internal int CurrentTime { get; set; } = new Random().Next(4500, 4700);

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
    
    // It is being worn at the beginning of the game
    public bool BeingWorn { get; set; } = true;
}

namespace Planetfall.Item.Feinstein;

public class Chronometer : ItemBase, ICanBeTakenAndDropped, ICanBeExamined, ICanBeRead, IAmClothing
{
    private static readonly Random Random = new();

    /// <summary>
    /// Morning wake-up times per day (from SLEEP_MECHANICS.md reset_time routine).
    /// Each day the player wakes up slightly later as the disease progresses.
    /// </summary>
    private static readonly Dictionary<int, int> MorningTimesByDay = new()
    {
        { 1, 4600 },  // Game start time (not used for wake-up)
        { 2, 1600 },  // Day 2: ~1600-1680
        { 3, 1750 },  // Day 3: ~1750-1830
        { 4, 1950 },  // Day 4: ~1950-2030
        { 5, 2150 },  // Day 5: ~2150-2230
        { 6, 2450 },  // Day 6: ~2450-2530
        { 7, 2800 },  // Day 7: ~2800-2880
        { 8, 3200 }   // Day 8: ~3200-3280
    };

    // Start time of the game (random between 4500-4700)
    public int CurrentTime { get; set; } = Random.Next(4500, 4700);

    public override string[] NounsForMatching => ["chronometer", "watch", "wrist-watch"];

    /// <summary>
    /// Resets the time to morning for the given day (called on wake-up).
    /// Per original game, each day starts at a progressively later time.
    /// </summary>
    public void ResetToMorning(int day)
    {
        if (MorningTimesByDay.TryGetValue(day, out var baseTime))
        {
            CurrentTime = baseTime + Random.Next(80);
        }
    }

    // It is being worn at the beginning of the game
    public bool BeingWorn { get; set; } = true;

    public string ExaminationDescription => $"It is a standard wrist chronometer with a digital display. According " +
                                            $"to the chronometer, the current time is {CurrentTime}. The back is " +
                                            $"engraved with the message \"Good luck in the Patrol! Love, Mom and Dad.\"";

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
        return "A chronometer" + (BeingWorn ? " (being worn)" : "");
    }
}
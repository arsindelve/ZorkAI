namespace Stationfall.Item.Duffy;

/// <summary>
///     The soup in the Thermos (ship.zil:1335-1343). In the original it slowly cools; that timer only
///     matters for flavor, so the port keeps the soup simple until the survival clocks land in Phase 3.
/// </summary>
public class BlueSoup : ItemBase, ICanBeExamined, ICanBeEaten
{
    public override string[] NounsForMatching => ["blue soup", "soup"];

    public override int Size => 2;

    public string ExaminationDescription =>
        "A serving of blue soup — blueberry and walnut, according to the Thermos. It is still warm. ";

    public (string Message, bool WasConsumed) OnEating(IContext context)
    {
        if (context is StationfallContext stationfallContext)
        {
            if (!stationfallContext.IsHungry)
                return (StationfallContext.NotHungryMessage, false);

            stationfallContext.Eat();
        }

        return ("You drink the soup. It is sweet, faintly nutty, and by a wide margin the best thing " +
                "the Patrol has ever fed you. ", true);
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "Some blue soup";
    }
}

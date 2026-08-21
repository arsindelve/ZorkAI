using Model.AIGeneration;

namespace Stationfall.Item.Duffy;

/// <summary>
///     The soup in the Thermos (ship.zil SOUP-F). It has a temperature, and that temperature falls the
///     whole time you are carrying it — four times faster with the lid off (ship.zil I-THERMOS). Nothing
///     hangs on it mechanically; it is there so that the one comfort the Patrol issued you visibly
///     stops being one.
/// </summary>
public class BlueSoup : ItemBase, ICanBeExamined, ICanBeEaten, ITurnBasedActor
{
    /// <summary>
    ///     Millichrons between one degree of cooling and the next.
    /// </summary>
    private const int CoolingInterval = 100;

    private const int DegreesLostSealed = 1;

    /// <summary>
    ///     An open Thermos is barely a Thermos (ship.zil I-THERMOS).
    /// </summary>
    private const int DegreesLostOpen = 4;

    /// <summary>
    ///     How much a night's sleep costs the soup (globals.zil:1150-1155).
    /// </summary>
    public const int DegreesLostOvernight = 30;

    // Adjectives are matched too, so "drink the hot soup" works while it still is hot - and stops
    // working, correctly, once it isn't.
    public override string[] NounsForMatching =>
        ["blue soup", "soup", "blueberry soup", "walnut soup", "hot soup", "steaming soup",
         "lukewarm soup", "cool soup", "cold soup"];

    public override int Size => 2;

    /// <summary>
    ///     100 is straight from the urn; 0 is stone cold.
    /// </summary>
    [UsedImplicitly]
    public int Warmth { get; set; } = 100;

    /// <summary>
    ///     When the next degree comes off. Zero until the clock is started, which the original does the
    ///     first time the player boards the truck rather than at the start of the game.
    /// </summary>
    [UsedImplicitly]
    public int NextCoolingAt { get; set; }

    public string ExaminationDescription => $"The soup seems to be {TemperatureWord}. ";

    /// <summary>
    ///     The original's six-step ladder (ship.zil DESCRIBE-SOUP-TEMPERATURE).
    /// </summary>
    public string TemperatureWord => Warmth switch
    {
        > 80 => "steaming hot",
        > 60 => "quite hot",
        > 40 => "fairly hot",
        > 20 => "lukewarm",
        > 0 => "tepid",
        _ => "on the cool side"
    };

    /// <summary>
    ///     Starts the soup cooling. Called the first time the player boards the truck; calling it again
    ///     is harmless, so it does not need its own has-this-happened flag.
    /// </summary>
    public void StartCooling(IContext context)
    {
        if (NextCoolingAt == 0 && context is StationfallContext stationfall)
            NextCoolingAt = stationfall.CurrentTime + CoolingInterval;

        context.RegisterActor(this);
    }

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        if (NextCoolingAt == 0 || context is not StationfallContext stationfall)
            return Task.FromResult(string.Empty);

        if (stationfall.CurrentTime < NextCoolingAt)
            return Task.FromResult(string.Empty);

        Cool(Repository.GetItem<Thermos>().IsOpen ? DegreesLostOpen : DegreesLostSealed);
        NextCoolingAt = stationfall.CurrentTime + CoolingInterval;

        // Cooling is never announced - the player finds out by looking, which is the whole point.
        return Task.FromResult(string.Empty);
    }

    public void Cool(int degrees)
    {
        Warmth = Math.Max(0, Warmth - degrees);
    }

    public (string Message, bool WasConsumed) OnEating(IContext context)
    {
        if (context is StationfallContext stationfallContext)
        {
            if (!stationfallContext.IsHungry)
                return (StationfallContext.NotHungryMessage, false);

            stationfallContext.Eat();
        }

        return ($"You drink the soup. It is sweet, faintly nutty, {TemperatureWord}, and by a wide " +
                "margin the best thing the Patrol has ever fed you. ", true);
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "Some blue soup";
    }
}

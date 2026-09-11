namespace Stationfall.Item.Duffy;

/// <summary>
///     Shared behavior for the spacetruck's two seats. The autopilot refuses to activate, or to accept a
///     course, unless both seats are filled — one by the player and the other by Floyd
///     (globals.zil:766-774, BOTH-SEATS-NOT-OCCUPIED). Floyd climbs into the free seat by himself as
///     soon as the player sits down (ship.zil:1070-1092), which is why he is the only viable robot.
/// </summary>
public abstract class SeatBase : ItemBase, ISubLocation, ICanBeExamined
{
    /// <summary>
    ///     True when Floyd is sitting here. The player's own occupancy is tracked by the engine, as the
    ///     room's <see cref="ILocation.SubLocation" />.
    /// </summary>
    [UsedImplicitly]
    public bool OccupiedByFloyd { get; set; }

    protected abstract SeatBase OtherSeat { get; }

    /// <summary>
    ///     "pilot seat" / "copilot seat" — used in the generated prose.
    /// </summary>
    protected abstract string SeatName { get; }

    public string ExaminationDescription =>
        $"A padded {SeatName} with a full harness, bolted to the deck in front of the viewport. " +
        (OccupiedByFloyd ? "Floyd is sitting in it, feet dangling well short of the floor. " : "");

    public string LocationDescription => $", in the {SeatName}";

    public string GetIn(IContext context)
    {
        if (context.CurrentLocation.SubLocation == this)
            return $"You're already in the {SeatName}. ";

        if (OccupiedByFloyd)
            return $"Floyd is using the {SeatName}, and shows no sign of giving it up. ";

        // Standing up out of the other seat is implied by sitting down in this one.
        if (context.CurrentLocation.SubLocation is SeatBase previous)
            previous.VacateByPlayer(context);

        context.CurrentLocation.SubLocation = this;

        return $"You settle into the {SeatName} and strap yourself in. " + SeatFloydIfPossible(context);
    }

    public string GetOut(IContext context)
    {
        if (context.CurrentLocation.SubLocation != this)
            return $"You're not in the {SeatName}. ";

        context.CurrentLocation.SubLocation = null;
        return "You unstrap yourself and stand up. ";
    }

    private void VacateByPlayer(IContext context)
    {
        if (context.CurrentLocation.SubLocation == this)
            context.CurrentLocation.SubLocation = null;
    }

    /// <summary>
    ///     Floyd takes the other seat the moment the player takes one — provided he is present, and was
    ///     the robot actually requisitioned back at the Robot Pool.
    /// </summary>
    private string SeatFloydIfPossible(IContext context)
    {
        var floyd = Repository.GetItem<Floyd>();

        if (!floyd.IsSelected)
            return string.Empty;

        if (floyd.CurrentLocation != context.CurrentLocation)
            return string.Empty;

        if (OtherSeat.OccupiedByFloyd)
            return string.Empty;

        OtherSeat.OccupiedByFloyd = true;

        return $"Floyd clambers into the {OtherSeat.SeatName}, his feet dangling a few centimeters " +
               "short of the floor. \"Can Floyd drive? Floyd has not crashed a truck in weeks!\" ";
    }
}

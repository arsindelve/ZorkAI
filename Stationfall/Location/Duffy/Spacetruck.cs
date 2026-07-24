using GameEngine.StaticCommand.Implementation;
using Model.AIGeneration;
using Stationfall.Command;

namespace Stationfall.Location.Duffy;

/// <summary>
///     The spacetruck: a twelve-meter Class Three rig with seats for a pilot and a copilot
///     (ship.zil:995-1003). This room is also its own turn-based actor — once a course is accepted it
///     runs the launch sequence that carries the player to the space station.
///     The departure puzzle, in order: shut the hatch, sit down (Floyd takes the other seat), feed the
///     activation form into the slot, and type the course heading computed from the chronometer.
/// </summary>
public class Spacetruck : LocationBase, ITurnBasedActor
{
    /// <summary>
    ///     The launch has not begun. Matches the original's SPACETRUCK-COUNTER sentinel
    ///     (ship.zil:1152).
    /// </summary>
    private const int NotLaunched = -1;

    /// <summary>
    ///     Set once the activation form has been accepted; the keypad is inert until then.
    /// </summary>
    [UsedImplicitly]
    public bool IsActivated { get; set; }

    /// <summary>
    ///     The heading the player typed, or 0 if they haven't yet.
    /// </summary>
    [UsedImplicitly]
    public int CoursePicked { get; set; }

    /// <summary>
    ///     The heading that actually reaches the station, computed from the chronometer at the moment
    ///     the player types (verbs.zil SPACETRUCK-TYPE).
    /// </summary>
    [UsedImplicitly]
    public int RightCourse { get; set; }

    /// <summary>
    ///     Position in the launch sequence: -1 before launch, then 0 through 5.
    /// </summary>
    [UsedImplicitly]
    public int LaunchCounter { get; set; } = NotLaunched;

    /// <summary>
    ///     Turns remaining before the engines fire. The autopilot announces a delay when it accepts the
    ///     course, so this gives the player the window the announcement promises — time to shut the
    ///     hatch or strap in, or to fail to.
    /// </summary>
    [UsedImplicitly]
    public int TurnsUntilLiftoff { get; set; }

    [UsedImplicitly]
    public bool HasDocked { get; set; }

    /// <summary>
    ///     Guards against the sequence advancing more than once per turn, since free commands can run
    ///     actors without advancing the move counter.
    /// </summary>
    [UsedImplicitly]
    public int? LastProcessedMoves { get; set; }

    /// <summary>
    ///     True while the truck is under way — the hatch stays sealed until it docks.
    /// </summary>
    public bool IsInFlight => LaunchCounter >= 0 && !HasDocked;

    private bool CourseIsCorrect => CoursePicked != 0 && CoursePicked == RightCourse;

    public override string Name => "Spacetruck";

    public override string[] NounsForMatching => ["spacetruck", "truck", "spacecraft"];

    protected override IReadOnlyList<SceneryItem> Scenery =>
    [
        new(["keypad", "keys", "autopilot", "console"],
            "A numeric keypad below the viewport, for entering a course heading. ",
            "The keypad is part of the console. "),
        new(["viewport", "window"],
            "A broad viewport above the console. ",
            "The viewport is part of the truck. "),
        new(["radio", "sb radio", "space band radio"],
            "A space-band radio. Its microphone is missing, so you can listen but never answer. ",
            "The radio is built into the console. "),
        new(["red button", "button", "beacon"],
            "A red button that fires the emergency distress beacon. ",
            "The button is part of the console. ")
    ];

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        var hatch = Repository.GetItem<SpacetruckHatch>();

        // Before launch the hatch leads back into the cargo bay; after docking, out into the station.
        ILocation destination = HasDocked
            ? Repository.GetLocation<DockingBayTwo>()
            : Repository.GetLocation<CargoBay>();

        return new Dictionary<Direction, MovementParameters>
        {
            {
                Direction.Out,
                new MovementParameters
                {
                    GatingItem = hatch,
                    Location = destination,
                    CanGo = _ => hatch.IsOpen && !IsInFlight,
                    CustomFailureMessage = IsInFlight
                        ? "You're in deep space. "
                        : "The hatch is closed. "
                }
            }
        };
    }

    /// <summary>
    ///     The autopilot keypad. Read from raw input because the course heading is computed at runtime
    ///     and can be any number; see <see cref="KeypadInput" />.
    /// </summary>
    public override Task<InteractionResult> RespondToSpecificLocationInteraction(string? input, IContext context,
        IGenerationClient client)
    {
        if (KeypadInput.TryParse(input, out var course))
            return Task.FromResult<InteractionResult>(new PositiveInteractionResult(SetCourse(course, context)));

        var normalized = input?.ToLowerInvariant().Replace("the ", "").Trim();

        // Sitting down is the pivotal move in this room (it's what puts Floyd in the other seat), so
        // accept the natural phrasings directly rather than relying on the parser to resolve them.
        if (normalized is not null && IsSitting(normalized))
        {
            var copilotNamed = normalized.Contains("copilot") || normalized.Contains("co-pilot");

            SeatBase seat = copilotNamed
                ? Repository.GetItem<CopilotSeat>()
                : Repository.GetItem<PilotSeat>();

            return Task.FromResult<InteractionResult>(new PositiveInteractionResult(seat.GetIn(context)));
        }

        // The hatch item lives in the Cargo Bay (where you first open it), so it isn't in scope from
        // inside the truck. Handle it here explicitly rather than seeding one shared instance into two
        // rooms, which would leave its CurrentLocation pointing at whichever room initialized last.
        if (normalized is "close hatch" or "shut hatch" or "close door" or "close the hatch")
        {
            var hatch = Repository.GetItem<SpacetruckHatch>();

            if (!hatch.IsOpen)
                return Task.FromResult<InteractionResult>(new PositiveInteractionResult("It is already closed. "));

            hatch.IsOpen = false;
            return Task.FromResult<InteractionResult>(
                new PositiveInteractionResult(hatch.NowClosed(this)));
        }

        if (normalized is "open hatch" or "open door" or "open the hatch")
        {
            var hatch = Repository.GetItem<SpacetruckHatch>();
            var refusal = hatch.CannotBeOpenedDescription(context);

            if (refusal is not null)
                return Task.FromResult<InteractionResult>(new PositiveInteractionResult(refusal));

            if (hatch.IsOpen)
                return Task.FromResult<InteractionResult>(new PositiveInteractionResult("It is already open. "));

            hatch.IsOpen = true;
            return Task.FromResult<InteractionResult>(new PositiveInteractionResult(hatch.NowOpen(this)));
        }

        switch (normalized)
        {
            case "stand":
            case "stand up":
            case "get up":
                return Task.FromResult<InteractionResult>(new PositiveInteractionResult(
                    context.CurrentLocation.SubLocation is SeatBase seat
                        ? seat.GetOut(context)
                        : "You're already standing. "));

            case "push red button":
            case "press red button":
            case "push button":
            case "press button":
                return Task.FromResult<InteractionResult>(new PositiveInteractionResult(
                    "You're not in trouble! Misuse of the emergency beacon is a court-martial offense. "));
        }

        return base.RespondToSpecificLocationInteraction(input, context, client);
    }

    /// <summary>
    ///     Recognizes "sit", "sit down", "sit in the pilot seat", "get in copilot chair", and friends.
    /// </summary>
    private static bool IsSitting(string normalized)
    {
        string[] sitVerbs = ["sit", "sit down", "get in", "get into", "enter", "climb in", "climb into"];

        if (sitVerbs.Any(verb => normalized == verb))
            return true;

        var mentionsSeat = normalized.Contains("seat") || normalized.Contains("chair");

        return mentionsSeat && sitVerbs.Any(verb => normalized.StartsWith(verb + " "));
    }

    /// <summary>
    ///     One seat holds the player and the other holds Floyd — in either arrangement
    ///     (globals.zil:766-774).
    /// </summary>
    public static bool BothSeatsOccupied(IContext context)
    {
        var pilot = Repository.GetItem<PilotSeat>();
        var copilot = Repository.GetItem<CopilotSeat>();

        var playerInPilot = context.CurrentLocation.SubLocation == pilot;
        var playerInCopilot = context.CurrentLocation.SubLocation == copilot;

        return (playerInPilot && copilot.OccupiedByFloyd) || (playerInCopilot && pilot.OccupiedByFloyd);
    }

    /// <summary>
    ///     Accepts a course heading and starts the launch. The correct heading is derived from the
    ///     chronometer reading with integer arithmetic (verbs.zil SPACETRUCK-TYPE) — which is why the
    ///     player has to read the time before typing.
    /// </summary>
    private string SetCourse(int course, IContext context)
    {
        if (!IsActivated)
            return "A recorded voice says, \"Keyboard is active only following authorization.\" ";

        if (HasDocked)
            return "A recorded voice says, \"Fuel level at zero.\" ";

        if (CoursePicked != 0)
            return "A recorded voice says, \"You have already made your selection.\" ";

        if (!BothSeatsOccupied(context))
            return "A recorded voice says, \"Safety precautions forbid the acceptance of course " +
                   "settings unless both the pilot and copilot seats are occupied.\" ";

        CoursePicked = course;
        RightCourse = CalculateCorrectCourse(((ITimeBasedContext)context).CurrentTime);

        LaunchCounter = NotLaunched;
        TurnsUntilLiftoff = 3;

        // Actors run later in this same turn, so claim it: the countdown should start next turn.
        LastProcessedMoves = context.Moves;
        context.RegisterActor(this);

        return "A recorded voice says, \"Course set. Launch in approximately 30 millichrons.\" ";
    }

    /// <summary>
    ///     The original's course formula, in integer arithmetic: divide the time by 50, subtract 132,
    ///     square it, divide by 4, and add 103 (verbs.zil SPACETRUCK-TYPE).
    /// </summary>
    public static int CalculateCorrectCourse(int chronometerTime)
    {
        var x = chronometerTime / 50;
        x -= 132;
        x *= x;
        x /= 4;
        return x + 103;
    }

    /// <summary>
    ///     Runs the launch and the flight. Registered only once a course has been accepted.
    /// </summary>
    public async Task<string> Act(IContext context, IGenerationClient client)
    {
        if (CoursePicked == 0 || HasDocked)
            return string.Empty;

        if (context.Moves == LastProcessedMoves)
            return string.Empty;

        LastProcessedMoves = context.Moves;

        if (TurnsUntilLiftoff > 0)
        {
            TurnsUntilLiftoff--;
            return TurnsUntilLiftoff == 0
                ? "Somewhere beneath the deck plates, the engines begin to whine. "
                : string.Empty;
        }

        LaunchCounter++;

        return LaunchCounter switch
        {
            0 => Liftoff(context),
            1 => "A recorded voice says, \"Fuel level at three-quarters.\" ",
            2 => "There is a moment of stillness as the rear engines cut out. It ends when the braking " +
                 "rockets in the nose roar to life. ",
            3 => "A recorded voice says, \"Fuel level at one-quarter.\" ",
            4 => Approach(),
            5 => await ArriveAsync(context, client),
            _ => string.Empty
        };
    }

    /// <summary>
    ///     The moment of launch, and three ways to die: standing in the bay, leaving the hatch open, or
    ///     failing to strap in (ship.zil:1158-1174).
    /// </summary>
    private string Liftoff(IContext context)
    {
        if (context.CurrentLocation is CargoBay)
        {
            context.RemoveActor(this);
            return new DeathProcessor().Process(
                "The truck roars out of the cargo bay, filling the bay with hot ion gasses. Since you " +
                "slept through most of the lectures at boot camp, you may not recall that hot ion " +
                "gasses are extremely deadly. ", context).InteractionMessage;
        }

        if (context.CurrentLocation is not Spacetruck)
        {
            // Left aboard the ship: the truck simply leaves without you.
            context.RemoveActor(this);
            HasDocked = false;
            return "Somewhere below, you hear the cargo bay doors cycle as the spacetruck departs " +
                   "without you. ";
        }

        if (Repository.GetItem<SpacetruckHatch>().IsOpen)
        {
            context.RemoveActor(this);
            return new DeathProcessor().Process(
                "The truck roars out of the cargo bay. Once clear of the ship, the cabin's air rushes " +
                "out through the hatch you never got around to closing. ", context).InteractionMessage;
        }

        if (context.CurrentLocation.SubLocation is not SeatBase)
        {
            context.RemoveActor(this);
            return new DeathProcessor().Process(
                "The truck roars out of the cargo bay, trailing an impressive cloud of ion dust. You " +
                "are in no condition to admire it, having been smeared across the rear wall of the " +
                "cabin. ", context).InteractionMessage;
        }

        return "The truck roars out of the cargo bay, slowly picking up speed. You settle back for the " +
               "long trip. ";
    }

    private string Approach()
    {
        if (CourseIsCorrect)
            return "A tiny star directly ahead grows brighter and resolves into a distant space " +
                   "station, swelling rapidly as you hurtle toward it. With a final burst, the braking " +
                   "rockets bring you to a halt a few thousand meters out. ";

        return "The braking rockets sputter and die, and the spacetruck comes to a dead stop. Through " +
               "the viewport there is no station — no planet, no ship, no sign of human civilization " +
               "at all. ";
    }

    /// <summary>
    ///     Docking (and the game's first five points), or the slow death that follows a wrong heading.
    /// </summary>
    private async Task<string> ArriveAsync(IContext context, IGenerationClient client)
    {
        context.RemoveActor(this);

        if (!CourseIsCorrect)
            return new DeathProcessor().Process(
                "A recorded voice says, \"Arrival at terminus of inputted course. Fuel level now " +
                "effectively zero. Oxygen supply for one person: approximately two chrons.\" \n\n" +
                "The air grows steadily thinner. Floyd, who does not breathe, becomes quite chipper " +
                "about the splendidly rust-inhibiting atmosphere. You, on the other hand, suffocate. ",
                context).InteractionMessage;

        HasDocked = true;
        context.AddPoints(5);

        // Floyd rides along, so move him before the player.
        var floyd = Repository.GetItem<Floyd>();
        if (floyd.IsSelected)
        {
            floyd.CurrentLocation?.RemoveItem(floyd);
            Repository.GetLocation<DockingBayTwo>().ItemPlacedHere(floyd);
        }

        Repository.GetItem<CopilotSeat>().OccupiedByFloyd = false;
        Repository.GetItem<PilotSeat>().OccupiedByFloyd = false;
        SubLocation = null;

        context.CurrentLocation = Repository.GetLocation<DockingBayTwo>();
        Repository.GetItem<SpacetruckHatch>().IsOpen = true;

        var arrival =
            "The maneuvering thrusters kick on, nudging you toward the station. A recorded voice says, " +
            "\"Docking bay one is occupied. Defaulting to bay two.\" \n\n" +
            "The truck glides into the docking bay and your stomach flips as the bay's arti-grav field " +
            "comes on. The truck settles the last few centimeters to the floor, the bay floods with " +
            "air, and a voice whispers, \"Stationfall.\" Through the viewport you see no one at all " +
            "waiting to meet you. Odd. \n\n";

        // Assigning CurrentLocation fires none of the enter hooks, so describe the new room explicitly.
        return arrival + await new LookProcessor().Process(string.Empty, context, client, Runtime.Unknown);
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        var hatch = Repository.GetItem<SpacetruckHatch>();

        return "This is a twelve-meter rig, the largest Class Three spacecraft built, with seats for a " +
               "pilot and a copilot. Below the viewport are a space-band radio, a red button for the " +
               "emergency beacon, and a slot and keypad for the autopilot. The hatch is " +
               $"{(hatch.IsOpen ? "open" : "closed")}, and the cargo section behind you is empty. ";
    }

    public override void Init()
    {
        StartWithItem<SpacetruckSlot>();
        StartWithItem<PilotSeat>();
        StartWithItem<CopilotSeat>();
        StartWithItem<SurvivalKit>();
    }
}

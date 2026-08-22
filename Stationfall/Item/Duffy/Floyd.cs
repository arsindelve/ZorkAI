using Model.AIGeneration;

namespace Stationfall.Item.Duffy;

/// <summary>
///     Floyd, waiting in bin three (ship.zil FLOYD-F, DESCRIBE-FLOYD, interrupts.zil I-FLOYD). He is the
///     only right answer at the selection keypad, and the game does not tell you so — it just lets him
///     campaign for the job while the other two stand there.
///     TODO: Floyd's AI-driven conversation, and the rest of his behaviour beyond the ship, come with
///     the station. This is the Duffy-scoped Floyd: he can be recognised, chosen, talked to, and
///     followed around, and he reacts to the couple of things aboard worth reacting to.
/// </summary>
public class Floyd : ShipRobot
{
    /// <summary>
    ///     One-shot: he only strains to see over the dashboard once (interrupts.zil PILOT-SEAT-COMMENT).
    /// </summary>
    [UsedImplicitly]
    public bool HasComplainedAboutTheSeat { get; set; }

    public override int BinNumber => 3;

    // Deliberately does NOT claim the bare noun "robot", though the original's Floyd does. The
    // conversation handler treats any of a character's nouns appearing anywhere in the input as
    // addressing them, so a Floyd who answered to "robot" hijacked "put robot use authorization form
    // in slot" - a required command - into a conversation. Rex and Helen were already qualified;
    // Floyd now matches the same way, and the bare word is disambiguated by the room.
    public override string[] NounsForMatching =>
        ["floyd", "multiple purpose robot", "short robot", "third robot"];

    public override string InTheBinDescription => HasBeenSeen
        ? "Bin number three holds a short robot with a hopeful expression stencilled more or less " +
          "permanently onto his face. He notices you looking and waves both arms. "
        : "You can't get a good look at the robot in the third bin — he's hunched in the corner with " +
          "his back to you, apparently playing marbles. ";

    /// <summary>
    ///     How he reads depends entirely on where he stands in the selection: hopeful, chosen, or
    ///     passed over. The dejected case is the one worth getting right.
    /// </summary>
    public override string ExaminationDescription
    {
        get
        {
            if (!HasBeenSeen)
                return InTheBinDescription;

            if (Repository.GetLocation<RobotPool>() == CurrentLocation && !IsSelected)
                return AnyRobotPicked
                    ? "Floyd sits down in bin three and looks at his feet. "
                    : "Floyd is hopping about in bin number three with entirely unconcealed hope. ";

            return "Floyd is a squat, multiple-purpose robot in a scuffed boron-titanium finish, with " +
                   "an expression of relentless good cheer. He is, by a wide margin, the friendliest " +
                   "thing aboard this ship. ";
        }
    }

    protected override string GreetingResponse => "\"Hi!\" Floyd grins and bounces on the spot. ";

    protected override string FollowResponse => "\"Okay!\" ";

    protected override string CatchAllResponse =>
        "\"Enough talking!\" says Floyd. \"Let's play Hider-and-Seeker!\" ";

    /// <summary>
    ///     The opening's three Floyd beats: being recognised, campaigning for the job, and — once you
    ///     are both strapped in and moving — discovering that the seat does not fit him.
    /// </summary>
    public override async Task<string> Act(IContext context, IGenerationClient client)
    {
        if (context.CurrentLocation == CurrentLocation)
        {
            if (!HasBeenSeen)
            {
                HasBeenSeen = true;

                return "\nThe third robot looks up from his marbles, scrambles to his feet and starts " +
                       "waving both arms at once. It's Floyd — your companion from Resida. You've seen " +
                       "him only a handful of times since he took his own assignment with the Patrol, " +
                       "five years ago now, and he does not appear to have changed in any respect " +
                       "whatsoever. ";
            }

            if (!IsSelected && !AnyRobotPicked)
                return "\nFloyd jumps up and down. \"Oh boy oh boy oh boy pick Floyd pick Floyd pick " +
                       "Floyd!\" ";

            if (!HasComplainedAboutTheSeat && IsSelected &&
                context.CurrentLocation is Spacetruck { IsInFlight: true } &&
                context.CurrentLocation.SubLocation is SeatBase)
            {
                HasComplainedAboutTheSeat = true;

                return "\nFloyd strains to see over the top of the dashboard. \"Boy, seats are low! " +
                       "Floyd could sure use a phone book!\" ";
            }
        }

        return await base.Act(context, client);
    }

    protected override string FollowThePlayer(IContext context)
    {
        MoveToPlayer(context);
        return "Floyd skips along behind you, humming tunelessly. ";
    }
}

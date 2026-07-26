using GameEngine;
using JetBrains.Annotations;
using Planetfall.Item.Computer;
using Planetfall.Item.Kalamontee.Mech;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Item.Lawanda;
using Planetfall.Item.Lawanda.Lab;
using Planetfall.Location.Computer;
using Planetfall.Location.Kalamontee.Admin;
using Planetfall.Location.Lawanda;
using Planetfall.Location.Lawanda.Lab;

namespace Planetfall.Tests.Walkthrough;

// Soft-locks: nothing here kills you. The game keeps answering, the clocks keep running, and the
// ending is already decided. These are the states the hint system is meant to detect and say
// "restore an earlier save" about rather than hint toward (see Docs/hints/planetfall/03-softlock.md).
//
// Each fixture reaches the state and then demonstrates it, by walking to the door that no longer
// opens and showing that it does not open.

/// <summary>
///     Floyd fetches the miniaturization card out of the bio lab and dies doing it. That is the
///     intended, authored sequence. What is NOT recoverable is Floyd dying in there without the card:
///     he is the only thing in the game that can survive that room, and the card is the only way into
///     the computer.
/// </summary>
[TestFixture]
public sealed class WalkthroughLetFloydDieWithoutTheCard : WalkthroughTestBase
{
    [UsedImplicitly]
    public void AtTheBioLockWithFloyd()
    {
        StartHere<BioLockWest>();

        var bioLockEast = Repository.GetLocation<BioLockEast>();
        bioLockEast.StateMachine.HasWaitedOneTurnInBioLockEast = false;
        bioLockEast.StateMachine.FloydHasSaidLookAMiniCard = false;
        bioLockEast.StateMachine.FloydHasSaidNeedToGetCard = false;
        bioLockEast.StateMachine.FloydWaitingForDoorOpenCount = 0;
        bioLockEast.StateMachine.LabSequenceState = BioLockStateMachineManager.FloydLabSequenceState.NotStarted;
        bioLockEast.StateMachine.FloydFightingTurnCount = 0;
        bioLockEast.StateMachine.TurnsAfterFloydRushedIn = 0;
        bioLockEast.StateMachine.TurnsAfterDoorReopened = 0;

        Repository.GetItem<BioLockInnerDoor>().IsOpen = false;

        var floyd = Repository.GetItem<Floyd>();
        bioLockEast.ItemPlacedHere(floyd);
        floyd.IsOn = true;

        Repository.GetLocation<ComputerRoom>().FloydHasExpressedConcern = true;
    }

    [Test]
    [TestCase("e", "AtTheBioLockWithFloyd", "Bio Lock East")]
    [TestCase("z", null, "Floyd stands on his tiptoes", "We'll need card there to fix computer", "Floyd will get card")]
    [TestCase("open door", null, "plunges into the Bio Lab", "Floyd shrieks and yells to you to close the door")]
    [TestCase("close door", null, "The door closes", "And not a moment too soon")]
    // Three fast knocks and the sound of tearing metal: Floyd has the card and is at the door. The
    // next turn is the one that matters, and this walkthrough spends it waiting.
    [TestCase("z", null, "three fast knocks", "the distinctive sound of tearing metal")]
    [TestCase("z", null,
        "a final metallic scream from behind the door",
        "the sound of Floyd's body being torn apart",
        "Floyd is dead")]
    // He died on the wrong side of the door, and the card died with him. Opening it now just lets the
    // mutations out, so the only thing left to do is leave. Everything below this line is the game
    // politely refusing to end.
    [TestCase("w", null, "Bio Lock West")]
    [TestCase("open door", null, "The door opens")]
    [TestCase("w", null, "Main Lab")]
    [TestCase("w", null, "Project Corridor East")]
    [TestCase("s", null, "Computer Room", "The only sign of activity is a glowing red light")]
    [TestCase("s", null, "Miniaturization Booth")]
    // Without the card the booth is a cupboard, and the computer stays broken for good.
    [TestCase("type 384", null, "Internal computer repair booth not activated")]
    public async Task Walkthrough(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);
}

/// <summary>
///     The laser's shipped battery has three to five shots in it, which is why there is a fresh one in
///     Lab Storage. Spend the old one on target practice and the speck - the single thing the laser
///     exists to destroy - can never be burned away, so the computer is never repaired.
/// </summary>
[TestFixture]
public sealed class WalkthroughRunTheLaserBatteryFlatBeforeTheSpeck : WalkthroughTestBase
{
    [UsedImplicitly]
    public void OnTheStripWithATiredBattery()
    {
        StartHere<StripNearRelay>();

        var laser = Repository.GetItem<Laser>();
        laser.Items.Clear();

        // One shot left in the old battery, and the fresh one still sitting in Lab Storage where it
        // would have been picked up on the way past.
        var battery = Repository.GetItem<OldBattery>();
        battery.ChargesRemaining = 1;
        laser.ItemPlacedHere(battery);
        Take<Laser>();
    }

    [Test]
    [TestCase("set laser to 1", "OnTheStripWithATiredBattery", "The dial is now set to 1.")]
    [TestCase("shoot speck with laser", null, "The speck is hit by the beam! It sizzles a little, but isn't destroyed yet.")]
    // That was the last charge. There is no second speck and no second laser.
    [TestCase("shoot speck with laser", null, "Click.")]
    [TestCase("shoot speck with laser", null, "Click.")]
    public async Task Walkthrough(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);
}

/// <summary>
///     A softer soft-lock, and worth telling apart from the others: losing the good bedistor costs you
///     Course Control, which is optional. You can still finish the game - you just cannot save the
///     planet's orbit while you do it, and the ending says so.
/// </summary>
[TestFixture]
public sealed class WalkthroughLoseTheGoodBedistorInTheRift : WalkthroughTestBase
{
    [UsedImplicitly]
    public void AtTheRiftWithTheBedistor()
    {
        StartHere<AdminCorridor>();
        Take<GoodBedistor>();
    }

    /// <summary>
    ///     Skips the shuttle ride back to Lawanda - it is a puzzle in its own right and
    ///     WalkthroughCrashTheShuttleIntoTheStation already plays it out. What matters here is what
    ///     Course Control says once the part is gone.
    /// </summary>
    [UsedImplicitly]
    public void TravelBackToCourseControl()
    {
        StartHere<CourseControl>();
    }

    [Test]
    [TestCase("examine bedistor", "AtTheRiftWithTheBedistor", "bedistor")]
    [TestCase("throw bedistor into rift", null, "sails gracefully into the rift")]
    // The fused one is still in the socket at Course Control and there is no second spare, so the
    // orbit stays uncorrected however the rest of the game goes.
    [TestCase("look", "TravelBackToCourseControl",
        "Course Control",
        "Bedistur Faalyur!",
        "Kritikul diivurjins frum pland kors")]
    [TestCase("open cube", null, "The lid swings open", "fused")]
    public async Task Walkthrough(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);
}

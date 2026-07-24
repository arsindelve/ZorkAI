using FluentAssertions;
using GameEngine;
using Stationfall.Location.Duffy;

namespace Stationfall.Tests.Walkthrough;

/// <summary>
///     The documented ship-departure walkthrough, executed verbatim.
///     This is the oracle for the Duffy: every command below is exactly what
///     Stationfall/Walkthroughs/Walkthrough-ShipDeparture.md tells a player to type, in the same order.
///     If a phrasing in the document stops working, this fixture fails — which is the whole point.
/// </summary>
[TestFixture]
public class WalkthroughShipDeparture : WalkthroughTestBase
{
    /// <summary>
    ///     Chronometer reading pinned before the course is typed. 4600 yields course 503 via
    ///     ((T/50 - 132)^2 / 4) + 103 -- deliberately not the formula's flat 103 floor.
    /// </summary>
    public const int PinnedTime = 4600;

    [Test]
    [TestCase("east", null, "Cargo Bay Entrance")]
    [TestCase("north", null, "Robot Pool")]
    [TestCase("put robot use authorization form in slot", null, "Authorization approved")]
    [TestCase("type 3", null, "Yippee")]
    [TestCase("south", null, "Cargo Bay Entrance")]
    [TestCase("east", null, "Cargo Bay")]
    [TestCase("open hatch", null, "open")]
    [TestCase("enter truck", null, "Spacetruck")]
    [TestCase("close hatch", null, "shut")]
    [TestCase("read time", null, "Galactic Standard Time")]
    [TestCase("enter pilot seat", null, "pilot seat", "Floyd clambers into the copilot seat")]
    [TestCase("put class three activation form in slot", null, "Spacecraft activated")]
    [TestCase("type 503", "PinChronometer", "Course set")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "Time passes")]
    [TestCase("wait", null, "engines begin to whine")]
    [TestCase("wait", null, "roars out of the cargo bay")]
    [TestCase("wait", null, "three-quarters")]
    [TestCase("wait", null, "braking rockets")]
    [TestCase("wait", null, "one-quarter")]
    [TestCase("wait", null, "space station")]
    // Docking leaves you aboard the truck; you walk out to the bay yourself (ship.zil:1023-1032).
    [TestCase("wait", null, "Defaulting to bay two", "Stationfall")]
    [TestCase("get up", null, "stand")]
    [TestCase("take kit", null, "Taken")]
    [TestCase("open hatch", null, "open")]
    [TestCase("exit", null, "Docking Bay #2")]
    // The walkthrough's stated outcome: arrival at bay two is worth the game's first five points.
    // Asserted as a final row rather than a separate test so it can't run out of order.
    [TestCase("score", null, "would be 5", "out of 80 points")]
    public async Task Walkthrough(string input, string? setup, params string[] expectedResponses)
    {
        if (!string.IsNullOrWhiteSpace(setup))
            InvokeSetup(setup);

        await Do(input, expectedResponses);

        // Once docked, confirm the checkpoints the walkthrough document names.
        if (input == "exit")
        {
            Ctx.CurrentLocation.Should().BeOfType<DockingBayTwo>();
            Repository.GetLocation<Spacetruck>().HasDocked.Should().BeTrue();
        }
    }
}

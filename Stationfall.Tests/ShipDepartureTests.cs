using FluentAssertions;
using GameEngine;
using Stationfall.Item.Duffy;
using Stationfall.Location.Duffy;

namespace Stationfall.Tests;

/// <summary>
///     End-to-end coverage of the S.P.S. Duffy — the game's opening segment, from Deck Twelve to
///     docking at the space station. Encodes Walkthroughs/Walkthrough-ShipDeparture.md.
/// </summary>
[TestFixture]
public class ShipDepartureTests : EngineTestsBase
{
    /// <summary>
    ///     A chronometer reading chosen so the course formula lands on a tidy number:
    ///     6600/50 = 132; 132-132 = 0; 0 squared = 0; 0/4 = 0; 0+103 = 103.
    /// </summary>
    private const int PinnedTime = 6600;

    private const int ExpectedCourse = 103;

    [Test]
    public void CourseFormula_MatchesTheOriginalIntegerArithmetic()
    {
        // Verified against verbs.zil SPACETRUCK-TYPE: ((T/50 - 132)^2 / 4) + 103, integer math.
        // 6600/50=132; 132-132=0; 0*0=0; 0/4=0; 0+103
        Spacetruck.CalculateCorrectCourse(6600).Should().Be(103);
        // 6650/50=133; 133-132=1; 1*1=1; 1/4=0 (integer division); 0+103
        Spacetruck.CalculateCorrectCourse(6650).Should().Be(103);
        // 5000/50=100; 100-132=-32; 1024; 1024/4=256; 256+103
        Spacetruck.CalculateCorrectCourse(5000).Should().Be(359);
        // 7000/50=140; 140-132=8; 64; 64/4=16; 16+103
        Spacetruck.CalculateCorrectCourse(7000).Should().Be(119);
    }

    [Test]
    public async Task StartingInventory_HoldsTheThreeFormsAndWornKit()
    {
        var engine = GetTarget();

        var response = await engine.GetResponse("inventory");

        response.Should().Contain("Assignment Completion");
        response.Should().Contain("Robot Use Authorization");
        response.Should().Contain("Class Three Spacecraft Activation");
        response.Should().Contain("chronometer");
        response.Should().Contain("Patrol uniform");
    }

    [Test]
    public async Task DeckTwelve_WestDoor_NeverOpens()
    {
        var engine = GetTarget();

        var response = await engine.GetResponse("west");

        response.Should().Contain("door is closed");
        engine.Context.CurrentLocation.Should().BeOfType<DeckTwelve>();
    }

    [Test]
    public async Task DeckTwelveSlot_RejectsTheAssignmentForm_AsUnvalidated()
    {
        var engine = GetTarget();

        var response = await engine.GetResponse("put assignment in slot");

        response.Should().Contain("not been validated");
    }

    [Test]
    public async Task RobotPoolKeypad_IsInert_UntilTheFormIsInserted()
    {
        var engine = GetTarget();
        await engine.GetResponse("east");
        await engine.GetResponse("north");

        var response = await engine.GetResponse("type 3");

        response.Should().Contain("only following authorization");
        Repository.GetItem<Floyd>().IsSelected.Should().BeFalse();
    }

    [Test]
    public async Task InsertingTheAuthorizationForm_UnlocksTheKeypad()
    {
        var engine = GetTarget();
        await engine.GetResponse("east");
        await engine.GetResponse("north");

        var response = await engine.GetResponse("put authorization in slot");

        response.Should().Contain("Authorization approved");
        Repository.GetLocation<RobotPool>().IsAuthorized.Should().BeTrue();
    }

    [Test]
    public async Task TypingThree_SelectsFloyd()
    {
        var engine = GetTarget();
        await AuthorizeRobotPool(engine);

        var response = await engine.GetResponse("type 3");

        response.Should().Contain("Yippee");
        Repository.GetItem<Floyd>().IsSelected.Should().BeTrue();
    }

    [Test]
    public async Task TypingAnUnoccupiedBin_IsRefused()
    {
        var engine = GetTarget();
        await AuthorizeRobotPool(engine);

        var response = await engine.GetResponse("type 7");

        response.Should().Contain("bin is unoccupied");
        Repository.GetItem<Floyd>().IsSelected.Should().BeFalse();
    }

    [Test]
    public async Task SelectingARobotTwice_IsRefused()
    {
        var engine = GetTarget();
        await AuthorizeRobotPool(engine);
        await engine.GetResponse("type 3");

        var response = await engine.GetResponse("type 1");

        response.Should().Contain("already made your selection");
        Repository.GetItem<Rex>().IsSelected.Should().BeFalse();
    }

    [Test]
    public async Task PickingRex_GetsYouCrushed_AtTheCargoBayEntrance()
    {
        var engine = GetTarget();
        await AuthorizeRobotPool(engine);
        await engine.GetResponse("type 1");

        // Rex follows you south, and forgets to stop.
        var response = await engine.GetResponse("south");

        response.Should().Contain("You have died");
    }

    [Test]
    public async Task Autopilot_RefusesActivation_WhenOnlyThePlayerIsSeated()
    {
        // Floyd is never selected here, so the copilot seat stays empty.
        var engine = GetTarget();
        await WalkToTruckWithoutFloyd(engine);
        await engine.GetResponse("sit in pilot seat");

        var response = await engine.GetResponse("put activation in slot");

        response.Should().Contain("both the pilot and copilot seats");
        Repository.GetLocation<Spacetruck>().IsActivated.Should().BeFalse();
    }

    [Test]
    public async Task Autopilot_RefusesCourse_BeforeActivation()
    {
        var engine = GetTarget();
        await WalkToTruckWithFloyd(engine);
        await engine.GetResponse("sit in pilot seat");

        var response = await engine.GetResponse("type 103");

        response.Should().Contain("only following authorization");
    }

    [Test]
    public async Task SittingDown_MakesFloydTakeTheOtherSeat()
    {
        var engine = GetTarget();
        await WalkToTruckWithFloyd(engine);

        var response = await engine.GetResponse("sit in pilot seat");

        response.Should().Contain("Floyd clambers into the copilot seat");
        Repository.GetItem<CopilotSeat>().OccupiedByFloyd.Should().BeTrue();
    }

    [Test]
    public async Task FullDeparture_DocksAtBayTwo_AndScoresFivePoints()
    {
        var engine = GetTarget();
        await WalkToTruckWithFloyd(engine);

        await engine.GetResponse("close hatch");
        await engine.GetResponse("sit in pilot seat");

        var activation = await engine.GetResponse("put activation in slot");
        activation.Should().Contain("Spacecraft activated");

        // Read the time, then type the heading derived from it.
        Repository.GetItem<Chronometer>().CurrentTime = PinnedTime;
        var course = await engine.GetResponse($"type {ExpectedCourse}");
        course.Should().Contain("Course set");

        var log = string.Empty;
        for (var turn = 0; turn < 14 && !Repository.GetLocation<Spacetruck>().HasDocked; turn++)
            log += await engine.GetResponse("wait");

        log.Should().Contain("Defaulting to bay two");
        log.Should().Contain("Stationfall");
        engine.Context.Score.Should().Be(5);

        // Docking leaves you aboard; the truck's exit now leads into the station
        // (SPACETRUCK-EXIT-F, ship.zil:1023-1032).
        engine.Context.CurrentLocation.Should().BeOfType<Spacetruck>();

        await engine.GetResponse("get up");
        await engine.GetResponse("open hatch");
        await engine.GetResponse("exit");

        engine.Context.CurrentLocation.Should().BeOfType<DockingBayTwo>();
    }

    [Test]
    public async Task LaunchingWithTheHatchOpen_IsFatal()
    {
        var engine = GetTarget();
        await WalkToTruckWithFloyd(engine);

        // Deliberately leave the hatch open.
        await engine.GetResponse("sit in pilot seat");
        await engine.GetResponse("put activation in slot");
        Repository.GetItem<Chronometer>().CurrentTime = PinnedTime;
        await engine.GetResponse($"type {ExpectedCourse}");

        var log = string.Empty;
        for (var turn = 0; turn < 14; turn++)
            log += await engine.GetResponse("wait");

        log.Should().Contain("You have died");
    }

    [Test]
    public async Task LaunchingWithAWrongCourse_SuffocatesYou()
    {
        var engine = GetTarget();
        await WalkToTruckWithFloyd(engine);

        await engine.GetResponse("close hatch");
        await engine.GetResponse("sit in pilot seat");
        await engine.GetResponse("put activation in slot");

        Repository.GetItem<Chronometer>().CurrentTime = PinnedTime;
        await engine.GetResponse($"type {ExpectedCourse + 1}");

        var log = string.Empty;
        for (var turn = 0; turn < 14; turn++)
            log += await engine.GetResponse("wait");

        log.Should().Contain("no station");
        log.Should().Contain("You have died");
    }

    // --- helpers -------------------------------------------------------------------------------

    private static async Task AuthorizeRobotPool(GameEngine<StationfallGame, StationfallContext> engine)
    {
        await engine.GetResponse("east");
        await engine.GetResponse("north");
        await engine.GetResponse("put authorization in slot");
    }

    /// <summary>
    ///     Deck Twelve → Robot Pool, pick Floyd, then on to the truck with the hatch open.
    /// </summary>
    private static async Task WalkToTruckWithFloyd(GameEngine<StationfallGame, StationfallContext> engine)
    {
        await AuthorizeRobotPool(engine);
        await engine.GetResponse("type 3");
        await engine.GetResponse("south");
        await engine.GetResponse("east");
        await engine.GetResponse("open hatch");
        await engine.GetResponse("in");
    }

    private static async Task WalkToTruckWithoutFloyd(GameEngine<StationfallGame, StationfallContext> engine)
    {
        await engine.GetResponse("east");
        await engine.GetResponse("east");
        await engine.GetResponse("open hatch");
        await engine.GetResponse("in");
    }
}

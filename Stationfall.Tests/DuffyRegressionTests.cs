using FluentAssertions;
using GameEngine;
using Stationfall.Item.Duffy;
using Stationfall.Location.Duffy;

namespace Stationfall.Tests;

/// <summary>
///     Regression coverage for defects found by review of the Duffy implementation. Each test pins a
///     behavior that was previously wrong — several of them game-breaking.
/// </summary>
[TestFixture]
public class DuffyRegressionTests : EngineTestsBase
{
    private const int PinnedTime = 4600;
    private const int CorrectCourse = 503;

    [Test]
    public async Task WanderingOffBeforeLiftoff_IsFatal_RatherThanStrandingThePlayer()
    {
        // Previously the truck "departed without you" and left the player alive in a truck that was
        // permanently in flight, with no exits and no actor — a hard softlock. In the original the
        // only survivable outcome at liftoff is being strapped into a seat (ship.zil:1158-1174).
        var engine = GetTarget();
        await ArmTheAutopilot(engine);

        // Collect everything from here on: liftoff can land on any of these turns, and once the player
        // dies the game restarts, so a later wait loop would see only the fresh game.
        var log = await engine.GetResponse("get up");
        log += await engine.GetResponse("open hatch");
        log += await engine.GetResponse("out");
        log += await engine.GetResponse("west");

        for (var turn = 0; turn < 8; turn++)
            log += await engine.GetResponse("wait");

        log.Should().Contain("You have died");
    }

    [Test]
    public async Task LeavingTheTruck_VacatesTheSeat()
    {
        // A stale seat both satisfied the launch interlock for a standing player and dodged the
        // failed-to-strap-in death.
        var engine = GetTarget();
        await BoardTheTruckWithFloyd(engine);
        await engine.GetResponse("sit in pilot seat");

        await engine.GetResponse("out");

        Repository.GetLocation<Spacetruck>().SubLocation.Should().BeNull();
    }

    [Test]
    public async Task GivingARobotAForm_LeavesItRecoverable()
    {
        // The robots are containers. Previously they were opaque, so a form handed over vanished from
        // inventory, examine and take — quietly making the game unwinnable.
        var engine = GetTarget();
        await SelectFloyd(engine);

        await engine.GetResponse("put activation in floyd");
        var response = await engine.GetResponse("take activation");

        response.Should().NotBeNullOrWhiteSpace();
        Repository.GetItem<ClassThreeSpacecraftActivationForm>().Consumed.Should().BeFalse();
        engine.Context.Items.Should().Contain(Repository.GetItem<ClassThreeSpacecraftActivationForm>());
    }

    [TestCase("close hatch")]
    [TestCase("shut hatch")]
    [TestCase("shut the door")]
    [TestCase("close spacetruck hatch")]
    [TestCase("close truck hatch")]
    public async Task TheHatchCanBeShutFromInsideTheTruck_ByAnyPhrasingItAdvertises(string command)
    {
        // Only four exact strings used to work, and every miss was silently fatal at liftoff.
        var engine = GetTarget();
        await BoardTheTruckWithFloyd(engine);

        await engine.GetResponse(command);

        Repository.GetItem<SpacetruckHatch>().IsOpen.Should().BeFalse($"\"{command}\" should shut it");
    }

    [Test]
    public async Task ExaminingTheHatchFromInsideTheTruck_DescribesIt()
    {
        var engine = GetTarget();
        await BoardTheTruckWithFloyd(engine);

        var response = await engine.GetResponse("examine hatch");

        response.Should().Contain("hatch");
    }

    [Test]
    public async Task TypingZeroAsACourse_IsRefused_AndDoesNotStallTheLaunch()
    {
        // 0 doubles as the "no course yet" sentinel, so accepting it froze the countdown forever.
        var engine = GetTarget();
        await ArmTheAutopilot(engine, typeCourse: false);

        var response = await engine.GetResponse("type 0");

        response.Should().Contain("Error");
        Repository.GetLocation<Spacetruck>().CoursePicked.Should().Be(0);

        // The autopilot still accepts a real heading afterwards.
        Repository.GetItem<Chronometer>().CurrentTime = PinnedTime;
        var second = await engine.GetResponse($"type {CorrectCourse}");
        second.Should().Contain("Course set");
    }

    [Test]
    public async Task HelenShredsAFormSheIsGiven()
    {
        var engine = GetTarget();
        await engine.GetResponse("east");
        await engine.GetResponse("north");
        await engine.GetResponse("put authorization in slot");
        await engine.GetResponse("type 2");

        var response = await engine.GetResponse("give activation to helen");

        response.Should().Contain("confetti");
        engine.Context.Items.Should().NotContain(Repository.GetItem<ClassThreeSpacecraftActivationForm>());
    }

    [Test]
    public async Task TheSlotDoesNotNarrateHandlingAFormYouAreNotHolding()
    {
        // Matching on the command text rather than on state made the slot describe physically taking
        // and returning a form the player had left in another room.
        var engine = GetTarget();
        await engine.GetResponse("drop assignment");

        await engine.GetResponse("east");
        var response = await engine.GetResponse("put assignment in slot");

        response.Should().NotContain("pushes it back out");
    }

    [Test]
    public async Task TheChronometerGivesNoReadingWhenItIsNotWorn()
    {
        var engine = GetTarget();

        await engine.GetResponse("remove chronometer");
        var response = await engine.GetResponse("read chronometer");

        response.Should().NotContain("Galactic Standard Time");
    }

    [Test]
    public async Task TheEmergencyBeaconOnlyTransmitsOnceDocked()
    {
        var engine = GetTarget();

        await BoardTheTruckWithFloyd(engine);
        var beforeLaunch = await engine.GetResponse("press red button");
        beforeLaunch.Should().Contain("court-martial");

        await engine.GetResponse("close hatch");
        await engine.GetResponse("sit in pilot seat");
        await engine.GetResponse("put activation in slot");
        Repository.GetItem<Chronometer>().CurrentTime = PinnedTime;
        await engine.GetResponse($"type {CorrectCourse}");

        for (var turn = 0; turn < 14 && !Repository.GetLocation<Spacetruck>().HasDocked; turn++)
            await engine.GetResponse("wait");

        var afterDocking = await engine.GetResponse("press red button");
        afterDocking.Should().Contain("Nothing can go wrong");
    }

    [Test]
    public void TheStartingClockIsRandomized()
    {
        // A fixed opening time would give every game the same heading for a given turn count.
        var readings = new HashSet<int>();

        for (var attempt = 0; attempt < 40; attempt++)
        {
            Repository.Reset();
            readings.Add(Repository.GetItem<Chronometer>().CurrentTime);
        }

        readings.Count.Should().BeGreaterThan(1);
        readings.Should().OnlyContain(t => t >= 4431 && t <= 5650);
    }

    // --- helpers -------------------------------------------------------------------------------

    private static async Task SelectFloyd(GameEngine<StationfallGame, StationfallContext> engine)
    {
        await engine.GetResponse("east");
        await engine.GetResponse("north");
        await engine.GetResponse("put authorization in slot");
        await engine.GetResponse("type 3");
    }

    private static async Task BoardTheTruckWithFloyd(GameEngine<StationfallGame, StationfallContext> engine)
    {
        await SelectFloyd(engine);
        await engine.GetResponse("south");
        await engine.GetResponse("east");
        await engine.GetResponse("open hatch");
        await engine.GetResponse("in");
    }

    /// <summary>
    ///     Boards, seals, seats, and activates — optionally stopping just short of typing the heading.
    /// </summary>
    private static async Task ArmTheAutopilot(GameEngine<StationfallGame, StationfallContext> engine,
        bool typeCourse = true)
    {
        await BoardTheTruckWithFloyd(engine);
        await engine.GetResponse("close hatch");
        await engine.GetResponse("sit in pilot seat");
        await engine.GetResponse("put activation in slot");

        if (!typeCourse)
            return;

        Repository.GetItem<Chronometer>().CurrentTime = PinnedTime;
        await engine.GetResponse($"type {CorrectCourse}");
    }
}

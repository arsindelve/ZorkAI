using FluentAssertions;
using Planetfall.GlobalCommand;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Location.Feinstein;
using Planetfall.Location.Kalamontee.Mech;

namespace Planetfall.Tests;

/// <summary>
/// Issue #552. Mentioning Floyd used to fall through to the AI narrator, which improvised a
/// different joke every time — including tonally wrong ones after his death. These are the canned,
/// deterministic replacements, split by whether the player has met Floyd yet.
/// </summary>
public class FloydMentionTests : EngineTestsBase
{
    private const string FourthWall = "There's nobody here by that name";

    /// <summary>
    /// Part 1: before the player has any in-game way of knowing the name, ANY mention of "floyd"
    /// gets the fourth-wall wink — not just the where-is phrasings.
    /// </summary>
    [TestFixture]
    public class BeforeMeetingFloyd : FloydMentionTests
    {
        [TestCase("where is floyd")]
        [TestCase("where's floyd")]
        [TestCase("examine floyd")]
        [TestCase("take floyd")]
        [TestCase("floyd, hello")]
        [TestCase("ask floyd about lazarus")]
        public async Task AnyMention_GetsTheFourthWallLine(string input)
        {
            var target = GetTarget();
            StartHere<DeckNine>();

            var response = await target.GetResponse(input);

            response.Should().Contain("Floyd? There's nobody here by that name. " +
                                      "Someone's played Planetfall before, haven't they?");
        }

        [Test]
        public async Task MentionInTheRobotShop_GetsTheFourthWallLine()
        {
            var target = GetTarget();
            StartHere<RobotShop>();

            var response = await target.GetResponse("search floyd");

            response.Should().Contain(FourthWall);
        }

        // CI-critical: "activate floyd" is the walkthrough spine, typed before any met-flag flips.
        // If the intercept swallowed it, Floyd would be unactivatable by name.
        [Test]
        public async Task ActivateFloyd_StillActivatesHim()
        {
            var target = GetTarget();
            StartHere<RobotShop>();

            var response = await target.GetResponse("activate floyd");

            response.Should().NotContain(FourthWall);
            response.Should().Contain("Nothing happens");
            GetItem<Floyd>().HasAwardedActivationPoints.Should().BeTrue();
            Context.Score.Should().Be(2);
        }

        // The other activation phrasings are asserted against the matcher rather than end-to-end:
        // the test harness's stub parser has no mapping for them (it takes the second word as the
        // noun, so "turn on floyd" resolves to nothing), which would make an end-to-end assertion
        // pass for the wrong reason. What matters here is that the intercept does not claim them.
        [TestCase("turn on floyd")]
        [TestCase("start floyd")]
        [TestCase("switch on floyd")]
        public void OtherActivationPhrasings_AreNotClaimed(string input)
        {
            GetTarget();

            new PlanetfallGlobalCommandFactory().GetGlobalCommands(input).Should().BeNull();
        }

        // Only the NAME is intercepted. The deactivated-robot description is part of the intended
        // discovery flow, so the nouns the player CAN know still behave normally.
        [TestCase("examine robot")]
        [TestCase("examine B-19-7")]
        public async Task OtherNounsForFloyd_AreUnchanged(string input)
        {
            var target = GetTarget();
            StartHere<RobotShop>();

            var response = await target.GetResponse(input);

            response.Should().NotContain(FourthWall);
            response.Should().Contain("The deactivated robot is leaning against the wall");
        }

        // God mode is matched by the ENGINE's command factory, which is consulted before the
        // game-specific one - so the debugging commands that name Floyd keep working even while the
        // intercept is armed.
        [Test]
        public async Task GodModeCommandsNamingFloyd_AreNotIntercepted()
        {
            var target = GetTarget();
            StartHere<RobotShop>();

            var response = await target.GetResponse("god mode where floyd");

            response.Should().NotContain(FourthWall);
            response.Should().Contain("Robot Shop");
        }

        // The gate can't be "!IsOn && !HasEverBeenOn" alone: neither flag flips until Floyd finishes
        // his 3-turn wake-up, so the joke would fire while the robot is visibly booting.
        [Test]
        public async Task DuringTheWakeUpCountdown_NoFourthWallLine()
        {
            var target = GetTarget();
            StartHere<RobotShop>();

            await target.GetResponse("activate floyd");
            var floyd = GetItem<Floyd>();
            floyd.IsOn.Should().BeFalse();
            floyd.HasEverBeenOn.Should().BeFalse();

            var response = await target.GetResponse("examine floyd");

            response.Should().NotContain(FourthWall);
            response.Should().Contain("The deactivated robot is leaning against the wall");
        }
    }

    /// <summary>
    /// Part 2: once the player knows him, only the where-is-shaped queries are claimed, and the
    /// answer is keyed off Floyd's actual state.
    /// </summary>
    [TestFixture]
    public class AfterMeetingFloyd : FloydMentionTests
    {
        [TestCase("where is floyd")]
        [TestCase("where's floyd")]
        [TestCase("find floyd")]
        [TestCase("where did floyd go")]
        public async Task InTheRoomAndOn_FloydIsAlwaysRightHere(string input)
        {
            var target = GetTarget();
            StartHere<RobotShop>();
            var floyd = GetItem<Floyd>();
            floyd.IsOn = true;
            floyd.HasEverBeenOn = true;

            var response = await target.GetResponse(input);

            response.Should().Contain("Floyd is right here! Floyd is always right here.");
        }

        [Test]
        public async Task InTheRoomButSwitchedOff_SlumpedWhereYouLeftHim()
        {
            var target = GetTarget();
            StartHere<RobotShop>();
            var floyd = GetItem<Floyd>();
            floyd.IsOn = false;
            floyd.HasEverBeenOn = true;

            var response = await target.GetResponse("where is floyd");

            response.Should().Contain("Floyd is right here, slumped where you left him.");
        }

        [Test]
        public async Task Absent_IsOffExploring()
        {
            var target = GetTarget();
            StartHere<RobotShop>();
            var floyd = GetItem<Floyd>();
            floyd.IsOn = true;
            floyd.HasEverBeenOn = true;
            GetLocation<MachineShop>().ItemPlacedHere(floyd);

            var response = await target.GetResponse("where is floyd");

            response.Should().Contain("Floyd is off exploring somewhere. He'll turn up.");
        }

        // #545 item 3: the narrator used to crack jokes about the dead.
        [Test]
        public async Task Dead_GetsTheGriefRegisterLine()
        {
            var target = GetTarget();
            StartHere<RobotShop>();
            var floyd = GetItem<Floyd>();
            floyd.HasEverBeenOn = true;
            floyd.HasDied = true;

            var response = await target.GetResponse("where is floyd");

            response.Should().Contain("Floyd is gone.");
            response.Should().NotContain(FourthWall);
        }

        // Part 2 claims the where-is queries ONLY. Everything else keeps its existing behavior.
        [Test]
        public async Task ExamineFloyd_IsNotClaimed()
        {
            var target = GetTarget();
            StartHere<RobotShop>();
            var floyd = GetItem<Floyd>();
            floyd.IsOn = true;
            floyd.HasEverBeenOn = true;

            var response = await target.GetResponse("examine floyd");

            response.Should().Contain("From its design, the robot seems to be of the multi-purpose sort");
            response.Should().NotContain("right here");
        }

        // A conversational line that merely contains the word "where" must still reach Floyd rather
        // than being answered by the canned locator.
        [Test]
        public async Task AskingFloydAboutSomethingElse_IsNotClaimed()
        {
            var target = GetTarget();
            StartHere<RobotShop>();
            var floyd = GetItem<Floyd>();
            floyd.IsOn = true;
            floyd.HasEverBeenOn = true;

            var response = await target.GetResponse("floyd, do you know where the card is");

            response.Should().NotContain("Floyd is right here");
        }
    }
}

using FluentAssertions;
using Planetfall.GlobalCommand;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Location.Feinstein;
using Planetfall.Location.Kalamontee.Mech;
using Planetfall.Location.Lawanda.Lab;

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

        // Bug: the exemption required the particle to be adjacent, so the separated-particle form
        // was swallowed. On main this phrasing scores the +2 and starts the countdown; the parser
        // normalizes "turn X on" to the verb "activate" (Model/ParsingHelper.cs), but the intercept
        // runs on RAW text and never sees that.
        [TestCase("turn floyd on")]
        [TestCase("switch floyd on")]
        [TestCase("power floyd up")]
        [TestCase("turn the floyd on")]
        public void SeparatedParticleActivation_IsNotClaimed(string input)
        {
            GetTarget();

            new PlanetfallGlobalCommandFactory().GetGlobalCommands(input).Should().BeNull();
        }

        // Bug: the exemption tested the WHOLE command for an activation word rather than the verb
        // governing "floyd", so any sentence containing "start" escaped the intercept entirely.
        [TestCase("floyd, let's start singing")]
        [TestCase("where can i start looking for floyd")]
        [TestCase("turn on the lamp and show it to floyd")]
        public async Task ActivationWordElsewhereInTheSentence_StillGetsTheFourthWallLine(string input)
        {
            var target = GetTarget();
            StartHere<DeckNine>();

            var response = await target.GetResponse(input);

            response.Should().Contain(FourthWall);
        }

        // Bug: the intercept matched "floyd" anywhere with no regard for who was being addressed,
        // so it ate speech aimed at the NPC the whole opening sequence is built around.
        [Test]
        public async Task AddressingAnotherPresentNpc_IsNotClaimed()
        {
            var target = GetTarget();
            StartHere<DeckNine>();

            var response = await target.GetResponse("blather, where is floyd");

            response.Should().NotContain(FourthWall);
        }

        // Bug: a parser-level refusal is not an in-world action and must not cost survival time.
        // On Deck Nine the explosion clock makes this lethal, not merely untidy.
        [Test]
        public async Task TheFourthWallLine_DoesNotCostATurn()
        {
            var target = GetTarget();
            StartHere<DeckNine>();
            var movesBefore = Context.Moves;
            var timeBefore = Context.CurrentTime;

            await target.GetResponse("where is floyd");

            Context.Moves.Should().Be(movesBefore);
            Context.CurrentTime.Should().Be(timeBefore);
        }

        // Noun scoping holds on the CONVERSATION path too, not just for examine: addressing the
        // pristine robot as "robot" still reaches Floyd.OnBeingTalkedTo and gets the turned-off
        // line. Worth pinning because the never-activated state (IsOn and HasEverBeenOn both false)
        // is a real, reachable state - the robot standing in the shop before you ever touch him -
        // and the name is the ONLY thing issue #552 takes away from the player there.
        [Test]
        public async Task AddressingThePristineRobotAsRobot_StillReachesHim()
        {
            var target = GetTarget();
            StartHere<RobotShop>();

            var response = await target.GetResponse("robot, are you okay");

            response.Should().NotContain(FourthWall);
            response.Should().Contain("appears to be turned off");
        }

        [Test]
        public async Task WhereIsTheRobot_BeforeMeetingHim_IsNotClaimed()
        {
            var target = GetTarget();
            StartHere<RobotShop>();

            var response = await target.GetResponse("where is the robot");

            response.Should().NotContain(FourthWall);
            response.Should().NotContain("Floyd is");
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

        // Bug: "floyd" was matched anywhere while only the leading where-word was anchored, so
        // questions about the things Floyd CARRIES were answered with Floyd's location.
        [TestCase("where is floyd's card")]
        [TestCase("where did floyd put the card")]
        [TestCase("where is the card floyd is holding")]
        [TestCase("find the card floyd took")]
        [TestCase("where is the survival kit, floyd")]
        public async Task WhereQuestionsAboutOtherThings_AreNotClaimed(string input)
        {
            var target = GetTarget();
            StartHere<RobotShop>();
            var floyd = GetItem<Floyd>();
            floyd.IsOn = true;
            floyd.HasEverBeenOn = true;

            var response = await target.GetResponse(input);

            response.Should().NotContain("Floyd is right here");
        }

        [Test]
        public async Task WhereIsFloyd_DoesNotCostATurn()
        {
            var target = GetTarget();
            StartHere<RobotShop>();
            var floyd = GetItem<Floyd>();
            floyd.IsOn = true;
            floyd.HasEverBeenOn = true;
            var movesBefore = Context.Moves;
            var timeBefore = Context.CurrentTime;

            await target.GetResponse("where is floyd");

            Context.Moves.Should().Be(movesBefore);
            Context.CurrentTime.Should().Be(timeBefore);
        }

        // Bug: the absent branch was blind to the states Floyd actually tracks. During the Bio Lab
        // sacrifice he has CurrentLocation null and IsAwayOnScriptedSequence set, with HasDied still
        // false - and the breezy line fired while the player listened to him being torn apart.
        [Test]
        public async Task AwayOnAScriptedSequence_DoesNotSayHeIsOffExploring()
        {
            var target = GetTarget();
            StartHere<BioLockEast>();
            var floyd = GetItem<Floyd>();
            floyd.IsOn = true;
            floyd.HasEverBeenOn = true;
            floyd.CurrentLocation = null;
            floyd.IsAwayOnScriptedSequence = true;

            var response = await target.GetResponse("where is floyd");

            response.Should().NotContain("off exploring somewhere");
            response.Should().NotContain("He'll turn up");
        }

        // Bug: the absent branch ignored IsOn, so a robot the player personally switched off was
        // described as wandering and due back.
        [Test]
        public async Task AbsentAndSwitchedOff_DoesNotPromiseHeWillTurnUp()
        {
            var target = GetTarget();
            StartHere<RobotShop>();
            var floyd = GetItem<Floyd>();
            floyd.HasEverBeenOn = true;
            floyd.IsOn = false;
            GetLocation<MachineShop>().ItemPlacedHere(floyd);

            var response = await target.GetResponse("where is floyd");

            response.Should().NotContain("off exploring somewhere");
            response.Should().NotContain("He'll turn up");
        }

        // Bug: PlayerKnowsFloydByName flips at activation but IsOn does not until the countdown
        // ends, so the boot-up was answered with the copy written for a deactivated Floyd.
        [Test]
        public async Task DuringTheWakeUpCountdown_DoesNotSayYouLeftHimThere()
        {
            var target = GetTarget();
            StartHere<RobotShop>();
            await target.GetResponse("activate floyd");

            var response = await target.GetResponse("where is floyd");

            response.Should().NotContain("slumped where you left him");
        }

        // Asking after the dead companion by the synonym must not slip past into the narrator's
        // improvisation - that is #545's tonal bug, still reachable while the matcher was name-only.
        [Test]
        public async Task WhereIsTheRobot_AfterHeDies_IsAnsweredNotNarrated()
        {
            var target = GetTarget();
            StartHere<RobotShop>();
            var floyd = GetItem<Floyd>();
            floyd.HasEverBeenOn = true;
            floyd.HasDied = true;

            var response = await target.GetResponse("where is the robot");

            response.Should().Contain("Floyd is gone.");
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

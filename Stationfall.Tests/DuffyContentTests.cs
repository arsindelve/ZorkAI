using FluentAssertions;
using GameEngine;
using Model.Interface;
using Moq;
using Stationfall.Item.Duffy;
using Stationfall.Location.Duffy;

namespace Stationfall.Tests;

/// <summary>
///     The Duffy's fittings: the things a player pokes at on the way through rather than the puzzle
///     chain itself. Phase 2 built the chain; these are the objects that make the ship feel inhabited,
///     and each one here answers for a verb the original answers for.
/// </summary>
[TestFixture]
public class DuffyContentTests : EngineTestsBase
{
    private GameEngine<StationfallGame, StationfallContext> _target = null!;

    [SetUp]
    public void SetUp()
    {
        _target = GetTarget();
    }

    /// <summary>
    ///     Restarts the game with the production grammar ahead of the unit-test parser. The unit-test
    ///     parser knows a two-word noun ("red button", "gray goo") only for the exact sentences in its
    ///     static table, so any other verb aimed at one never reaches the game through it.
    /// </summary>
    private void ThroughTheProductionGrammar()
    {
        _target = GetTarget(Walkthrough.DeterministicFirst.Parser());
    }

    private async Task WalkToCargoBay()
    {
        await _target.GetResponse("east");
        await _target.GetResponse("east");
    }

    private async Task BoardTheTruck()
    {
        await WalkToCargoBay();
        await _target.GetResponse("open hatch");
        await _target.GetResponse("in");
    }

    private async Task GoToRobotPool()
    {
        await _target.GetResponse("east");
        await _target.GetResponse("north");
    }

    [TestFixture]
    public class TheHatch : DuffyContentTests
    {
        [Test]
        public async Task IsTheSameHatch_SeenFromEveryRoomItAppearsIn()
        {
            await WalkToCargoBay();
            await _target.GetResponse("open hatch");
            await _target.GetResponse("in");

            // Opened from the cargo bay; the cab must agree, because there is only one hatch.
            var response = await _target.GetResponse("examine hatch");

            response.Should().Contain("open");
            Repository.GetItem<SpacetruckCabHatch>().IsOpen.Should().BeTrue();
            Repository.GetItem<SpacetruckHatch>().IsOpen.Should().BeTrue();
        }

        [Test]
        public async Task ShuttingItFromTheCab_ShutsItEverywhere()
        {
            await BoardTheTruck();

            await _target.GetResponse("close hatch");

            Repository.GetItem<SpacetruckHatch>().IsOpen.Should()
                .BeFalse("the cargo bay's view of the hatch is the same hatch");
            Repository.GetItem<DockingBayHatch>().IsOpen.Should().BeFalse();
        }

        // "enter truck" is an enter intent in production, so the engine has to let the truck answer it.
        [TestCase("enter truck")]
        [TestCase("board truck")]
        public async Task EnteringTheTruckByName_WithTheHatchShut_SaysTheHatchIsClosed(string input)
        {
            ThroughTheProductionGrammar();
            await WalkToCargoBay();

            (await _target.GetResponse(input)).Should().Contain("hatch is closed");
        }

        // Verbs.CloseItemVerbs: from outside, the truck is closed, shut or sealed by its hatch.
        [TestCase("close truck")]
        [TestCase("shut truck")]
        [TestCase("seal truck")]
        public async Task ClosingTheTruckFromOutside_ShutsItsHatch(string input)
        {
            await WalkToCargoBay();
            await _target.GetResponse("open hatch");
            Repository.GetItem<SpacetruckHatch>().IsOpen.Should().BeTrue();

            await _target.GetResponse(input);

            Repository.GetItem<SpacetruckHatch>().IsOpen.Should().BeFalse();
        }

        [Test]
        public async Task RefusesToOpenInDeepSpace()
        {
            await BoardTheTruck();
            await _target.GetResponse("close hatch");
            Repository.GetLocation<Spacetruck>().LaunchCounter = 1;

            var response = await _target.GetResponse("open hatch");

            response.Should().Contain("deep space");
            Repository.GetItem<SpacetruckHatch>().IsOpen.Should().BeFalse();
        }
    }

    [TestFixture]
    public class TheRadio : DuffyContentTests
    {
        [Test]
        public async Task SaysSoWhenYouListenToItSwitchedOff()
        {
            await BoardTheTruck();

            (await _target.GetResponse("listen to radio")).Should().Contain("isn't on");
        }

        // Verbs.TurnOnVerbs / TurnOffVerbs: the radio answers the same words as anything else that
        // switches on and off.
        [TestCase("start radio", "stop radio")]
        [TestCase("enable radio", "disable radio")]
        [TestCase("activate radio", "silence radio")]
        public async Task GoesOnAndOff_InAnyOfTheWordsForIt(string on, string off)
        {
            await BoardTheTruck();

            await _target.GetResponse(on);
            Repository.GetItem<Radio>().IsOn.Should().BeTrue($"\"{on}\" switches it on");

            await _target.GetResponse(off);
            Repository.GetItem<Radio>().IsOn.Should().BeFalse($"\"{off}\" switches it off");
        }

        [Test]
        public async Task TurnsOnAndOff()
        {
            await BoardTheTruck();

            (await _target.GetResponse("turn on radio")).Should().Contain("static");
            Repository.GetItem<Radio>().IsOn.Should().BeTrue();
            (await _target.GetResponse("listen to radio")).Should().Contain("Crackle");

            (await _target.GetResponse("turn off radio")).Should().Contain("switch the radio off");
            Repository.GetItem<Radio>().IsOn.Should().BeFalse();
        }

        [Test]
        public async Task ChattersWhileYouAreAboard()
        {
            await BoardTheTruck();
            await _target.GetResponse("turn on radio");

            var alwaysChatter = new Mock<IRandomChooser>();
            alwaysChatter.Setup(r => r.RollDice(100)).Returns(1);
            alwaysChatter.Setup(r => r.Choose(It.IsAny<List<string>>())).Returns("Test traffic.");
            Repository.GetItem<Radio>().Chooser = alwaysChatter.Object;

            (await _target.GetResponse("wait")).Should().Contain("Breaker. Test traffic. Over.");
        }

        [Test]
        public async Task IsSilentOnceYouHaveLeftTheCab()
        {
            await BoardTheTruck();
            await _target.GetResponse("turn on radio");

            var alwaysChatter = new Mock<IRandomChooser>();
            alwaysChatter.Setup(r => r.RollDice(100)).Returns(1);
            alwaysChatter.Setup(r => r.Choose(It.IsAny<List<string>>())).Returns("Test traffic.");
            Repository.GetItem<Radio>().Chooser = alwaysChatter.Object;

            await _target.GetResponse("out");

            // A radio left on and left behind must not narrate over the rest of the game.
            (await _target.GetResponse("wait")).Should().NotContain("Breaker");
        }
    }

    /// <summary>
    ///     Objects that share a usage share a verb grouping, so each answers every word of it. Each case
    ///     runs an established wording and another word from the same grouping on fresh games and
    ///     requires the same reply.
    /// </summary>
    [TestFixture]
    public class SharedGroupings : DuffyContentTests
    {
        private async Task<string?> ReplyTo(string[] route, string command)
        {
            ThroughTheProductionGrammar();

            foreach (var step in route)
                await _target.GetResponse(step);

            return await _target.GetResponse(command);
        }

        private static readonly string[] InTheTruckWithTheKitOpen = ["east", "east", "open hatch", "in", "open kit"];
        private static readonly string[] WithTheThermosOpen = [..InTheTruckWithTheKitOpen, "open thermos"];
        private static readonly string[] AtThePallets = ["south"];
        private static readonly string[] AtTheCargoBay = ["east", "east"];

        // Verbs.EmptyVerbs
        [TestCase("kit", "empty kit", "spill kit")]
        [TestCase("kit", "empty thermos", "spill thermos")]
        [TestCase("thermos", "empty soup", "dump soup")]
        // Verbs.TouchVerbs, Verbs.TasteVerbs, Verbs.SearchVerbs: all report the soup's temperature
        [TestCase("thermos", "touch soup", "rub soup")]
        [TestCase("thermos", "touch soup", "lick soup")]
        [TestCase("thermos", "touch soup", "check soup")]
        // Verbs.InsertVerbs
        [TestCase("kit", "put radio in thermos", "stick radio in thermos")]
        // Verbs.OpenItemVerbs, Verbs.SearchVerbs, Verbs.DestroyPaperVerbs
        [TestCase("pallets", "open pallets", "force pallets")]
        [TestCase("pallets", "open pallets", "check pallets")]
        [TestCase("pallets", "destroy pallets", "wreck pallets")]
        [TestCase("start", "crumple assignment", "deface assignment")]
        [TestCase("start", "crumple assignment", "smash assignment")]
        [TestCase("cargo bay", "open spacetruck", "force spacetruck")]
        public async Task AnotherWordForTheSameAction_GetsTheSameReply(string where, string established,
            string inOtherWords)
        {
            var route = where switch
            {
                "kit" => InTheTruckWithTheKitOpen,
                "thermos" => WithTheThermosOpen,
                "pallets" => AtThePallets,
                "cargo bay" => AtTheCargoBay,
                _ => []
            };

            var expected = await ReplyTo(route, established);
            var actual = await ReplyTo(route, inOtherWords);

            expected.Should().NotBeNullOrWhiteSpace($"\"{established}\" is the wording the handler already answers");
            actual.Should().Be(expected, $"\"{inOtherWords}\" is the same action as \"{established}\"");
        }
    }

    [TestFixture]
    public class TheGoo : DuffyContentTests
    {
        private async Task WithTheKitOpen()
        {
            ThroughTheProductionGrammar();
            await BoardTheTruck();
            await _target.GetResponse("open kit");
        }

        // Verbs.TakeVerbs and Verbs.RemoveVerbs: picking it up, in any of the words for that.
        [TestCase("take gray goo")]
        [TestCase("remove gray goo")]
        [TestCase("hold gray goo")]
        [TestCase("carry gray goo")]
        public async Task CannotBePickedUp_HoweverThePlayerSaysIt(string input)
        {
            await WithTheKitOpen();

            (await _target.GetResponse(input)).Should().Contain("ooze through your fingers");
        }

        // Verbs.ThrowOrDropVerbs: getting rid of it, in any of the words for that.
        [TestCase("drop gray goo")]
        [TestCase("throw gray goo")]
        [TestCase("toss gray goo")]
        [TestCase("hurl gray goo")]
        public async Task CannotBePutDown_HoweverThePlayerSaysIt(string input)
        {
            await WithTheKitOpen();

            (await _target.GetResponse(input)).Should().Contain("sticks where it is");
        }

        // Verbs.EatOrDrinkVerbs: the goo is eaten in the same words as any other food.
        [TestCase("eat gray goo")]
        [TestCase("consume gray goo")]
        [TestCase("chew gray goo")]
        public async Task IsEaten_HoweverThePlayerSaysIt(string input)
        {
            await WithTheKitOpen();
            LeakRecorder.Clear();

            await _target.GetResponse(input);

            LeakRecorder.CompletenessLeaks.Should().BeEmpty(
                $"\"{input}\" is answered by the goo, not handed to the narrator");
        }
    }

    [TestFixture]
    public class TheEmergencyBeacon : DuffyContentTests
    {
        // Verbs.PushVerbs: a button is pushed, pressed, clicked or hit, here as on every other button.
        [TestCase("push red button")]
        [TestCase("click red button")]
        [TestCase("depress red button")]
        [TestCase("hit red button")]
        [TestCase("punch red button")]
        public async Task RefusesWhileYouAreNotActuallyInTrouble(string input)
        {
            ThroughTheProductionGrammar();
            await BoardTheTruck();

            (await _target.GetResponse(input)).Should().Contain("court-martial");
        }

        [Test]
        public async Task PlaysItsRecording_OnceTheFlightIsOver()
        {
            await BoardTheTruck();
            // The counter reaching its last step is what matters, not whether you arrived anywhere -
            // which is exactly the case where a player would reach for the beacon.
            Repository.GetLocation<Spacetruck>().LaunchCounter = 5;

            (await _target.GetResponse("push red button")).Should().Contain("Nothing can go wrong");
        }
    }

    [TestFixture]
    public class TheRobotPoolBins : DuffyContentTests
    {
        [Test]
        public async Task EachOneShowsTheRobotStandingInIt()
        {
            await GoToRobotPool();

            (await _target.GetResponse("look in first bin")).Should().Contain("Rex");
            (await _target.GetResponse("look in second bin")).Should().Contain("Helen");
            (await _target.GetResponse("look in third bin")).Should().Contain("hopeful");
        }

        [Test]
        public async Task AreEmptyOnceTheirRobotHasBeenRequisitioned()
        {
            await GoToRobotPool();
            await _target.GetResponse("put authorization in slot");
            await _target.GetResponse("type 3");

            (await _target.GetResponse("look in third bin")).Should().Contain("empty");
        }

        // Verbs.BoardVerbs: getting into a thing, in any of the words for that.
        [TestCase("enter first bin")]
        [TestCase("board first bin")]
        public async Task AreNotForClimbingInto(string input)
        {
            ThroughTheProductionGrammar();
            await GoToRobotPool();

            (await _target.GetResponse(input)).Should().Contain("only for robots");
        }
    }

    [TestFixture]
    public class TheFormsStorageRoom : DuffyContentTests
    {
        [Test]
        public async Task TheFormsAreSealedInTheirBoxes()
        {
            await _target.GetResponse("south");

            (await _target.GetResponse("examine forms")).Should().Contain("sealed inside the boxes");
        }

        [Test]
        public async Task DestroyingPatrolPaperworkIsRefused()
        {
            await _target.GetResponse("south");

            (await _target.GetResponse("tear forms")).Should().Contain("Uniform Code of Paperwork");
        }

        [Test]
        public async Task OpeningABoxIsItsOwnPunishment()
        {
            await _target.GetResponse("south");

            var response = await _target.GetResponse("open box");

            response.Should().Contain("and forms");
            response.Should().Contain("reseal");
        }
    }

    [TestFixture]
    public class ThePortDoor : DuffyContentTests
    {
        [Test]
        public async Task NamesWhatWouldOpenIt()
        {
            // The refusal has to name the remedy, or the player is left rattling a door for no reason.
            (await _target.GetResponse("open door")).Should().Contain("validated Assignment Completion Form");
        }

        // Verbs.CloseItemVerbs: it is already closed, however the player says to close it.
        [TestCase("close door")]
        [TestCase("shut door")]
        [TestCase("seal door")]
        public async Task IsAlreadyClosed_HoweverThePlayerSaysToCloseIt(string input)
        {
            (await _target.GetResponse(input)).Should().Contain("It's already closed");
        }

        [Test]
        public async Task StaysShutWhenYouWalkAtIt()
        {
            (await _target.GetResponse("west")).Should().Contain("The door is closed");
            Ctx.CurrentLocation.Should().BeOfType<DeckTwelve>();
        }
    }

    [TestFixture]
    public class TheSoup : DuffyContentTests
    {
        [Test]
        public async Task StartsSteamingHot()
        {
            await BoardTheTruck();
            await _target.GetResponse("open kit");
            // The Thermos is opaque, so the soup is not in scope until it is open.
            await _target.GetResponse("open thermos");

            (await _target.GetResponse("examine soup")).Should().Contain("steaming hot");
        }

        [Test]
        public async Task CoolsOnlyOnceYouAreAboard()
        {
            // Before boarding, the clock has not started - a player who dawdles on the ship should not
            // find cold soup waiting.
            Repository.GetItem<Chronometer>().CurrentTime += 5000;
            await _target.GetResponse("wait");
            Repository.GetItem<BlueSoup>().Warmth.Should().Be(100);

            await BoardTheTruck();
            var start = Repository.GetItem<BlueSoup>().Warmth;

            Repository.GetItem<Chronometer>().CurrentTime += 100;
            await _target.GetResponse("wait");

            Repository.GetItem<BlueSoup>().Warmth.Should().BeLessThan(start);
        }

        [Test]
        public async Task CoolsFasterWithTheThermosOpen()
        {
            await BoardTheTruck();
            await _target.GetResponse("open kit");
            await _target.GetResponse("open thermos");

            var soup = Repository.GetItem<BlueSoup>();
            var start = soup.Warmth;

            Repository.GetItem<Chronometer>().CurrentTime += 100;
            await _target.GetResponse("wait");

            // Four degrees with the lid off against one with it on.
            (start - soup.Warmth).Should().Be(4);
        }

        [Test]
        public async Task IsStillReachable_ThroughTheThermosThatHoldsIt()
        {
            // Regression: a container's RespondToSimpleInteraction offers the action to the items
            // INSIDE it before considering itself, so a container override that early-returns
            // NoNounMatch for a noun that isn't its own silently deletes its contents from the parser.
            // The soup answered to nothing at all until this was fixed.
            await BoardTheTruck();
            await _target.GetResponse("open kit");
            await _target.GetResponse("open thermos");

            (await _target.GetResponse("examine soup")).Should()
                .Contain("soup", "the Thermos must not shadow what it contains");
        }

        [Test]
        public async Task CanBePouredOut_AndIsThenGoneForGood()
        {
            await BoardTheTruck();
            await _target.GetResponse("open kit");

            var response = await _target.GetResponse("empty thermos");

            response.Should().Contain("blueberries");
            Repository.GetItem<Thermos>().Items.Should().NotContain(Repository.GetItem<BlueSoup>());
        }
    }

    [TestFixture]
    public class TheSurvivalKit : DuffyContentTests
    {
        [Test]
        public async Task TheThermosNeckIsTooNarrowForAnythingElse()
        {
            await BoardTheTruck();
            await _target.GetResponse("open kit");

            (await _target.GetResponse("put authorization in thermos")).Should().Contain("too narrow");
        }

        [Test]
        public async Task TheGooCannotBePickedUp_AndTheRefusalSaysWhatToDoInstead()
        {
            await BoardTheTruck();
            await _target.GetResponse("open kit");

            var response = await _target.GetResponse("take gray goo");

            response.Should().Contain("ooze through your fingers");
            response.Should().Contain("survival kit");
        }
    }

    protected StationfallContext Ctx => _target.Context;
}

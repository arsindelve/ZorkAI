using FluentAssertions;
using GameEngine;
using Model.Movement;
using Planetfall.Location.Kalamontee.Admin;
using Planetfall.Location.Lawanda.LabOffice;

namespace Planetfall.Tests;

[TestFixture]
public class CryoAnteroomEndingTests : EngineTestsBase
{
    private CryoAnteroomLocation _cryoAnteroom = null!;
    private SystemsMonitors _systemsMonitor = null!;
    private PlanetfallContext _context = null!;

    [SetUp]
    public void Setup()
    {
        Repository.Reset();
        GetTarget(); // Initialize the game
        _cryoAnteroom = Repository.GetLocation<CryoAnteroomLocation>();
        _systemsMonitor = Repository.GetLocation<SystemsMonitors>();
        _context = Context;

        // Start with all systems broken (default state after Init)
        _systemsMonitor.Init();
    }

    #region Helper Methods

    private void FixAllSystems()
    {
        _systemsMonitor.MarkCommunicationsFixed();
        _systemsMonitor.MarkPlanetaryDefenseFixed();
        _systemsMonitor.MarkCourseControlFixed();
    }

    private void FixCourseControlOnly()
    {
        _systemsMonitor.MarkCourseControlFixed();
    }

    private void FixCourseControlAndDefense()
    {
        _systemsMonitor.MarkCourseControlFixed();
        _systemsMonitor.MarkPlanetaryDefenseFixed();
    }

    private void FixCourseControlAndComms()
    {
        _systemsMonitor.MarkCourseControlFixed();
        _systemsMonitor.MarkCommunicationsFixed();
    }

    private void FixDefenseAndCommsOnly()
    {
        _systemsMonitor.MarkPlanetaryDefenseFixed();
        _systemsMonitor.MarkCommunicationsFixed();
    }

    private void FixDefenseOnly()
    {
        _systemsMonitor.MarkPlanetaryDefenseFixed();
    }

    private void FixCommsOnly()
    {
        _systemsMonitor.MarkCommunicationsFixed();
    }

    private async Task<string> EnterCryoAnteroom()
    {
        var previousLocation = Repository.GetLocation<CryoElevatorLocation>();
        return await _cryoAnteroom.AfterEnterLocation(_context, previousLocation, null!);
    }

    #endregion

    #region Ending 1: Complete Victory (All 3 Fixed)

    [TestFixture]
    public class CompleteVictoryTests : CryoAnteroomEndingTests
    {
        [Test]
        public async Task AllSystemsFixed_ReturnsCompleteVictoryEnding()
        {
            FixAllSystems();

            var result = await EnterCryoAnteroom();

            result.Should().Contain("A door slides open and a medical robot glides in");
            result.Should().Contain("I am Veldina, leader of Resida");
            result.Should().Contain("the cure has been discovered, and the planetary systems repaired");
            result.Should().Contain("S.P.S. Flathead");
            result.Should().Contain("promotion to Lieutenant First Class");
            result.Should().Contain("Blather");
            result.Should().Contain("demoted to Ensign Twelfth Class");
            result.Should().Contain("personal toilet attendant");
            result.Should().Contain("antidote for The Disease");
            result.Should().Contain("Floyd feeling better now");
            result.Should().Contain("Maybe we can use them in the sequel");
        }

        [Test]
        public async Task AllSystemsFixed_MentionsFloydReturning()
        {
            FixAllSystems();

            var result = await EnterCryoAnteroom();

            result.Should().Contain("A team of robot technicians step into the anteroom");
            result.Should().Contain("familiar figure comes bounding toward you");
            result.Should().Contain("\"Hi!\" shouts Floyd");
            result.Should().Contain("helicopter key");
            result.Should().Contain("reactor elevator card");
            result.Should().Contain("paddleball set");
        }

        [Test]
        public async Task AllSystemsFixed_MentionsPromotion()
        {
            FixAllSystems();

            var result = await EnterCryoAnteroom();

            result.Should().Contain("Captain Sterling of the Flathead acknowledges your heroic actions");
            result.Should().Contain("promotion to Lieutenant First Class");
        }

        [Test]
        public async Task AllSystemsFixed_MentionsLeadershipOffer()
        {
            FixAllSystems();

            var result = await EnterCryoAnteroom();

            result.Should().Contain("grateful people of Resida offer you leadership of their world");
        }

        [Test]
        public async Task AllSystemsFixed_MentionsBlatherHumiliation()
        {
            FixAllSystems();

            var result = await EnterCryoAnteroom();

            result.Should().Contain("Blather is with them");
            result.Should().Contain("babbling cravenly");
            result.Should().Contain("demoted to Ensign Twelfth Class");
            result.Should().Contain("personal toilet attendant");
        }

        [Test]
        public async Task AllSystemsFixed_MentionsMutantHunters()
        {
            FixAllSystems();

            var result = await EnterCryoAnteroom();

            result.Should().Contain("team of mutant hunters head for the cryo-elevator");
        }

        [Test]
        public async Task AllSystemsFixed_DoesNotMentionStranded()
        {
            FixAllSystems();

            var result = await EnterCryoAnteroom();

            result.Should().NotContain("stranded");
            result.Should().NotContain("plunge into the sun");
            result.Should().NotContain("doomed");
        }
    }

    #endregion

    #region Ending 2: Stranded - Defense Failed

    [TestFixture]
    public class StrandedDefenseFailedTests : CryoAnteroomEndingTests
    {
        [Test]
        public async Task CourseControlFixed_DefenseBroken_CommsBroken_ReturnsStrandedDefenseEnding()
        {
            FixCourseControlOnly();

            var result = await EnterCryoAnteroom();

            result.Should().Contain("A door slides open and a medical robot glides in");
            result.Should().Contain("I am Veldina, leader of Resida");
            result.Should().Contain("destroyed by our malfunctioning meteor defenses");
            result.Should().Contain("stranded on Resida, possibly forever");
            result.Should().Contain("unlimited bank account and a house in the country");
        }

        [Test]
        public async Task CourseControlFixed_DefenseBroken_CommsFixed_ReturnsStrandedDefenseEnding()
        {
            FixCourseControlAndComms();

            var result = await EnterCryoAnteroom();

            result.Should().Contain("destroyed by our malfunctioning meteor defenses");
            result.Should().Contain("stranded on Resida, possibly forever");
        }

        [Test]
        public async Task StrandedDefense_DoesNotMentionFloyd()
        {
            FixCourseControlOnly();

            var result = await EnterCryoAnteroom();

            result.Should().NotContain("Floyd");
            result.Should().NotContain("sequel");
        }

        [Test]
        public async Task StrandedDefense_DoesNotMentionPromotion()
        {
            FixCourseControlOnly();

            var result = await EnterCryoAnteroom();

            result.Should().NotContain("Lieutenant First Class");
            result.Should().NotContain("Captain Sterling");
        }

        [Test]
        public async Task StrandedDefense_DoesNotMentionDoomedPlanet()
        {
            FixCourseControlOnly();

            var result = await EnterCryoAnteroom();

            result.Should().NotContain("plunge into the sun");
            result.Should().NotContain("orbit has now decayed");
        }

        [Test]
        public async Task StrandedDefense_MentionsCompensation()
        {
            FixCourseControlOnly();

            var result = await EnterCryoAnteroom();

            result.Should().Contain("unlimited bank account");
            result.Should().Contain("house in the country");
        }
    }

    #endregion

    #region Ending 3: Stranded - Communications Failed

    [TestFixture]
    public class StrandedCommsFailedTests : CryoAnteroomEndingTests
    {
        [Test]
        public async Task CourseControlFixed_DefenseFixed_CommsBroken_ReturnsStrandedCommsEnding()
        {
            FixCourseControlAndDefense();

            var result = await EnterCryoAnteroom();

            result.Should().Contain("A door slides open and a medical robot glides in");
            result.Should().Contain("I am Veldina, leader of Resida");
            result.Should().Contain("malfunctioning communications system");
            result.Should().Contain("has given up and departed");
            result.Should().Contain("stranded on Resida, possibly forever");
            result.Should().Contain("unlimited bank account and a house in the country");
        }

        [Test]
        public async Task StrandedComms_DoesNotMentionMeteorDefenses()
        {
            FixCourseControlAndDefense();

            var result = await EnterCryoAnteroom();

            result.Should().NotContain("meteor defenses");
            result.Should().NotContain("destroyed by");
        }

        [Test]
        public async Task StrandedComms_MentionsShipDeparted()
        {
            FixCourseControlAndDefense();

            var result = await EnterCryoAnteroom();

            result.Should().Contain("come looking for survivors");
            result.Should().Contain("given up and departed");
        }

        [Test]
        public async Task StrandedComms_DoesNotMentionFloyd()
        {
            FixCourseControlAndDefense();

            var result = await EnterCryoAnteroom();

            result.Should().NotContain("Floyd");
        }

        [Test]
        public async Task StrandedComms_DoesNotMentionDoomedPlanet()
        {
            FixCourseControlAndDefense();

            var result = await EnterCryoAnteroom();

            result.Should().NotContain("plunge into the sun");
        }
    }

    #endregion

    #region Ending 4: Doomed Planet with Rescue

    [TestFixture]
    public class DoomedWithRescueTests : CryoAnteroomEndingTests
    {
        [Test]
        public async Task CourseControlBroken_DefenseFixed_CommsFixed_ReturnsDoomedWithRescueEnding()
        {
            FixDefenseAndCommsOnly();

            var result = await EnterCryoAnteroom();

            result.Should().Contain("A door slides open and a medical robot glides in");
            result.Should().Contain("planetary course control system has malfunctioned");
            result.Should().Contain("orbit has now decayed beyond correction");
            result.Should().Contain("Resida will plunge into the sun");
            result.Should().Contain("Fortunately, another ship from your Stellar Patrol has arrived");
            result.Should().Contain("S.P.S. Flathead materializes");
            result.Should().Contain("takes you away from the doomed world");
        }

        [Test]
        public async Task DoomedWithRescue_MentionsGratitudeButInVain()
        {
            FixDefenseAndCommsOnly();

            var result = await EnterCryoAnteroom();

            result.Should().Contain("Cure has been discovered, and we are grateful");
            result.Should().Contain("it was all in vain");
        }

        [Test]
        public async Task DoomedWithRescue_DoesNotMentionVeldina()
        {
            FixDefenseAndCommsOnly();

            var result = await EnterCryoAnteroom();

            // In doomed ending, Veldina doesn't introduce herself the same way
            result.Should().NotContain("I am Veldina, leader of Resida");
        }

        [Test]
        public async Task DoomedWithRescue_DoesNotMentionStranded()
        {
            FixDefenseAndCommsOnly();

            var result = await EnterCryoAnteroom();

            result.Should().NotContain("stranded");
            result.Should().NotContain("unlimited bank account");
        }

        [Test]
        public async Task DoomedWithRescue_DoesNotMentionFloyd()
        {
            FixDefenseAndCommsOnly();

            var result = await EnterCryoAnteroom();

            result.Should().NotContain("Floyd");
        }

        [Test]
        public async Task DoomedWithRescue_MentionsVeldinaExaminesPanel()
        {
            FixDefenseAndCommsOnly();

            var result = await EnterCryoAnteroom();

            result.Should().Contain("Veldina examines the control panel again");
        }
    }

    #endregion

    #region Ending 5: Doomed Planet, No Rescue

    [TestFixture]
    public class DoomedNoRescueTests : CryoAnteroomEndingTests
    {
        [Test]
        public async Task AllSystemsBroken_ReturnsDoomedNoRescueEnding()
        {
            // Don't fix anything - all systems remain broken

            var result = await EnterCryoAnteroom();

            result.Should().Contain("A door slides open and a medical robot glides in");
            result.Should().Contain("planetary course control system has malfunctioned");
            result.Should().Contain("Resida will plunge into the sun");
        }

        [Test]
        public async Task CourseControlBroken_DefenseBroken_CommsFixed_ReturnsDoomedNoRescueEnding()
        {
            FixCommsOnly();

            var result = await EnterCryoAnteroom();

            result.Should().Contain("Resida will plunge into the sun");
            result.Should().NotContain("Fortunately");
            result.Should().NotContain("takes you away");
        }

        [Test]
        public async Task CourseControlBroken_DefenseFixed_CommsBroken_ReturnsDoomedNoRescueEnding()
        {
            FixDefenseOnly();

            var result = await EnterCryoAnteroom();

            result.Should().Contain("Resida will plunge into the sun");
            result.Should().NotContain("Fortunately");
            result.Should().NotContain("takes you away");
        }

        [Test]
        public async Task DoomedNoRescue_DoesNotMentionRescue()
        {
            // Don't fix anything

            var result = await EnterCryoAnteroom();

            result.Should().NotContain("Fortunately");
            result.Should().NotContain("S.P.S. Flathead");
            result.Should().NotContain("takes you away");
        }

        [Test]
        public async Task DoomedNoRescue_DoesNotMentionStranded()
        {
            // Don't fix anything

            var result = await EnterCryoAnteroom();

            result.Should().NotContain("stranded");
            result.Should().NotContain("unlimited bank account");
        }

        [Test]
        public async Task DoomedNoRescue_DoesNotMentionFloyd()
        {
            // Don't fix anything

            var result = await EnterCryoAnteroom();

            result.Should().NotContain("Floyd");
        }

        [Test]
        public async Task DoomedNoRescue_MentionsGratitude()
        {
            // Don't fix anything

            var result = await EnterCryoAnteroom();

            result.Should().Contain("we are grateful");
            result.Should().Contain("it was all in vain");
        }
    }

    #endregion

    #region SystemsMonitor State Verification

    [TestFixture]
    public class SystemsMonitorStateTests : CryoAnteroomEndingTests
    {
        [Test]
        public void Init_AllCriticalSystemsAreBroken()
        {
            _systemsMonitor.Init();

            _systemsMonitor.Busted.Should().Contain("KUMUUNIKAASHUNZ");
            _systemsMonitor.Busted.Should().Contain("PLANATEREE DEFENS");
            _systemsMonitor.Busted.Should().Contain("PLANATEREE KORS KUNTROOL");
        }

        [Test]
        public void MarkCommunicationsFixed_MovesFromBustedToFixed()
        {
            _systemsMonitor.Init();

            _systemsMonitor.MarkCommunicationsFixed();

            _systemsMonitor.Fixed.Should().Contain("KUMUUNIKAASHUNZ");
            _systemsMonitor.Busted.Should().NotContain("KUMUUNIKAASHUNZ");
        }

        [Test]
        public void MarkPlanetaryDefenseFixed_MovesFromBustedToFixed()
        {
            _systemsMonitor.Init();

            _systemsMonitor.MarkPlanetaryDefenseFixed();

            _systemsMonitor.Fixed.Should().Contain("PLANATEREE DEFENS");
            _systemsMonitor.Busted.Should().NotContain("PLANATEREE DEFENS");
        }

        [Test]
        public void MarkCourseControlFixed_MovesFromBustedToFixed()
        {
            _systemsMonitor.Init();

            _systemsMonitor.MarkCourseControlFixed();

            _systemsMonitor.Fixed.Should().Contain("PLANATEREE KORS KUNTROOL");
            _systemsMonitor.Busted.Should().NotContain("PLANATEREE KORS KUNTROOL");
        }

        [Test]
        public void Init_SomeSystemsStartFixed()
        {
            _systemsMonitor.Init();

            _systemsMonitor.Fixed.Should().Contain("LIIBREREE");
            _systemsMonitor.Fixed.Should().Contain("REEAKTURZ");
            _systemsMonitor.Fixed.Should().Contain("LIIF SUPORT");
        }
    }

    #endregion

    #region Game Over Behavior

    [TestFixture]
    public class GameOverBehaviorTests : CryoAnteroomEndingTests
    {
        [Test]
        public void Map_ReturnsNoExits()
        {
            var map = _cryoAnteroom.GetType()
                .GetMethod("Map", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(_cryoAnteroom, new object[] { _context }) as Dictionary<Direction, MovementParameters>;

            map.Should().NotBeNull();
            map.Should().BeEmpty();
        }

        [Test]
        public void GetContextBasedDescription_ReturnsEmpty()
        {
            var description = _cryoAnteroom.GetType()
                .GetMethod("GetContextBasedDescription", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .Invoke(_cryoAnteroom, new object[] { _context }) as string;

            description.Should().BeEmpty();
        }
    }

    #endregion
}

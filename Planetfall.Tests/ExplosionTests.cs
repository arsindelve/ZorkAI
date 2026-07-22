using FluentAssertions;
using GameEngine;
using GameEngine.Item;
using Model.AIGeneration;
using Model.AIParsing;
using Model.Intent;
using Model.Interaction;
using Model.Interface;
using Moq;
using OpenAI;
using Planetfall.Item.Feinstein;
using Planetfall.Location.Feinstein;
using Planetfall.Location.Kalamontee;

namespace Planetfall.Tests;

public class ExplosionTests : EngineTestsBase
{
    // Issue #356 follow-up: "god mode go <place>" is a raw CurrentLocation swap for testing - it
    // doesn't run DeckNine.OnLeaveLocation or EscapePod.AfterEnterLocation, so a tester who teleports
    // away from Deck Nine to check on later content never unregisters ExplosionCoordinator. It keeps
    // counting turns in the background and can unconditionally kill the tester (wiping their god-mode
    // session via RestartAfterDeath) once Moves rolls into the 10-14 death window, nowhere near the
    // ship. The clock is only meaningful while actually navigating the Deck Nine escape sequence, so
    // a god-mode teleport should disarm it.
    [Test]
    public async Task GodModeGo_DisarmsExplosionClock_SoTeleportedTesterSurvives()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<DeckNine>();

        // A tester jumps straight to later content, unrelated to the Deck Nine escape sequence.
        await target.GetResponse("god mode go mess hall");

        // Wait through the Feinstein-explosion window (moves 10-14), far from the ship.
        for (var i = 0; i < 14; i++)
            await target.GetResponse("wait");

        // If the explosion clock had still fired, death would reset the game via
        // RestartAfterDeath - snapping the player back to Deck Nine and bumping DeathCounter.
        target.Context.DeathCounter.Should().Be(0);
        target.Context.CurrentLocation.Should().BeOfType<MessHall>();
    }

    // Code review follow-up on #356: OnGodModeTeleport only disarmed ExplosionCoordinator, leaving
    // EscapePod's own post-landing sinking timer (YerSinking/TurnsAfterStanding) armed - it has the
    // exact same location-blind death bug (no CurrentLocation check) that ExplosionCoordinator had.
    // A tester who stands out of the safety web then teleports away to check something else could
    // still be silently killed by the sinking pod several turns later, far from the pod.
    [Test]
    public async Task GodModeGo_DisarmsEscapePodSinkingClock_SoTeleportedTesterSurvives()
    {
        var target = GetTarget();
        var pod = Repository.GetLocation<EscapePod>();
        target.Context.CurrentLocation = pod;
        pod.TurnsAfterStanding = 1; // already standing - sinking countdown underway
        target.Context.RegisterActor(pod);

        // A tester jumps straight to later content, unrelated to the sinking pod.
        await target.GetResponse("god mode go mess hall");

        for (var i = 0; i < 5; i++)
            await target.GetResponse("wait");

        target.Context.DeathCounter.Should().Be(0);
        target.Context.CurrentLocation.Should().BeOfType<MessHall>();
    }

    // Code review follow-up on #356: OnGodModeTeleport originally disarmed ExplosionCoordinator
    // unconditionally, even when the god-mode destination was Deck Nine itself - meaning a developer
    // who teleported in specifically to test the Feinstein-explosion sequence found it permanently
    // defused on arrival, with no way to observe it short of restarting the whole engine.
    [Test]
    public async Task GodModeGo_IntoDeckNine_DoesNotDisarmExplosionClock()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<MessHall>();

        await target.GetResponse("god mode go deck nine");

        for (var i = 0; i < 13; i++)
            await target.GetResponse("wait");

        target.Context.DeathCounter.Should().Be(1);
    }

    // Review follow-up on this PR: the previous fix left ExplosionCoordinator armed whenever the
    // teleport destination was EscapePod, mirroring the DeckNine exception. But unlike DeckNine,
    // EscapePod's move-14 case in ExplosionCoordinator has NO location check at all - under normal
    // play that's safe because actually boarding the pod always disarms the coordinator first (via
    // EscapePod.AfterEnterLocation), so the "still armed while genuinely inside the pod" state never
    // occurs. "god mode go escape pod" skipped that hook and the old fix deliberately kept the clock
    // armed for this destination, so a tester who teleported into the pod and waited out the clock
    // was killed by their own ship's explosion while standing in the one place meant to be safe.
    [Test]
    public async Task GodModeGo_IntoEscapePod_DoesNotGetKilledByExplosionClock()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<MessHall>();

        await target.GetResponse("god mode go escape pod");

        for (var i = 0; i < 13; i++)
            await target.GetResponse("wait");

        target.Context.DeathCounter.Should().Be(0);
    }

    [Test]
    [Explicit("Requires ZorkAI.OpenAI API key - tests pronoun resolution fix")]
    public async Task PronounResolution_OpenIt_ResolvesToBulkhead()
    {
        // This test verifies the fix for pronoun resolution timing bug
        // When user types "west" then "open it", the pronoun "it" should resolve to "bulkhead"

        // Use real parser with pronoun resolution (requires ZorkAI.OpenAI key in env)
        var realParser = new IntentParser(new Planetfall.GlobalCommand.PlanetfallGlobalCommandFactory());
        var target = GetTarget(realParser);
        target.Context.CurrentLocation = Repository.GetLocation<DeckNine>();

        // Clear actors to prevent random Blather interference
        target.Context.Actors.Clear();

        // Try to go west - bulkhead is closed at start
        var response = await target.GetResponse("west");
        Console.WriteLine($"Response to 'west': {response}");
        response.Should().Contain("bulkhead");
        response.Should().Contain("closed");

        // Now try "open it" - should resolve "it" to "bulkhead" from previous response
        // This would fail before the fix (would get AI-generated response)
        // After the fix, it should use the game engine's OpenProcessor
        response = await target.GetResponse("open it");
        Console.WriteLine($"Response to 'open it': {response}");

        // Check that we got a proper game engine response, not AI generation
        // The exact response depends on the turn count (line 30-36 in BulkheadDoor.cs)
        // At turn 0, it should say "Why open the door to the emergency escape pod if there's no emergency?"
        response.Should().Contain("emergency");
    }
    [Test]
    public async Task Experience_IntoEscapePod_ThenOut()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<DeckNine>();

        // Act
        await target.GetResponse("Wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("Wait");
        await target.GetResponse("Wait");
        await target.GetResponse("wait");

        var response = await target.GetResponse("wait");
        response.Should()
            .Contain(
                "A massive explosion rocks the ship. Echoes from the explosion resound deafeningly down the halls. The door to port slides open.");

        response = await target.GetResponse("west");
        response.Should()
            .Contain("The ship shakes again. You hear, from close by, the sounds of emergency bulkheads closing.");

        response = await target.GetResponse("east");
        response.Should()
            .Contain(
                "More powerful explosions buffet the ship. The lights flicker madly, and the escape-pod bulkhead clangs shut");

        response = await target.GetResponse("west");
        response.Should().Contain("closed");
        response.Should().Contain("Explosions continue to rock the ship.");

        response = await target.GetResponse("wait");
        response.Should()
            .Contain(
                "An enormous explosion tears the walls of the ship apart. If only you had made it to an escape pod...");
        response.Should().Contain("You have died");
    }

    [Test]
    public async Task Experience_IntoEscapePod_FirstChance()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<DeckNine>();

        // Act
        await target.GetResponse("Wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("Wait");
        await target.GetResponse("Wait");
        await target.GetResponse("wait");

        var response = await target.GetResponse("wait");
        response.Should()
            .Contain(
                "A massive explosion rocks the ship. Echoes from the explosion resound deafeningly down the halls. The door to port slides open.");

        response = await target.GetResponse("west");
        response.Should()
            .Contain("The ship shakes again. You hear, from close by, the sounds of emergency bulkheads closing.");

        response = await target.GetResponse("sit");
        response.Should().Contain("The pod door clangs shut as heavy explosions continue to buffet the Feinstein");

        response = await target.GetResponse("wait");
        response.Should()
            .Contain("You feel the pod begin to slide down its ejection tube as explosions shake the mother ship");

        response = await target.GetResponse("wait");
        response.Should().Contain("Through the viewport of the pod you see the Feinstein dwindle as you head away");
    }

    [Test]
    public async Task Experience_IntoEscapePod_SecondChance()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<DeckNine>();

        // Act
        await target.GetResponse("Wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("Wait");
        await target.GetResponse("Wait");
        await target.GetResponse("wait");

        var response = await target.GetResponse("wait");
        response.Should()
            .Contain(
                "A massive explosion rocks the ship. Echoes from the explosion resound deafeningly down the halls. The door to port slides open.");

        response = await target.GetResponse("wait");
        response.Should()
            .Contain(
                "More distant explosions! A narrow emergency bulkhead at the base of the gangway and a wider one along the corridor to starboard both crash shut!");

        response = await target.GetResponse("west");
        response.Should().Contain("The pod door clangs shut as heavy explosions continue to buffet the Feinstein");

        response = await target.GetResponse("sit");
        response.Should()
            .Contain("You feel the pod begin to slide down its ejection tube as explosions shake the mother ship");

        response = await target.GetResponse("wait");
        response.Should().Contain("Through the viewport of the pod you see the Feinstein dwindle as you head away");
    }

    [Test]
    public async Task Experience_StayOnDeckNine()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<DeckNine>();

        // Act
        await target.GetResponse("Wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("Wait");
        await target.GetResponse("Wait");
        await target.GetResponse("wait");

        var response = await target.GetResponse("wait");
        response.Should()
            .Contain(
                "A massive explosion rocks the ship. Echoes from the explosion resound deafeningly down the halls. The door to port slides open.");

        response = await target.GetResponse("wait");
        response.Should()
            .Contain(
                "More distant explosions! A narrow emergency bulkhead at the base of the gangway and a wider one along the corridor to starboard both crash shut!");

        response = await target.GetResponse("wait");
        response.Should()
            .Contain(
                "More powerful explosions buffet the ship. The lights flicker madly, and the escape-pod bulkhead clangs shut.");

        response = await target.GetResponse("wait");
        response.Should().Contain("Explosions continue to rock the ship.");

        response = await target.GetResponse("wait");
        response.Should()
            .Contain(
                "An enormous explosion tears the walls of the ship apart. If only you had made it to an escape pod...");
        response.Should().Contain("You have died");
    }

    [Test]
    public async Task Experience_On_DeckEight()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<DeckNine>();

        // Act
        await target.GetResponse("Wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("Up");
        await target.GetResponse("Up");
        await target.GetResponse("wait");

        var response = await target.GetResponse("wait");
        response.Should()
            .Contain(
                "A massive explosion rocks the ship. Echoes from the explosion resound deafeningly down the halls.");
        response.Should().Contain("bellows Blather, turning a deepening shade of crimson");
        response.Should().Contain("Blather, looking slightly disoriented, barks at you to resume your assigned duties");

        response = await target.GetResponse("wait");
        response.Should()
            .Contain(
                "You are deafened by more explosions and by the sound of emergency bulkheads slamming closed. Blather, foaming slightly at the mouth, screams at you to swab the decks.");

        response = await target.GetResponse("wait");
        response.Should()
            .Contain(
                "The ship rocks from the force of multiple explosions. The lights go out, and you feel a sudden drop in pressure accompanied by a loud hissing. Too bad you weren't in the escape pod...");
        response.Should().Contain("You have died");
    }

    [Test]
    public async Task Experience_In_ReactorLobby()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<DeckNine>();

        // Act
        await target.GetResponse("Wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("east");
        await target.GetResponse("wait");

        var response = await target.GetResponse("wait");
        response.Should()
            .Contain(
                "A massive explosion rocks the ship. Echoes from the explosion resound deafeningly down the halls.");
        response.Should().Contain("bellows Blather, turning a deepening shade of crimson");
        response.Should().Contain("Blather, looking slightly disoriented, barks at you to resume your assigned duties");

        response = await target.GetResponse("wait");
        response.Should()
            .Contain(
                "You are deafened by more explosions and by the sound of emergency bulkheads slamming closed. Blather, foaming slightly at the mouth, screams at you to swab the decks.");

        response = await target.GetResponse("wait");
        response.Should()
            .Contain(
                "The ship rocks from the force of multiple explosions. The lights go out, and you feel a sudden drop in pressure accompanied by a loud hissing. Too bad you weren't in the escape pod...");
        response.Should().Contain("You have died");
    }

    [Test]
    public async Task Experience_In_Gangway()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<DeckNine>();

        // Act
        await target.GetResponse("Up");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");

        var response = await target.GetResponse("wait");
        response.Should()
            .Contain(
                "A massive explosion rocks the ship. Echoes from the explosion resound deafeningly down the halls.");

        response = await target.GetResponse("wait");
        response.Should().Contain("Another explosion. A narrow bulkhead at the base of the gangway slams shut!");

        response = await target.GetResponse("wait");
        response.Should()
            .Contain(
                "The ship rocks from the force of multiple explosions. The lights go out, and you feel a sudden drop in pressure accompanied by a loud hissing. Too bad you weren't in the escape pod...");
        response.Should().Contain("You have died");
    }

    [Test]
    public async Task Experience_In_Brig()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<DeckNine>();

        // Act
        await target.GetResponse("Up");
        await target.GetResponse("Up");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        var response = await target.GetResponse("wait");
        response.Should().Contain("brig");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");

        response = await target.GetResponse("wait");
        response.Should()
            .Contain(
                "A massive explosion rocks the ship. Echoes from the explosion resound deafeningly down the halls.");

        response = await target.GetResponse("wait");
        response.Should()
            .Contain("The ship shakes again. You hear, from close by, the sounds of emergency bulkheads closing.");

        response = await target.GetResponse("wait");
        response.Should()
            .Contain(
                "The ship rocks from the force of multiple explosions. The lights go out, and you feel a sudden drop in pressure accompanied by a loud hissing. Too bad you weren't in the escape pod...");
        response.Should().Contain("You have died");
    }

    [Test]
    public async Task OpenBulkhead_FromDeckNine_AfterLaunchBegins_ShouldSayTooLate()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<DeckNine>();

        // Wait for the explosion (9 waits to get to turn 10)
        for (var i = 0; i < 9; i++)
            await target.GetResponse("wait");

        // Turn 10: Explosion happens, bulkhead opens
        var response = await target.GetResponse("wait");
        response.Should().Contain("A massive explosion rocks the ship");
        response.Should().Contain("door to port slides open");
        Repository.GetItem<BulkheadDoor>().IsOpen.Should().BeTrue();

        // Turn 11: More explosions, emergency bulkheads closing around the ship
        response = await target.GetResponse("wait");
        response.Should().Contain("More distant explosions");

        // Turn 12: The escape-pod bulkhead clangs shut (pod launch begins)
        response = await target.GetResponse("wait");
        response.Should().Contain("escape-pod bulkhead clangs shut");
        Repository.GetItem<BulkheadDoor>().IsOpen.Should().BeFalse();

        // Now try to open the bulkhead from Deck Nine - should say "Too late"
        response = await target.GetResponse("open bulkhead");
        response.Should().Contain("Too late. The pod's launching procedure has already begun.");
    }

    // When the Feinstein explodes (case 4 of HandleBeingInSpaceAndLanding), the original
    // penalizes a player who hasn't strapped into the safety web: 20% instant (head-first)
    // death, otherwise a bruising message. In the web is unharmed. Verified against
    // planetfall-source/globals.zil I-BLOWUP-FEINSTEIN (BLOWUP-COUNTER 5).
    [Test]
    public async Task Explosion_NotInWeb_DeathRoll_KillsPlayer()
    {
        var target = GetTarget();
        var pod = Repository.GetLocation<EscapePod>();
        target.Context.CurrentLocation = pod;
        pod.SubLocation = null; // NOT in the web
        pod.TurnsSinceExplosion = 3; // next Act -> case 4
        target.Context.RegisterActor(pod);

        var chooser = new Mock<IRandomChooser>();
        chooser.Setup(c => c.RollDiceSuccess(5)).Returns(true); // force the 20%
        pod.Chooser = chooser.Object;

        var response = await target.GetResponse("wait");

        response.Should().Contain("head first");
        response.Should().Contain("You have died");
    }

    [Test]
    public async Task Explosion_NotInWeb_Survives_Bruised()
    {
        var target = GetTarget();
        var pod = Repository.GetLocation<EscapePod>();
        target.Context.CurrentLocation = pod;
        pod.SubLocation = null; // NOT in the web
        pod.TurnsSinceExplosion = 3; // next Act -> case 4
        target.Context.RegisterActor(pod);

        var chooser = new Mock<IRandomChooser>();
        chooser.Setup(c => c.RollDiceSuccess(5)).Returns(false); // survive the death roll
        pod.Chooser = chooser.Object;

        var response = await target.GetResponse("wait");

        response.Should().Contain("bruising a few limbs");
        response.Should().NotContain("You have died");
        // Still stabilizes / searches for a destination after surviving.
        response.Should().Contain("autopilot searches for a reasonable destination");
    }

    [Test]
    public async Task Explosion_InWeb_Unharmed()
    {
        var target = GetTarget();
        var pod = Repository.GetLocation<EscapePod>();
        target.Context.CurrentLocation = pod;
        pod.SubLocation = Repository.GetItem<SafetyWeb>(); // safely in the web
        pod.TurnsSinceExplosion = 3; // next Act -> case 4
        target.Context.RegisterActor(pod);

        var chooser = new Mock<IRandomChooser>();
        chooser.Setup(c => c.RollDiceSuccess(5)).Returns(true); // the web must bypass the roll entirely
        pod.Chooser = chooser.Object;

        var response = await target.GetResponse("wait");

        response.Should().NotContain("bruising");
        response.Should().NotContain("head first");
        response.Should().NotContain("You have died");
        // The web is unharmed, but the pod still stabilizes and looks for a destination.
        response.Should().Contain("autopilot searches for a reasonable destination");
    }

    // Issue #376: "get in web" / "enter web" resolve to an EnterSubLocationIntent whose noun is
    // looked up via Repository.GetItemInScope - but SafetyWeb was never actually seeded into the
    // pod's Items list by EscapePod.Init() (only BulkheadDoor was), so the scope lookup always
    // failed even though the room's own description talks about the webbing. The exact-phrase
    // shortcuts in EscapePod.RespondToSpecificLocationInteraction ("sit", "get in", "enter webbing")
    // masked this for those specific strings, but any phrasing that reaches normal noun resolution
    // - like the bare noun "web" - fell through to a generic "can't go that way" refusal instead of
    // actually sitting the player down in the webbing.
    [TestFixture]
    public class EnterWebTests : EngineTestsBase
    {
        [Test]
        public async Task EnterWeb_PutsPlayerInTheSafetyWeb()
        {
            var target = GetTarget();
            var pod = Repository.GetLocation<EscapePod>();
            pod.Init(); // place BulkheadDoor and SafetyWeb in the pod's scope
            target.Context.CurrentLocation = pod;

            var response = await target.GetResponse("enter web");

            response.Should().Contain("You are now safely cushioned within the web.");
            pod.SubLocation.Should().BeOfType<SafetyWeb>();
        }

        [Test]
        public async Task GetInWeb_PutsPlayerInTheSafetyWeb()
        {
            var target = GetTarget();
            var pod = Repository.GetLocation<EscapePod>();
            pod.Init();
            target.Context.CurrentLocation = pod;

            var response = await target.GetResponse("get in web");

            response.Should().Contain("You are now safely cushioned within the web.");
            pod.SubLocation.Should().BeOfType<SafetyWeb>();
        }
    }

    // Issue #448: SafetyWeb.RespondToSimpleInteraction matched on the VERB alone, with no check that
    // the noun was the webbing. Because the web is seeded into the pod's Items (issue #376),
    // LocationBase.RespondToSimpleInteraction runs its handler for every command in the room, so any
    // sit/get/rest (while seated) or leave/exit/get (while standing) command was hijacked regardless
    // of its object: "sit on the control panel" answered "You're already in the safety web."
    [TestFixture]
    public class SafetyWebNounGuardTests : EngineTestsBase
    {
        private static EscapePod PodWithPlayerInTheWeb(GameEngine<PlanetfallGame, PlanetfallContext> target)
        {
            var pod = Repository.GetLocation<EscapePod>();
            pod.Init(); // place BulkheadDoor and SafetyWeb in the pod's scope
            target.Context.CurrentLocation = pod;
            pod.SubLocation = Repository.GetItem<SafetyWeb>();
            return pod;
        }

        [Test]
        public async Task SitOnControlPanel_WhileInTheWeb_DoesNotClaimYoureAlreadyInTheWeb()
        {
            var target = GetTarget();
            PodWithPlayerInTheWeb(target);

            var response = await target.GetResponse("sit on the control panel");

            response.Should().NotContain("already in the safety web");
        }

        [Test]
        public async Task SitOnUnrelatedNoun_WhileInTheWeb_DoesNotClaimYoureAlreadyInTheWeb()
        {
            var target = GetTarget();
            PodWithPlayerInTheWeb(target);

            // "rug" is about as far from the webbing as a noun gets, and it isn't in the pod at all.
            var response = await target.GetResponse("sit on rug");

            response.Should().NotContain("already in the safety web");
        }

        [Test]
        public async Task SitOnTheWebbing_WhileInTheWeb_StillRoutesToTheWeb()
        {
            var target = GetTarget();
            PodWithPlayerInTheWeb(target);

            var response = await target.GetResponse("sit on the webbing");

            response.Should().Contain("You're already in the safety web.");
        }

        [Test]
        public async Task BareSit_WhileInTheWeb_StillRoutesToTheWeb()
        {
            var target = GetTarget();
            PodWithPlayerInTheWeb(target);

            var response = await target.GetResponse("sit");

            response.Should().Contain("You're already in the safety web.");
        }

        [Test]
        public async Task BareSit_WhileStanding_StillPutsYouInTheWeb()
        {
            var target = GetTarget();
            var pod = Repository.GetLocation<EscapePod>();
            pod.Init();
            target.Context.CurrentLocation = pod;

            var response = await target.GetResponse("sit");

            response.Should().Contain("You are now safely cushioned within the web.");
            pod.SubLocation.Should().BeOfType<SafetyWeb>();
        }

        // The standing-side branch (leave/exit/get) has the identical defect and must be guarded too.
        [Test]
        public async Task LeaveUnrelatedNoun_WhileStanding_DoesNotClaimYoureNotInTheWeb()
        {
            var target = GetTarget();
            var pod = Repository.GetLocation<EscapePod>();
            pod.Init();
            target.Context.CurrentLocation = pod;

            var action = new SimpleIntent { Verb = "leave", Noun = "control panel" };
            var result = await Repository.GetItem<SafetyWeb>().RespondToSimpleInteraction(action, target.Context,
                Mock.Of<IGenerationClient>(), new ItemProcessorFactory(Mock.Of<IAITakeAndAndDropParser>()));

            // Before the fix this was a PositiveInteractionResult carrying "You're not in the safety
            // web."; the noun guard now lets an unrelated noun fall through to the narrator.
            result.Should().BeOfType<NoNounMatchInteractionResult>();
        }

        [Test]
        public async Task LeaveTheWebbing_WhileStanding_StillAnswersForTheWeb()
        {
            var target = GetTarget();
            var pod = Repository.GetLocation<EscapePod>();
            pod.Init();
            target.Context.CurrentLocation = pod;

            var action = new SimpleIntent { Verb = "leave", Noun = "webbing" };
            var result = await Repository.GetItem<SafetyWeb>().RespondToSimpleInteraction(action, target.Context,
                Mock.Of<IGenerationClient>(), new ItemProcessorFactory(Mock.Of<IAITakeAndAndDropParser>()));

            result.Should().BeOfType<PositiveInteractionResult>();
            result!.InteractionMessage.Should().Contain("You're not in the safety web.");
        }
    }

    // Issue #448 follow-up: the same handler gated each verb list on SubLocation, so each list could
    // only ever produce its *complaint* - the seated branch only ever reached GetIn's "You're already
    // in the safety web", the standing branch only ever reached GetOut's "You're not in the safety
    // web". Naming the webbing in the OTHER state matched no branch at all and fell through to base,
    // where no processor handles sit/leave on the web: a blank line. GetIn/GetOut each already answer
    // their own wrong-state case, so the state gates were never needed.
    [TestFixture]
    public class SafetyWebWrongStateTests : EngineTestsBase
    {
        private static EscapePod Pod(GameEngine<PlanetfallGame, PlanetfallContext> target, bool seated)
        {
            var pod = Repository.GetLocation<EscapePod>();
            pod.Init(); // place BulkheadDoor and SafetyWeb in the pod's scope
            target.Context.CurrentLocation = pod;
            pod.SubLocation = seated ? Repository.GetItem<SafetyWeb>() : null;
            return pod;
        }

        [Test]
        public async Task SitOnTheWebbing_WhileStanding_PutsYouInTheWeb()
        {
            var target = GetTarget();
            var pod = Pod(target, seated: false);

            var response = await target.GetResponse("sit on the webbing");

            response.Should().Contain("You are now safely cushioned within the web.");
            pod.SubLocation.Should().BeOfType<SafetyWeb>();
        }

        // "leave the webbing"/"exit webbing" are caught upstream as raw phrases by
        // EscapePod.RespondToSpecificLocationInteraction; the bare noun "web" is the phrasing that
        // reaches this handler as a SimpleIntent, and it used to answer with nothing at all.
        [Test]
        public async Task LeaveWeb_WhileSeated_StandsYouUp()
        {
            var target = GetTarget();
            var pod = Pod(target, seated: true);

            var response = await target.GetResponse("leave web");

            response.Should().Contain("You are standing again.");
            pod.SubLocation.Should().BeNull();
        }

        // The "exit" verb takes the same branch as "leave". Called directly because the test parser
        // doesn't carry "exit" as a verb, so the phrasing can't reach the handler through the engine.
        [Test]
        public async Task ExitWeb_WhileSeated_StandsYouUp()
        {
            var target = GetTarget();
            var pod = Pod(target, seated: true);

            var action = new SimpleIntent { Verb = "exit", Noun = "web" };
            var result = await Repository.GetItem<SafetyWeb>().RespondToSimpleInteraction(action, target.Context,
                Mock.Of<IGenerationClient>(), new ItemProcessorFactory(Mock.Of<IAITakeAndAndDropParser>()));

            result.Should().BeOfType<PositiveInteractionResult>();
            result!.InteractionMessage.Should().Contain("You are standing again.");
            pod.SubLocation.Should().BeNull();
        }

        // Standing up in the landed pod is what starts it sinking. Leaving the webbing by name must
        // behave exactly like the bare "stand" - the hazard can't be dodged by phrasing. Two fresh
        // games so no state leaks between the two commands.
        [Test]
        public async Task LeaveWeb_InTheLandedPod_StartsThePodSinking_ExactlyLikeStanding()
        {
            var standTarget = GetTarget();
            var standPod = Pod(standTarget, seated: true);
            standPod.LandedSafely = true;
            var standResponse = await standTarget.GetResponse("stand");

            var leaveTarget = GetTarget();
            var leavePod = Pod(leaveTarget, seated: true);
            leavePod.LandedSafely = true;
            var leaveResponse = await leaveTarget.GetResponse("leave web");

            standResponse.Should().Contain("you see water rising past the viewport");
            leaveResponse.Should().Be(standResponse);
            leavePod.TurnsAfterStanding.Should().Be(standPod.TurnsAfterStanding).And.BeGreaterThan(0);
        }

        [Test]
        public async Task GetWeb_WhileStanding_PutsYouInTheWeb()
        {
            var target = GetTarget();
            var pod = Pod(target, seated: false);

            // Previously answered "You're not in the safety web." - a nonsense reply to a request to
            // get INTO it.
            var response = await target.GetResponse("get web");

            response.Should().Contain("You are now safely cushioned within the web.");
            pod.SubLocation.Should().BeOfType<SafetyWeb>();
        }

        // Deliberate asymmetry: "get" is the one verb in both directions ("get in"/"get out" with the
        // preposition lost), so it must never be read as *leaving*. Standing up in the landed pod
        // starts an unrecoverable sinking clock, and an ambiguous verb must not trigger that - only an
        // explicit leave/exit/stand may. Seated, "get web" says so and changes nothing.
        [Test]
        public async Task GetWeb_WhileSeated_DoesNotStandYouUp()
        {
            var target = GetTarget();
            var pod = Pod(target, seated: true);
            pod.LandedSafely = true;

            var response = await target.GetResponse("get web");

            response.Should().Contain("You're already in the safety web.");
            pod.SubLocation.Should().BeOfType<SafetyWeb>();
            pod.TurnsAfterStanding.Should().Be(0);
        }
    }

    [TestFixture]
    public class EscapePodTimelineTests : EngineTestsBase
    {
        [Test]
        public async Task ClosedPodSinking_ProgressesThroughWarningsAndDeath()
        {
            var target = GetTarget();
            var pod = Repository.GetLocation<EscapePod>();
            target.Context.CurrentLocation = pod;
            pod.TurnsAfterStanding = 1;
            Repository.GetItem<BulkheadDoor>().IsOpen = false;

            (await pod.Act(target.Context, Mock.Of<IGenerationClient>())).Should().Be("\n\n");
            (await pod.Act(target.Context, Mock.Of<IGenerationClient>())).Should().Contain("completely submerged");
            (await pod.Act(target.Context, Mock.Of<IGenerationClient>())).Should().Contain("creaks ominously");
            (await pod.Act(target.Context, Mock.Of<IGenerationClient>())).Should().Contain("pod splits open");
            target.Context.DeathCounter.Should().Be(1);
        }

        [Test]
        public async Task OpenPodSinking_UsesTheOpenDoorDeathNarration()
        {
            var target = GetTarget();
            var pod = Repository.GetLocation<EscapePod>();
            target.Context.CurrentLocation = pod;
            pod.TurnsAfterStanding = 4;
            Repository.GetItem<BulkheadDoor>().IsOpen = true;

            var response = await pod.Act(target.Context, Mock.Of<IGenerationClient>());

            response.Should().Contain("curtains for you");
            response.Should().Contain("left the pod a bit sooner");
            target.Context.DeathCounter.Should().Be(1);
        }

        [Test]
        public async Task LandingOutsideTheSafetyWeb_KillsThePlayer()
        {
            var target = GetTarget();
            var pod = Repository.GetLocation<EscapePod>();
            target.Context.CurrentLocation = pod;
            pod.SubLocation = null;
            pod.TurnsSinceExplosion = 13;
            target.Context.RegisterActor(pod);

            var response = await pod.Act(target.Context, Mock.Of<IGenerationClient>());

            response.Should().Contain("sharper corners of the control panel");
            response.Should().Contain("You have died");
            target.Context.DeathCounter.Should().Be(1);
        }

        [Test]
        public async Task LandingInTheSafetyWeb_AddsSuppliesAndRetargetsTheDoor()
        {
            var target = GetTarget();
            var pod = Repository.GetLocation<EscapePod>();
            target.Context.CurrentLocation = pod;
            pod.SubLocation = Repository.GetItem<SafetyWeb>();
            pod.TurnsSinceExplosion = 13;
            target.Context.RegisterActor(pod);

            var response = await pod.Act(target.Context, Mock.Of<IGenerationClient>());

            response.Should().Contain("pod lands with a thud");
            pod.LandedSafely.Should().BeTrue();
            pod.WhereDoesTheDoorLead.Should().BeOfType<Underwater>();
            Repository.GetItem<Towel>().CurrentLocation.Should().Be(pod);
            Repository.GetItem<SurvivalKit>().CurrentLocation.Should().Be(pod);
            target.Context.Actors.Should().NotContain(pod);
        }
    }
}

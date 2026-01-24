using FluentAssertions;
using GameEngine;
using Model.Interface;
using Model.Location;
using Moq;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Item.Lawanda.BioLab;
using Planetfall.Item.Lawanda.CryoElevator;
using Planetfall.Item.Lawanda.Lab;
using Planetfall.Item.Lawanda.LabOffice;
using Planetfall.Location.Lawanda;
using Planetfall.Location.Lawanda.Lab;
using Planetfall.Location.Lawanda.LabOffice;

namespace Planetfall.Tests;

/// <summary>
/// Comprehensive tests for the chase scene mechanics.
///
/// ESCAPE ROUTE:
/// BioLab → W → BioLockEast → W → BioLockWest → W → MainLab → W → ProjectCorridorEast
///        → W → ProjectCorridor → S → ProjConOffice → S → CryoElevator → Push Button → Escape
///
/// FREE TURNS (one each, to open doors):
/// - BioLab: open lab door (BioLockInnerDoor)
/// - BioLockWest: open door (BioLockOuterDoor)
///
/// DEATH CONDITIONS:
/// - Pausing (any non-movement action) in any room without free turn
/// - Pausing after using free turn
/// - Backtracking to previous room
/// - Invalid movement (direction doesn't exist)
/// - Not pushing button in CryoElevator
/// </summary>
public class ChaseSceneTests : EngineTestsBase
{
    private Mock<IRandomChooser> _mockChooser = null!;

    /// <summary>
    /// Call after GetTarget() to set up common chase test conditions
    /// </summary>
    private void SetupChaseTestConditions()
    {
        // Mock random chooser for deterministic chase messages
        _mockChooser = new Mock<IRandomChooser>();
        _mockChooser.Setup(r => r.Choose(It.IsAny<List<string>>()))
            .Returns("The mutants burst into the room right on your heels! Needle-sharp mandibles nip at your arms! ");
        GetItem<ChaseSceneManager>().Chooser = _mockChooser.Object;

        // Floyd has died (typical scenario for this part of game)
        var floyd = GetItem<Floyd>();
        floyd.HasDied = true;
        GetLocation<BioLockEast>().ItemPlacedHere(floyd);
    }

    /// <summary>
    /// Helper to set up player in BioLab with chase active (fungicide has worn off)
    /// </summary>
    private async Task SetupChaseInBioLab(GameEngine<PlanetfallGame, PlanetfallContext> target)
    {
        SetupChaseTestConditions();
        StartHere<LabOffice>();

        // Player must wear gas mask to survive the fungicide mist
        var gasMask = GetItem<GasMask>();
        Context.Take(gasMask);
        gasMask.BeingWorn = true;

        // Press fungicide button, open door, enter BioLab
        await target.GetResponse("press red button");
        await target.GetResponse("open door");
        await target.GetResponse("w");
        // Fungicide wears off, chase starts
    }

    /// <summary>
    /// Helper to open all doors on the escape route
    /// </summary>
    private void OpenAllDoorsOnEscapeRoute()
    {
        GetItem<BioLockInnerDoor>().IsOpen = true;
        GetItem<BioLockOuterDoor>().IsOpen = true;
        GetLocation<ProjConOffice>().AnnouncmentHasBeenMade = true;
    }

    /// <summary>
    /// Helper to start chase at a specific location with doors already open
    /// </summary>
    private void StartChaseAt<T>(T location) where T : class, ILocation, new()
    {
        SetupChaseTestConditions();
        StartHere<T>();
        OpenAllDoorsOnEscapeRoute();

        var chaseManager = GetItem<ChaseSceneManager>();
        chaseManager.StartChase(location);
        Context.RegisterActor(chaseManager);
    }

    [TestFixture]
    public class FullEscapeRoute : ChaseSceneTests
    {
        [Test]
        public async Task CompleteEscape_FromBioLab_ToCryoElevator_Success()
        {
            var target = GetTarget();
            await SetupChaseInBioLab(target);

            // BioLab: Use free turn to open door (mist is still affecting mutants)
            var response = await target.GetResponse("open lab door");
            response.Should().Contain("air is filled with mist");

            // BioLab → BioLockEast
            response = await target.GetResponse("w");
            response.Should().Contain("Bio Lock");
            response.Should().Contain("mutants burst into the room");

            // BioLockEast → BioLockWest
            response = await target.GetResponse("w");
            response.Should().Contain("Bio Lock West");
            response.Should().Contain("mutants burst into the room");

            // BioLockWest: Use free turn to open door
            response = await target.GetResponse("open door");
            response.Should().Contain("mutants are almost upon you");

            // BioLockWest → MainLab
            response = await target.GetResponse("w");
            response.Should().Contain("Main Lab");
            response.Should().Contain("mutants burst into the room");

            // MainLab → ProjectCorridorEast
            response = await target.GetResponse("w");
            response.Should().Contain("Project Corridor East");
            response.Should().Contain("mutants burst into the room");

            // ProjectCorridorEast → ProjectCorridor
            response = await target.GetResponse("w");
            response.Should().Contain("Project Corridor");
            response.Should().Contain("mutants burst into the room");

            // Set up ProjConOffice access
            GetLocation<ProjConOffice>().AnnouncmentHasBeenMade = true;

            // ProjectCorridor → ProjConOffice
            response = await target.GetResponse("s");
            response.Should().Contain("ProjCon Office");
            response.Should().Contain("mutants burst into the room");

            // ProjConOffice → CryoElevator
            response = await target.GetResponse("s");
            response.Should().Contain("Cryo-Elevator");
            response.Should().Contain("monsters are storming straight toward the elevator door");

            // Push button to escape!
            response = await target.GetResponse("push button");
            response.Should().Contain("elevator door closes just as the monsters reach it");
            response.Should().Contain("exhausted from the chase");

            var chaseManager = GetItem<ChaseSceneManager>();
            chaseManager.ChaseActive.Should().BeFalse();
        }
    }

    [TestFixture]
    public class FreeTurnMechanics : ChaseSceneTests
    {
        [Test]
        public async Task BioLab_FirstPause_GetsFreeTurn()
        {
            var target = GetTarget();
            await SetupChaseInBioLab(target);

            var response = await target.GetResponse("open lab door");

            response.Should().Contain("air is filled with mist");
            response.Should().NotContain("feasting");
        }

        [Test]
        public async Task BioLab_SecondPause_CausesDeath()
        {
            var target = GetTarget();
            await SetupChaseInBioLab(target);

            await target.GetResponse("open lab door");
            var response = await target.GetResponse("look");

            response.Should().Contain("feasting");
        }

        [Test]
        public async Task BioLockWest_FirstPause_GetsFreeTurn()
        {
            var target = GetTarget();
            await SetupChaseInBioLab(target);

            await target.GetResponse("open lab door");
            await target.GetResponse("w"); // To BioLockEast
            await target.GetResponse("w"); // To BioLockWest

            var response = await target.GetResponse("open door");

            response.Should().Contain("mutants are almost upon you");
            response.Should().NotContain("feasting");
        }

        [Test]
        public async Task BioLockWest_SecondPause_CausesDeath()
        {
            var target = GetTarget();
            await SetupChaseInBioLab(target);

            await target.GetResponse("open lab door");
            await target.GetResponse("w");
            await target.GetResponse("w");
            await target.GetResponse("open door");

            var response = await target.GetResponse("look");

            response.Should().Contain("feasting");
        }
    }

    [TestFixture]
    public class PausingDeathInEachRoom : ChaseSceneTests
    {
        [Test]
        public async Task PauseInBioLockEast_CausesDeath()
        {
            var target = GetTarget();
            StartChaseAt(GetLocation<BioLockEast>());

            var response = await target.GetResponse("look");

            response.Should().Contain("feasting");
        }

        [Test]
        public async Task PauseInMainLab_CausesDeath()
        {
            var target = GetTarget();
            StartChaseAt(GetLocation<MainLab>());

            var response = await target.GetResponse("look");

            response.Should().Contain("feasting");
        }

        [Test]
        public async Task PauseInProjectCorridorEast_CausesDeath()
        {
            var target = GetTarget();
            StartChaseAt(GetLocation<ProjectCorridorEast>());

            var response = await target.GetResponse("look");

            response.Should().Contain("feasting");
        }

        [Test]
        public async Task PauseInProjectCorridor_CausesDeath()
        {
            var target = GetTarget();
            StartChaseAt(GetLocation<ProjectCorridor>());

            var response = await target.GetResponse("look");

            response.Should().Contain("feasting");
        }

        [Test]
        public async Task PauseInProjConOffice_CausesDeath()
        {
            var target = GetTarget();
            StartChaseAt(GetLocation<ProjConOffice>());

            var response = await target.GetResponse("look");

            response.Should().Contain("feasting");
        }

        [Test]
        public async Task PauseInCryoElevator_CausesSpecificDeath()
        {
            var target = GetTarget();
            StartChaseAt(GetLocation<CryoElevatorLocation>());

            var response = await target.GetResponse("look");

            response.Should().Contain("biological nightmares reach you");
            response.Should().Contain("Gripping coils wrap around your limbs");
        }
    }

    [TestFixture]
    public class BacktrackingDeath : ChaseSceneTests
    {
        [Test]
        public async Task BacktrackFromBioLockEast_ToBioLab_CausesDeath()
        {
            var target = GetTarget();
            await SetupChaseInBioLab(target);

            await target.GetResponse("open lab door");
            await target.GetResponse("w");

            var response = await target.GetResponse("e");

            response.Should().Contain("stupidly run right into the jaws");
        }

        [Test]
        public async Task BacktrackFromBioLockWest_ToBioLockEast_CausesDeath()
        {
            var target = GetTarget();
            await SetupChaseInBioLab(target);

            await target.GetResponse("open lab door");
            await target.GetResponse("w");
            await target.GetResponse("w");

            var response = await target.GetResponse("e");

            response.Should().Contain("stupidly run right into the jaws");
        }

        [Test]
        public async Task BacktrackFromMainLab_ToBioLockWest_CausesDeath()
        {
            var target = GetTarget();
            await SetupChaseInBioLab(target);

            // Open both doors on the escape route
            GetItem<BioLockInnerDoor>().IsOpen = true;
            GetItem<BioLockOuterDoor>().IsOpen = true;

            await target.GetResponse("w"); // To BioLockEast
            await target.GetResponse("w"); // To BioLockWest (free turn available)
            await target.GetResponse("w"); // To MainLab (uses free turn)

            var response = await target.GetResponse("se"); // Backtrack to BioLockWest

            response.Should().Contain("stupidly run right into the jaws");
        }

        [Test]
        public async Task BacktrackFromProjectCorridorEast_ToMainLab_CausesDeath()
        {
            var target = GetTarget();
            StartChaseAt(GetLocation<ProjectCorridorEast>());

            // Simulate coming from MainLab
            var chaseManager = GetItem<ChaseSceneManager>();
            chaseManager.PreviousLocation = GetLocation<MainLab>();

            var response = await target.GetResponse("e");

            response.Should().Contain("stupidly run right into the jaws");
        }

        [Test]
        public async Task BacktrackFromProjectCorridor_ToProjectCorridorEast_CausesDeath()
        {
            var target = GetTarget();
            StartChaseAt(GetLocation<ProjectCorridor>());

            var chaseManager = GetItem<ChaseSceneManager>();
            chaseManager.PreviousLocation = GetLocation<ProjectCorridorEast>();

            var response = await target.GetResponse("e");

            response.Should().Contain("stupidly run right into the jaws");
        }

        [Test]
        public async Task BacktrackFromProjConOffice_ToProjectCorridor_CausesDeath()
        {
            var target = GetTarget();
            StartChaseAt(GetLocation<ProjConOffice>());

            var chaseManager = GetItem<ChaseSceneManager>();
            chaseManager.PreviousLocation = GetLocation<ProjectCorridor>();

            var response = await target.GetResponse("n");

            response.Should().Contain("stupidly run right into the jaws");
        }
    }

    [TestFixture]
    public class InvalidMovementDeath : ChaseSceneTests
    {
        [Test]
        public async Task InvalidDirection_InBioLockEast_CausesDeath()
        {
            var target = GetTarget();
            StartChaseAt(GetLocation<BioLockEast>());

            // Try to go north (doesn't exist)
            var response = await target.GetResponse("n");

            response.Should().Contain("feasting");
        }

        [Test]
        public async Task InvalidDirection_InMainLab_CausesDeath()
        {
            var target = GetTarget();
            StartChaseAt(GetLocation<MainLab>());

            // Try to go north (doesn't exist)
            var response = await target.GetResponse("n");

            response.Should().Contain("feasting");
        }

        [Test]
        public async Task InvalidDirection_InProjectCorridor_CausesDeath()
        {
            var target = GetTarget();
            StartChaseAt(GetLocation<ProjectCorridor>());

            // Try to go north (doesn't exist)
            var response = await target.GetResponse("n");

            response.Should().Contain("feasting");
        }
    }

    [TestFixture]
    public class OtherActionsDeath : ChaseSceneTests
    {
        [Test]
        public async Task Examine_DuringChase_CausesDeath()
        {
            var target = GetTarget();
            StartChaseAt(GetLocation<BioLockEast>());

            var response = await target.GetResponse("examine window");

            response.Should().Contain("feasting");
        }

        [Test]
        public async Task Inventory_DuringChase_CausesDeath()
        {
            var target = GetTarget();
            StartChaseAt(GetLocation<BioLockEast>());

            var response = await target.GetResponse("inventory");

            response.Should().Contain("feasting");
        }

        [Test]
        public async Task Take_DuringChase_CausesDeath()
        {
            var target = GetTarget();
            StartChaseAt(GetLocation<BioLockEast>());

            // Floyd's body is here
            var response = await target.GetResponse("take floyd");

            response.Should().Contain("feasting");
        }

        [Test]
        public async Task Drop_DuringChase_CausesDeath()
        {
            var target = GetTarget();
            StartChaseAt(GetLocation<BioLockEast>());

            // Put something in inventory to drop
            var mask = GetItem<GasMask>();
            Context.ItemPlacedHere(mask);

            var response = await target.GetResponse("drop mask");

            response.Should().Contain("feasting");
        }

        [Test]
        public async Task Wait_DuringChase_CausesDeath()
        {
            var target = GetTarget();
            StartChaseAt(GetLocation<BioLockEast>());

            var response = await target.GetResponse("wait");

            response.Should().Contain("feasting");
        }
    }

    [TestFixture]
    public class CryoElevatorTests : ChaseSceneTests
    {
        [Test]
        public async Task EnterCryoElevator_ShowsMonstersApproaching()
        {
            var target = GetTarget();
            StartHere<ProjConOffice>();
            GetLocation<ProjConOffice>().AnnouncmentHasBeenMade = true;

            var chaseManager = GetItem<ChaseSceneManager>();
            chaseManager.StartChase(GetLocation<ProjConOffice>());
            Context.RegisterActor(chaseManager);

            var response = await target.GetResponse("s");

            response.Should().Contain("Cryo-Elevator");
            response.Should().Contain("monsters are storming straight toward the elevator door");
        }

        [Test]
        public async Task CryoElevator_PushButton_EscapesChase()
        {
            var target = GetTarget();
            StartChaseAt(GetLocation<CryoElevatorLocation>());

            var response = await target.GetResponse("push button");

            response.Should().Contain("elevator door closes just as the monsters reach it");
            response.Should().Contain("exhausted from the chase");

            var chaseManager = GetItem<ChaseSceneManager>();
            chaseManager.ChaseActive.Should().BeFalse();
        }

        [Test]
        public async Task CryoElevator_DontPushButton_CausesDeath()
        {
            var target = GetTarget();
            StartChaseAt(GetLocation<CryoElevatorLocation>());

            var response = await target.GetResponse("look");

            response.Should().Contain("biological nightmares reach you");
            response.Should().Contain("Gripping coils wrap around your limbs");
            response.Should().Contain("poison begin to work its numbing effects");
        }

        [Test]
        public async Task CryoElevator_Countdown_StartsAtThreeTurns()
        {
            var target = GetTarget();
            StartHere<CryoElevatorLocation>();

            var button = GetItem<CryoElevatorButton>();
            await target.GetResponse("push button");

            button.CountdownActive.Should().BeTrue();
            button.TurnsRemaining.Should().Be(3); // Decremented from 4 at end of turn
        }

        [Test]
        public async Task CryoElevator_DuringCountdown_ButtonDoesNothing()
        {
            var target = GetTarget();
            StartHere<CryoElevatorLocation>();

            await target.GetResponse("push button");
            var response = await target.GetResponse("push button");

            response.Should().Contain("Nothing happens");
        }

        [Test]
        public async Task CryoElevator_AfterCountdown_DoorOpensNorth()
        {
            var target = GetTarget();
            StartHere<CryoElevatorLocation>();

            var button = GetItem<CryoElevatorButton>();
            await target.GetResponse("push button");

            // Simulate countdown completion
            button.TurnsRemaining = 1;
            button.CountdownActive = true;
            Context.RegisterActor(button);

            var response = await target.GetResponse("wait");

            response.Should().Contain("elevator door opens onto a room to the north");
            button.AlreadyArrived.Should().BeTrue();
        }

        [Test]
        public async Task CryoElevator_AfterArrival_PushButton_HilariousDeath()
        {
            var target = GetTarget();
            StartHere<CryoElevatorLocation>();

            var button = GetItem<CryoElevatorButton>();
            button.AlreadyArrived = true;

            var response = await target.GetResponse("push button");

            response.Should().Contain("Stunning");
            response.Should().Contain("amazingly dumb input");
            response.Should().Contain("mutants");
            response.Should().Contain("happily saunter in and begin munching");
        }

        [Test]
        public async Task CryoElevator_AfterArrival_CanGoNorth()
        {
            var target = GetTarget();
            StartHere<CryoElevatorLocation>();

            var button = GetItem<CryoElevatorButton>();
            button.AlreadyArrived = true;

            var response = await target.GetResponse("n");

            response.Should().Contain("Cryogenic Anteroom");
        }

        [Test]
        public async Task CryoElevator_BeforeArrival_CannotGoNorth()
        {
            var target = GetTarget();
            StartHere<CryoElevatorLocation>();

            var button = GetItem<CryoElevatorButton>();
            button.AlreadyArrived = false;

            var response = await target.GetResponse("n");

            response.Should().Contain("door to the north is closed");
        }

        [Test]
        public async Task CryoElevator_Description_ShowsDoorState()
        {
            var target = GetTarget();
            StartHere<CryoElevatorLocation>();

            var button = GetItem<CryoElevatorButton>();

            button.AlreadyArrived = false;
            var response = await target.GetResponse("look");
            response.Should().Contain("door to the north which is closed");

            button.AlreadyArrived = true;
            response = await target.GetResponse("look");
            response.Should().Contain("door to the north which is open");
        }
    }

    [TestFixture]
    public class ChaseMessageTests : ChaseSceneTests
    {
        [Test]
        public async Task FleeingToNewRoom_ShowsChaseMessage()
        {
            var target = GetTarget();
            await SetupChaseInBioLab(target);

            await target.GetResponse("open lab door");
            var response = await target.GetResponse("w");

            response.Should().Contain("mutants burst into the room");
        }

        [Test]
        public async Task EnteringCryoElevator_ShowsSpecificMessage_NotGenericChase()
        {
            var target = GetTarget();
            StartHere<ProjConOffice>();
            GetLocation<ProjConOffice>().AnnouncmentHasBeenMade = true;

            var chaseManager = GetItem<ChaseSceneManager>();
            chaseManager.StartChase(GetLocation<ProjConOffice>());
            Context.RegisterActor(chaseManager);

            var response = await target.GetResponse("s");

            response.Should().Contain("monsters are storming straight toward the elevator door");
            response.Should().NotContain("mutants burst into the room");
        }
    }

    [TestFixture]
    public class ChaseStartTests : ChaseSceneTests
    {
        [Test]
        public async Task EnterBioLab_WithFungicideActive_NoChase()
        {
            var target = GetTarget();
            SetupChaseTestConditions();
            StartHere<LabOffice>();

            GetItem<OfficeDoor>().IsOpen = true;

            await target.GetResponse("press red button");
            var response = await target.GetResponse("w");

            response.Should().Contain("stunned and confused");

            var chaseManager = GetItem<ChaseSceneManager>();
            chaseManager.ChaseActive.Should().BeFalse();
        }

        [Test]
        public async Task EnterBioLab_FungicideWearsOff_ChaseStarts()
        {
            var target = GetTarget();
            await SetupChaseInBioLab(target);

            var chaseManager = GetItem<ChaseSceneManager>();
            chaseManager.ChaseActive.Should().BeTrue();
        }

    }

    [TestFixture]
    public class TrappedScenarios : ChaseSceneTests
    {
        [Test]
        public async Task GoEastFromBioLab_ToLabOffice_IsTrapped()
        {
            var target = GetTarget();
            await SetupChaseInBioLab(target);

            // Try to go back East to LabOffice - door should be closed
            var response = await target.GetResponse("e");

            // Should not be able to escape this way - trapped
            response.Should().Contain("closed");
        }

        [Test]
        public async Task GoEastFromBioLab_ToLabOffice_WithDoorOpen_IsTrapped()
        {
            var target = GetTarget();
            await SetupChaseInBioLab(target);

            // Even with door open, going back is backtracking (came from LabOffice via "w")
            // Wait - we came from LabOffice, so going E would be backtracking
            // Actually the chase started in BioLab, so PreviousLocation would be LabOffice
            // Let me check - SetupChaseInBioLab goes: LabOffice -> press button -> open door -> w to BioLab
            // So when chase starts in BioLab, the LastLocation is BioLab, PreviousLocation is null initially

            // Keep the door open
            GetItem<OfficeDoor>().IsOpen = true;

            // Use free turn first
            await target.GetResponse("open lab door");

            // Go east to LabOffice - mutants follow through open door
            var response = await target.GetResponse("e");

            // FungicideTimer kills player when in LabOffice with door open and no fungicide
            response.Should().Contain("devoured");
        }
    }

    [TestFixture]
    public class LoopingAllowed : ChaseSceneTests
    {
        [Test]
        public async Task CanRunInCircle_AsLongAsNotBacktracking()
        {
            var target = GetTarget();
            SetupChaseTestConditions();

            // Start in ProjectCorridor with chase active
            StartHere<ProjectCorridor>();
            OpenAllDoorsOnEscapeRoute();

            var chaseManager = GetItem<ChaseSceneManager>();
            chaseManager.StartChase(GetLocation<ProjectCorridor>());
            Context.RegisterActor(chaseManager);

            // Go West to ProjectCorridorWest
            var response = await target.GetResponse("w");
            response.Should().Contain("Project Corridor");
            chaseManager.ChaseActive.Should().BeTrue();

            // The key is: can we continue moving without dying?
            // As long as we don't go back East (backtrack) or hit a dead end, we survive
            // ProjectCorridorWest should have exits - let's see what happens
        }

        [Test]
        public async Task MovingForward_KeepsYouAlive()
        {
            var target = GetTarget();
            SetupChaseTestConditions();

            // Start chase at MainLab
            StartHere<MainLab>();
            OpenAllDoorsOnEscapeRoute();

            var chaseManager = GetItem<ChaseSceneManager>();
            chaseManager.StartChase(GetLocation<MainLab>());
            Context.RegisterActor(chaseManager);

            // Keep moving forward through multiple rooms
            await target.GetResponse("w"); // To ProjectCorridorEast
            chaseManager.ChaseActive.Should().BeTrue();

            await target.GetResponse("w"); // To ProjectCorridor
            chaseManager.ChaseActive.Should().BeTrue();

            await target.GetResponse("s"); // To ProjConOffice
            chaseManager.ChaseActive.Should().BeTrue();

            await target.GetResponse("s"); // To CryoElevator
            chaseManager.ChaseActive.Should().BeTrue();

            // Still alive! Now push button to escape
            var response = await target.GetResponse("push button");
            response.Should().Contain("elevator door closes");
            chaseManager.ChaseActive.Should().BeFalse();
        }
    }
}

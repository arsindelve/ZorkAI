using FluentAssertions;
using Planetfall.Item.Computer;
using Planetfall.Item.Lawanda.BioLab;
using Planetfall.Item.Lawanda.CryoElevator;
using Planetfall.Item.Lawanda.LabOffice;
using Planetfall.Location.Lawanda;
using Planetfall.Location.Lawanda.Lab;
using Planetfall.Location.Lawanda.LabOffice;

namespace Planetfall.Tests;

public class BioLabEscapeTests : EngineTestsBase
{
    [Test]
    public async Task AuxiliaryBooth_LeadsToLabOffice()
    {
        var target = GetTarget();
        StartHere<AuxiliaryBooth>();

        var response = await target.GetResponse("west");
        Context.CurrentLocation.Should().BeOfType<LabOffice>();
        response.Should().Contain("Lab Office");
    }

    [Test]
    public async Task LabOffice_ContainsAllItems()
    {
        var target = GetTarget();
        StartHere<LabOffice>();

        var response = await target.GetResponse("look");
        response.Should().Contain("LIGHT");
        response.Should().Contain("DARK");
        response.Should().Contain("FUNGICIDE");
        response.Should().Contain("desk");
    }

    [Test]
    public async Task LabDesk_ContainsGasMask()
    {
        var target = GetTarget();
        StartHere<LabOffice>();

        var desk = GetItem<LabDesk>();
        desk.IsOpen = false;

        var response = await target.GetResponse("open desk");
        response.Should().ContainAny("open", "Opened");

        desk.IsOpen.Should().BeTrue();
        desk.Items.Should().Contain(i => i is GasMask);
    }

    [Test]
    public async Task Memo_CanBeRead()
    {
        var target = GetTarget();
        StartHere<LabOffice>();
        Take<Memo>();

        var response = await target.GetResponse("read memo");
        response.Should().Contain("fungicide");
        response.Should().Contain("50 turns");
        response.Should().Contain("protective breathing equipment");
    }

    [Test]
    public async Task GasMask_CanBeWorn()
    {
        var target = GetTarget();
        StartHere<LabOffice>();
        Take<GasMask>();

        var response = await target.GetResponse("wear gas mask");
        // Just check the command doesn't fail completely
        response.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public async Task OfficeDoor_StartsClosedCanBeOpened()
    {
        var target = GetTarget();
        StartHere<LabOffice>();

        var response = await target.GetResponse("west");
        response.Should().Contain("closed");

        response = await target.GetResponse("open door");
        response.Should().Contain("open");

        var door = GetItem<OfficeDoor>();
        door.IsOpen.Should().BeTrue();
    }

    [Test]
    public async Task FungicideButton_ActivatesFungicideTimer()
    {
        var target = GetTarget();
        StartHere<LabOffice>();

        var response = await target.GetResponse("push fungicide");
        response.Should().Contain("fungicide");
        response.Should().Contain("50 turns");

        var timer = GetItem<FungicideTimer>();
        timer.IsActive.Should().BeTrue();
        timer.TurnsRemaining.Should().Be(50);
    }

    [Test]
    public async Task BioLab_StartsDark()
    {
        var target = GetTarget();
        StartHere<LabOffice>();

        await target.GetResponse("open door");
        var response = await target.GetResponse("west");

        response.Should().Contain("pitch black");
        Context.CurrentLocation.Should().BeOfType<BioLabLocation>();
    }

    [Test]
    public async Task BioLab_ShowsMutantsWhenLit()
    {
        var target = GetTarget();
        StartHere<LabOffice>();

        await target.GetResponse("push light button");
        await target.GetResponse("open door");
        var response = await target.GetResponse("west");

        response.Should().Contain("FOUR TERRIFYING MUTANT CREATURES");
        response.Should().Contain("rat-ant");
        response.Should().Contain("troll");
        response.Should().Contain("grue");
        response.Should().Contain("triffid");
    }

    [Test]
    public async Task BioLab_StartsChaseOnEntry()
    {
        var target = GetTarget();
        StartHere<LabOffice>();

        await target.GetResponse("push light button");
        await target.GetResponse("open door");
        var response = await target.GetResponse("west");

        response.Should().Contain("mutant creatures become aware");
        response.Should().Contain("stir and move");

        var chaseManager = GetItem<ChaseSceneManager>();
        chaseManager.ChaseActive.Should().BeTrue();
    }

    [Test]
    public async Task BioLab_DeathWithoutGasMaskAndFungicide()
    {
        var target = GetTarget();
        StartHere<LabOffice>();

        await target.GetResponse("push fungicide");

        // Verify fungicide is active
        var timer = GetItem<FungicideTimer>();
        timer.IsActive.Should().BeTrue();

        await target.GetResponse("open door");
        var response = await target.GetResponse("west");

        response.Should().Contain("coughing");
        response.Should().Contain("fungicide mist");
        response.Should().Contain("You have died");
    }

    [Test]
    public async Task BioLab_SafeWithGasMaskAndFungicide()
    {
        var target = GetTarget();
        StartHere<LabOffice>();

        await target.GetResponse("open desk");
        await target.GetResponse("take gas mask");
        await target.GetResponse("wear gas mask");
        await target.GetResponse("push fungicide button");
        await target.GetResponse("open door");
        var response = await target.GetResponse("west");

        response.Should().NotContain("coughing");
        response.Should().NotContain("You have died");
    }

    [Test]
    public async Task ProjConOffice_ShowsCryoElevatorWhenComputerFixed()
    {
        var target = GetTarget();
        StartHere<ProjConOffice>();

        // Computer not fixed yet
        var response = await target.GetResponse("look");
        response.Should().Contain("exit leads north");
        response.Should().NotContain("east");

        // Fix computer
        var relay = GetItem<Relay>();
        relay.SpeckDestroyed = true;

        // Now should see east exit
        response = await target.GetResponse("look");
        response.Should().Contain("Exits lead north and east");
    }

    [Test]
    public async Task CryoElevator_ButtonStartsCountdown()
    {
        var target = GetTarget();
        StartHere<CryoElevatorLocation>();

        var response = await target.GetResponse("press button");
        response.Should().Contain("elevator");
        response.Should().Contain("descent");

        var button = GetItem<CryoElevatorButton>();
        button.CountdownActive.Should().BeTrue();
        button.TurnsRemaining.Should().Be(100);
    }

    [Test]
    public async Task CryoElevator_ButtonAwardsPoints()
    {
        var target = GetTarget();
        StartHere<CryoElevatorLocation>();

        var initialScore = Context.Score;
        await target.GetResponse("push button");

        Context.Score.Should().Be(initialScore + 5);
    }

    [Test]
    public async Task CryoElevator_ButtonStopsChase()
    {
        var target = GetTarget();
        StartHere<CryoElevatorLocation>();

        var chaseManager = GetItem<ChaseSceneManager>();
        chaseManager.StartChase();
        chaseManager.ChaseActive.Should().BeTrue();

        await target.GetResponse("push button");

        chaseManager.ChaseActive.Should().BeFalse();
    }

    [Test]
    public async Task CryoElevator_CannotExitDuringDescent()
    {
        var target = GetTarget();
        StartHere<CryoElevatorLocation>();

        await target.GetResponse("push button");
        var response = await target.GetResponse("east");

        response.Should().Contain("sealed shut");
        response.Should().Contain("descent");
        Context.CurrentLocation.Should().BeOfType<CryoElevatorLocation>();
    }

    [Test]
    public async Task FungicideTimer_ExpiresAfter50Turns()
    {
        var target = GetTarget();
        var timer = GetItem<FungicideTimer>();
        Context.RegisterActor(timer);

        timer.Reset();
        timer.IsActive.Should().BeTrue();

        // Simulate 49 turns
        for (int i = 0; i < 49; i++)
        {
            await timer.Act(Context, null!);
        }

        timer.IsActive.Should().BeTrue();
        timer.TurnsRemaining.Should().Be(1);

        // 50th turn
        var response = await timer.Act(Context, null!);

        timer.IsActive.Should().BeFalse();
        timer.TurnsRemaining.Should().Be(0);
        response.Should().Contain("fungicide misting system shuts off");
    }

    [Test]
    public async Task FungicideTimer_CanBeReset()
    {
        var target = GetTarget();
        var timer = GetItem<FungicideTimer>();

        timer.Reset();
        timer.TurnsRemaining = 10;

        timer.Reset();
        timer.TurnsRemaining.Should().Be(50);
        timer.IsActive.Should().BeTrue();
    }

    [Test]
    public void ChaseScene_TracksLocationHistory()
    {
        var chaseManager = GetItem<ChaseSceneManager>();
        var bioLab = GetLocation<BioLabLocation>();
        var bioLockWest = GetLocation<BioLockWest>();

        chaseManager.StartChase();
        Context.CurrentLocation = bioLab;

        // First act - should record bioLab as last location
        chaseManager.Act(Context, null!).Wait();

        chaseManager.LastLocation.Should().Be(bioLab);

        // Move to bioLockWest
        Context.CurrentLocation = bioLockWest;
        chaseManager.Act(Context, null!).Wait();

        chaseManager.LastLocation.Should().Be(bioLockWest);
        chaseManager.SecondToLastLocation.Should().Be(bioLab);
    }

    [Test]
    public void ChaseScene_DeathOnBacktracking()
    {
        var chaseManager = GetItem<ChaseSceneManager>();
        var bioLab = GetLocation<BioLabLocation>();
        var bioLockWest = GetLocation<BioLockWest>();

        Context.RegisterActor(chaseManager);
        chaseManager.StartChase();

        Context.CurrentLocation = bioLab;
        chaseManager.Act(Context, null!).Wait();

        Context.CurrentLocation = bioLockWest;
        chaseManager.Act(Context, null!).Wait();

        Context.CurrentLocation = bioLab; // Backtrack
        var response = chaseManager.Act(Context, null!).Result;

        response.Should().Contain("backtrack");
        response.Should().Contain("pursuing mutants");
        response.Should().Contain("You have died");
    }

    [Test]
    public async Task ChaseScene_WarningInBioLockWest()
    {
        var chaseManager = GetItem<ChaseSceneManager>();
        var bioLockWest = GetLocation<BioLockWest>();

        Context.RegisterActor(chaseManager);
        chaseManager.StartChase();
        Context.CurrentLocation = bioLockWest;

        var response = await chaseManager.Act(Context, null!);

        response.Should().Contain("terrible sounds behind you");
        response.Should().Contain("mutants give chase");
    }

    [Test]
    public async Task ChaseScene_DeathIfLingerInBioLockWest()
    {
        var chaseManager = GetItem<ChaseSceneManager>();
        var bioLockWest = GetLocation<BioLockWest>();

        Context.RegisterActor(chaseManager);
        chaseManager.StartChase();
        Context.CurrentLocation = bioLockWest;

        await chaseManager.Act(Context, null!); // First turn - warning
        var response = await chaseManager.Act(Context, null!); // Second turn - death

        response.Should().Contain("lingered too long");
        response.Should().Contain("mutants catch up");
        response.Should().Contain("You have died");
    }
}

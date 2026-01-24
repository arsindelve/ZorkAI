using FluentAssertions;
using GameEngine;
using Planetfall.Item.Lawanda.LabOffice;
using Planetfall.Location.Lawanda.Lab;
using Planetfall.Location.Lawanda.LabOffice;

namespace Planetfall.Tests;

public class LabsAndDoorsTests : EngineTestsBase
{
    [Test]
    public async Task RadDoorClosedCannotGo()
    {
        var target = GetTarget();
        StartHere<MainLab>();

        var response = await target.GetResponse("NE");

        response.Should().Contain("The radiation-lock door is closed");
    }

    [Test]
    public async Task BioDoorClosedCannotGo()
    {
        var target = GetTarget();
        StartHere<MainLab>();

        var response = await target.GetResponse("SE");

        response.Should().Contain("The bio-lock door is closed");
    }

    [Test]
    public async Task Rad_OpenAndGo()
    {
        var target = GetTarget();
        StartHere<MainLab>();

        await target.GetResponse("open radiation-lock door");
        var response = await target.GetResponse("NE");

        response.Should().Contain("Radiation Lock West");
    }

    [Test]
    public async Task Bio_OpenAndGo()
    {
        var target = GetTarget();
        StartHere<MainLab>();

        await target.GetResponse("open bio-lock door");
        var response = await target.GetResponse("SE");

        response.Should().Contain("Bio Lock West");
    }

    [Test]
    public async Task BioWest_ClosedDoor_CannotGo()
    {
        var target = GetTarget();
        StartHere<BioLockWest>();

        var response = await target.GetResponse("W");

        response.Should().Contain("The bio-lock door is closed");
    }

    [Test]
    public async Task RadWest_ClosedDoor_CannotGo()
    {
        var target = GetTarget();
        StartHere<RadiationLockWest>();

        var response = await target.GetResponse("W");

        response.Should().Contain("The radiation-lock door is closed");
    }

    [Test]
    public async Task DoorDisambiguation()
    {
        var target = GetTarget();
        StartHere<MainLab>();

        var response = await target.GetResponse("open door");
        Console.WriteLine(response);

        response.Should().Contain("Do you mean the bio-lock door or the radiation-lock door?");
    }

    [Test]
    public async Task DoorDisambiguation_Resolve_Bio()
    {
        var target = GetTarget();
        StartHere<MainLab>();

        var response = await target.GetResponse("open door");
        Console.WriteLine(response);

        response = await target.GetResponse("bio-lock");
        Console.WriteLine(response);

        response = await target.GetResponse("SE");
        Console.WriteLine(response);

        response.Should().Contain("Bio Lock West");
    }

    [Test]
    public async Task DoorDisambiguation_Resolve_Rad()
    {
        var target = GetTarget();
        StartHere<MainLab>();

        var response = await target.GetResponse("open door");
        Console.WriteLine(response);

        response = await target.GetResponse("radiation");
        Console.WriteLine(response);

        response = await target.GetResponse("NE");
        Console.WriteLine(response);

        response.Should().Contain("Radiation Lock West");
    }

    [Test]
    public async Task BioWest_OpenDoor_CanGo()
    {
        var target = GetTarget();
        StartHere<BioLockWest>();

        await target.GetResponse("open door");
        var response = await target.GetResponse("W");

        response.Should().Contain("Main Lab");
    }

    [Test]
    public async Task RadWest_OpenDoor_CanGo()
    {
        var target = GetTarget();
        StartHere<RadiationLockWest>();

        await target.GetResponse("open door");
        var response = await target.GetResponse("W");

        response.Should().Contain("Main Lab");
    }

    [Test]
    public async Task BioEast_OpenDoor_Die()
    {
        var target = GetTarget();
        StartHere<BioLockWest>();

        await target.GetResponse("E");
        var response = await target.GetResponse("open door");

        response.Should().Contain("devour");
    }

    [Test]
    public async Task BioWest_OpenDoor_CloseDoor_CannotGo()
    {
        var target = GetTarget();
        StartHere<BioLockWest>();

        await target.GetResponse("open door");
        var response = await target.GetResponse("close door");
        response.Should().Contain("door closes");

        response = await target.GetResponse("W");
        response.Should().Contain("is closed");
    }

    [Test]
    public async Task RadWest_OpenDoorCloseDoor_CannotGo()
    {
        var target = GetTarget();
        StartHere<RadiationLockWest>();

        await target.GetResponse("open door");
        var response = await target.GetResponse("close door");
        response.Should().Contain("door closes");

        response = await target.GetResponse("W");
        response.Should().Contain("is closed");
    }

    [Test]
    public async Task RadLab_HaveSpool()
    {
        var target = GetTarget();
        StartHere<RadiationLab>();

        var response = await target.GetResponse("look");
        response.Should().Contain("Sitting on a long table is a small brown spool");
    }

    [Test]
    public async Task RadLab_HaveLamp()
    {
        var target = GetTarget();
        StartHere<RadiationLab>();

        var response = await target.GetResponse("look");
        response.Should().Contain("There is a powerful portable lamp here, currently off");
    }

    [Test]
    public async Task RadLab_ExamineSpool()
    {
        var target = GetTarget();
        StartHere<RadiationLab>();

        var response = await target.GetResponse("examine spool");
        response.Should().Contain("The spool is labelled \"Instrukshunz foor Reepaareeng Reepaar Roobots");
    }

    [Test]
    public async Task RadLab_GetSickAndDie()
    {
        var target = GetTarget();
        StartHere<RadiationLockEast>();

        await target.GetResponse("open door");
        await target.GetResponse("e");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");

        var response = await target.GetResponse("wait");
        response.Should().Contain("You suddenly feel sick and dizzy");

        response = await target.GetResponse("wait");
        response.Should()
            .Contain("You feel incredibly nauseous and begin vomiting. Also, all your hair has fallen out");

        response = await target.GetResponse("wait");
        response.Should().Contain("It seems you have picked up a bad case of radiation poisoning");
        response.Should().Contain("died");
    }

    [Test]
    public async Task RadLab_ExamineCrack()
    {
        var target = GetTarget();
        StartHere<RadiationLab>();

        var response = await target.GetResponse("examine crack");
        response.Should().Contain("The crack is too small to go through, but large enough to look through.");
    }

    [Test]
    public async Task RadLab_LookThroughCrack()
    {
        var target = GetTarget();
        StartHere<RadiationLab>();

        var response = await target.GetResponse("look through crack");
        response.Should().Contain("You see a dimly lit Bio Lab. Sinister shapes lurk about within.");
    }

    [Test]
    public async Task RadLab_ExamineEquipment()
    {
        var target = GetTarget();
        StartHere<RadiationLab>();

        var response = await target.GetResponse("examine equipment");
        response.Should()
            .Contain(
                "The equipment here is so complicated that you couldn't even begin to figure out how to operate it.");
    }

    [Test]
    public void LabDesk_Init_SetsIsOpenToFalse()
    {
        Repository.Reset();
        var desk = GetItem<LabDesk>();

        desk.IsOpen.Should().BeFalse();
    }

    [Test]
    public void LabDesk_Init_ContainsGasMask()
    {
        Repository.Reset();
        var desk = GetItem<LabDesk>();
        var gasMask = GetItem<GasMask>();

        desk.Items.Should().Contain(gasMask);
    }

    [Test]
    public void LabDesk_ExaminationDescription_WhenClosed_MemoNotTaken_DescribesMemo()
    {
        Repository.Reset();
        var desk = GetItem<LabDesk>();
        desk.IsOpen = false;
        GetItem<Memo>().HasEverBeenPickedUp = false;

        desk.ExaminationDescription.Should().Contain("memo");
        desk.ExaminationDescription.Should().Contain("closed");
        desk.ExaminationDescription.Should().Contain("doesn't look locked");
    }

    [Test]
    public void LabDesk_ExaminationDescription_WhenOpen_MemoNotTaken_ListsContents()
    {
        Repository.Reset();
        var desk = GetItem<LabDesk>();
        desk.IsOpen = true;
        GetItem<Memo>().HasEverBeenPickedUp = false;

        desk.ExaminationDescription.Should().Contain("desk");
        desk.ExaminationDescription.Should().Contain("gas mask");
    }

    [Test]
    public void LabDesk_ExaminationDescription_WhenClosed_MemoTaken_ShowsSimpleDescription()
    {
        Repository.Reset();
        var desk = GetItem<LabDesk>();
        desk.IsOpen = false;
        GetItem<Memo>().HasEverBeenPickedUp = true;

        desk.ExaminationDescription.Should().Be("The desk is closed. ");
        desk.ExaminationDescription.Should().NotContain("memo");
    }

    [Test]
    public void LabDesk_ExaminationDescription_WhenOpen_MemoTaken_Empty_ShowsSimpleDescription()
    {
        Repository.Reset();
        var desk = GetItem<LabDesk>();
        desk.Items.Clear();
        desk.IsOpen = true;
        GetItem<Memo>().HasEverBeenPickedUp = true;

        desk.ExaminationDescription.Should().Be("The desk is open. ");
    }

    [Test]
    public void LabDesk_ExaminationDescription_WhenOpen_MemoTaken_HasItems_ShowsContents()
    {
        Repository.Reset();
        var desk = GetItem<LabDesk>();
        desk.Init(); // This puts the gas mask inside
        desk.IsOpen = true;
        GetItem<Memo>().HasEverBeenPickedUp = true;

        desk.ExaminationDescription.Should().Contain("gas mask");
    }

    [Test]
    public async Task LabDesk_ExamineDesk_WhenClosed_MemoNotTaken_ShowsMemoDescription()
    {
        var target = GetTarget();
        StartHere<LabOffice>();
        GetItem<LabDesk>().IsOpen = false;
        GetItem<Memo>().HasEverBeenPickedUp = false;

        var response = await target.GetResponse("examine desk");

        response.Should().Contain("memo");
        response.Should().Contain("closed");
    }

    [Test]
    public async Task LabDesk_ExamineDesk_WhenOpen_MemoNotTaken_ShowsContents()
    {
        var target = GetTarget();
        StartHere<LabOffice>();
        var desk = GetItem<LabDesk>();
        desk.IsOpen = true;
        GetItem<Memo>().HasEverBeenPickedUp = false;

        var response = await target.GetResponse("examine desk");

        response.Should().Contain("gas mask");
    }

    [Test]
    public async Task LabDesk_ExamineDesk_AfterTakingMemo_ShowsSimpleDescription()
    {
        var target = GetTarget();
        StartHere<LabOffice>();
        GetItem<LabDesk>().IsOpen = false;

        // Take the memo
        await target.GetResponse("take memo");

        var response = await target.GetResponse("examine desk");

        response.Should().Contain("The desk is closed");
        response.Should().NotContain("memo");
    }

    [Test]
    public async Task LabDesk_OpenDesk_OpensIt()
    {
        var target = GetTarget();
        StartHere<LabOffice>();
        var desk = GetItem<LabDesk>();
        desk.IsOpen = false;

        await target.GetResponse("open desk");

        desk.IsOpen.Should().BeTrue();
    }

    [Test]
    public async Task LabDesk_CloseDesk_ClosesIt()
    {
        var target = GetTarget();
        StartHere<LabOffice>();
        var desk = GetItem<LabDesk>();
        desk.IsOpen = true;

        await target.GetResponse("close desk");

        desk.IsOpen.Should().BeFalse();
    }

    [Test]
    public async Task LabDesk_TakeGasMask_FromOpenDesk()
    {
        var target = GetTarget();
        StartHere<LabOffice>();
        var desk = GetItem<LabDesk>();
        desk.IsOpen = true;

        await target.GetResponse("take mask from desk");

        Context.HasItem<GasMask>().Should().BeTrue();
        desk.Items.Should().NotContain(GetItem<GasMask>());
    }

    [Test]
    public async Task LabDesk_TakeGasMask_FromClosedDesk_Fails()
    {
        var target = GetTarget();
        StartHere<LabOffice>();
        var desk = GetItem<LabDesk>();
        desk.IsOpen = false;

        await target.GetResponse("take mask from desk");

        Context.HasItem<GasMask>().Should().BeFalse();
    }

    [Test]
    public async Task LabOffice_PressRedButton_ActivatesFungicide()
    {
        var target = GetTarget();
        StartHere<LabOffice>();

        // Use "press red" to avoid noun matching issues with "red button"
        var response = await target.GetResponse("press red");

        response.Should().Contain("hissing");
        var timer = GetItem<FungicideTimer>();
        timer.IsActive.Should().BeTrue();
        // Timer starts at FullProtection and stays there (activation turn is free)
        timer.State.Should().Be(FungicideTimer.FungicideState.FullProtection);
    }

    [Test]
    public async Task LabOffice_PressRedButton_RegistersTimerAsActor()
    {
        var target = GetTarget();
        StartHere<LabOffice>();

        await target.GetResponse("press red");

        var timer = GetItem<FungicideTimer>();
        Context.Actors.Should().Contain(timer);
    }

    [Test]
    public async Task FungicideTimer_AdvancesState()
    {
        var target = GetTarget();
        StartHere<LabOffice>();

        await target.GetResponse("press red");
        var timer = GetItem<FungicideTimer>();
        // After press red + turn end: FullProtection (activation turn is free)
        timer.State.Should().Be(FungicideTimer.FungicideState.FullProtection);

        await target.GetResponse("wait");

        // After wait + turn end: PartialProtection
        timer.State.Should().Be(FungicideTimer.FungicideState.PartialProtection);
    }

    [Test]
    public async Task FungicideTimer_Turn1_OpenDoor_ShowsMistMessage()
    {
        var target = GetTarget();
        StartHere<LabOffice>();

        await target.GetResponse("press red");
        var response = await target.GetResponse("open door");

        response.Should().Contain("filled with a light mist");
        response.Should().Contain("choking noises");
    }

    [Test]
    public async Task FungicideTimer_Turn2_DoorOpen_MistClearsMessage()
    {
        var target = GetTarget();
        StartHere<LabOffice>();

        await target.GetResponse("press red");
        await target.GetResponse("open door");
        var response = await target.GetResponse("wait");

        response.Should().Contain("filled with a light mist");
        response.Should().Contain("choking noises");
        response.Should().Contain("mist in the Bio Lab clears");
        response.Should().Contain("mutants recover and rush toward the door");
    }

    [Test]
    public async Task FungicideTimer_Turn3_DoorOpen_PlayerDies()
    {
        var target = GetTarget();
        StartHere<LabOffice>();

        await target.GetResponse("press red");
        await target.GetResponse("open door");
        await target.GetResponse("wait");
        var response = await target.GetResponse("wait");

        response.Should().Contain("Mutated monsters from the Bio Lab pour into the office");
        response.Should().Contain("devoured");
        // Note: PendingDeath is cleared after restart, so we check the death message instead
    }

    [Test]
    public async Task FungicideTimer_Turn3_DoorClosed_PlayerSurvives()
    {
        var target = GetTarget();
        StartHere<LabOffice>();

        await target.GetResponse("press red");
        await target.GetResponse("open door");
        await target.GetResponse("close door");
        await target.GetResponse("wait");
        var response = await target.GetResponse("wait");

        response.Should().NotContain("devoured");
        Context.PendingDeath.Should().BeNull();
    }

    [Test]
    public async Task FungicideTimer_PlayerNotInOffice_NoMessagesOrDeath()
    {
        var target = GetTarget();
        StartHere<LabOffice>();

        // Press button and open door
        await target.GetResponse("press red");
        await target.GetResponse("open door");

        // Leave the office (go to auxiliary booth)
        await target.GetResponse("e");

        // Wait for timer to expire
        await target.GetResponse("wait");
        var response = await target.GetResponse("wait");

        // Should not see mist messages or death since we're not in the office
        response.Should().NotContain("mist");
        response.Should().NotContain("devoured");
        Context.PendingDeath.Should().BeNull();
    }

    [Test]
    public async Task FungicideTimer_FullSequence_MatchesExpectedBehavior()
    {
        // This test validates the exact sequence from the original Planetfall:
        // 1. Press red button → "hissing from beyond the door"
        // 2. Open door → mist message (once, no duplicate)
        // 3. Wait → mist message + "mist clears, mutants rush"
        // 4. Wait → death

        var target = GetTarget();
        StartHere<LabOffice>();

        // Step 1: Press button
        var response = await target.GetResponse("press red button");
        response.Should().Contain("hissing from beyond the door");

        // Step 2: Open door - should show mist message ONCE
        response = await target.GetResponse("open door");
        response.Should().Contain("office door is now open");
        response.Should().Contain("filled with a light mist");
        // Count occurrences of mist message - should be exactly 1
        var mistCount = response.Split("filled with a light mist").Length - 1;
        mistCount.Should().Be(1, "mist message should appear exactly once when opening door");

        // Step 3: First wait - mist still visible, then clears
        response = await target.GetResponse("z");
        response.Should().Contain("filled with a light mist");
        response.Should().Contain("mist in the Bio Lab clears");
        response.Should().Contain("mutants recover and rush toward the door");

        // Step 4: Second wait - death
        response = await target.GetResponse("z");
        response.Should().Contain("Mutated monsters from the Bio Lab pour into the office");
        response.Should().Contain("devoured");
    }

    [Test]
    public async Task LabOffice_ButtonDisambiguation_Red()
    {
        var target = GetTarget();
        StartHere<LabOffice>();

        await target.GetResponse("press button");
        var response = await target.GetResponse("red");

        response.Should().Contain("hissing");
    }

    [Test]
    public async Task LabOffice_ButtonDisambiguation_White()
    {
        var target = GetTarget();
        StartHere<LabOffice>();

        await target.GetResponse("press button");
        var response = await target.GetResponse("white");

        response.Should().Contain("relay clicking");
    }

    [Test]
    public async Task LabOffice_ButtonDisambiguation_Black()
    {
        var target = GetTarget();
        StartHere<LabOffice>();

        await target.GetResponse("press button");
        var response = await target.GetResponse("black");

        response.Should().Contain("relay clicking");
    }

    [Test]
    public async Task LabOffice_PressWhiteButton_RelayClicking()
    {
        var target = GetTarget();
        StartHere<LabOffice>();

        var response = await target.GetResponse("press white");

        response.Should().Contain("relay clicking");
    }

    [Test]
    public async Task LabOffice_PressBlackButton_RelayClicking()
    {
        var target = GetTarget();
        StartHere<LabOffice>();

        var response = await target.GetResponse("press black");

        response.Should().Contain("relay clicking");
    }
}
using GameEngine;
using JetBrains.Annotations;
using Model;
using Planetfall.Item.Computer;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Item.Lawanda.BioLab;
using Planetfall.Item.Lawanda.CryoElevator;
using Planetfall.Item.Lawanda.Lab;
using Planetfall.Item.Lawanda.LabOffice;
using Planetfall.Location.Computer;
using Planetfall.Location.Lawanda.Lab;
using Planetfall.Location.Lawanda.LabOffice;

namespace Planetfall.Tests.Walkthrough;

public sealed class WalkthroughMutantChase : WalkthroughTestBase
{
    [UsedImplicitly]
    public void SetupBioLock()
    {
        StartHere<StripNearStation>();
        var floyd = Repository.GetItem<Floyd>();

        floyd.HasDied = true;
        floyd.CurrentLocation = Repository.GetLocation<BioLockEast>();
        Repository.GetItem<Relay>().SpeckDestroyed = true;

        // Reset chase-related state to prevent test pollution
        var bioLab = Repository.GetLocation<BioLabLocation>();
        bioLab.ChaseStarted = false;

        // Verbose mode always shows full descriptions regardless of visit count
        Context.Verbosity = Verbosity.Verbose;

        var chaseManager = Repository.GetItem<ChaseSceneManager>();
        chaseManager.StopChase();

        var fungicideTimer = Repository.GetItem<FungicideTimer>();
        fungicideTimer.State = FungicideTimer.FungicideState.Inactive;
        fungicideTimer.TurnFlags = FungicideTimer.FreeTurnFlags.None;
        fungicideTimer.PlayerExitedBioLabToLabOffice = false;

        // Reset doors
        var officeDoor = Repository.GetItem<OfficeDoor>();
        officeDoor.IsOpen = false;
        officeDoor.JustOpenedThisTurn = false;

        var labDoor = Repository.GetItem<BioLockInnerDoor>();
        labDoor.IsOpen = false;
        
        Repository.GetItem<BioLockOuterDoor>().IsOpen = false;
        Repository.GetItem<BioLockOuterDoor>().TurnsSinceOpening = 0;

        // Reset desk and gas mask
        var desk = Repository.GetItem<LabDesk>();
        desk.IsOpen = false;

        var gasMask = Repository.GetItem<GasMask>();
        gasMask.BeingWorn = false;
        // Remove from inventory if present
        if (Context.Items.Contains(gasMask))
            Context.Drop(gasMask);
        // Put it back in desk
        desk.ItemPlacedHere(gasMask);
 
    }
    
    [Test]
    [TestCase("w", "SetupBioLock", "Auxiliary")]
    [TestCase("n", null, "Lab Office")]
    [TestCase("open door", null, "The office door is now open.", "Mutated monsters from the Bio Lab pour into the office. You are devoured.")]
    public async Task OpenDoorWithoutFungicide(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);

    [Test]
    [TestCase("w", "SetupBioLock", "Auxiliary")]
    [TestCase("n", null, "Lab Office")]
    [TestCase("press red button", null, "hissing")]
    [TestCase("open door", null, "The office door is now open.", "Through the open doorway you can see the Bio Lab. It seems to be filled with a light mist. Horrifying biological nightmares stagger about making choking noises")]
    [TestCase("w", "", "Unfortunately, you don't seem to be that hardy", "According to the Treaty of Gishen IV")]
    public async Task EnterNoGasMask(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);
    
    [Test]
    [TestCase("w", "SetupBioLock", "Auxiliary")]
    [TestCase("n", null, "Lab Office")]
    [TestCase("press red button", null, "hissing")]
    [TestCase("open door", null, "The office door is now open.", "Through the open doorway you can see the Bio Lab. It seems to be filled with a light mist. Horrifying biological nightmares stagger about making choking noises")]
    [TestCase("z", null, "stagger about making choking noises", "The mist in the Bio Lab clears. The mutants recover and rush toward the door!")]
    [TestCase("z", null, "Mutated monsters from the Bio Lab pour into the office. You are devoured", "According to the Treaty of Gishen IV")]
    public async Task SitAndWaitForFungicideToDissappear(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);
    
    [Test]
    [TestCase("w", "SetupBioLock", "Auxiliary")]
    [TestCase("n", null, "Lab Office")]
    [TestCase("open desk", null, "Opening the desk reveals a gas mask.")]
    [TestCase("take mask", null, "Taken")]
    [TestCase("wear mask", null, "You are wearing the gas mask.")]
    [TestCase("press red button", null, "hissing")]
    [TestCase("open door", null, "The office door is now open.", "Through the open doorway you can see the Bio Lab. It seems to be filled with a light mist. Horrifying biological nightmares stagger about making choking noises")]
    [TestCase("wait", null, "The mist in the Bio Lab clears. The mutants recover and rush toward the door")]
    [TestCase("w", "", "The mutants attack you and rip you to shreds within seconds.")]
    public async Task WaitOneTurnTooLongToEnterLab(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);
    
    [Test]
    [TestCase("w", "SetupBioLock", "Auxiliary")]
    [TestCase("n", null, "Lab Office")]
    [TestCase("open desk", null, "Opening the desk reveals a gas mask.")]
    [TestCase("take mask", null, "Taken")]
    [TestCase("wear mask", null, "You are wearing the gas mask.")]
    [TestCase("press red button", null, "hissing")]
    [TestCase("open door", null, "The office door is now open.", "Through the open doorway you can see the Bio Lab. It seems to be filled with a light mist. Horrifying biological nightmares stagger about making choking noises")]
    [TestCase("w", "", "They appear to be stunned and confused, but are slowly recovering")]
    [TestCase("wait", null, "The last traces of mist in the air vanish. The mutants, recovering quickly, notice you and begin salivating", "Dozens of hungry eyes fix on you as the mutations surround you and begin feasting")]
    public async Task WaitOneTurnInLab(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);
    
    [Test]
    [TestCase("w", "SetupBioLock", "Auxiliary")]
    [TestCase("n", null, "Lab Office")]
    [TestCase("open desk", null, "Opening the desk reveals a gas mask.")]
    [TestCase("take mask", null, "Taken")]
    [TestCase("wear mask", null, "You are wearing the gas mask.")]
    [TestCase("press red button", null, "hissing")]
    [TestCase("open door", null, "The office door is now open.", "Through the open doorway you can see the Bio Lab. It seems to be filled with a light mist. Horrifying biological nightmares stagger about making choking noises")]
    [TestCase("w", "", "They appear to be stunned and confused, but are slowly recovering")]
    [TestCase("open lab door", null, "The door opens.", "The air is filled with mist, which is affecting the mutants. They appear to be stunned and confused, but are slowly recovering.")]
    [TestCase("wait", null, "The last traces of mist in the air vanish. The mutants, recovering quickly, notice you and begin salivating", "Dozens of hungry eyes fix on you as the mutations surround you and begin feasting")]
    public async Task OpenLabDoorButStay(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);
    
    [Test]
    [TestCase("w", "SetupBioLock", "Auxiliary")]
    [TestCase("n", null, "Lab Office")]
    [TestCase("open desk", null, "Opening the desk reveals a gas mask.")]
    [TestCase("take mask", null, "Taken")]
    [TestCase("wear mask", null, "You are wearing the gas mask.")]
    [TestCase("press red button", null, "hissing")]
    [TestCase("open door", null, "The office door is now open.", "Through the open doorway you can see the Bio Lab. It seems to be filled with a light mist. Horrifying biological nightmares stagger about making choking noises")]
    [TestCase("w", "", "They appear to be stunned and confused, but are slowly recovering")]
    [TestCase("e", null, "Lab Office", "The mist in the Bio Lab clears. The mutants recover and rush toward the door!")]
    [TestCase("wait", null, "Mutated monsters from the Bio Lab pour into the office. You are devoured.")]
    public async Task GoBackToOffice(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);
    
    [Test]
    [TestCase("w", "SetupBioLock", "Auxiliary")]
    [TestCase("n", null, "Lab Office")]
    [TestCase("open desk", null, "Opening the desk reveals a gas mask.")]
    [TestCase("take mask", null, "Taken")]
    [TestCase("wear mask", null, "You are wearing the gas mask.")]
    [TestCase("press red button", null, "hissing")]
    [TestCase("open door", null, "The office door is now open.", "Through the open doorway you can see the Bio Lab. It seems to be filled with a light mist. Horrifying biological nightmares stagger about making choking noises")]
    [TestCase("w", "", "They appear to be stunned and confused, but are slowly recovering")]
    [TestCase("open lab door", null, "The door opens.", "The air is filled with mist, which is affecting the mutants. They appear to be stunned and confused, but are slowly recovering.")]
    [TestCase("w", null, "The bio lock continues to the west.")]
    [TestCase("close door", null, "The door closes, but not soon enough!", "Dozens of hungry eyes fix on you as the mutations surround you and begin feasting.")]
    public async Task TryToCloseDoor(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);
    
    [Test]
    [TestCase("w", "SetupBioLock", "Auxiliary")]
    [TestCase("n", null, "Lab Office")]
    [TestCase("open desk", null, "Opening the desk reveals a gas mask.")]
    [TestCase("take mask", null, "Taken")]
    [TestCase("wear mask", null, "You are wearing the gas mask.")]
    [TestCase("press red button", null, "hissing")]
    [TestCase("open door", null, "The office door is now open.", "Through the open doorway you can see the Bio Lab. It seems to be filled with a light mist. Horrifying biological nightmares stagger about making choking noises")]
    [TestCase("w", "", "They appear to be stunned and confused, but are slowly recovering")]
    [TestCase("open lab door", null, "The door opens.", "The air is filled with mist, which is affecting the mutants. They appear to be stunned and confused, but are slowly recovering.")]
    [TestCase("w", null, "The bio lock continues to the west.")]
    [TestCase("wait", null, "Dozens of hungry eyes fix on you as the mutations surround you and begin feasting")]
    public async Task LingerAfterExit(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);
    
    [Test]
    [TestCase("w", "SetupBioLock", "Auxiliary")]
    [TestCase("n", null, "Lab Office")]
    [TestCase("open desk", null, "Opening the desk reveals a gas mask.")]
    [TestCase("take mask", null, "Taken")]
    [TestCase("wear mask", null, "You are wearing the gas mask.")]
    [TestCase("press red button", null, "hissing")]
    [TestCase("open door", null, "The office door is now open.", "Through the open doorway you can see the Bio Lab. It seems to be filled with a light mist. Horrifying biological nightmares stagger about making choking noises")]
    [TestCase("w", "", "They appear to be stunned and confused, but are slowly recovering")]
    [TestCase("e", null, "Lab Office", "The mist in the Bio Lab clears. The mutants recover and rush toward the door!")]
    [TestCase("w", null, "Bio Lab", "You stupidly run right into the jaws of the pursuing mutants.")]
    public async Task GoBackToOfficeAndBacktrack(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);
    
    [Test]
    [TestCase("w", "SetupBioLock", "Auxiliary")]
    [TestCase("n", null, "Lab Office")]
    [TestCase("open desk", null, "Opening the desk reveals a gas mask.")]
    [TestCase("take mask", null, "Taken")]
    [TestCase("wear mask", null, "You are wearing the gas mask.")]
    [TestCase("press red button", null, "hissing")]
    [TestCase("open door", null, "The office door is now open.", "Through the open doorway you can see the Bio Lab. It seems to be filled with a light mist. Horrifying biological nightmares stagger about making choking noises")]
    [TestCase("w", "", "They appear to be stunned and confused, but are slowly recovering")]
    [TestCase("e", null, "Lab Office", "The mist in the Bio Lab clears. The mutants recover and rush toward the door!")]
    [TestCase("s", null, "Auxiliary Booth", "The mutants burst into the room right on your heels! Needle-sharp mandibles nip at your arms!")]
    [TestCase("wait", null, "Dozens of hungry eyes fix on you as the mutations surround you and begin feasting")]
    public async Task GoBackToOfficeAndAuxAndGetTrapped(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);
    
    [Test]
    [TestCase("w", "SetupBioLock", "Auxiliary")]
    [TestCase("n", null, "Lab Office")]
    [TestCase("open desk", null, "Opening the desk reveals a gas mask.")]
    [TestCase("take mask", null, "Taken")]
    [TestCase("wear mask", null, "You are wearing the gas mask.")]
    [TestCase("press red button", null, "hissing")]
    [TestCase("open door", null, "The office door is now open.", "Through the open doorway you can see the Bio Lab. It seems to be filled with a light mist. Horrifying biological nightmares stagger about making choking noises")]
    [TestCase("w", "", "They appear to be stunned and confused, but are slowly recovering")]
    [TestCase("e", null, "Lab Office", "The mist in the Bio Lab clears. The mutants recover and rush toward the door!")]
    [TestCase("s", null, "Auxiliary Booth", "The mutants burst into the room right on your heels! Needle-sharp mandibles nip at your arms!")]
    [TestCase("n", null, "You stupidly run right into the jaws of the pursuing mutants")]
    public async Task GoBackToOfficeAndAuxAndGetTrappedBacktrack(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);
    
    [Test]
    [TestCase("w", "SetupBioLock", "Auxiliary")]
    [TestCase("n", null, "Lab Office")]
    [TestCase("open desk", null, "Opening the desk reveals a gas mask.")]
    [TestCase("take mask", null, "Taken")]
    [TestCase("wear mask", null, "You are wearing the gas mask.")]
    [TestCase("press red button", null, "hissing")]
    [TestCase("open door", null, "The office door is now open.", "Through the open doorway you can see the Bio Lab. It seems to be filled with a light mist. Horrifying biological nightmares stagger about making choking noises")]
    [TestCase("w", "", "They appear to be stunned and confused, but are slowly recovering")]
    [TestCase("w", "", "The lab door is closed.", "They appear to be stunned and confused, but are slowly recovering")]
    [TestCase("open lab door", null, "The door opens.", "The air is filled with mist, which is affecting the mutants. They appear to be stunned and confused, but are slowly recovering.")]
    [TestCase("w", null, "The bio lock continues to the west.")]
    public async Task TryToLeaveLabBeforeOpeningDoor(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);
    
    [Test]
    [TestCase("w", "SetupBioLock", "Auxiliary")]
    [TestCase("n", null, "Lab Office")]
    [TestCase("open desk", null, "Opening the desk reveals a gas mask.")]
    [TestCase("take mask", null, "Taken")]
    [TestCase("wear mask", null, "You are wearing the gas mask.")]
    [TestCase("press red button", null, "hissing")]
    [TestCase("open door", null, "The office door is now open.", "Through the open doorway you can see the Bio Lab. It seems to be filled with a light mist. Horrifying biological nightmares stagger about making choking noises")]
    [TestCase("w", "", "They appear to be stunned and confused, but are slowly recovering")]
    [TestCase("w", "", "The lab door is closed.", "They appear to be stunned and confused, but are slowly recovering")]
    [TestCase("open lab door", null, "The door opens.", "The air is filled with mist, which is affecting the mutants. They appear to be stunned and confused, but are slowly recovering.")]
    [TestCase("w", null, "The bio lock continues to the west.", "The mutants burst into the room right on your heels! Needle-sharp mandibles nip at your arms!")]
    [TestCase("w", null, "Bio Lock West", "The mutants burst into the room right on your heels! Needle-sharp mandibles nip at your arms!")]
    [TestCase("open door", null, "The door opens", "The mutants are almost upon you now!")]
    [TestCase("w", null, "Main Lab", "The mutants burst into the room right on your heels! Needle-sharp mandibles nip at your arms!")]
    public async Task MakeItToMainLab(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);
    
    [Test]
    [TestCase("w", "SetupBioLock", "Auxiliary")]
    [TestCase("n", null, "Lab Office")]
    [TestCase("open desk", null, "Opening the desk reveals a gas mask.")]
    [TestCase("take mask", null, "Taken")]
    [TestCase("wear mask", null, "You are wearing the gas mask.")]
    [TestCase("press red button", null, "hissing")]
    [TestCase("open door", null, "The office door is now open.", "Through the open doorway you can see the Bio Lab. It seems to be filled with a light mist. Horrifying biological nightmares stagger about making choking noises")]
    [TestCase("w", "", "They appear to be stunned and confused, but are slowly recovering")]
    [TestCase("w", "", "The lab door is closed.", "They appear to be stunned and confused, but are slowly recovering")]
    [TestCase("open lab door", null, "The door opens.", "The air is filled with mist, which is affecting the mutants. They appear to be stunned and confused, but are slowly recovering.")]
    [TestCase("w", null, "The bio lock continues to the west.", "The mutants burst into the room right on your heels! Needle-sharp mandibles nip at your arms!")]
    [TestCase("w", null, "Bio Lock West", "The mutants burst into the room right on your heels! Needle-sharp mandibles nip at your arms!")]
    [TestCase("open door", null, "The door opens", "The mutants are almost upon you now!")]
    [TestCase("w", null, "Main Lab", "The mutants burst into the room right on your heels! Needle-sharp mandibles nip at your arms!")]
    [TestCase("close bio-lock door", null, "The door closes", "Dozens of hungry eyes fix on you as the mutations surround you and begin feasting")]
    public async Task TryToCloseBioLockOuterDoor(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);
    
    [Test]
    [TestCase("w", "SetupBioLock", "Auxiliary")]
    [TestCase("n", null, "Lab Office")]
    [TestCase("open desk", null, "Opening the desk reveals a gas mask.")]
    [TestCase("take mask", null, "Taken")]
    [TestCase("wear mask", null, "You are wearing the gas mask.")]
    [TestCase("press red button", null, "hissing")]
    [TestCase("open door", null, "The office door is now open.", "Through the open doorway you can see the Bio Lab. It seems to be filled with a light mist. Horrifying biological nightmares stagger about making choking noises")]
    [TestCase("w", "", "They appear to be stunned and confused, but are slowly recovering")]
    [TestCase("w", "", "The lab door is closed.", "They appear to be stunned and confused, but are slowly recovering")]
    [TestCase("open lab door", null, "The door opens.", "The air is filled with mist, which is affecting the mutants. They appear to be stunned and confused, but are slowly recovering.")]
    [TestCase("w", null, "The bio lock continues to the west.", "The mutants burst into the room right on your heels! Needle-sharp mandibles nip at your arms!")]
    [TestCase("w", null, "Bio Lock West", "The mutants burst into the room right on your heels! Needle-sharp mandibles nip at your arms!")]
    [TestCase("open door", null, "The door opens", "The mutants are almost upon you now!")]
    [TestCase("w", null, "Main Lab", "The mutants burst into the room right on your heels! Needle-sharp mandibles nip at your arms!")]
    [TestCase("w", null, "Project Corridor East")]
    [TestCase("look", null, "Dozens of hungry eyes fix on you as the mutations surround you and begin feasting.")]
    public async Task RandomPause(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);
    
        
    [Test]
    [TestCase("w", "SetupBioLock", "Auxiliary")]
    [TestCase("n", null, "Lab Office")]
    [TestCase("open desk", null, "Opening the desk reveals a gas mask.")]
    [TestCase("take mask", null, "Taken")]
    [TestCase("wear mask", null, "You are wearing the gas mask.")]
    [TestCase("press red button", null, "hissing")]
    [TestCase("open door", null, "The office door is now open.", "Through the open doorway you can see the Bio Lab. It seems to be filled with a light mist. Horrifying biological nightmares stagger about making choking noises")]
    [TestCase("w", "", "They appear to be stunned and confused, but are slowly recovering")]
    [TestCase("w", "", "The lab door is closed.", "They appear to be stunned and confused, but are slowly recovering")]
    [TestCase("open lab door", null, "The door opens.", "The air is filled with mist, which is affecting the mutants. They appear to be stunned and confused, but are slowly recovering.")]
    [TestCase("w", null, "The bio lock continues to the west.", "The mutants burst into the room right on your heels! Needle-sharp mandibles nip at your arms!")]
    [TestCase("w", null, "Bio Lock West", "The mutants burst into the room right on your heels! Needle-sharp mandibles nip at your arms!")]
    [TestCase("open door", null, "The door opens", "The mutants are almost upon you now!")]
    [TestCase("w", null, "Main Lab", "The mutants burst into the room right on your heels! Needle-sharp mandibles nip at your arms!")]
    [TestCase("w", null, "Project Corridor East")]
    [TestCase("e", null, "You stupidly run right into the jaws of the pursuing mutants")]
    public async Task RandomBacktrack(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);
    
    [Test]
    [TestCase("w", "SetupBioLock", "Auxiliary")]
    [TestCase("n", null, "Lab Office")]
    [TestCase("open desk", null, "Opening the desk reveals a gas mask.")]
    [TestCase("take mask", null, "Taken")]
    [TestCase("wear mask", null, "You are wearing the gas mask.")]
    [TestCase("press red button", null, "hissing")]
    [TestCase("open door", null, "The office door is now open.", "Through the open doorway you can see the Bio Lab. It seems to be filled with a light mist. Horrifying biological nightmares stagger about making choking noises")]
    [TestCase("w", "", "They appear to be stunned and confused, but are slowly recovering")]
    [TestCase("w", "", "The lab door is closed.", "They appear to be stunned and confused, but are slowly recovering")]
    [TestCase("open lab door", null, "The door opens.", "The air is filled with mist, which is affecting the mutants. They appear to be stunned and confused, but are slowly recovering.")]
    [TestCase("w", null, "The bio lock continues to the west.", "The mutants burst into the room right on your heels! Needle-sharp mandibles nip at your arms!")]
    [TestCase("w", null, "Bio Lock West", "The mutants burst into the room right on your heels! Needle-sharp mandibles nip at your arms!")]
    [TestCase("open door", null, "The door opens", "The mutants are almost upon you now!")]
    [TestCase("w", null, "Main Lab", "The mutants burst into the room right on your heels! Needle-sharp mandibles nip at your arms!")]
    [TestCase("w", null, "Project Corridor East", "The mutants burst into the room right on your heels! Needle-sharp mandibles nip at your arms!")]
    [TestCase("s", null, "Computer Room", "The mutants burst into the room right on your heels! Needle-sharp mandibles nip at your arms!")]
    [TestCase("w", null, "ProjCon Office", "The mutants burst into the room right on your heels! Needle-sharp mandibles nip at your arms!")]
    [TestCase("n", null, "Project Corridor", "The mutants burst into the room right on your heels! Needle-sharp mandibles nip at your arms!")]
    [TestCase("e", null, "Project Corridor East", "The mutants burst into the room right on your heels! Needle-sharp mandibles nip at your arms!")]
    [TestCase("s", null, "Computer Room", "The mutants burst into the room right on your heels! Needle-sharp mandibles nip at your arms!")]
    [TestCase("w", null, "ProjCon Office", "The mutants burst into the room right on your heels! Needle-sharp mandibles nip at your arms!")]
    [TestCase("n", null, "Project Corridor", "The mutants burst into the room right on your heels! Needle-sharp mandibles nip at your arms!")]
    [TestCase("e", null, "Project Corridor East", "The mutants burst into the room right on your heels! Needle-sharp mandibles nip at your arms!")]
    [TestCase("s", null, "Computer Room", "The mutants burst into the room right on your heels! Needle-sharp mandibles nip at your arms!")]
    [TestCase("w", null, "ProjCon Office", "The mutants burst into the room right on your heels! Needle-sharp mandibles nip at your arms!")]
    [TestCase("n", null, "Project Corridor", "The mutants burst into the room right on your heels! Needle-sharp mandibles nip at your arms!")]
    public async Task LoopForever(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);
    
    [Test]
    [TestCase("w", "SetupBioLock", "Auxiliary")]
    [TestCase("n", null, "Lab Office")]
    [TestCase("open desk", null, "Opening the desk reveals a gas mask.")]
    [TestCase("take mask", null, "Taken")]
    [TestCase("wear mask", null, "You are wearing the gas mask.")]
    [TestCase("press red button", null, "hissing")]
    [TestCase("open door", null, "The office door is now open.", "Through the open doorway you can see the Bio Lab. It seems to be filled with a light mist. Horrifying biological nightmares stagger about making choking noises")]
    [TestCase("w", "", "They appear to be stunned and confused, but are slowly recovering")]
    [TestCase("w", "", "The lab door is closed.", "They appear to be stunned and confused, but are slowly recovering")]
    [TestCase("open door", null, "Do you mean the office door or the lab door")]
    [TestCase("lab door", null, "The door opens.", "The air is filled with mist, which is affecting the mutants. They appear to be stunned and confused, but are slowly recovering.")]
    [TestCase("w", null, "The bio lock continues to the west.", "The mutants burst into the room right on your heels! Needle-sharp mandibles nip at your arms!")]
    public async Task DoorDisambiguation(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);
    
    [Test]
    [TestCase("w", "SetupBioLock", "Auxiliary")]
    [TestCase("n", null, "Lab Office")]
    [TestCase("open desk", null, "Opening the desk reveals a gas mask.")]
    [TestCase("take mask", null, "Taken")]
    [TestCase("wear mask", null, "You are wearing the gas mask.")]
    [TestCase("press red button", null, "hissing")]
    [TestCase("open door", null, "The office door is now open.", "Through the open doorway you can see the Bio Lab. It seems to be filled with a light mist. Horrifying biological nightmares stagger about making choking noises")]
    [TestCase("w", "", "They appear to be stunned and confused, but are slowly recovering")]
    [TestCase("w", "", "The lab door is closed.", "They appear to be stunned and confused, but are slowly recovering")]
    [TestCase("open lab door", null, "The door opens.", "The air is filled with mist, which is affecting the mutants. They appear to be stunned and confused, but are slowly recovering.")]
    [TestCase("w", null, "The bio lock continues to the west.", "The mutants burst into the room right on your heels! Needle-sharp mandibles nip at your arms!")]
    [TestCase("w", null, "Bio Lock West", "The mutants burst into the room right on your heels! Needle-sharp mandibles nip at your arms!")]
    [TestCase("open door", null, "The door opens", "The mutants are almost upon you now!")]
    [TestCase("w", null, "Main Lab", "The mutants burst into the room right on your heels! Needle-sharp mandibles nip at your arms!")]
    [TestCase("w", null, "Project Corridor East", "The mutants burst into the room right on your heels! Needle-sharp mandibles nip at your arms!")]
    [TestCase("s", null, "Computer Room", "The mutants burst into the room right on your heels! Needle-sharp mandibles nip at your arms!")]
    [TestCase("w", null, "ProjCon Office", "revealing an open doorway to a large elevator!", "The mutants burst into the room right on your heels! Needle-sharp mandibles nip at your arms!")]
    [TestCase("s", null, "Cryo-Elevator", "The monsters are storming straight toward the elevator door!")]
    public async Task MakeItToElevator(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);
    
    [Test]
    [TestCase("w", "SetupBioLock", "Auxiliary")]
    [TestCase("n", null, "Lab Office")]
    [TestCase("open desk", null, "Opening the desk reveals a gas mask.")]
    [TestCase("take mask", null, "Taken")]
    [TestCase("wear mask", null, "You are wearing the gas mask.")]
    [TestCase("press red button", null, "hissing")]
    [TestCase("open door", null, "The office door is now open.", "Through the open doorway you can see the Bio Lab. It seems to be filled with a light mist. Horrifying biological nightmares stagger about making choking noises")]
    [TestCase("w", "", "They appear to be stunned and confused, but are slowly recovering")]
    [TestCase("w", "", "The lab door is closed.", "They appear to be stunned and confused, but are slowly recovering")]
    [TestCase("open lab door", null, "The door opens.", "The air is filled with mist, which is affecting the mutants. They appear to be stunned and confused, but are slowly recovering.")]
    [TestCase("w", null, "The bio lock continues to the west.", "The mutants burst into the room right on your heels! Needle-sharp mandibles nip at your arms!")]
    [TestCase("w", null, "Bio Lock West", "The mutants burst into the room right on your heels! Needle-sharp mandibles nip at your arms!")]
    [TestCase("open door", null, "The door opens", "The mutants are almost upon you now!")]
    [TestCase("w", null, "Main Lab", "The mutants burst into the room right on your heels! Needle-sharp mandibles nip at your arms!")]
    [TestCase("w", null, "Project Corridor East", "The mutants burst into the room right on your heels! Needle-sharp mandibles nip at your arms!")]
    [TestCase("s", null, "Computer Room", "The mutants burst into the room right on your heels! Needle-sharp mandibles nip at your arms!")]
    [TestCase("w", null, "ProjCon Office", "revealing an open doorway to a large elevator!", "The mutants burst into the room right on your heels! Needle-sharp mandibles nip at your arms!")]
    [TestCase("s", null, "Cryo-Elevator", "The monsters are storming straight toward the elevator door!")]
    [TestCase("wait", null, "The biological nightmares reach you. Gripping coils wrap around your limbs as powerful teeth begin tearing at your flesh. Something bites your leg, and you feel a powerful poison begin to work its numbing effects")]
    public async Task DieInElevator(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);
    
    [Test]
    [TestCase("w", "SetupBioLock", "Auxiliary")]
    [TestCase("n", null, "Lab Office")]
    [TestCase("open desk", null, "Opening the desk reveals a gas mask.")]
    [TestCase("take mask", null, "Taken")]
    [TestCase("wear mask", null, "You are wearing the gas mask.")]
    [TestCase("press red button", null, "hissing")]
    [TestCase("open door", null, "The office door is now open.", "Through the open doorway you can see the Bio Lab. It seems to be filled with a light mist. Horrifying biological nightmares stagger about making choking noises")]
    [TestCase("w", "", "They appear to be stunned and confused, but are slowly recovering")]
    [TestCase("w", "", "The lab door is closed.", "They appear to be stunned and confused, but are slowly recovering")]
    [TestCase("open lab door", null, "The door opens.", "The air is filled with mist, which is affecting the mutants. They appear to be stunned and confused, but are slowly recovering.")]
    [TestCase("w", null, "The bio lock continues to the west.", "The mutants burst into the room right on your heels! Needle-sharp mandibles nip at your arms!")]
    [TestCase("w", null, "Bio Lock West", "The mutants burst into the room right on your heels! Needle-sharp mandibles nip at your arms!")]
    [TestCase("open door", null, "The door opens", "The mutants are almost upon you now!")]
    [TestCase("w", null, "Main Lab", "The mutants burst into the room right on your heels! Needle-sharp mandibles nip at your arms!")]
    [TestCase("w", null, "Project Corridor East", "The mutants burst into the room right on your heels! Needle-sharp mandibles nip at your arms!")]
    [TestCase("s", null, "Computer Room", "The mutants burst into the room right on your heels! Needle-sharp mandibles nip at your arms!")]
    [TestCase("w", null, "ProjCon Office", "revealing an open doorway to a large elevator!", "The mutants burst into the room right on your heels! Needle-sharp mandibles nip at your arms!")]
    [TestCase("s", null, "Cryo-Elevator", "The monsters are storming straight toward the elevator door!")] 
    [TestCase("press button", null, "The elevator door closes just as the monsters reach it!")]
    [TestCase("z", null, "Time passes")] 
    [TestCase("z", null, "Time passes")] 
    [TestCase("z", null, "Time passes", "The elevator door opens onto a room to the north.")] 
    [TestCase("press button", null, "Stunning", "you blow it all in one")]
    public async Task MakeItToElevatorAndThenBackUp(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);

}
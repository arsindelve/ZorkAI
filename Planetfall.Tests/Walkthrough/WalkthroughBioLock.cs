using GameEngine;
using JetBrains.Annotations;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Item.Lawanda.Lab;
using Planetfall.Location.Lawanda;
using Planetfall.Location.Lawanda.Lab;

namespace Planetfall.Tests.Walkthrough;

[TestFixture]
public sealed class WalkthroughBioLock : WalkthroughTestBase
{
    /// <summary>
    /// God mode method to set up BioLock scenarios:
    /// - Floyd is in BioLockEast and turned on
    /// - ComputerRoom.FloydHasExpressedConcern is true
    /// - Player starts in BioLockWest
    /// </summary>
    [UsedImplicitly]
    public void SetupBioLock()
    {
        StartHere<BioLockWest>();
        var bioLockEast = Repository.GetLocation<BioLockEast>();
        var floyd = Repository.GetItem<Floyd>();
        var computerRoom = Repository.GetLocation<ComputerRoom>();

        // Reset the state machine for this test
        bioLockEast.StateMachine.HasWaitedOneTurnInBioLockEast = false;
        bioLockEast.StateMachine.FloydHasSaidLookAMiniCard = false;
        bioLockEast.StateMachine.FloydHasSaidNeedToGetCard = false;
        bioLockEast.StateMachine.FloydWaitingForDoorOpenCount = 0;
        bioLockEast.StateMachine.LabSequenceState = BioLockStateMachineManager.FloydLabSequenceState.NotStarted;
        bioLockEast.StateMachine.FloydFightingTurnCount = 0;
        bioLockEast.StateMachine.TurnsAfterFloydRushedIn = 0;
        bioLockEast.StateMachine.TurnsAfterDoorReopened = 0;

        // Reset the door state
        var door = Repository.GetItem<BioLockInnerDoor>();
        door.IsOpen = false;

        // Place Floyd in BioLockEast and turn him on
        bioLockEast.ItemPlacedHere(floyd);
        floyd.IsOn = true;

        // Set the computer room concern flag
        computerRoom.FloydHasExpressedConcern = true;
    }

    [Test]
    [TestCase("e", "SetupBioLock", "Bio Lock East")]
    [TestCase("z", null, "Time passes", "Floyd stands on his tiptoes", "Looks dangerous in there", "We'll need card there to fix computer", "Floyd will get card", "You open the door, then Floyd will rush in", "Floyd's voice trembles")]
    [TestCase("z", null, "Time passes", "Floyd looks at you with a dash of impatience", "Are you going to open the door?")]
    [TestCase("z", null, "Time passes", "Floyd looks at you with a dash of impatience", "Are you going to open the door?")]
    [TestCase("z", null, "Time passes", "Floyd looks at you with a dash of impatience", "Are you going to open the door?")]
    [TestCase("z", null, "Time passes", "Floyd looks at you with a dash of impatience", "Are you going to open the door?")]
    [TestCase("z", null, "Time passes", "Okay", "says Floyd with uncharacteristic annoyance", "Forget about the stupid card", "goes to the other end of the bio-lock and sulks")]
    public async Task FloydWaitsForDoorToOpen_ThenSulks(string input, string? setup, params string[] expectedResponses)
    {
        if (!string.IsNullOrWhiteSpace(setup))
            InvokeGodMode(setup);

        await Do(input, expectedResponses);
    }

    [Test]
    [TestCase("e", "SetupBioLock", "Bio Lock East")]
    [TestCase("z", null, "Time passes", "Floyd stands on his tiptoes", "Looks dangerous in there", "We'll need card there to fix computer", "Floyd will get card", "You open the door, then Floyd will rush in", "Floyd's voice trembles")]
    [TestCase("open door", null, "The door opens and Floyd", "plunges into the Bio Lab", "he is set upon by hideous, mutated monsters", "More are heading straight toward the open door", "Floyd shrieks and yells to you to close the door")]
    [TestCase("z", null, "Time passes", "biological nightmares reach you", "Gripping coils wrap around your limbs", "powerful teeth begin tearing at your flesh", "you feel a powerful poison", "You have died")]
    public async Task PlayerDoesNotCloseDoor_PlayerDies(string input, string? setup, params string[] expectedResponses)
    {
        if (!string.IsNullOrWhiteSpace(setup))
            InvokeGodMode(setup);

        await Do(input, expectedResponses);
    }

    [Test]
    [TestCase("e", "SetupBioLock", "Bio Lock East")]
    [TestCase("open door", null, "Opening the door reveals a Bio-Lab full of horrible mutations", "You stare at them, frozen with horror", "Growling with hunger and delight", "the mutations march into the bio-lock and devour you", "You have died")]
    public async Task PlayerOpensDoorTooSoon_PlayerDies(string input, string? setup, params string[] expectedResponses)
    {
        if (!string.IsNullOrWhiteSpace(setup))
            InvokeGodMode(setup);

        await Do(input, expectedResponses);
    }

    [Test]
    [TestCase("e", "SetupBioLock", "Bio Lock East")]
    [TestCase("z", null, "Time passes", "Floyd stands on his tiptoes", "Looks dangerous in there", "We'll need card there to fix computer", "Floyd will get card", "You open the door, then Floyd will rush in", "Floyd's voice trembles")]
    [TestCase("open door", null, "The door opens and Floyd", "plunges into the Bio Lab", "he is set upon by hideous, mutated monsters", "More are heading straight toward the open door", "Floyd shrieks and yells to you to close the door")]
    [TestCase("close door", null, "The door closes", "And not a moment too soon")]
    [TestCase("open door", null, "The door opens", "biological nightmares reach you", "Gripping coils wrap around your limbs", "powerful teeth begin tearing at your flesh", "you feel a powerful poison", "You have died")]
    public async Task PlayerReopensDoorTooSoon_PlayerDies(string input, string? setup, params string[] expectedResponses)
    {
        if (!string.IsNullOrWhiteSpace(setup))
            InvokeGodMode(setup);

        await Do(input, expectedResponses);
    }

    [Test]
    [TestCase("e", "SetupBioLock", "Bio Lock East")]
    [TestCase("z", null, "Time passes", "Floyd stands on his tiptoes", "Looks dangerous in there", "We'll need card there to fix computer", "Floyd will get card", "You open the door, then Floyd will rush in", "Floyd's voice trembles")]
    [TestCase("open door", null, "The door opens and Floyd", "plunges into the Bio Lab", "he is set upon by hideous, mutated monsters", "More are heading straight toward the open door", "Floyd shrieks and yells to you to close the door")]
    [TestCase("close door", null, "The door closes", "And not a moment too soon", "From within the lab you hear ferocious growlings", "the sounds of a skirmish", "high-pitched metallic scream")]
    [TestCase("z", null, "Time passes", "You hear, slightly muffled by the door, three fast knocks", "followed by the distinctive sound of tearing metal")]
    [TestCase("z", null, "Time passes", "You hear a final metallic scream from behind the door", "followed by the sound of Floyd's body being torn apart", "Floyd is dead")]
    public async Task PlayerWaitsTooLong_FloydDies(string input, string? setup, params string[] expectedResponses)
    {
        if (!string.IsNullOrWhiteSpace(setup))
            InvokeGodMode(setup);

        await Do(input, expectedResponses);
    }

    [Test]
    [TestCase("e", "SetupBioLock", "Bio Lock East")]
    [TestCase("z", null, "Time passes", "Floyd stands on his tiptoes", "Looks dangerous in there", "We'll need card there to fix computer", "Floyd will get card", "You open the door, then Floyd will rush in", "Floyd's voice trembles")]
    [TestCase("open door", null, "The door opens and Floyd", "plunges into the Bio Lab", "he is set upon by hideous, mutated monsters", "More are heading straight toward the open door", "Floyd shrieks and yells to you to close the door")]
    [TestCase("close door", null, "The door closes", "And not a moment too soon", "From within the lab you hear ferocious growlings", "the sounds of a skirmish", "high-pitched metallic scream")]
    [TestCase("z", null, "Time passes", "You hear, slightly muffled by the door, three fast knocks", "followed by the distinctive sound of tearing metal")]
    [TestCase("open door", null, "The door opens", "Floyd stumbles out of the Bio Lab", "clutching the mini-booth card", "The mutations rush toward the open doorway")]
    [TestCase("close door", null, "The door closes", "And not a moment too soon", "Floyd staggers to the ground", "dropping the mini card", "badly torn apart", "loose wires and broken circuits", "Oil flows from his lubrication system", "only moments to live", "You drop to your knees and cradle Floyd's head", "Floyd did it", "got card", "Floyd a good friend", "Floyd smiles with contentment", "his eyes close", "in memory of a brave friend")]
    public async Task SuccessfulSequence_FloydSacrificesHimself(string input, string? setup, params string[] expectedResponses)
    {
        if (!string.IsNullOrWhiteSpace(setup))
            InvokeGodMode(setup);

        await Do(input, expectedResponses);
    }
    
    [Test]
    [TestCase("e", "SetupBioLock", "Bio Lock East")]
    [TestCase("z", null, "Time passes", "Floyd stands on his tiptoes", "Looks dangerous in there", "We'll need card there to fix computer", "Floyd will get card", "You open the door, then Floyd will rush in", "Floyd's voice trembles")]
    [TestCase("z", null, "Time passes", "Floyd looks at you with a dash of impatience", "Are you going to open the door?")]
    [TestCase("z", null, "Time passes", "Floyd looks at you with a dash of impatience", "Are you going to open the door?")]
    [TestCase("z", null, "Time passes", "Floyd looks at you with a dash of impatience", "Are you going to open the door?")]
    [TestCase("open door", null, "The door opens and Floyd", "plunges into the Bio Lab", "he is set upon by hideous, mutated monsters", "More are heading straight toward the open door", "Floyd shrieks and yells to you to close the door")]
    [TestCase("close door", null, "The door closes", "And not a moment too soon", "From within the lab you hear ferocious growlings", "the sounds of a skirmish", "high-pitched metallic scream")]
    [TestCase("z", null, "Time passes", "You hear, slightly muffled by the door, three fast knocks", "followed by the distinctive sound of tearing metal")]
    [TestCase("open door", null, "The door opens", "Floyd stumbles out of the Bio Lab", "clutching the mini-booth card", "The mutations rush toward the open doorway")]
    [TestCase("close door", null, "The door closes", "And not a moment too soon", "Floyd staggers to the ground", "dropping the mini card", "badly torn apart", "loose wires and broken circuits", "Oil flows from his lubrication system", "only moments to live", "You drop to your knees and cradle Floyd's head", "Floyd did it", "got card", "Floyd a good friend", "Floyd smiles with contentment", "his eyes close", "in memory of a brave friend")]
    public async Task SuccessfulSequence_PlayerDelays_FloydSacrificesHimself(string input, string? setup, params string[] expectedResponses)
    {
        if (!string.IsNullOrWhiteSpace(setup))
            InvokeGodMode(setup);

        await Do(input, expectedResponses);
    }

    [Test]
    [TestCase("e", "SetupBioLock", "Bio Lock East")]
    [TestCase("z", null, "Time passes", "Floyd stands on his tiptoes", "Looks dangerous in there", "We'll need card there to fix computer", "Floyd will get card", "You open the door, then Floyd will rush in", "Floyd's voice trembles")]
    [TestCase("open door", null, "The door opens and Floyd", "plunges into the Bio Lab", "he is set upon by hideous, mutated monsters", "More are heading straight toward the open door", "Floyd shrieks and yells to you to close the door")]
    [TestCase("close door", null, "The door closes", "And not a moment too soon", "From within the lab you hear ferocious growlings", "the sounds of a skirmish", "high-pitched metallic scream")]
    [TestCase("z", null, "Time passes", "You hear, slightly muffled by the door, three fast knocks", "followed by the distinctive sound of tearing metal")]
    [TestCase("open door", null, "The door opens", "Floyd stumbles out of the Bio Lab", "clutching the mini-booth card", "The mutations rush toward the open doorway")]
    [TestCase("z", null, "Time passes", "biological nightmares reach you", "Gripping coils wrap around your limbs", "powerful teeth begin tearing at your flesh", "you feel a powerful poison", "You have died")]
    public async Task PlayerFailsToCloseTheFinalDoor_PlayerDies(string input, string? setup, params string[] expectedResponses)
    {
        if (!string.IsNullOrWhiteSpace(setup))
            InvokeGodMode(setup);

        await Do(input, expectedResponses);
    }
}

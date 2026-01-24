using GameEngine;
using JetBrains.Annotations;
using Planetfall.Item.Computer;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Location.Computer;
using Planetfall.Location.Lawanda.Lab;

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
    [TestCase("z", null, "The mist in the Bio Lab clears. The mutants recover and rush toward the door")]
    [TestCase("w", "", "The mutants attack you and rip you to shreds within seconds.")]
    public async Task WaitOneTurnTooLongToEnterLab(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);

}
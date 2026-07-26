using JetBrains.Annotations;
using Planetfall.Location.Lawanda;

namespace Planetfall.Tests.Walkthrough;

// Unwinnable walkthroughs, part three: Lawanda.
//
// Lawanda is a full shuttle ride and roughly a hundred and fifty moves from the opening turn, so
// unlike the Feinstein and Kalamontee fixtures these start from a god-mode drop next to the trap -
// the same technique WalkthroughBioLock and WalkthroughMutantChase use - and then walk the last few
// rooms in normally.

/// <summary>
///     The infirmary is a clean, well-lighted place full of beds, and you have been walking for a very
///     long time. Lying down in one is fatal: the diagnostic robot that comes out to look after you has
///     been rusting unattended for several hundred years.
/// </summary>
[TestFixture]
public sealed class WalkthroughSleepInTheInfirmaryBed : WalkthroughTestBase
{
    /// <summary>
    ///     Drop the player into Lawanda at the fork, a few rooms short of the infirmary, with Floyd
    ///     left behind. Floyd's Lazarus scene fires on the second turn spent in the infirmary with him
    ///     present, and it is not what this walkthrough is about.
    /// </summary>
    [UsedImplicitly]
    public void ArriveInLawanda()
    {
        StartHere<Fork>();
    }

    [Test]
    [TestCase("NE", "ArriveInLawanda", "Systems Corridor West")]
    [TestCase("NW", null, "Infirmary", "a number of beds", "complicated looking equipment")]
    // Nothing here is hostile, nothing is on a timer, and the beds are the most inviting thing you
    // have seen since the Feinstein. That is the trap.
    [TestCase("examine equipment", null, "so complicated that you couldn't even begin to figure out how to operate it")]
    [TestCase("lie down in bed", null,
        "You climb into the bed. It is soft and comfortable",
        "a diagnostic robot comes wheeling out",
        "It is very rusty and sways unsteadily",
        "the robot injects you with all 347 serums and medicines it carries",
        "the robot preparing to saw your legs off",
        "*** You have died ***")]
    public async Task Walkthrough(string input, string? setup, params string[] expectedResponses)
        => await DoWithSetup(input, setup, expectedResponses);
}

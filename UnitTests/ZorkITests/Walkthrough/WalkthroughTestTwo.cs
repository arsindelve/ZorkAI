namespace UnitTests.ZorkITests.Walkthrough;

[TestFixture]
public sealed class WalkthroughTestTwo : WalkthroughTestBase
{
    // https://www.allthingsjacq.com/intfic_clubfloyd_20160401.html
    [Test]
    [TestCase("open mailbox", null, "Opening the small mailbox reveals a leaflet.")]
    [TestCase("take leaflet", null, "Taken")]
    [TestCase("read it", null, "ZORK is a game of adventure, danger, and low cunning. In it you will explore")]
    [TestCase("inventory", null, "You are carrying", "A leaflet")]
    [TestCase("examine door", null, "The door is closed.")]
    [TestCase("open door", null, "The door cannot be opened.")]
    [TestCase("examine house", null, "colonial", "wealthy")]
    [TestCase("E", null, "The door is boarded and you can't remove the boards")]
    [TestCase("N", null, "You are facing the north side of a white house")]
    [TestCase("S", null, "The windows are all boarded")]
    [TestCase("SE", null, "Behind House")]
    [TestCase("S", null, "South of House")]
    [TestCase("S", null, "Forest", "dimly lit")]
    [TestCase("N", null, "Clearing", "well marked forest path")]
    [TestCase("E", null, "Canyon View", "Aragain")]
    [TestCase("E", null, "Rocky Ledge", "Above you is more cliff")]
    [TestCase("Down", null, "Canyon Bottom", "You are beneath the walls of the river")]
    [TestCase("N", null, "End of Rainbow", "sunlight shines")]
    [TestCase("SW", null, "Canyon Bottom", "You are beneath the walls of the river")]
    [TestCase("Up", null, "Rocky Ledge", "Above you is more cliff")]
    [TestCase("Up", null, "Canyon View", "Aragain")]
    [TestCase("NW", null, "Clearing", "well marked forest path")]
    [TestCase("S", null, "Forest", "dimly lit")]
    [TestCase("S", null, "Storm-tossed")]
    [TestCase("N", null, "Clearing", "well marked forest path")]
    [TestCase("E", null, "Canyon View", "Aragain")]
    [TestCase("W", null, "Forest", "dimly lit")]
    [TestCase("W", null, "Forest", "appears to be sunlight")]
    [TestCase("E", null, "Forest Path", "low branches")]
    [TestCase("S", null, "North of House")]
    [TestCase("N", null, "Forest Path", "low branches")]
    [TestCase("N", null, "Clearing", "On the ground is a pile of leaves")]
    [TestCase("take leaves", null, "a grating is revealed", "Taken")]
    [TestCase("examine grating", null, "grating is closed")]
    [TestCase("open grating", null, "grating is locked")]
    [TestCase("N", null, "The forest becomes impenetrable to the north")]
    [TestCase("S", null, "Forest Path", "low branches")]
    public async Task Walkthrough(string input, string setup, params string[] expectedResponses)
    {
        if (!string.IsNullOrWhiteSpace(setup)) InvokeGodMode(setup);

        await Do(input, expectedResponses);
    }
}
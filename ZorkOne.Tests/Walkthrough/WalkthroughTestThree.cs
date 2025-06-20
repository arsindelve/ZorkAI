namespace ZorkOne.Tests.Walkthrough;

[TestFixture]
public sealed class WalkthroughTestThree : WalkthroughTestBase
{
    // Book: A shortcut through Adventureland: Vol II - Infocom.
    [Test]
    [TestCase("N", null, "North of House")]
    [TestCase("N", null, "Path")]
    [TestCase("Up", null, "Up A Tree")]
    [TestCase("take egg", null, "Taken")]
    [TestCase("Down", null, "Path")]
    [TestCase("S", null, "North of House")]
    [TestCase("E", null, "Behind House")]
    [TestCase("open window", null, "allow entry")]
    [TestCase("W", null, "Kitchen")]
    [TestCase("W", null, "Living Room")]
    [TestCase("take all", null, "Taken")]
    [TestCase("move rug", null, "trap door")]
    [TestCase("open trap door", null, "staircase")]
    [TestCase("open case", null, "Opened")]
    [TestCase("turn on lantern", null, "on")]
    
    [TestCase("Down", null, "crashes shut")]
    [TestCase("S", null, "Chasm")]
    [TestCase("E", null, "Gallery")]
    [TestCase("take painting", null, "Taken")]
    
    [TestCase("W", null, "Chasm")]
    [TestCase("N", null, "Cellar")]
    [TestCase("N", null, "Troll")]
    [TestCase("kill the troll with the sword", null, "black fog envelops him")]
    [TestCase("drop sword", null, "Dropped")]
    

    [TestCase("W", null, "Maze")]
    [TestCase("S", null, "Maze")]
    [TestCase("E", null, "Maze")]
    [TestCase("Up", null, "Maze")]
    [TestCase("take bag", null, "Taken")]
    [TestCase("SW", null, "Maze")]
    [TestCase("E", null, "Maze")]
    [TestCase("S", null, "Maze")]
    [TestCase("SE", null, "Cyclops")]
    [TestCase("Ulysses", null, "name of his father's deadly nemesis, flees the room by knocking ")]
    [TestCase("E", null, "Passage")]
    [TestCase("E", null, "Living Room")]
    [TestCase("open trap door", null, "staircase")]
    [TestCase("put painting in case", null, "Done")]
    [TestCase("put bag in case", null, "Done")]
    [TestCase("put egg in case", null, "Done")]
    [TestCase("Down", null, "Cellar")]
    [TestCase("N", null, "Troll Room")]
    [TestCase("E", null, "Passage")]
    [TestCase("E", null, "Round Room")]
    [TestCase("E", null, "Loud Room")]
    [TestCase("echo", null, "acoustics" )]
    [TestCase("take bar", null, "Taken" )]
    [TestCase("W", null, "Round Room")]
    [TestCase("S", null, "Narrow")]
    [TestCase("S", null, "Mirror")]
    [TestCase("rub mirror", null, "rumble")]
    
    
    public async Task Walkthrough(string input, string? setup, params string[] expectedResponses)
    {
        if (!string.IsNullOrWhiteSpace(setup))
            InvokeGodMode(setup);

        await Do(input, expectedResponses);
    }
}
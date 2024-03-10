using ZorkOne;

namespace UnitTests;

public sealed class WalkthroughTest : EngineTestsBase
{
    private GameEngine<ZorkI> _target;

    public WalkthroughTest()
    {
        _target = GetTarget();
    }

    [Test]
    public async Task Walkthrough()
    {
        // https://web.mit.edu/marleigh/www/portfolio/Files/zork/transcript.html
        
        _target = GetTarget();

        await Do("open mailbox", "Opening the small mailbox reveals a leaflet.");
        await Do("read leaflet", "ZORK is a game of adventure, danger, and low cunning. In it you will explore");
        await Do("drop leaflet", "Dropped");
        await Do("S", "South of House");
        await Do("E", "Behind House");
        await Do("open window", "With great effort, you open the window far enough to allow entry.");        
        await Do("In", "Kitchen");
        await Do("W", "Living Room");
        await Do("take lantern", "Taken");
        await Do("move rug", "With a great effort,");
        await Do("open trap door", "The door reluctantly opens to reveal a rickety staircase descending into darkness");
        await Do("turn on lantern", "The brass lantern is now on.");

    }

    private async Task Do(string input, string output)
    {
        var result = await _target.GetResponse(input);
        result.Should().Contain(output);
    }
}
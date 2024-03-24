using ZorkOne.GlobalCommand;

namespace UnitTests.GlobalCommands;

[TestFixture]
public class FoolTests : EngineTestsBase
{
    [Test]
    public async Task Xyzzy()
    {
        var target = GetTarget(new IntentParser(new ZorkOneGlobalCommandFactory()));
        var response = await target.GetResponse("xyzzy");
        response.Should().Contain("fool");
    }
    
    [Test]
    public async Task Plugh()
    {
        var target = GetTarget(new IntentParser(new ZorkOneGlobalCommandFactory()));
        var response = await target.GetResponse("Plugh");
        response.Should().Contain("fool");
    }
}
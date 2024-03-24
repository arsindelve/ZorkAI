using ZorkOne.GlobalCommand;

namespace UnitTests.GlobalCommands;

public class RestartCommandsTests : EngineTestsBase
{
    [Test]
    public async Task Restart_Affirmative()
    {
        var engine = GetTarget(new IntentParser(new ZorkOneGlobalCommandFactory()));

        // Act
        await engine.GetResponse("start over");
        var response = await engine.GetResponse("yes");

        // Assert
        response.Should().Contain("-2");
    }

    [Test]
    public async Task Restart_Affirmative_AlternativeResponse()
    {
        var engine = GetTarget(new IntentParser(new ZorkOneGlobalCommandFactory()));

        // Act
        await engine.GetResponse("restart");
        var response = await engine.GetResponse("yup");

        // Assert
        response.Should().Contain("-2");
    }

    [Test]
    public async Task Restart_Cancel_WithBlankInput()
    {
        var engine = GetTarget(new IntentParser(new ZorkOneGlobalCommandFactory()));

        // Act
        await engine.GetResponse("restart");
        await engine.GetResponse("");
        await engine.GetResponse("look");

        var response = await engine.GetResponse("look around");

        // Assert
        response.Should().Contain("You are standing in an open field");
    }
}
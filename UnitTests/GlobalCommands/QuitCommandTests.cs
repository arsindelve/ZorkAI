namespace UnitTests.GlobalCommands;

public class QuitCommandTests : EngineTestsBase
{
    [Test]
    public async Task Quit_Cancel()
    {
        var engine = GetTarget(new IntentParser());

        // Act
        await engine.GetResponse("quit");
        await engine.GetResponse("nevermind");
        await engine.GetResponse("look");

        var response = await engine.GetResponse("look around");

        // Assert
        response.Should().Contain("You are standing in an open field");
    }

    [Test]
    public async Task Quit_Cancel_WithBlankInput()
    {
        var engine = GetTarget(new IntentParser());

        // Act
        await engine.GetResponse("quit");
        await engine.GetResponse("");
        await engine.GetResponse("look");

        var response = await engine.GetResponse("look around");

        // Assert
        response.Should().Contain("You are standing in an open field");
    }

    [Test]
    public async Task Quit_Affirmative()
    {
        var engine = GetTarget(new IntentParser());

        // Act
        await engine.GetResponse("quit");
        var response = await engine.GetResponse("yes");

        // Assert
        response.Should().Contain("-1");
    }

    [Test]
    public async Task Quit_Affirmative_AlternativeResponse()
    {
        var engine = GetTarget(new IntentParser());

        // Act
        await engine.GetResponse("I want to quit");
        var response = await engine.GetResponse("yup");

        // Assert
        response.Should().Contain("-1");
    }
}
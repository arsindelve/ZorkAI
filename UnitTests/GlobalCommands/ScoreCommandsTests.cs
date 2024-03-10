namespace UnitTests.GlobalCommands;

public class ScoreCommandsTests : EngineTestsBase
{
    [Test]
    public async Task Score()
    {
        var engine = GetTarget(new IntentParser());

        // Act
        var response = await engine.GetResponse("score");

        // Assert
        response.Should().Contain("Beginner");
    }

    [Test]
    public async Task Score_AlternateScore_AlternateVery()
    {
        var engine = GetTarget(new IntentParser());
        engine.Context.AddPoints(345);

        // Act
        var response = await engine.GetResponse("what is my score");

        // Assert
        response.Should().Contain("Wizard");
    }
}
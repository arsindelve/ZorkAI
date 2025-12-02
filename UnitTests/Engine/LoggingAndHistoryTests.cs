using CloudWatch;
using CloudWatch.Model;
using GameEngine;
using GameEngine.Item;
using Model.AIGeneration;
using Model.AIParsing;
using Model.Interface;
using Model.AIGeneration.Requests;
using ZorkOne;
using ZorkOne.GlobalCommand;

namespace UnitTests.Engine;

public class LoggingAndHistoryTests
{
    private static GameEngine<ZorkI, ZorkIContext> BuildEngine(
        Mock<IGenerationClient> client,
        Mock<ICloudWatchLogger<TurnLog>> turnLogger)
    {
        // Simple take/drop parser used by ItemProcessorFactory in tests
        var takeAndDropParser = new Mock<IAITakeAndAndDropParser>();
        takeAndDropParser
            .Setup(s => s.GetListOfItemsToDrop(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((string input, string _) =>
            {
                var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return words.Length > 1 ? new[] { words[1] } : Array.Empty<string>();
            });
        takeAndDropParser
            .Setup(s => s.GetListOfItemsToTake(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((string input, string _) =>
            {
                var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return words.Length > 1 ? new[] { words[1] } : Array.Empty<string>();
            });

        var itemProcessorFactory = new ItemProcessorFactory(takeAndDropParser.Object);
        var parser = new IntentParser(Mock.Of<IAIParser>(), new ZorkOneGlobalCommandFactory());
        var secrets = Mock.Of<ISecretsManager>();

        var engine = new GameEngine<ZorkI, ZorkIContext>(
            itemProcessorFactory,
            parser,
            client.Object,
            secrets,
            turnLogger.Object);

        // Ensure starting location is initialized consistently
        Repository.Reset();
        Repository.GetLocation<WestOfHouse>().Init();
        engine.Context.Verbosity = Verbosity.Verbose;
        return engine;
    }

    [Test]
    public async Task PostProcessing_WritesTurnLog_And_UpdatesLastFive()
    {
        // Arrange
        var client = new Mock<IGenerationClient>();
        client.SetupAllProperties();
        client.Object.LastFiveInputOutputs = new List<(string, string, bool)>();

        // For commands that generate narration (empty input path), provide a stubbed result
        client
            .Setup(c => c.GenerateNarration(It.IsAny<Request>(), It.IsAny<string>()))
            .ReturnsAsync("Generated");

        var turnLogger = new Mock<ICloudWatchLogger<TurnLog>>();
        var engine = BuildEngine(client, turnLogger);

        // Act: issue several commands that should all go through PostProcessing
        var inputs = new[] { "look", "inventory", "wait", "east", "west", "look" };
        foreach (var input in inputs)
        {
            var response = await engine.GetResponse(input);
            response.Should().NotBeNullOrEmpty();
        }

        // Assert CloudWatch logger called with expected payload at least once
        turnLogger.Verify(l => l.WriteLogEvents(It.Is<TurnLog>(t =>
                !string.IsNullOrEmpty(t.SessionId) &&
                !string.IsNullOrEmpty(t.Location) &&
                t.Moves >= 0 &&
                !string.IsNullOrEmpty(t.Input) &&
                !string.IsNullOrEmpty(t.Response)
            )),
            Times.AtLeastOnce);

        // Assert LastFiveInputOutputs capped and contains latest entry
        client.Object.LastFiveInputOutputs.Should().NotBeNull();
        client.Object.LastFiveInputOutputs.Count.Should().BeLessOrEqualTo(5);
        var last = client.Object.LastFiveInputOutputs.Last();
        last.Item1.Should().Be("look"); // last input
        last.Item2.Should().NotBeNullOrEmpty(); // response
    }

    [Test]
    public async Task PostProcessing_EmptyInput_NoTurnLog_ButUpdatesLastFive()
    {
        // Arrange
        var client = new Mock<IGenerationClient>();
        client.SetupAllProperties();
        client.Object.LastFiveInputOutputs = new List<(string, string, bool)>();
        client
            .Setup(c => c.GenerateNarration(It.IsAny<Request>(), It.IsAny<string>()))
            .ReturnsAsync("Empty");

        var turnLogger = new Mock<ICloudWatchLogger<TurnLog>>();
        var engine = BuildEngine(client, turnLogger);

        // Act: empty input path should not write a TurnLog
        var response = await engine.GetResponse(string.Empty);

        // Assert
        response.Should().NotBeNullOrEmpty();
        turnLogger.Verify(l => l.WriteLogEvents(It.IsAny<TurnLog>()), Times.Never);

        client.Object.LastFiveInputOutputs.Should().NotBeNull();
        client.Object.LastFiveInputOutputs.Should().HaveCount(1);
        var last = client.Object.LastFiveInputOutputs.Last();
        last.Item1.Should().Be(""); // input is empty string
        last.Item2.Should().Contain("Empty"); // generated response captured
    }

    [Test]
    public async Task LastFive_History_EvictionOrder()
    {
        // Arrange
        var client = new Mock<IGenerationClient>();
        client.SetupAllProperties();
        client.Object.LastFiveInputOutputs = new List<(string, string, bool)>();
        client
            .Setup(c => c.GenerateNarration(It.IsAny<Request>(), It.IsAny<string>()))
            .ReturnsAsync("Generated");

        var turnLogger = new Mock<ICloudWatchLogger<TurnLog>>();
        var engine = BuildEngine(client, turnLogger);

        var inputs = new[] { "look", "inventory", "wait", "east", "west", "look" };

        // Act
        foreach (var input in inputs)
            (await engine.GetResponse(input)).Should().NotBeNullOrEmpty();

        // Assert: capped at 5 and oldest evicted
        var history = client.Object.LastFiveInputOutputs;
        history.Should().NotBeNull();
        history.Should().HaveCount(5);

        // Oldest (index 0) should now be the second input, newest (last) the sixth
        history[0].Item1.Should().Be(inputs[1]);
        history.Last().Item1.Should().Be(inputs[5]);
    }
}

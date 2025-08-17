using FluentAssertions;
using GameEngine;
using GameEngine.Item;
using Moq;
using NUnit.Framework;
using ZorkOne;
using ZorkOne.GlobalCommand;
using Model.AIGeneration;
using Model.AIParsing;
using Model.Interface;

namespace UnitTests.Engine;

public class InitializeEngineResiliencyTests
{
    private static GameEngine<ZorkI, ZorkIContext> BuildEngine(Mock<ISecretsManager> secrets)
    {
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

        var client = new Mock<IGenerationClient>();
        client.SetupAllProperties();
        client.Object.LastFiveInputOutputs = new List<(string, string, bool)>();
        client.Setup(c => c.GenerateNarration(It.IsAny<Model.AIGeneration.Requests.Request>(), It.IsAny<string>()))
            .ReturnsAsync("Generated");

        var engine = new GameEngine<ZorkI, ZorkIContext>(
            itemProcessorFactory,
            parser,
            client.Object,
            secrets.Object,
            Mock.Of<CloudWatch.ICloudWatchLogger<CloudWatch.Model.TurnLog>>());

        Repository.Reset();
        Repository.GetLocation<WestOfHouse>().Init();
        engine.Context.Verbosity = Verbosity.Verbose;
        return engine;
    }

    [Test]
    public async Task InitializeEngine_DoesNotThrow_WhenSecretsThrows_AndEngineStillUsable()
    {
        // Arrange
        var secrets = new Mock<ISecretsManager>();
        secrets.Setup(s => s.GetSecret(It.IsAny<string>()))
            .ThrowsAsync(new Exception("boom"));

        var engine = BuildEngine(secrets);

        // Act: InitializeEngine should swallow exceptions and not throw
        await engine.InitializeEngine();

        // Engine should still process a simple command
        var output = await engine.GetResponse("look");

        // Assert
        output.Should().NotBeNullOrEmpty();
    }
}

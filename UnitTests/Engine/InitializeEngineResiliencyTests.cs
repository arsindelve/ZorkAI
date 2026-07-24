using ChatLambda;
using GameEngine;
using GameEngine.Item;
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
        return BuildEngine(secrets, out _);
    }

    // IGenerationClient.SystemPrompt is set-only, so tests that care about it observe the set on the
    // mock rather than reading the property back.
    private static GameEngine<ZorkI, ZorkIContext> BuildEngine(Mock<ISecretsManager> secrets,
        out Mock<IGenerationClient> generationClient)
    {
        var takeAndDropParser = new Mock<IAITakeAndAndDropParser>();
        takeAndDropParser
            .Setup(s => s.GetListOfItemsToDrop(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((string input, string _) =>
            {
                var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return words.Length > 1 ? [words[1]] : [];
            });
        takeAndDropParser
            .Setup(s => s.GetListOfItemsToTake(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((string input, string _) =>
            {
                var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return words.Length > 1 ? [words[1]] : [];
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
            Mock.Of<CloudWatch.ICloudWatchLogger<CloudWatch.Model.TurnLog>>(),
            Mock.Of<IParseConversation>());

        Repository.Reset();
        Repository.GetLocation<WestOfHouse>().Init();
        engine.Context.Verbosity = Verbosity.Verbose;
        generationClient = client;
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

    [Test]
    public async Task InitializeEngine_StillAppliesTheNarratorSystemPrompt_When_EndpointConfigIsInvalid()
    {
        // A typo in the self-hosted endpoint variables (issue #383) used to throw from inside
        // InitializeEngine's single shared try/catch, which skipped the system-prompt assignment
        // underneath it and left the narrator running promptless for the rest of the session. The
        // same hazard applied to a transient CloudWatch failure. Engine startup must not depend on -
        // or even read - the AI endpoint configuration, and the prompt must survive whatever else fails.
        var secrets = new Mock<ISecretsManager>();
        secrets.Setup(s => s.GetSecret(It.IsAny<string>())).ReturnsAsync("THE NARRATOR PROMPT");

        // Built before the variable is poisoned: a bad provider also throws from PronounResolver's
        // constructor, and that loud, immediate failure is a separate concern from the silent one
        // under test here. This isolates the startup path.
        var engine = BuildEngine(secrets, out var generationClient);

        var original = Environment.GetEnvironmentVariable("ZORKAI_PROVIDER");
        try
        {
            Environment.SetEnvironmentVariable("ZORKAI_PROVIDER", "not-a-real-provider");

            await engine.InitializeEngine();

            generationClient.VerifySet(c => c.SystemPrompt = "THE NARRATOR PROMPT", Times.Once,
                "the narrator's prompt must still be applied when endpoint configuration is broken");
        }
        finally
        {
            Environment.SetEnvironmentVariable("ZORKAI_PROVIDER", original);
        }
    }

    [TestCase("OPENAI_BASE_URL", "http://localhost:1234/v1")]
    [TestCase("ZORKAI_PROVIDER", "ollama")]
    public void CloudLoggingStaysEnabled_When_SelfHostedEndpointVariablesAreSet(string variable, string value)
    {
        // The endpoint variables say where the *model* lives, not whether we have AWS. OPENAI_BASE_URL
        // especially is a generic name that proxies and gateways also use, so letting it reach the
        // logging decision would mean a deployed Lambda silently stops emitting telemetry the moment
        // someone routes it through an LLM proxy. Only an explicit opt-out may disable logging.
        var original = Environment.GetEnvironmentVariable(variable);
        try
        {
            Environment.SetEnvironmentVariable(variable, value);

            var secrets = new Mock<ISecretsManager>();
            secrets.Setup(s => s.GetSecret(It.IsAny<string>())).ReturnsAsync("prompt");

            var engine = BuildEngine(secrets);

            engine.CloudLoggingEnabled.Should().BeTrue();
        }
        finally
        {
            Environment.SetEnvironmentVariable(variable, original);
        }
    }
}

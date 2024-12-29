using CloudWatch;
using CloudWatch.Model;
using GameEngine;
using Model.AIGeneration;
using Model.AIParsing;
using Model.Interface;
using ZorkOne;
using ZorkOne.GlobalCommand;

namespace UnitTests;

public class EngineTestsBase
{
    protected Mock<IGenerationClient> Client = new();
    protected IIntentParser Parser = new IntentParser(Mock.Of<IAIParser>(), new ZorkOneGlobalCommandFactory());

    /// <summary>
    ///     Returns an instance of the GameEngine class with the specified parser and client.
    ///     If parser is not provided, a default TestParser instance is used.
    /// </summary>
    /// <param name="parser">Optional parameter for the intent parser.</param>
    /// <returns>An instance of the GameEngine class.</returns>
    protected GameEngine<ZorkI, ZorkIContext> GetTarget(IIntentParser? parser = null)
    {
        Client = new Mock<IGenerationClient>();
        Parser = parser ?? new TestParser(new ZorkOneGlobalCommandFactory());

        Repository.Reset();

        var engine = new GameEngine<ZorkI, ZorkIContext>(Parser, Client.Object, Mock.Of<ISecretsManager>(), Mock.Of<ICloudWatchLogger<TurnLog>>());
        engine.Context.Verbosity = Verbosity.Verbose;
        Repository.GetLocation<WestOfHouse>().Init();

        return engine;
    }
}
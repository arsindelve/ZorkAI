using GameEngine;
using Model;
using Model.AIGeneration;
using Model.Interface;
using Moq;
using Planetfall.Location.Feinstein;
using UnitTests;


namespace Planetfall.Tests;

public class EngineTestsBase
{
    protected Mock<IGenerationClient> Client = new();
    protected IIntentParser Parser = new IntentParser(new PlanetfallGlobalCommandFactory());

    /// <summary>
    ///     Returns an instance of the GameEngine class with the specified parser and client.
    ///     If parser is not provided, a default TestParser instance is used.
    /// </summary>
    /// <param name="parser">Optional parameter for the intent parser.</param>
    /// <returns>An instance of the GameEngine class.</returns>
    protected GameEngine<PlanetfallGame, PlanetfallContext> GetTarget(IIntentParser? parser = null)
    {
        Client = new Mock<IGenerationClient>();
        Parser = parser ?? new TestParser(new PlanetfallGlobalCommandFactory());

        Repository.Reset();

        var engine = new GameEngine<PlanetfallGame, PlanetfallContext>(Parser, Client.Object, Mock.Of<ISecretsManager>());
        engine.Context.Verbosity = Verbosity.Verbose;
        Repository.GetLocation<DeckNine>().Init();

        return engine;
    }
}
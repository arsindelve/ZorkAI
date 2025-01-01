using CloudWatch;
using CloudWatch.Model;
using GameEngine;
using Model;
using Model.AIGeneration;
using Model.Interface;
using Model.Item;
using Model.Location;
using Moq;
using Planetfall.GlobalCommand;
using Planetfall.Location.Feinstein;
using UnitTests;

namespace Planetfall.Tests;

public class EngineTestsBase
{
    protected Mock<IGenerationClient> Client = new();
    protected IIntentParser Parser = Mock.Of<IIntentParser>();
    private PlanetfallContext Context { get; set; }


    protected void StartHere<T>() where T : class, ILocation, new()
    {
        Context.CurrentLocation = GetLocation<T>();
    }

    protected T Take<T>() where T : IItem, new()
    {
        var item = GetItem<T>();
        Context.ItemPlacedHere(item);
        return item;
    }
    
    /// <summary>
    ///     Returns an instance of the GameEngine class with the specified parser and client.
    ///     If parser is not provided, a default TestParser instance is used.
    /// </summary>
    /// <param name="parser">Optional parameter for the intent parser.</param>
    /// <returns>An instance of the GameEngine class.</returns>
    protected GameEngine<PlanetfallGame, PlanetfallContext> GetTarget(IIntentParser? parser = null)
    {
        Client = new Mock<IGenerationClient>();
        Parser = parser ?? new TestParser(new PlanetfallGlobalCommandFactory(), "Planetfall");

        Repository.Reset();

        var engine = new GameEngine<PlanetfallGame, PlanetfallContext>(Parser, Client.Object, Mock.Of<ISecretsManager>(), Mock.Of<ICloudWatchLogger<TurnLog>>());
        engine.Context.Verbosity = Verbosity.Verbose;
        Repository.GetLocation<DeckNine>().Init();

        Context = engine.Context;
        
        return engine;
    }
   

    protected T GetItem<T>() where T : IItem, new()
    {
        return Repository.GetItem<T>();
    }
    
    protected T GetLocation<T>() where T : class, ILocation, new()
    {
        return Repository.GetLocation<T>();
    }
}
using ChatLambda;
using CloudWatch;
using CloudWatch.Model;
using GameEngine;
using GameEngine.Item;
using Model;
using Model.AIGeneration;
using Model.AIParsing;
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
    private Mock<IGenerationClient> _client = new();
    private IIntentParser _parser = Mock.Of<IIntentParser>();
    private PlanetfallContext Context { get; set; } = null!;

    protected T StartHere<T>() where T : class, ILocation, new()
    {
        // Since the test wants to drop us into a specific location, remove any
        // prior actors. 
        Context.Actors.Clear();
        T location = GetLocation<T>();
        Context.CurrentLocation = location;
        return location;
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
        _client = new Mock<IGenerationClient>();
        _parser = parser ?? new TestParser(new PlanetfallGlobalCommandFactory(), "Planetfall");

        Repository.Reset();
        
        var takeAndDropParser = new Mock<IAITakeAndAndDropParser>();
        takeAndDropParser.Setup(s => s.GetListOfItemsToDrop(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((string input, string context) =>
            {
                var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return words.Length > 1 ? [words[1]] : [];
            });
        
        takeAndDropParser.Setup(s => s.GetListOfItemsToTake(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((string input, string context) =>
            {
                var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                return words.Length > 1 ? [words[1]] : [];
            });

        // Create a simple mock ParseConversation that returns "no conversation" for all inputs
        var mockParseConversation = new Mock<IParseConversation>();
        mockParseConversation.Setup(x => x.ParseAsync(It.IsAny<string>()))
            .ReturnsAsync((true, "")); // Always return "no conversation" so tests behave like before
        
        var engine = new GameEngine<PlanetfallGame, PlanetfallContext>(new ItemProcessorFactory(takeAndDropParser.Object), _parser, _client.Object,
            Mock.Of<ISecretsManager>(), Mock.Of<ICloudWatchLogger<TurnLog>>(), mockParseConversation.Object);
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
using GameEngine;
using Planetfall;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Location.Kalamontee.Tower;
using Planetfall.Location.Lawanda;
using Planetfall.Location.Shuttle;
using ZorkAI.OpenAI;

namespace IntegrationTests;

/// <summary>
/// Integration tests to verify Floyd's one-time comments when entering special locations.
/// These tests call the actual OpenAI API to generate Floyd's responses.
/// Run individual tests to see the AI-generated output.
/// </summary>
[TestFixture]
[Explicit("Requires OPEN_AI_KEY environment variable - Integration test")]
[Parallelizable(ParallelScope.Children)]
public class FloydLocationCommentTests
{
    [SetUp]
    public void Setup()
    {
        // Load environment variables from .env file if present
        Env.TraversePath().Load();
    }

    [Test]
    public async Task FloydComment_KalamonteePlatform_VagueMemoryOfShuttle()
    {
        Repository.Reset();

        // Create real generation client (requires OPEN_AI_KEY)
        var generationClient = new ChatGPTClient(null);

        // Set up the context
        var context = new PlanetfallContext();

        // Place Floyd in the location and turn him on
        var platform = Repository.GetLocation<KalamonteePlatform>();
        var floyd = Repository.GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        platform.ItemPlacedHere(floyd);

        // Enter the location with Floyd present
        context.CurrentLocation = platform;

        // Call AfterEnterLocation to trigger Floyd's comment
        var previousLocation = Repository.GetLocation<WaitingArea>();
        var result = await platform.AfterEnterLocation(context, previousLocation, generationClient);

        Console.WriteLine("=== Floyd's Comment at Kalamontee Platform ===");
        Console.WriteLine(result);
        Console.WriteLine("==============================================");

        // Verify we got a response
        result.Should().NotBeNullOrEmpty("Floyd should have made a comment");

        // Verify the interaction was marked as happened (so it won't repeat)
        platform.InteractionHasHappened.Should().BeTrue();
    }

    [Test]
    public async Task FloydComment_ObservationDeck_BreathtakingView()
    {
        Repository.Reset();

        // Create real generation client (requires OPEN_AI_KEY)
        var generationClient = new ChatGPTClient(null);

        // Set up the context
        var context = new PlanetfallContext();

        // Place Floyd in the location and turn him on
        var deck = Repository.GetLocation<ObservationDeck>();
        var floyd = Repository.GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        deck.ItemPlacedHere(floyd);

        // Enter the location with Floyd present
        context.CurrentLocation = deck;

        // Call AfterEnterLocation to trigger Floyd's comment
        var previousLocation = Repository.GetLocation<TowerCore>();
        var result = await deck.AfterEnterLocation(context, previousLocation, generationClient);

        Console.WriteLine("=== Floyd's Comment at Observation Deck ===");
        Console.WriteLine(result);
        Console.WriteLine("============================================");

        // Verify we got a response
        result.Should().NotBeNullOrEmpty("Floyd should have made a comment");

        // Verify the interaction was marked as happened (so it won't repeat)
        deck.InteractionHasHappened.Should().BeTrue();
    }

    [Test]
    public async Task FloydComment_DoesNotRepeat_WhenReenteringLocation()
    {
        Repository.Reset();

        // Create real generation client
        var generationClient = new ChatGPTClient(null);

        // Set up the context
        var context = new PlanetfallContext();

        // Place Floyd in the location and turn him on
        var platform = Repository.GetLocation<KalamonteePlatform>();
        var floyd = Repository.GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        platform.ItemPlacedHere(floyd);

        context.CurrentLocation = platform;
        var previousLocation = Repository.GetLocation<WaitingArea>();

        // First entry - should get comment
        var firstResult = await platform.AfterEnterLocation(context, previousLocation, generationClient);
        Console.WriteLine("=== First Entry ===");
        Console.WriteLine(firstResult);

        // Second entry - should NOT get comment
        var secondResult = await platform.AfterEnterLocation(context, previousLocation, generationClient);
        Console.WriteLine("=== Second Entry ===");
        Console.WriteLine(string.IsNullOrEmpty(secondResult) ? "(No comment - as expected)" : secondResult);
        Console.WriteLine("====================");

        // First entry should have a comment, second should be empty
        firstResult.Should().NotBeNullOrEmpty();
        secondResult.Should().BeEmpty("Floyd should not repeat the same comment");
    }

    [Test]
    public async Task FloydComment_NoComment_WhenFloydNotPresent()
    {
        Repository.Reset();

        // Create real generation client
        var generationClient = new ChatGPTClient(null);

        // Set up the context - but DON'T place Floyd here
        var context = new PlanetfallContext();
        var platform = Repository.GetLocation<KalamonteePlatform>();

        // Floyd exists but is elsewhere
        var floyd = Repository.GetItem<Floyd>();
        floyd.IsOn = true;
        var otherLocation = Repository.GetLocation<WaitingArea>();
        otherLocation.ItemPlacedHere(floyd);

        context.CurrentLocation = platform;

        // Enter without Floyd present
        var result = await platform.AfterEnterLocation(context, otherLocation, generationClient);

        Console.WriteLine("=== Entry Without Floyd ===");
        Console.WriteLine(string.IsNullOrEmpty(result) ? "(No comment - Floyd not here)" : result);
        Console.WriteLine("===========================");

        // Should get no comment since Floyd isn't in the location
        result.Should().BeEmpty("Floyd should not comment when not present");

        // The interaction should NOT be marked as happened
        platform.InteractionHasHappened.Should().BeFalse();
    }

    [Test]
    public async Task FloydComment_LawandaPlatform_WonderingWhereWeAre()
    {
        Repository.Reset();

        // Create real generation client (requires OPEN_AI_KEY)
        var generationClient = new ChatGPTClient(null);

        // Set up the context
        var context = new PlanetfallContext();

        // Place Floyd in the location and turn him on
        var platform = Repository.GetLocation<LawandaPlatform>();
        var floyd = Repository.GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        platform.ItemPlacedHere(floyd);

        // Enter the location with Floyd present
        context.CurrentLocation = platform;

        // Call AfterEnterLocation to trigger Floyd's comment
        // Coming from the shuttle car after a long trip
        var previousLocation = Repository.GetLocation<ShuttleCarAlfie>();
        var result = await platform.AfterEnterLocation(context, previousLocation, generationClient);

        Console.WriteLine("=== Floyd's Comment at Lawanda Platform ===");
        Console.WriteLine(result);
        Console.WriteLine("============================================");

        // Verify we got a response
        result.Should().NotBeNullOrEmpty("Floyd should have made a comment");

        // Verify the interaction was marked as happened (so it won't repeat)
        platform.InteractionHasHappened.Should().BeTrue();
    }
}
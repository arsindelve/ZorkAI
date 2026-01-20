using GameEngine;
using Planetfall;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
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

        // Place Floyd in a PREVIOUS location (he will follow the player)
        var previousLocation = Repository.GetLocation<WaitingArea>();
        var platform = Repository.GetLocation<KalamonteePlatform>();
        var floyd = Repository.GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.TurnOnCountdown = 0; // Skip the activation delay
        previousLocation.ItemPlacedHere(floyd);

        // Player moves to the platform
        context.CurrentLocation = platform;

        // Floyd follows the player - this triggers the special location comment
        var result = await floyd.Act(context, generationClient);

        Console.WriteLine("=== Floyd's Comment at Kalamontee Platform ===");
        Console.WriteLine(result);
        Console.WriteLine("==============================================");

        // Verify we got a response (includes "Floyd follows you. " prefix)
        result.Should().NotBeNullOrEmpty("Floyd should have made a comment");

        // Verify the interaction was marked as happened (so it won't repeat)
        platform.InteractionHasHappened.Should().BeTrue();
    }

    [Test]
    public async Task FloydComment_PhysicalPlant_HugeRoom()
    {
        Repository.Reset();

        // Create real generation client (requires OPEN_AI_KEY)
        var generationClient = new ChatGPTClient(null);

        // Set up the context
        var context = new PlanetfallContext();

        // Place Floyd in a PREVIOUS location (he will follow the player)
        var previousLocation = Repository.GetLocation<Planetfall.Location.Kalamontee.Mech.MechCorridorNorth>();
        var physicalPlant = Repository.GetLocation<Planetfall.Location.Kalamontee.Mech.PhysicalPlant>();
        var floyd = Repository.GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.TurnOnCountdown = 0; // Skip the activation delay
        previousLocation.ItemPlacedHere(floyd);

        // Player moves to the physical plant
        context.CurrentLocation = physicalPlant;

        // Floyd follows the player - this triggers the special location comment
        var result = await floyd.Act(context, generationClient);

        Console.WriteLine("=== Floyd's Comment at Physical Plant ===");
        Console.WriteLine(result);
        Console.WriteLine("==========================================");

        // Verify we got a response (includes "Floyd follows you. " prefix)
        result.Should().NotBeNullOrEmpty("Floyd should have made a comment");

        // Verify the interaction was marked as happened (so it won't repeat)
        physicalPlant.InteractionHasHappened.Should().BeTrue();
    }

    [Test]
    public async Task FloydComment_ObservationDeck_BreathtakingView()
    {
        Repository.Reset();

        // Create real generation client (requires OPEN_AI_KEY)
        var generationClient = new ChatGPTClient(null);

        // Set up the context
        var context = new PlanetfallContext();

        // Place Floyd in a PREVIOUS location (he will follow the player)
        var previousLocation = Repository.GetLocation<Planetfall.Location.Kalamontee.Tower.TowerCore>();
        var deck = Repository.GetLocation<Planetfall.Location.Kalamontee.Tower.ObservationDeck>();
        var floyd = Repository.GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.TurnOnCountdown = 0; // Skip the activation delay
        previousLocation.ItemPlacedHere(floyd);

        // Player moves to the observation deck
        context.CurrentLocation = deck;

        // Floyd follows the player - this triggers the special location comment
        var result = await floyd.Act(context, generationClient);

        Console.WriteLine("=== Floyd's Comment at Observation Deck ===");
        Console.WriteLine(result);
        Console.WriteLine("============================================");

        // Verify we got a response (includes "Floyd follows you. " prefix)
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

        // Place Floyd in a PREVIOUS location (he will follow the player)
        var previousLocation = Repository.GetLocation<WaitingArea>();
        var platform = Repository.GetLocation<KalamonteePlatform>();
        var floyd = Repository.GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.TurnOnCountdown = 0; // Skip the activation delay
        previousLocation.ItemPlacedHere(floyd);

        // Player moves to the platform
        context.CurrentLocation = platform;

        // First entry - Floyd follows and should get comment
        var firstResult = await floyd.Act(context, generationClient);
        Console.WriteLine("=== First Entry ===");
        Console.WriteLine(firstResult);

        // Move Floyd back to previous location to simulate leaving and re-entering
        platform.RemoveItem(floyd);
        previousLocation.ItemPlacedHere(floyd);

        // Second entry - should NOT get comment
        var secondResult = await floyd.Act(context, generationClient);
        Console.WriteLine("=== Second Entry ===");
        Console.WriteLine(secondResult.Contains("Floyd follows you.") && !secondResult.Contains("\n\n")
            ? "(No special comment - as expected, just follows)"
            : secondResult);
        Console.WriteLine("====================");

        // First entry should have a special comment (contains newlines for the comment)
        firstResult.Should().Contain("\n\n", "First entry should include special location comment");
        // Second entry should just be "Floyd follows you. " without the special comment
        secondResult.Should().NotContain("\n\n", "Floyd should not repeat the same comment");
    }

    [Test]
    public async Task FloydComment_NoComment_WhenFloydNotPresent()
    {
        Repository.Reset();

        // Create real generation client
        var generationClient = new ChatGPTClient(null);

        // Set up the context
        var context = new PlanetfallContext();
        var platform = Repository.GetLocation<KalamonteePlatform>();

        // Floyd exists and is already in the same location as the player
        // (so he won't follow - already there)
        var floyd = Repository.GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.TurnOnCountdown = 0; // Skip the activation delay
        platform.ItemPlacedHere(floyd);

        context.CurrentLocation = platform;

        // Floyd is already in the room, so HandleFollowingPlayer won't trigger
        // This simulates the player entering alone and Floyd already being there
        var result = await floyd.Act(context, generationClient);

        Console.WriteLine("=== Floyd Already In Room (No Follow) ===");
        Console.WriteLine(string.IsNullOrEmpty(result) ? "(No follow comment - Floyd already here)" : result);
        Console.WriteLine("=========================================");

        // Floyd doesn't follow, so no special location comment
        // (He might do random behavior, but no "Floyd follows you" comment)
        result.Should().NotContain("Floyd follows you", "Floyd was already in the room");

        // The interaction should NOT be marked as happened (Floyd didn't follow)
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

        // Place Floyd in a PREVIOUS location (he will follow the player)
        var previousLocation = Repository.GetLocation<ShuttleCarAlfie>();
        var platform = Repository.GetLocation<LawandaPlatform>();
        var floyd = Repository.GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.TurnOnCountdown = 0; // Skip the activation delay
        previousLocation.ItemPlacedHere(floyd);

        // Player moves to the platform
        context.CurrentLocation = platform;

        // Floyd follows the player - this triggers the special location comment
        var result = await floyd.Act(context, generationClient);

        Console.WriteLine("=== Floyd's Comment at Lawanda Platform ===");
        Console.WriteLine(result);
        Console.WriteLine("============================================");

        // Verify we got a response (includes "Floyd follows you. " prefix)
        result.Should().NotBeNullOrEmpty("Floyd should have made a comment");

        // Verify the interaction was marked as happened (so it won't repeat)
        platform.InteractionHasHappened.Should().BeTrue();
    }
}
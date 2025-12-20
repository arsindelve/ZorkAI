using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Model.Web;

namespace ZorkOne.Tests.Walkthrough;

/// <summary>
/// Exploring Zork by calling the REAL Lambda API running locally.
/// This gives us rich structured data (location, score, inventory, exits) for better assertions.
///
/// SETUP:
/// 1. Start the Lambda API: cd Lambda/src/Lambda && dotnet run
/// 2. API runs on http://localhost:5000
/// 3. Run these tests to explore the game
///
/// The GameResponse includes:
/// - Response (text)
/// - LocationName (where we are)
/// - Moves, Score, Time (game stats)
/// - Inventory (list of items)
/// - Exits (available directions)
/// </summary>
[TestFixture]
[Explicit("Integration test - requires Lambda API running on localhost:5000")]
public class ExploringZorkViaAPI
{
    private HttpClient _client = null!;
    private string _sessionId = null!;
    private JsonSerializerOptions _jsonOptions = null!;

    [SetUp]
    public void Setup()
    {
        _client = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };
        _sessionId = Guid.NewGuid().ToString();

        // Configure JSON options to handle record types properly
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
    }

    /// <summary>
    /// Send a command to the game and get back structured response
    /// </summary>
    private async Task<GameResponse> Do(string input)
    {
        var request = new GameRequest(input, _sessionId);
        var response = await _client.PostAsJsonAsync("/ZorkOne", request);
        response.EnsureSuccessStatusCode();

        // Read as JSON string first, then parse manually
        var jsonString = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(jsonString);
        var root = jsonDoc.RootElement;

        // Manually extract values from JSON
        var gameResponse = new GameResponse(
            Response: root.GetProperty("response").GetString()!,
            LocationName: root.GetProperty("locationName").GetString()!,
            Moves: root.GetProperty("moves").GetInt32(),
            Score: root.GetProperty("score").GetInt32(),
            Time: root.GetProperty("time").GetInt32(),
            PreviousLocationName: root.TryGetProperty("previousLocationName", out var prevLoc) ? prevLoc.GetString() : null,
            LastMovementDirection: root.TryGetProperty("lastMovementDirection", out var lastDir) ? lastDir.GetString() : null,
            Inventory: root.GetProperty("inventory").EnumerateArray().Select(e => e.GetString()!).ToList(),
            Exits: root.GetProperty("exits").EnumerateArray().Select(e => (Model.Movement.Direction)e.GetInt32()).ToList()
        );

        // Display for debugging
        Console.WriteLine($"\n> {input}");
        Console.WriteLine($"Location: {gameResponse.LocationName}");
        Console.WriteLine($"Score: {gameResponse.Score}, Moves: {gameResponse.Moves}");
        Console.WriteLine($"Inventory: {string.Join(", ", gameResponse.Inventory)}");
        Console.WriteLine($"Exits: {string.Join(", ", gameResponse.Exits)}");
        Console.WriteLine($"\n{gameResponse.Response}");

        return gameResponse;
    }

    [Test]
    public async Task StartGame_ExploreMailbox()
    {
        // Start the game - should be at West of House
        var response = await Do("look");
        response.LocationName.Should().Be("West Of House");
        response.Score.Should().Be(0);
        response.Moves.Should().Be(1); // "look" counts as a move
        response.Inventory.Should().BeEmpty();

        // Open the mailbox using natural language
        response = await Do("Could you please open that mailbox?");
        response.Response.Should().Contain("leaflet");
        response.LocationName.Should().Be("West Of House"); // Still at same location

        // Take the leaflet - should now be in inventory
        response = await Do("take the leaflet");
        response.Inventory.Should().Contain("leaflet");

        // Read it
        response = await Do("read the leaflet");
        response.Response.Should().Contain("ZORK");
    }

    [Test]
    public async Task TestQuestionPhrasing_HowDoI()
    {
        await Do("look"); // Initialize at West of House

        // EXPLORATION: How does the parser handle "How do I X?" questions?
        // Does it interpret them as requests for help, or as commands to DO the action?
        var response = await Do("How do I get inside the house?");

        Console.WriteLine("\n=== PARSER BEHAVIOR ANALYSIS ===");
        Console.WriteLine($"Player asked: 'How do I get inside the house?'");
        Console.WriteLine($"Response: {response.Response}");
        Console.WriteLine($"Location changed: {response.LocationName != "West Of House"}");
        Console.WriteLine($"================================\n");

        // Document the behavior: Does it try the boarded door? Give directions? Something else?
        if (response.Response.Contains("boarded") || response.Response.Contains("door"))
        {
            Console.WriteLine("FINDING: 'How do I X?' was interpreted as a COMMAND to execute X");
            Console.WriteLine("The parser tried to go inside through the door");
        }
        else if (response.Response.Contains("window") || response.Response.Contains("around"))
        {
            Console.WriteLine("FINDING: 'How do I X?' was interpreted as a REQUEST FOR HELP");
            Console.WriteLine("The parser provided directions/suggestions");
        }
    }

    [Test]
    public async Task ExploreConversationalPhrasing()
    {
        await Do("look"); // Initialize

        // Try polite request
        var response = await Do("Could you please open the mailbox for me?");
        Console.WriteLine($"Polite: {(response.Response.Contains("leaflet") ? "WORKED" : "FAILED")}");

        // Try "I want to" phrasing
        response = await Do("I want to take the leaflet");
        Console.WriteLine($"I want to: Inventory has {response.Inventory.Count} items");
        response.Inventory.Should().Contain("leaflet");

        // Try "Let's" phrasing
        response = await Do("Let's go south");
        Console.WriteLine($"Let's: Location is now {response.LocationName}");
        response.LocationName.Should().Be("South of House");

        // Try question form "Can I...?" - INTERESTING BEHAVIOR!
        response = await Do("Can I go back north?");
        Console.WriteLine($"Can I: Location is now {response.LocationName}");
        Console.WriteLine($"FINDING: 'Can I go north?' did NOT move - no north exit from South of House!");
        Console.WriteLine($"Available exits: {string.Join(", ", response.Exits)}");
        Console.WriteLine($"Response was: {response.Response}");
        // FINDING: No north exit from South of House (exits are W, E, S)

        // Actually go west to get back
        response = await Do("go west");
        response.LocationName.Should().Be("West Of House");

        // Try "Show me" command
        response = await Do("Show me what's in my inventory");
        Console.WriteLine($"Show me: Response length = {response.Response.Length}");
        Console.WriteLine($"\n=== MAJOR BUG DETECTED ===");
        Console.WriteLine($"API Inventory: {string.Join(", ", response.Inventory)}");
        Console.WriteLine($"AI Response: {response.Response}");
        Console.WriteLine($"BUG: API shows 'leaflet' in inventory, but AI says inventory is EMPTY!");
        Console.WriteLine($"This is a STATE/NARRATIVE mismatch!");
        Console.WriteLine($"=========================\n");

        // The structured data is correct (has leaflet), but narrative is wrong
        response.Inventory.Should().Contain("leaflet"); // This passes
        // response.Response.Should().Contain("leaflet"); // This would fail - narrative is wrong!
    }

    [Test]
    public async Task TestTakingSynonyms()
    {
        await Do("look");

        // Navigate to kitchen
        await Do("south");
        await Do("east");
        await Do("open window");
        var response = await Do("climb through the window");
        response.LocationName.Should().Be("Kitchen");

        // Test various "take" synonyms with natural language
        response = await Do("grab the sack");
        response.Inventory.Should().Contain("sack");
        Console.WriteLine($"'grab' works: {response.Inventory.Contains("sack")}");

        response = await Do("pick up the bottle");
        response.Inventory.Should().Contain("bottle");
        Console.WriteLine($"'pick up' works: {response.Inventory.Contains("bottle")}");

        response = await Do("get the elongated brown sack");  // Try with adjectives
        // Already have it, should get a message

        // Go to living room
        response = await Do("west");

        response = await Do("acquire the brass lamp");  // Formal synonym
        Console.WriteLine($"'acquire' result: Has lamp = {response.Inventory.Contains("lamp")}");

        response = await Do("pocket the sword");  // Creative verb
        Console.WriteLine($"'pocket' result: Has sword = {response.Inventory.Contains("sword")}");

        Console.WriteLine($"\nFinal inventory count: {response.Inventory.Count}");
        Console.WriteLine($"Items: {string.Join(", ", response.Inventory)}");
    }

    [Test]
    public async Task TestInvalidCommands_SeeHowAIResponds()
    {
        var response = await Do("look");

        // Try nonsense
        response = await Do("flibbertigibbet the mailbox");
        Console.WriteLine($"\nNonsense command response: {response.Response}");

        // Try taking something that can't be taken
        response = await Do("take the house");
        Console.WriteLine($"\nTake house response: {response.Response}");
        response.Inventory.Should().NotContain("house");

        // Try going nowhere
        response = await Do("go up into the sky");
        Console.WriteLine($"\nGo to sky response: {response.Response}");
        response.LocationName.Should().Be("West Of House"); // Shouldn't move

        // Try extremely verbose natural language
        response = await Do("I would very much appreciate it if you could be so kind as to open the small mailbox that is located nearby");
        Console.WriteLine($"\nVerbose command response: {response.Response}");

        // Try  command with typos
        response = await Do("opne teh mailbx");
        Console.WriteLine($"\nTypo command response: {response.Response}");
    }
}

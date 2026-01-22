using GameEngine;
using Model.Intent;
using Model.Interface;
using Moq;
using Planetfall;
using Planetfall.Item.Kalamontee;
using Planetfall.Item.Kalamontee.Admin;
using Planetfall.Item.Kalamontee.Mech;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Item.Lawanda.Library.Computer;
using Planetfall.Location.Kalamontee;
using Planetfall.Location.Kalamontee.Admin;
using SmallOffice = Planetfall.Location.Kalamontee.Admin.SmallOffice;
using Planetfall.Location.Kalamontee.Mech;
using Planetfall.Location.Kalamontee.Tower;
using Planetfall.Location.Lawanda;
using ProjectCorridor = Planetfall.Location.Lawanda.ProjectCorridor;
using Planetfall.Location.Shuttle;
using ZorkAI.OpenAI;
using PhysicalPlant = Planetfall.Location.Kalamontee.Mech.PhysicalPlant;

namespace IntegrationTests;

/// <summary>
/// Integration tests to verify Floyd's one-time comments when entering special locations.
/// These tests call the actual OpenAI API to generate Floyd's responses.
/// Run individual tests to see the AI-generated output.
/// </summary>
[TestFixture]
[Explicit("Requires OPEN_AI_KEY environment variable - Integration test")]
[NonParallelizable]
public class FloydLocationCommentTests
{
    [SetUp]
    public void Setup()
    {
        // Load environment variables from .env file if present
        Env.TraversePath().Load();
    }

    /// <summary>
    /// Configures Floyd for testing by setting standard properties and mocking the random chooser
    /// to ensure deterministic behavior (Floyd always follows the player instead of randomly wandering).
    /// </summary>
    private static void ConfigureFloydForTesting(Floyd floyd)
    {
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.TurnOnCountdown = 0; // Skip the activation delay

        // Mock the random chooser to ensure Floyd always follows the player
        // (prevents the 1-in-5 chance of Floyd wandering off instead of following)
        var mockChooser = new Mock<IRandomChooser>();
        mockChooser.Setup(r => r.RollDiceSuccess(It.IsAny<int>())).Returns(false);
        floyd.Chooser = mockChooser.Object;
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
        ConfigureFloydForTesting(floyd);
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
        var previousLocation = Repository.GetLocation<MechCorridorNorth>();
        var physicalPlant = Repository.GetLocation<PhysicalPlant>();
        var floyd = Repository.GetItem<Floyd>();
        ConfigureFloydForTesting(floyd);
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
        var previousLocation = Repository.GetLocation<TowerCore>();
        var deck = Repository.GetLocation<ObservationDeck>();
        var floyd = Repository.GetItem<Floyd>();
        ConfigureFloydForTesting(floyd);
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
        ConfigureFloydForTesting(floyd);
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

    [Test]
    public async Task FloydComment_UpperElevator_ElevatorComment()
    {
        Repository.Reset();

        // Create real generation client (requires OPEN_AI_KEY)
        var generationClient = new ChatGPTClient(null);

        // Set up the context
        var context = new PlanetfallContext();

        // Place Floyd in a PREVIOUS location (he will follow the player)
        var previousLocation = Repository.GetLocation<ElevatorLobby>();
        var elevator = Repository.GetLocation<UpperElevator>();
        var floyd = Repository.GetItem<Floyd>();
        ConfigureFloydForTesting(floyd);
        previousLocation.ItemPlacedHere(floyd);

        // Player moves to the elevator
        context.CurrentLocation = elevator;

        // Floyd follows the player - this triggers the special location comment
        var result = await floyd.Act(context, generationClient);

        Console.WriteLine("=== Floyd's Comment at Upper Elevator ===");
        Console.WriteLine(result);
        Console.WriteLine("==========================================");

        // Verify we got a response (includes "Floyd follows you. " prefix)
        result.Should().NotBeNullOrEmpty("Floyd should have made a comment");

        // Verify the interaction was marked as happened (so it won't repeat)
        elevator.InteractionHasHappened.Should().BeTrue();
    }

    [Test]
    public async Task FloydComment_LowerElevator_ElevatorComment()
    {
        Repository.Reset();

        // Create real generation client (requires OPEN_AI_KEY)
        var generationClient = new ChatGPTClient(null);

        // Set up the context
        var context = new PlanetfallContext();

        // Place Floyd in a PREVIOUS location (he will follow the player)
        var previousLocation = Repository.GetLocation<ElevatorLobby>();
        var elevator = Repository.GetLocation<LowerElevator>();
        var floyd = Repository.GetItem<Floyd>();
        ConfigureFloydForTesting(floyd);
        previousLocation.ItemPlacedHere(floyd);

        // Player moves to the elevator
        context.CurrentLocation = elevator;

        // Floyd follows the player - this triggers the special location comment
        var result = await floyd.Act(context, generationClient);

        Console.WriteLine("=== Floyd's Comment at Lower Elevator ===");
        Console.WriteLine(result);
        Console.WriteLine("==========================================");

        // Verify we got a response (includes "Floyd follows you. " prefix)
        result.Should().NotBeNullOrEmpty("Floyd should have made a comment");

        // Verify the interaction was marked as happened (so it won't repeat)
        elevator.InteractionHasHappened.Should().BeTrue();
    }

    [Test]
    public async Task FloydComment_Helicopter_HeliComment()
    {
        Repository.Reset();

        // Create real generation client (requires OPEN_AI_KEY)
        var generationClient = new ChatGPTClient(null);

        // Set up the context
        var context = new PlanetfallContext();

        // Place Floyd in a PREVIOUS location (he will follow the player)
        var previousLocation = Repository.GetLocation<Helipad>();
        var helicopter = Repository.GetLocation<Helicopter>();
        var floyd = Repository.GetItem<Floyd>();
        ConfigureFloydForTesting(floyd);
        previousLocation.ItemPlacedHere(floyd);

        // Player moves to the helicopter
        context.CurrentLocation = helicopter;

        // Floyd follows the player - this triggers the special location comment
        var result = await floyd.Act(context, generationClient);

        Console.WriteLine("=== Floyd's Comment at Helicopter ===");
        Console.WriteLine(result);
        Console.WriteLine("======================================");

        // Verify we got a response (includes "Floyd follows you. " prefix)
        result.Should().NotBeNullOrEmpty("Floyd should have made a comment");

        // Verify the interaction was marked as happened (so it won't repeat)
        helicopter.InteractionHasHappened.Should().BeTrue();
    }

    [Test]
    public async Task FloydComment_LargeOffice_ViewFromWindow()
    {
        Repository.Reset();

        // Create real generation client (requires OPEN_AI_KEY)
        var generationClient = new ChatGPTClient(null);

        // Set up the context
        var context = new PlanetfallContext();

        // Place Floyd in a PREVIOUS location (he will follow the player)
        var previousLocation = Repository.GetLocation<SmallOffice>();
        var largeOffice = Repository.GetLocation<LargeOffice>();
        var floyd = Repository.GetItem<Floyd>();
        ConfigureFloydForTesting(floyd);
        previousLocation.ItemPlacedHere(floyd);

        // Player moves to the large office
        context.CurrentLocation = largeOffice;

        // Floyd follows the player - this triggers the special location comment
        var result = await floyd.Act(context, generationClient);

        Console.WriteLine("=== Floyd's Comment at Large Office ===");
        Console.WriteLine(result);
        Console.WriteLine("========================================");

        // Verify we got a response (includes "Floyd follows you. " prefix)
        result.Should().NotBeNullOrEmpty("Floyd should have made a comment");

        // Verify the interaction was marked as happened (so it won't repeat)
        largeOffice.InteractionHasHappened.Should().BeTrue();
    }

    [Test]
    public async Task FloydComment_ProjConOffice_GarishMural()
    {
        Repository.Reset();

        // Create real generation client (requires OPEN_AI_KEY)
        var generationClient = new ChatGPTClient(null);

        // Set up the context
        var context = new PlanetfallContext();

        // Place Floyd in a PREVIOUS location (he will follow the player)
        var previousLocation = Repository.GetLocation<ProjectCorridor>();
        var projConOffice = Repository.GetLocation<ProjConOffice>();
        var floyd = Repository.GetItem<Floyd>();
        ConfigureFloydForTesting(floyd);
        previousLocation.ItemPlacedHere(floyd);

        // Player moves to the ProjCon Office
        context.CurrentLocation = projConOffice;

        // Floyd follows the player - this triggers the special location comment
        var result = await floyd.Act(context, generationClient);

        Console.WriteLine("=== Floyd's Comment at ProjCon Office ===");
        Console.WriteLine(result);
        Console.WriteLine("==========================================");

        // Verify we got a response (includes "Floyd follows you. " prefix)
        result.Should().NotBeNullOrEmpty("Floyd should have made a comment");

        // Verify the interaction was marked as happened (so it won't repeat)
        projConOffice.InteractionHasHappened.Should().BeTrue();
    }

    [Test]
    public async Task FloydComment_PadlockUnlocked_ExcitedAboutDoor()
    {
        Repository.Reset();

        var generationClient = new ChatGPTClient(null);
        var context = new PlanetfallContext();

        // Set up location - MessCorridor has the padlock
        var location = Repository.GetLocation<MessCorridor>();
        context.CurrentLocation = location;

        // Place Floyd in the same location
        var floyd = Repository.GetItem<Floyd>();
        ConfigureFloydForTesting(floyd);
        location.ItemPlacedHere(floyd);

        // Give the player the key
        var key = Repository.GetItem<Key>();
        context.ItemPlacedHere(key);

        // Unlock the padlock with the key - this triggers the comment
        var padlock = Repository.GetItem<Padlock>();
        var action = new MultiNounIntent
        {
            Verb = "unlock",
            NounOne = "padlock",
            Preposition = "with",
            NounTwo = "key",
            OriginalInput = "unlock padlock with key"
        };
        await padlock.RespondToMultiNounInteraction(action, context);

        // Floyd's Act() should now generate the pending comment
        var result = await floyd.Act(context, generationClient);

        Console.WriteLine("=== Floyd's Comment when Padlock Unlocked ===");
        Console.WriteLine(result);
        Console.WriteLine("==============================================");

        result.Should().NotBeNullOrEmpty("Floyd should have commented on the padlock being unlocked");
    }

    [Test]
    public async Task FloydComment_MagnetRetrievesKey_ImpressedByPlayer()
    {
        Repository.Reset();

        var generationClient = new ChatGPTClient(null);
        var context = new PlanetfallContext();

        // Set up location - AdminCorridorSouth has the key in the crevice
        var location = Repository.GetLocation<AdminCorridorSouth>();
        context.CurrentLocation = location;

        // Place Floyd in the same location
        var floyd = Repository.GetItem<Floyd>();
        ConfigureFloydForTesting(floyd);
        location.ItemPlacedHere(floyd);

        // Give the player the magnet
        var magnet = Repository.GetItem<Magnet>();
        context.ItemPlacedHere(magnet);

        // Use the magnet on the crevice - this triggers the comment
        var action = new MultiNounIntent
        {
            Verb = "put",
            NounOne = "magnet",
            Preposition = "over",
            NounTwo = "crevice",
            OriginalInput = "put magnet over crevice"
        };
        await location.RespondToMultiNounInteraction(action, context);

        // Floyd's Act() should now generate the pending comment
        var result = await floyd.Act(context, generationClient);

        Console.WriteLine("=== Floyd's Comment when Magnet Retrieves Key ===");
        Console.WriteLine(result);
        Console.WriteLine("=================================================");

        result.Should().NotBeNullOrEmpty("Floyd should have commented on the clever magnet use");
    }

    [Test]
    public async Task FloydComment_LaserPickedUp_NervousAboutWeapon()
    {
        Repository.Reset();

        var generationClient = new ChatGPTClient(null);
        var context = new PlanetfallContext();

        // Set up location - ToolRoom has the laser
        var location = Repository.GetLocation<ToolRoom>();
        context.CurrentLocation = location;

        // Place Floyd in the same location
        var floyd = Repository.GetItem<Floyd>();
        ConfigureFloydForTesting(floyd);
        location.ItemPlacedHere(floyd);

        // Pick up the laser - this triggers the comment
        var laser = Repository.GetItem<Laser>();
        laser.OnBeingTaken(context, location);
        context.ItemPlacedHere(laser);

        // Floyd's Act() should now generate the pending comment
        var result = await floyd.Act(context, generationClient);

        Console.WriteLine("=== Floyd's Comment when Laser Picked Up ===");
        Console.WriteLine(result);
        Console.WriteLine("=============================================");

        result.Should().NotBeNullOrEmpty("Floyd should have commented nervously about the laser");
    }

    [Test]
    public async Task FloydComment_LibraryComputerFirstUse_CuriousAboutComputer()
    {
        Repository.Reset();

        var generationClient = new ChatGPTClient(null);
        var context = new PlanetfallContext();

        // Set up location - Library has the computer terminal
        var location = Repository.GetLocation<Library>();
        context.CurrentLocation = location;

        // Place Floyd in the same location
        var floyd = Repository.GetItem<Floyd>();
        ConfigureFloydForTesting(floyd);
        location.ItemPlacedHere(floyd);

        // Type on the computer - this triggers the comment
        var terminal = Repository.GetItem<ComputerTerminal>();
        terminal.IsOn = true;
        var action = new SimpleIntent { Verb = "type", Noun = "1", OriginalInput = "type 1" };
        await terminal.RespondToSimpleInteraction(action, context, generationClient, null!);

        // Floyd's Act() should now generate the pending comment
        var result = await floyd.Act(context, generationClient);

        Console.WriteLine("=== Floyd's Comment when Using Library Computer ===");
        Console.WriteLine(result);
        Console.WriteLine("===================================================");

        result.Should().NotBeNullOrEmpty("Floyd should have commented on the computer use");
    }

    [Test]
    public async Task FloydComment_ShuttleControlsFirstUse_ExcitedAboutRide()
    {
        Repository.Reset();

        var generationClient = new ChatGPTClient(null);
        var context = new PlanetfallContext();

        // Set up location - Alfie control cabin
        var location = Repository.GetLocation<AlfieControlWest>();
        context.CurrentLocation = location;

        // Place Floyd in the same location
        var floyd = Repository.GetItem<Floyd>();
        ConfigureFloydForTesting(floyd);
        location.ItemPlacedHere(floyd);

        // Activate the shuttle first
        var shuttleControl = location as IShuttleControl;
        shuttleControl.Activate(context);

        // Push the lever - this triggers the comment
        var action = new SimpleIntent { Verb = "push", Noun = "lever", OriginalInput = "push lever" };
        await location.RespondToSimpleInteraction(action, context, generationClient, null!);

        // Floyd's Act() should now generate the pending comment
        var result = await floyd.Act(context, generationClient);

        Console.WriteLine("=== Floyd's Comment when Using Shuttle Controls ===");
        Console.WriteLine(result);
        Console.WriteLine("===================================================");

        result.Should().NotBeNullOrEmpty("Floyd should have commented excitedly on the shuttle controls");
    }

    [Test]
    public async Task FloydComment_ConferenceRoomDoorOpened_AmazedByCode()
    {
        Repository.Reset();

        var generationClient = new ChatGPTClient(null);
        var context = new PlanetfallContext();

        // Set up location - RecArea has the conference room door
        var location = Repository.GetLocation<RecArea>();
        context.CurrentLocation = location;

        // Place Floyd in the same location
        var floyd = Repository.GetItem<Floyd>();
        ConfigureFloydForTesting(floyd);
        location.ItemPlacedHere(floyd);

        // Get the door and set a known unlock code
        var door = Repository.GetItem<ConferenceRoomDoor>();
        door.UnlockCode = "123";

        // Turn the dial to the correct code - this triggers the comment
        var action = new MultiNounIntent
        {
            Verb = "set",
            NounOne = "dial",
            Preposition = "to",
            NounTwo = "123",
            OriginalInput = "set dial to 123"
        };
        await door.RespondToMultiNounInteraction(action, context);

        // Floyd's Act() should now generate the pending comment
        var result = await floyd.Act(context, generationClient);

        Console.WriteLine("=== Floyd's Comment when Conference Room Door Opens ===");
        Console.WriteLine(result);
        Console.WriteLine("=======================================================");

        result.Should().NotBeNullOrEmpty("Floyd should have commented on cracking the code");
    }
}
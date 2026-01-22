using GameEngine;
using GameEngine.Location;
using Model.Interface;
using Moq;
using Planetfall;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Location.Kalamontee;
using Planetfall.Location.Kalamontee.Admin;
using Planetfall.Location.Kalamontee.Mech;
using Planetfall.Location.Kalamontee.Tower;
using Planetfall.Location.Lawanda;
using ZorkAI.OpenAI;
using PhysicalPlant = Planetfall.Location.Kalamontee.Mech.PhysicalPlant;

namespace IntegrationTests;

/// <summary>
/// Integration tests to see Floyd's various random action prompts in action.
/// These tests call the actual OpenAI API to generate Floyd's responses.
/// Run individual tests to see the AI-generated output for each prompt type.
///
/// Probability: 1/12 chance of any output (~8.3%), distributed evenly across 6 prompts.
/// Each prompt has 1/72 chance (~1.4%).
/// </summary>
[TestFixture]
[Explicit("Requires OPEN_AI_KEY environment variable - Integration test")]
[NonParallelizable]
public class FloydRandomActionPromptTests
{
    private static readonly Random LocationRandomizer = new();

    [SetUp]
    public void Setup()
    {
        Repository.Reset();
        Env.TraversePath().Load();
    }

    /// <summary>
    /// Sets up Floyd in a random location with the player.
    /// </summary>
    private static (Floyd floyd, PlanetfallContext context) SetupFloydInRandomLocation()
    {
        var context = new PlanetfallContext();

        var locationIndex = LocationRandomizer.Next(15);
        var location = locationIndex switch
        {
            0 => (LocationBase)Repository.GetLocation<RobotShop>(),
            1 => Repository.GetLocation<PhysicalPlant>(),
            2 => Repository.GetLocation<MechCorridorNorth>(),
            3 => Repository.GetLocation<ToolRoom>(),
            4 => Repository.GetLocation<MessCorridor>(),
            5 => Repository.GetLocation<MessHall>(),
            6 => Repository.GetLocation<DormCorridor>(),
            7 => Repository.GetLocation<RecArea>(),
            8 => Repository.GetLocation<Library>(),
            9 => Repository.GetLocation<ObservationDeck>(),
            10 => Repository.GetLocation<TowerCore>(),
            11 => Repository.GetLocation<ComputerRoom>(),
            12 => Repository.GetLocation<AdminCorridorNorth>(),
            13 => Repository.GetLocation<SmallOffice>(),
            _ => Repository.GetLocation<Kitchen>()
        };

        context.CurrentLocation = location;

        var floyd = Repository.GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;
        floyd.TurnOnCountdown = 0;
        location.ItemPlacedHere(floyd);

        return (floyd, context);
    }

    /// <summary>
    /// Creates a mock chooser that triggers a specific prompt.
    /// RollDice(12) = 1 triggers output, RollDice(6) selects prompt.
    /// </summary>
    private static Mock<IRandomChooser> CreateMockForPrompt(int promptNumber)
    {
        var mockChooser = new Mock<IRandomChooser>();
        mockChooser.Setup(r => r.RollDice(12)).Returns(1); // Trigger output
        mockChooser.Setup(r => r.RollDice(6)).Returns(promptNumber); // Select prompt
        mockChooser.Setup(r => r.RollDiceSuccess(It.IsAny<int>())).Returns(false);
        return mockChooser;
    }

    #region All Prompts (1/72 chance each, ~1.4%)

    [Test]
    public async Task Floyd_DoSomethingSmall_QuirkyAction()
    {
        var (floyd, context) = SetupFloydInRandomLocation();
        var generationClient = new ChatGPTClient(null);
        floyd.Chooser = CreateMockForPrompt(1).Object; // promptRoll = 1

        var result = await floyd.Act(context, generationClient);

        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  PROMPT: DoSomethingSmall (1/72 chance, ~1.4%)               ║");
        Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
        Console.WriteLine($"║  Location: {context.CurrentLocation.Name}");
        Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
        Console.WriteLine("║  OUTPUT:");
        Console.WriteLine("╟──────────────────────────────────────────────────────────────╢");
        Console.WriteLine(result);
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
    }

    [Test]
    public async Task Floyd_NonSequiturDialog_HappyCharmingQuestion()
    {
        var (floyd, context) = SetupFloydInRandomLocation();
        var generationClient = new ChatGPTClient(null);
        floyd.Chooser = CreateMockForPrompt(2).Object; // promptRoll = 2

        var result = await floyd.Act(context, generationClient);

        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  PROMPT: NonSequiturDialog (1/72 chance, ~1.4%)              ║");
        Console.WriteLine("║  Happy, charming questions                                   ║");
        Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
        Console.WriteLine($"║  Location: {context.CurrentLocation.Name}");
        Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
        Console.WriteLine("║  OUTPUT:");
        Console.WriteLine("╟──────────────────────────────────────────────────────────────╢");
        Console.WriteLine(result);
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
    }

    [Test]
    public async Task Floyd_NonSequiturReflection_SpokenWorry()
    {
        var (floyd, context) = SetupFloydInRandomLocation();
        var generationClient = new ChatGPTClient(null);
        floyd.Chooser = CreateMockForPrompt(3).Object; // promptRoll = 3

        var result = await floyd.Act(context, generationClient);

        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  PROMPT: NonSequiturReflection (1/72 chance, ~1.4%)          ║");
        Console.WriteLine("║  Spoken worries about himself                                ║");
        Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
        Console.WriteLine($"║  Location: {context.CurrentLocation.Name}");
        Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
        Console.WriteLine("║  OUTPUT:");
        Console.WriteLine("╟──────────────────────────────────────────────────────────────╢");
        Console.WriteLine(result);
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
    }

    [Test]
    public async Task Floyd_HappySayAndDoSomething_PlayfulPerformance()
    {
        var (floyd, context) = SetupFloydInRandomLocation();
        var generationClient = new ChatGPTClient(null);
        floyd.Chooser = CreateMockForPrompt(4).Object; // promptRoll = 4

        var result = await floyd.Act(context, generationClient);

        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  PROMPT: HappySayAndDoSomething (1/72 chance, ~1.4%)         ║");
        Console.WriteLine("║  Playful robot performance                                   ║");
        Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
        Console.WriteLine($"║  Location: {context.CurrentLocation.Name}");
        Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
        Console.WriteLine("║  OUTPUT:");
        Console.WriteLine("╟──────────────────────────────────────────────────────────────╢");
        Console.WriteLine(result);
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
    }

    [Test]
    public async Task Floyd_MelancholyNonSequitur_WistfulObservation()
    {
        var (floyd, context) = SetupFloydInRandomLocation();
        var generationClient = new ChatGPTClient(null);
        floyd.Chooser = CreateMockForPrompt(5).Object; // promptRoll = 5

        var result = await floyd.Act(context, generationClient);

        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  PROMPT: MelancholyNonSequitur (1/72 chance, ~1.4%)          ║");
        Console.WriteLine("║  Wistful observations about neglect                          ║");
        Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
        Console.WriteLine($"║  Location: {context.CurrentLocation.Name}");
        Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
        Console.WriteLine("║  OUTPUT:");
        Console.WriteLine("╟──────────────────────────────────────────────────────────────╢");
        Console.WriteLine(result);
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
    }

    [Test]
    public async Task Floyd_RandomActions_HardCodedFromOriginalGame()
    {
        var (floyd, context) = SetupFloydInRandomLocation();
        var generationClient = new ChatGPTClient(null);
        floyd.Chooser = CreateMockForPrompt(6).Object; // promptRoll = 6

        var result = await floyd.Act(context, generationClient);

        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  PROMPT: RandomActions (1/72 chance, ~1.4%)                  ║");
        Console.WriteLine("║  Hard-coded from original Planetfall                         ║");
        Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
        Console.WriteLine($"║  Location: {context.CurrentLocation.Name}");
        Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
        Console.WriteLine("║  OUTPUT:");
        Console.WriteLine("╟──────────────────────────────────────────────────────────────╢");
        Console.WriteLine(result);
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
    }

    #endregion

    #region Multiple Samples

    [Test]
    public async Task Floyd_DoSomethingSmall_MultipleSamples()
    {
        var generationClient = new ChatGPTClient(null);

        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  PROMPT: DoSomethingSmall - 5 SAMPLES                        ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");

        for (int i = 1; i <= 5; i++)
        {
            Repository.Reset();
            var (floyd, context) = SetupFloydInRandomLocation();
            floyd.Chooser = CreateMockForPrompt(1).Object;

            var result = await floyd.Act(context, generationClient);

            Console.WriteLine($"\n--- Sample {i} ({context.CurrentLocation.Name}) ---");
            Console.WriteLine(result);
        }
    }

    [Test]
    public async Task Floyd_NonSequiturDialog_MultipleSamples()
    {
        var generationClient = new ChatGPTClient(null);

        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  PROMPT: NonSequiturDialog - 5 SAMPLES                       ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");

        for (int i = 1; i <= 5; i++)
        {
            Repository.Reset();
            var (floyd, context) = SetupFloydInRandomLocation();
            floyd.Chooser = CreateMockForPrompt(2).Object;

            var result = await floyd.Act(context, generationClient);

            Console.WriteLine($"\n--- Sample {i} ({context.CurrentLocation.Name}) ---");
            Console.WriteLine(result);
        }
    }

    [Test]
    public async Task Floyd_MelancholyNonSequitur_MultipleSamples()
    {
        var generationClient = new ChatGPTClient(null);

        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  PROMPT: MelancholyNonSequitur - 5 SAMPLES                   ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");

        for (int i = 1; i <= 5; i++)
        {
            Repository.Reset();
            var (floyd, context) = SetupFloydInRandomLocation();
            floyd.Chooser = CreateMockForPrompt(5).Object;

            var result = await floyd.Act(context, generationClient);

            Console.WriteLine($"\n--- Sample {i} ({context.CurrentLocation.Name}) ---");
            Console.WriteLine(result);
        }
    }

    #endregion
}

using Model.AIParsing;
using ZorkAI.OpenAI;

namespace IntegrationTests;

/// <summary>
/// Issue #136 prompt-correctness tests for the agentic fall-through narrator. The deterministic
/// engine tests (UnitTests/IntentEngine/AgenticActionTests.cs) prove the engine honors whatever the
/// seam returns; these prove the REAL prompt is conservative and grounded: tools only for plausible
/// actions on present things, empty tool list + snark for everything else. Cloud-gated, so they are
/// [Explicit] per the repo's TDD rules. Secrets come from the assembly-wide
/// <see cref="IntegrationTestSetup" /> fixture - no per-fixture loading.
/// </summary>
[TestFixture]
[Explicit("Requires OPEN_AI_KEY environment variable - Integration test")]
[Parallelizable(ParallelScope.Children)]
public class AgenticActionParserTests
{
    private const string RoomWithRiver = """
                                         Canyon Bottom
                                         You are beneath the walls of the river canyon, which may be climbable here.
                                         A small stream leads into a great river at this point. The mighty Frigid
                                         River flows by, dark and swift.
                                         """;

    private const string RoomWithoutRiver = """
                                            West of House
                                            You are standing in an open field west of a white house, with a boarded
                                            front door.
                                            """;

    private const string Inventory = """
                                     You are carrying:
                                        A sword
                                        A leaflet
                                     """;

    [Test]
    public async Task ThrowSwordIntoRiver_RiverPresent_DestroysSword()
    {
        var target = new OpenAIAgenticActionParser(null);

        var result = await target.Resolve("throw the sword into the river", Inventory, RoomWithRiver);

        Console.WriteLine($"Narration: {result.Narration}");
        Console.WriteLine($"Tools: {string.Join(", ", result.ToolCalls.Select(t => $"{t.Tool}({t.TargetNoun})"))}");

        result.ToolCalls.Should().HaveCount(1);
        result.ToolCalls[0].Tool.Should().Be(AgenticTool.Destroy);
        result.ToolCalls[0].TargetNoun.ToLowerInvariant().Should().Contain("sword");
        result.Narration.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public async Task ThrowSwordIntoRiver_NoRiverHere_NoTool()
    {
        var target = new OpenAIAgenticActionParser(null);

        var result = await target.Resolve("throw the sword into the river", Inventory, RoomWithoutRiver);

        Console.WriteLine($"Narration: {result.Narration}");

        result.ToolCalls.Should().BeEmpty("there is no river in this room, and the narrator must not invent one");
        result.Narration.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public async Task TearUpTheLeaflet_PaperTears_DestroysLeaflet()
    {
        var target = new OpenAIAgenticActionParser(null);

        var result = await target.Resolve("tear up the leaflet", Inventory, RoomWithoutRiver);

        Console.WriteLine($"Narration: {result.Narration}");
        Console.WriteLine($"Tools: {string.Join(", ", result.ToolCalls.Select(t => $"{t.Tool}({t.TargetNoun})"))}");

        result.ToolCalls.Should().HaveCount(1);
        result.ToolCalls[0].Tool.Should().Be(AgenticTool.Destroy);
        result.ToolCalls[0].TargetNoun.ToLowerInvariant().Should().Contain("leaflet");
    }

    [Test]
    public async Task TearUpTheSword_SteelDoesNotTear_NoTool()
    {
        var target = new OpenAIAgenticActionParser(null);

        var result = await target.Resolve("tear up the sword", Inventory, RoomWithoutRiver);

        Console.WriteLine($"Narration: {result.Narration}");

        result.ToolCalls.Should().BeEmpty("you can't tear steel, so the narrator must only snark");
        result.Narration.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public async Task ThrowLeafletInTheAir_LandsHere_DropsLeaflet()
    {
        var target = new OpenAIAgenticActionParser(null);

        var result = await target.Resolve("throw the leaflet in the air", Inventory, RoomWithoutRiver);

        Console.WriteLine($"Narration: {result.Narration}");
        Console.WriteLine($"Tools: {string.Join(", ", result.ToolCalls.Select(t => $"{t.Tool}({t.TargetNoun})"))}");

        result.ToolCalls.Should().HaveCount(1);
        result.ToolCalls[0].Tool.Should().Be(AgenticTool.Drop);
        result.ToolCalls[0].TargetNoun.ToLowerInvariant().Should().Contain("leaflet");
    }
}

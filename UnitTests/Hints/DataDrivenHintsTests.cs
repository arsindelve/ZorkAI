using FluentAssertions;
using GameEngine.Hints;
using GameEngine.Hints.Data;
using Model.Interface;
using Model.Location;
using Moq;

namespace UnitTests.Hints;

/// <summary>The data-driven puzzle graph and corpus (a generated corpus loaded as JSON), over a fake flattened state.</summary>
[TestFixture]
public class DataDrivenHintsTests
{
    private static HintData Corpus() => new()
    {
        Game = "Test",
        Nodes =
        {
            new HintNodeData
            {
                Id = "MAGNET", Title = "Take the magnet", Location = "Tool Room", Aliases = { "magnet" },
                Done = { new StatePredicate { Path = "Item:Magnet.HasEverBeenPickedUp", Op = "eq", Value = "True" } },
                Rungs = { "A", "B", "Tool Room: take magnet" }
            },
            new HintNodeData
            {
                Id = "KEY", Title = "Fish out the key", Location = "Admin Corridor South", Prerequisites = { "MAGNET" }, Aliases = { "crevice", "key" },
                Done = { new StatePredicate { Path = "Location:AdminCorridorSouth.HasTakenTheKey", Op = "eq", Value = "True" } },
                Rungs = { "A", "B", "C" }
            },
            new HintNodeData
            {
                Id = "FLASK", Title = "Fill the flask", Location = "Machine Shop", Prerequisites = { "KEY" }, Aliases = { "flask" },
                Done = { new StatePredicate { Path = "Item:Flask.LiquidColor", Op = "eq", Value = "gray" } },
                Rungs = { "A", "B", "C" }
            },
            new HintNodeData
            {
                Id = "POUR", Title = "Pour the fluid", Location = "Comm Room", Prerequisites = { "FLASK" }, Optional = true,
                Done = { new StatePredicate { Path = "Location:SystemsMonitors.Fixed", Op = "contains", Value = "KUMUUNIKAASHUNZ" } },
                Rungs = { "A", "B", "C" }
            },
            new HintNodeData
            {
                Id = "RIDE", Title = "Ride the elevator", Location = "Elevator Lobby", Prerequisites = { "KEY" },
                Done = { new StatePredicate { Path = "Location:TowerCore.VisitCount", Op = "gte", Value = "1" } },
                Rungs = { "A", "B", "C" }
            }
        }
    };

    private static IContext At(string room)
    {
        var location = new Mock<ILocation>();
        location.SetupGet(l => l.Name).Returns(room);
        var ctx = new Mock<IContext>();
        ctx.SetupGet(c => c.CurrentLocation).Returns(location.Object);
        return ctx.Object;
    }

    private static DataPuzzleGraph Graph(HintData data, params (string Key, string Value)[] state) =>
        new(data, _ => state.ToDictionary(s => s.Key, s => s.Value, StringComparer.Ordinal));

    [Test]
    public void Predicates_EqGteContains()
    {
        var flat = new Dictionary<string, string>
        {
            ["a"] = "True", ["n"] = "3", ["l"] = "list:BAR|FOO"
        };
        new StatePredicate { Path = "a", Op = "eq", Value = "True" }.Holds(flat).Should().BeTrue();
        new StatePredicate { Path = "a", Op = "eq", Value = "False" }.Holds(flat).Should().BeFalse();
        new StatePredicate { Path = "n", Op = "gte", Value = "3" }.Holds(flat).Should().BeTrue();
        new StatePredicate { Path = "n", Op = "gte", Value = "4" }.Holds(flat).Should().BeFalse();
        new StatePredicate { Path = "l", Op = "contains", Value = "FOO" }.Holds(flat).Should().BeTrue();
        new StatePredicate { Path = "l", Op = "contains", Value = "BAZ" }.Holds(flat).Should().BeFalse();
        new StatePredicate { Path = "missing", Op = "eq", Value = "True" }.Holds(flat).Should().BeFalse();
    }

    [Test]
    public void Map_NothingDone_FirstNodeAvailable_RestLocked()
    {
        var state = Graph(Corpus()).Map(At("Tool Room"));
        state.StatusOf("MAGNET").Should().Be(NodeStatus.Available);
        state.StatusOf("KEY").Should().Be(NodeStatus.Locked);
        state.StatusOf("FLASK").Should().Be(NodeStatus.Locked);
    }

    [Test]
    public void Map_DoneNode_UnlocksDependents()
    {
        var state = Graph(Corpus(), ("Item:Magnet.HasEverBeenPickedUp", "True")).Map(At("Tool Room"));
        state.StatusOf("MAGNET").Should().Be(NodeStatus.Done);
        state.StatusOf("KEY").Should().Be(NodeStatus.Available);
        state.StatusOf("FLASK").Should().Be(NodeStatus.Locked);
    }

    [Test]
    public void Map_BackFillsPrerequisitesOfADoneNode()
    {
        // The flask has been emptied again (its signal no longer holds), but the comms are fixed: the
        // walkthrough proves the flask was filled first, so it is done, and so is everything before it.
        var state = Graph(Corpus(), ("Location:SystemsMonitors.Fixed", "list:KUMUUNIKAASHUNZ")).Map(At("Comm Room"));
        state.StatusOf("POUR").Should().Be(NodeStatus.Done);
        state.StatusOf("FLASK").Should().Be(NodeStatus.Done);
        state.StatusOf("KEY").Should().Be(NodeStatus.Done);
        state.StatusOf("MAGNET").Should().Be(NodeStatus.Done);
        state.StatusOf("RIDE").Should().Be(NodeStatus.Available);
    }

    [Test]
    public void ActiveBlockers_PrefersTheRoomThePlayerIsIn_ThenMandatory()
    {
        var graph = Graph(Corpus(), ("Item:Magnet.HasEverBeenPickedUp", "True"),
            ("Location:AdminCorridorSouth.HasTakenTheKey", "True"), ("Item:Flask.LiquidColor", "gray"));
        var state = graph.Map(At("Comm Room"));

        state.StatusOf("POUR").Should().Be(NodeStatus.Available);
        state.StatusOf("RIDE").Should().Be(NodeStatus.Available);
        graph.ActiveBlockers(state, At("Comm Room")).Should().Equal("POUR", "RIDE");
        graph.ActiveBlockers(state, At("Elevator Lobby")).Should().Equal("RIDE", "POUR");
        graph.ActiveBlockers(state, At("Nowhere")).Should().Equal("RIDE", "POUR"); // mandatory before optional
    }

    [Test]
    public void Nodes_CarryTitlesLocationsAndAliases_ForTheRouter()
    {
        var node = Graph(Corpus()).Nodes.Single(n => n.Id == "KEY");
        node.Title.Should().Be("Fish out the key");
        node.Location.Should().Be("Admin Corridor South");
        node.Aliases.Should().Equal("crevice", "key");
        node.Prerequisites.Should().Equal("MAGNET");
    }

    [Test]
    public void Corpus_ServesTheLadders()
    {
        var corpus = new DataHintCorpus(Corpus());
        corpus.TryGetLadder("MAGNET", out var ladder).Should().BeTrue();
        ladder.Rungs.Should().Equal("A", "B", "Tool Room: take magnet");
        corpus.TryGetLadder("NOPE", out _).Should().BeFalse();
    }

    [Test]
    public void Json_RoundTrips()
    {
        var json = Corpus().ToJson();
        var back = HintData.FromJson(json);
        back.Game.Should().Be("Test");
        back.Nodes.Should().HaveCount(5);
        back.Nodes[3].Optional.Should().BeTrue();
        back.Nodes[3].Done.Single().ToString().Should().Be("Location:SystemsMonitors.Fixed contains KUMUUNIKAASHUNZ");
    }
}

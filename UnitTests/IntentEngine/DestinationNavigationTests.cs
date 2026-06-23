using GameEngine.IntentEngine;
using GameEngine.Location;
using Model.Interface;
using Model.Location;
using Model.Movement;

namespace UnitTests.IntentEngine;

/// <summary>
/// Focused unit tests for the issue #268 destination-navigation disambiguation builder. Uses a minimal
/// real <see cref="LocationBase"/> so we can control names/synonyms without standing up a game world.
/// (Full resolver + engine behaviour is covered against real rooms in ZorkOne.Tests / Planetfall.Tests.)
/// </summary>
[TestFixture]
public class DestinationNavigationTests
{
    private sealed class FakeRoom(string name, string[] nouns) : LocationBase
    {
        public override string Name => name;
        public override string[] NounsForMatching => nouns;
        public override void Init() { }
        protected override Dictionary<Direction, MovementParameters> Map(IContext context) => new();
        protected override string GetContextBasedDescription(IContext context) => string.Empty;
    }

    // A room whose exits we control, so we can place specific neighbours around it for resolver tests.
    private sealed class HubRoom(Dictionary<Direction, MovementParameters> exits) : LocationBase
    {
        public override string Name => "Hub";
        public override void Init() { }
        protected override Dictionary<Direction, MovementParameters> Map(IContext context) => exits;
        protected override string GetContextBasedDescription(IContext context) => string.Empty;
    }

    [Test]
    public void BuildDisambiguation_TreatsIntraRoomDuplicateSynonymAsDistinguishing_NotShared()
    {
        // Regression for the share/dedupe trap: a synonym a SINGLE room lists twice ("dome" and "the
        // dome" both normalize to "dome") must not be mistaken for a noun SHARED across the matched
        // rooms and stripped from that room's reply keys. Only a noun owned by 2+ distinct rooms is
        // "shared". Before the fix, "dome" was counted twice from one room, marked shared, and dropped.
        var dome = new FakeRoom("Dome Room", ["dome room", "dome", "the dome"]);
        var control = new FakeRoom("Control Room", ["control room", "control"]);

        var matches = new List<(Direction, ILocation)> { (Direction.N, dome), (Direction.S, control) };

        var result = DestinationNavigation.BuildDisambiguation(matches);

        // "dome" belongs only to the Dome Room, so it must remain a usable reply key that resolves there.
        result.PossibleResponses.Should().ContainKey("dome");
        result.PossibleResponses["dome"].Should().Be("dome room");
    }

    [Test]
    public void BuildDisambiguation_ExcludesTrulySharedSynonym_FromReplyKeys()
    {
        // Guard the other side: a term genuinely owned by BOTH rooms ("elevator") must NOT be a reply
        // key — answering it would be ambiguous all over again — while the distinguishing words remain.
        var upper = new FakeRoom("Upper Elevator", ["upper elevator", "elevator", "upper"]);
        var lower = new FakeRoom("Lower Elevator", ["lower elevator", "elevator", "lower"]);

        var matches = new List<(Direction, ILocation)> { (Direction.N, upper), (Direction.S, lower) };

        var result = DestinationNavigation.BuildDisambiguation(matches);

        result.PossibleResponses.Should().NotContainKey("elevator");
        result.PossibleResponses.Should().ContainKey("upper");
        result.PossibleResponses.Should().ContainKey("lower");
    }

    [Test]
    public void BuildDisambiguation_DoesNotEmitSingleCharacterReplyKeys()
    {
        // Rooms whose titles differ by a single character ("Dorm C" vs "Dorm D") would otherwise yield
        // one-letter reply keys "c"/"d". The DisambiguationProcessor matches replies by SUBSTRING, so
        // "the second one" (contains 'c') would silently route to Dorm C. Single-character keys are too
        // collision-prone to be safe; only the full names remain as keys.
        var dormC = new FakeRoom("Dorm C", []);
        var dormD = new FakeRoom("Dorm D", []);

        var matches = new List<(Direction, ILocation)> { (Direction.S, dormC), (Direction.N, dormD) };

        var result = DestinationNavigation.BuildDisambiguation(matches);

        result.PossibleResponses.Should().NotContainKey("c");
        result.PossibleResponses.Should().NotContainKey("d");
        // The unambiguous full names are still usable answers.
        result.PossibleResponses.Should().ContainKey("dorm c");
        result.PossibleResponses.Should().ContainKey("dorm d");
    }

    [Test]
    public void ResolveAllAdjacent_RefusesIndistinguishableRepeatedRooms()
    {
        // The maze/forest/coal-mine trap: when 2+ ADJACENT rooms share the SAME name (the 15 identical
        // "Maze" rooms, the four "Forest" rooms, the two "Cave"s), the player cannot pick one by name
        // and a "the Maze or the Maze?" prompt is nonsense that loops. Such repeated rooms must not be
        // navigable by name at all — resolution returns nothing.
        var mazeA = new FakeRoom("Maze", []);
        var mazeB = new FakeRoom("Maze", []);
        var here = new HubRoom(new Dictionary<Direction, MovementParameters>
        {
            { Direction.N, new MovementParameters { Location = mazeA } },
            { Direction.S, new MovementParameters { Location = mazeB } }
        });
        var context = Mock.Of<IContext>(c => c.CurrentLocation == here);

        DestinationNavigation.ResolveAllAdjacent("maze", context).Should().BeEmpty();
    }

    [Test]
    public void ResolveAllAdjacent_KeepsDistinctlyNamedAdjacentRooms()
    {
        // Guard against over-reach: rooms with DISTINCT names (Upper/Lower Elevator, Dorm C/Dorm D)
        // are genuinely distinguishable and must still disambiguate normally.
        var upper = new FakeRoom("Upper Elevator", []);
        var lower = new FakeRoom("Lower Elevator", []);
        var here = new HubRoom(new Dictionary<Direction, MovementParameters>
        {
            { Direction.N, new MovementParameters { Location = upper } },
            { Direction.S, new MovementParameters { Location = lower } }
        });
        var context = Mock.Of<IContext>(c => c.CurrentLocation == here);

        DestinationNavigation.ResolveAllAdjacent("elevator", context).Should().HaveCount(2);
    }
}

using FluentAssertions;
using GameEngine.Hints.Oracle;
using Model.Hints;
using Model.Interface;
using Moq;

namespace UnitTests.Hints;

[TestFixture]
public class HintOracleServiceTests
{
    private sealed class Provider : IOracleProvider
    {
        public HintPersona Persona { get; } = new("voice", "Testfall");
        public string GameKnowledge => "THE BIBLE";
        public string DescribeSituation(IContext state) => "Location: the rift";
    }

    private sealed class Oracle(string? reply) : IHintOracle
    {
        public string? Knowledge, Situation, Question;
        public IReadOnlyList<HintExchange> History = [];

        public Task<string?> Answer(string gameKnowledge, HintPersona persona, string situation,
            IReadOnlyList<HintExchange> history, string question)
        {
            (Knowledge, Situation, History, Question) = (gameKnowledge, situation, history, question);
            return Task.FromResult(reply);
        }
    }

    private static readonly IContext State = Mock.Of<IContext>();

    [Test]
    public async Task HandsTheOracle_TheKnowledge_TheSituation_TheTranscript_AndTheConversation()
    {
        var oracle = new Oracle("Think about the ladder.");
        var history = new List<HintExchange> { new("what now?", "Look around.") };

        var (text, isHint) = await new HintOracleService(new Provider(), oracle)
            .GetHint(new OracleHintRequest(State, "  how do I cross?  ", history, "> look\nYou see a rift."));

        text.Should().Be("Think about the ladder.");
        isHint.Should().BeTrue();
        oracle.Knowledge.Should().Be("THE BIBLE");
        oracle.Situation.Should().StartWith("Location: the rift").And.Contain("AS THEY SAW IT").And.Contain("You see a rift.");
        oracle.History.Should().Equal(history);
        oracle.Question.Should().Be("how do I cross?");
    }

    [Test]
    public async Task NoTranscript_IsJustTheSituation()
    {
        var oracle = new Oracle("ok");
        await new HintOracleService(new Provider(), oracle).GetHint(new OracleHintRequest(State, "q", []));
        oracle.Situation.Should().Be("Location: the rift");
    }

    [Test]
    public async Task ALongTranscript_KeepsTheRecentEnd_AndALongConversation_TheLastTwenty()
    {
        var oracle = new Oracle("ok");
        var transcript = new string('a', 20000) + "THE END";
        var history = Enumerable.Range(0, 30).Select(i => new HintExchange($"q{i}", $"a{i}")).ToList();

        await new HintOracleService(new Provider(), oracle).GetHint(new OracleHintRequest(State, "q", history, transcript));

        oracle.Situation.Should().EndWith("THE END");
        oracle.Situation!.Length.Should().BeLessThan(10000);
        oracle.History.Should().HaveCount(20);
        oracle.History[0].Question.Should().Be("q10");
    }

    [Test]
    public async Task AnUnavailableOracle_IsADecline_NotAGuess()
    {
        var (text, isHint) = await new HintOracleService(new Provider(), new Oracle(null))
            .GetHint(new OracleHintRequest(State, "q", []));
        isHint.Should().BeFalse();
        text.Should().Be(HintOracleService.Unavailable);
    }
}

[TestFixture]
public class WorldDeltaTests
{
    [Test]
    public void ReportsWhatChanged_NotClocks_NotLateRegisteredDefaults()
    {
        var baseline = new Dictionary<string, string>
        {
            ["Item:Ladder.IsAcrossRift"] = "False",
            ["Item:Padlock.Locked"] = "True",
            ["Item:Magnet.CurrentLocation"] = "ToolRoom",
            ["Item:Chronometer.TurnsRemaining"] = "50",
            ["Location:ToolRoom.VisitCount"] = "0"
        };
        var now = new Dictionary<string, string>
        {
            ["Item:Ladder.IsAcrossRift"] = "True",
            ["Item:Padlock.Locked"] = "True",
            ["Item:Magnet.CurrentLocation"] = "Player",
            ["Item:Chronometer.TurnsRemaining"] = "12",
            ["Location:ToolRoom.VisitCount"] = "2",
            ["Item:Flask.HasEverBeenPickedUp"] = "False", // registered late, default
            ["Item:Laser.HasBeenFired"] = "True", // registered late, telling
            ["Location:SystemsMonitors.Fixed"] = "list:KUMUUNIKAASHUNZ"
        };

        var lines = WorldDelta.Describe(baseline, now);

        lines.Should().BeEquivalentTo(
            "Ladder.IsAcrossRift = True (was False)",
            "Magnet.CurrentLocation = Player (was ToolRoom)",
            "Laser.HasBeenFired = True",
            "SystemsMonitors.Fixed = [KUMUUNIKAASHUNZ]");
    }

    [Test]
    public void VisitedRooms_AreNamedInWords()
    {
        WorldDelta.VisitedRooms(new Dictionary<string, string>
        {
            ["Location:AdminCorridorSouth.VisitCount"] = "3",
            ["Location:ToolRoom.VisitCount"] = "0",
            ["Location:DeckNine.VisitCount"] = "1",
            ["Item:Magnet.VisitCount"] = "9"
        }).Should().Equal("Admin Corridor South", "Deck Nine");
    }
}

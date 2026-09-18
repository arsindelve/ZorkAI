using System.Text.Json;
using FluentAssertions;
using Model.Hints;
using Model.Web;
using NUnit.Framework;

namespace UnitTests.Hints;

/// <summary>
///     The wire contract between the hint endpoint and the web client, through the same serializer
///     settings ASP.NET Core uses (camelCase, case-insensitive).
/// </summary>
[TestFixture]
public class HintApiContractTests
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);

    [Test]
    public void Response_IsTextAndWhetherToRecordIt()
    {
        JsonSerializer.Serialize(new HintApiResponse("A nudge"), Web)
            .Should().Be("{\"text\":\"A nudge\",\"isHint\":true}");
        JsonSerializer.Serialize(new HintApiResponse("no game yet", IsHint: false), Web)
            .Should().Contain("\"isHint\":false");
    }

    [Test]
    public void Request_CarriesTheConversationAndTheTranscript()
    {
        const string body = """
            {"sessionId":"s","question":"more","transcript":"> look\nYou see a rift.","history":[
              {"question":"what do I do?","revealed":"A nudge"},
              {"question":"why?","revealed":"You can't know yet."}
            ]}
            """;

        var request = JsonSerializer.Deserialize<HintApiRequest>(body, Web)!;

        request.Question.Should().Be("more");
        request.Transcript.Should().Contain("You see a rift.");
        request.History.Should().Equal(
            new HintExchange("what do I do?", "A nudge"),
            new HintExchange("why?", "You can't know yet."));
    }

    [Test]
    public void AnExchangeRecordedByAnOlderClient_WithLadderFields_StillBinds()
    {
        const string body = """
            {"sessionId":"s","question":"more","history":[
              {"question":"what do I do?","revealed":"A nudge","topic":"CROSS_RIFT","rung":0,"kind":"Progress"}
            ]}
            """;

        var request = JsonSerializer.Deserialize<HintApiRequest>(body, Web)!;

        request.History.Should().Equal(new HintExchange("what do I do?", "A nudge"));
        request.Transcript.Should().BeNull();
    }
}

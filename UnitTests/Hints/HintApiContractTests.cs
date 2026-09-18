using System.Text.Json;
using FluentAssertions;
using Model.Hints;
using Model.Web;
using NUnit.Framework;

namespace UnitTests.Hints;

/// <summary>
///     The wire contract between the hint endpoint and the web client, through the same serializer
///     settings ASP.NET Core uses (camelCase, case-insensitive). The client must be able to echo what the
///     endpoint returned, field for field, or the ladder never advances.
/// </summary>
[TestFixture]
public class HintApiContractTests
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);

    [Test]
    public void Response_SerializesTheFieldsTheClientEchoes_InCamelCase()
    {
        var json = JsonSerializer.Serialize(new HintApiResponse("A nudge", "Progress", "CROSS_RIFT", 1, 3, "Warning"), Web);

        json.Should().Contain("\"text\":\"A nudge\"")
            .And.Contain("\"kind\":\"Progress\"")
            .And.Contain("\"topic\":\"CROSS_RIFT\"")
            .And.Contain("\"rung\":1")
            .And.Contain("\"totalRungs\":3")
            .And.Contain("\"softLock\":\"Warning\"")
            .And.Contain("\"isHint\":true");
    }

    [Test]
    public void ABareDecline_IsMarkedAsNotAHint()
    {
        var json = JsonSerializer.Serialize(new HintApiResponse("no game yet", IsHint: false), Web);

        json.Should().Contain("\"kind\":\"Decline\"").And.Contain("\"isHint\":false");
    }

    [Test]
    public void Request_DeserializesTheEchoedHistory_WithTopicRungAndKind()
    {
        const string body = """
            {"sessionId":"s","question":"more","history":[
              {"question":"what do I do?","revealed":"A nudge","topic":"CROSS_RIFT","rung":0,"kind":"Progress"},
              {"question":"why?","revealed":"You can't know yet.","kind":"Lore"},
              {"question":"old client","revealed":"an old exchange"}
            ]}
            """;

        var request = JsonSerializer.Deserialize<HintApiRequest>(body, Web)!;

        request.History.Should().HaveCount(3);
        request.History![0].Should().Be(new HintExchange("what do I do?", "A nudge", "CROSS_RIFT", 0, "Progress"));
        request.History[1].Topic.Should().BeNull();
        request.History[1].Rung.Should().BeNull();
        request.History[1].Kind.Should().Be("Lore");
        request.History[2].Kind.Should().BeNull(); // an exchange recorded by an older client still binds
    }

    [Test]
    public void TheClientCanEchoAResponseAsAnExchange_Losslessly()
    {
        // What HintServer.toExchange builds from a response, round-tripped through the request body.
        var response = new HintApiResponse("There's a ladder.", "Progress", "CROSS_RIFT", 1, 3);
        var exchangeJson = JsonSerializer.Serialize(
            new { question = "more", revealed = response.Text, topic = response.Topic, rung = response.Rung, kind = response.Kind }, Web);

        var exchange = JsonSerializer.Deserialize<HintExchange>(exchangeJson, Web);

        exchange.Should().Be(new HintExchange("more", "There's a ladder.", "CROSS_RIFT", 1, "Progress"));
    }
}

using FluentAssertions;
using ZorkAI.OpenAI;

namespace UnitTests.SelfHosted;

/// <summary>
///     Tolerant JSON extraction from raw LLM output — the compatibility layer for self-hosted
///     models (issue #383) that wrap JSON in code fences or surrounding prose.
/// </summary>
public class LlmJsonTests
{
    [Test]
    public void Should_PassThroughCleanJson()
    {
        LlmJson.ExtractJsonObject("{\"items\": [\"lamp\"]}").Should().Be("{\"items\": [\"lamp\"]}");
    }

    [Test]
    public void Should_StripMarkdownCodeFences()
    {
        var raw = "```json\n{\"items\": [\"lamp\"]}\n```";

        LlmJson.ExtractJsonObject(raw).Should().Be("{\"items\": [\"lamp\"]}");
    }

    [Test]
    public void Should_StripBareCodeFences()
    {
        var raw = "```\n{\"items\": []}\n```";

        LlmJson.ExtractJsonObject(raw).Should().Be("{\"items\": []}");
    }

    [Test]
    public void Should_IgnoreProseAroundTheObject()
    {
        var raw = "Sure! Here is the JSON you asked for:\n{\"items\": [\"sword\"]}\nLet me know if you need more.";

        LlmJson.ExtractJsonObject(raw).Should().Be("{\"items\": [\"sword\"]}");
    }

    [Test]
    public void Should_ReturnNull_When_NoObjectIsPresent()
    {
        LlmJson.ExtractJsonObject("I could not find any items.").Should().BeNull();
        LlmJson.ExtractJsonObject("").Should().BeNull();
        LlmJson.ExtractJsonObject(null).Should().BeNull();
    }
}

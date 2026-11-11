using FluentAssertions;
using ZorkAI.OpenAI;

namespace UnitTests.SelfHosted;

/// <summary>
///     The take/drop list parser's tolerant output parsing — needed for self-hosted servers
///     (issue #383) that ignore or reject the JSON response format.
/// </summary>
public class TakeAndDropParseItemsTests
{
    [Test]
    public void Should_ParseCleanItemsObject()
    {
        OpenAITakeAndDropListParser.ParseItems("{\"items\": [\"lamp\", \"sword\"]}")
            .Should().BeEquivalentTo("lamp", "sword");
    }

    [Test]
    public void Should_ParseFencedItemsObject()
    {
        OpenAITakeAndDropListParser.ParseItems("```json\n{\"items\": [\"leaflet\"]}\n```")
            .Should().BeEquivalentTo("leaflet");
    }

    [Test]
    public void Should_ReturnEmpty_When_OutputIsUnusable()
    {
        OpenAITakeAndDropListParser.ParseItems("no json here").Should().BeEmpty();
        OpenAITakeAndDropListParser.ParseItems("{\"items\": \"not-an-array\"}").Should().BeEmpty();
        OpenAITakeAndDropListParser.ParseItems(null).Should().BeEmpty();
    }
}

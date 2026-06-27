using ZorkAI.OpenAI;

namespace UnitTests;

[TestFixture]
public class ChatGPTClientTests
{
    [Test]
    public void NormalizeQuotes_ConvertsCurlyDoubleQuotes_ToStraight()
    {
        var input = "Floyd points. “This one is broken.”";
        ChatGPTClient.NormalizeQuotes(input).Should().Be("Floyd points. \"This one is broken.\"");
    }

    [Test]
    public void NormalizeQuotes_ConvertsCurlyApostrophes_ToStraight()
    {
        ChatGPTClient.NormalizeQuotes("Floyd’s gears feel creaky.")
            .Should().Be("Floyd's gears feel creaky.");
    }

    [Test]
    public void NormalizeQuotes_LeavesStraightQuotesUntouched()
    {
        const string already = "Floyd asks, \"What's that?\"";
        ChatGPTClient.NormalizeQuotes(already).Should().Be(already);
    }
}

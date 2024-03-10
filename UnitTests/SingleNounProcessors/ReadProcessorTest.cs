namespace UnitTests.SingleNounProcessors;

public class ReadProcessorTest : EngineTestsBase
{
    [Test]
    public async Task ReadProcessor()
    {
        var target = GetTarget();

        // Act
        await target.GetResponse("open mailbox");
        var result = await target.GetResponse("read leaflet");

        result.Should().Contain("low cunning");
    }
}
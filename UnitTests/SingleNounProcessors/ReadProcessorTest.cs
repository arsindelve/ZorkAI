using GameEngine;

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

    [Test]
    public async Task ReadInTheDarkProcessor()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Attic>();
        target.Context.Take(Repository.GetItem<Leaflet>());

        // Act
        var result = await target.GetResponse("read leaflet");

        result.Should().Contain("too dark");
    }
}
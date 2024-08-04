using GameEngine;

namespace UnitTests.SingleNounProcessors;

public class ExamineProcessorTests : EngineTestsBase
{
    [Test]
    public async Task ExamineProcessor()
    {
        var target = GetTarget();

        // Act
        await target.GetResponse("open mailbox");
        var result = await target.GetResponse("examine leaflet");

        result.Should().Contain("low cunning");
    }

    [Test]
    public async Task Examine_ItemIsClosed()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Kitchen>();

        // Act
        var result = await target.GetResponse("examine sack");

        // Assert
        result.Should().Contain("The brown sack is closed.");
    }

    [Test]
    public async Task Examine_ItemIsOpen_HasItems()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Kitchen>();

        // Act
        await target.GetResponse("open sack");
        var result = await target.GetResponse("examine sack");

        // Assert
        result.Should().Contain("The brown sack contains:");
        result.Should().Contain("A lunch");
        result.Should().Contain("A clove of garlic");
    }

    [Test]
    public async Task ExamineInTheDarkProcessor()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Attic>();
        target.Context.Take(Repository.GetItem<Leaflet>());

        // Act
        var result = await target.GetResponse("examine leaflet");

        result.Should().Contain("too dark");
    }
}
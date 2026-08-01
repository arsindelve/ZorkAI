using GameEngine;

namespace UnitTests.SingleNounProcessors;

/// <summary>
/// Read and examine are the two "look closely at a thing" verbs, and they share every interesting
/// edge: the same leaflet text, the same container-state reporting, and the same darkness guard.
/// Merged from the former ReadProcessorTest / ExamineProcessorTests.
/// </summary>
public class ReadAndExamineProcessorTests : EngineTestsBase
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

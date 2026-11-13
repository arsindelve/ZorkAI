using FluentAssertions;
using GameEngine;
using ZorkOne.Item;

namespace ZorkOne.Tests.Things;

public class BlackBookTests : EngineTestsBase
{
    // Verbs.BurnVerbs: setting fire to the book with nothing named to do it with.
    [TestCase("burn book")]
    [TestCase("light book")]
    [TestCase("torch book")]
    [TestCase("ignite book")]
    public async Task BurningTheBook_WithNothingNamed_AsksWhatWith(string input)
    {
        var target = GetTarget();
        target.Context.ItemPlacedHere(Repository.GetItem<BlackBook>());

        var response = await target.GetResponse(input);

        response.Should().Contain("You'll need to specify what you want to burn it with");
    }
}

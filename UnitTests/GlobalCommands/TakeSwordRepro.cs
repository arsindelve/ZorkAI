using GameEngine;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ZorkOne.Item;
using ZorkOne.Location;

namespace UnitTests.GlobalCommands;

[TestFixture]
public class TakeSwordRepro : EngineTestsBase
{
    [Test]
    public async Task TakeSword_FromLivingRoom_ShouldSucceed()
    {
        var target = GetTarget();
        // Force AI parser to return "sword" plain
        TakeAndDropParser.Setup(s => s.GetListOfItemsToTake(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new[] { "sword" });

        target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();
        var response = await target.GetResponse("take sword");

        response.Should().Contain("Taken");
    }

    [Test]
    public async Task TakeSword_AIReturnsElvishSword_ShouldSucceed()
    {
        var target = GetTarget();
        // Force AI parser to return "elvish sword" — what may happen in production
        TakeAndDropParser.Setup(s => s.GetListOfItemsToTake(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new[] { "elvish sword" });

        target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();
        var response = await target.GetResponse("take sword");

        response.Should().Contain("Taken");
    }
}

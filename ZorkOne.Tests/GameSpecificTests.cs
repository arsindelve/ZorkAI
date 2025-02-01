using FluentAssertions;
using GameEngine;
using Model.AIParsing;
using Moq;
using ZorkOne.GlobalCommand;
using ZorkOne.Item;
using ZorkOne.Location;
using ZorkOne.Location.ForestLocation;

namespace ZorkOne.Tests;

[TestFixture]
public class GameSpecificTests : EngineTestsBase
{
    [Test]
    public async Task EnteringTheKitchen_AddTenPoints()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<BehindHouse>();

        // Act
        await target.GetResponse("open window");
        await target.GetResponse("W");

        // Assert
        target.Context.Score.Should().Be(10);
    }

    [Test]
    public async Task GoingBackToTheKitchen_DoesNotAddTenPoints()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<BehindHouse>();

        // Act
        await target.GetResponse("open window");
        await target.GetResponse("W");
        await target.GetResponse("W");
        await target.GetResponse("E");

        // Assert
        target.Context.Score.Should().Be(10);
    }

    [Test]
    public async Task JumpOutOfATree()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<UpATree>();

        // Act
        await target.GetResponse("jump");

        // Assert
        target.Context.CurrentLocation.Should().Be(Repository.GetLocation<ForestPath>());
    }

    [Test]
    public async Task ClimbATree()
    {
        var target = GetTarget(new IntentParser(Mock.Of<IAIParser>(), new ZorkOneGlobalCommandFactory()));
        target.Context.CurrentLocation = Repository.GetLocation<ForestPath>();

        // Act
        await target.GetResponse("climb up the tree");

        // Assert
        target.Context.CurrentLocation.Should().Be(Repository.GetLocation<UpATree>());
    }

    [Test]
    public async Task PrayAtTheAltar()
    {
        var target = GetTarget(new TestParser(new ZorkOneGlobalCommandFactory()));
        target.Context.CurrentLocation = Repository.GetLocation<Altar>();

        // Act
        var result = await target.GetResponse("pray");

        // Assert
        target.Context.CurrentLocation.Should().Be(Repository.GetLocation<ForestOne>());
        result.Should().Contain("sunlight");
    }

    [Test]
    public async Task PuttingStuffInTheTrophyCase_IncreaseScore()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();
        target.Context.Take(Repository.GetItem<Painting>());
        Repository.GetItem<TrophyCase>().IsOpen = true;

        // Act
        var response = await target.GetResponse("put painting in case");

        // Assert
        response.Should().Contain("Done");
        target.Context.Score.Should().Be(10); // 6 for picking it up, 4 for going in the case.
    }

    [Test]
    public async Task PuttingStuffInTheTrophyCase_SecondTime_DoesNotIncreaseScore()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();
        target.Context.Take(Repository.GetItem<Painting>());
        Repository.GetItem<TrophyCase>().IsOpen = true;

        // Act
        await target.GetResponse("put painting inside case");
        await target.GetResponse("take painting");
        var response = await target.GetResponse("put painting in case");

        // Assert
        response.Should().Contain("Done");
        target.Context.Score.Should().Be(10); // 6 for picking it up, 4 for going in the case.
    }

    [Test]
    public async Task PuttingGarbageInTheTrophyCase_DoesNotIncreaseScore()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();
        target.Context.Take(Repository.GetItem<Leaflet>());
        Repository.GetItem<TrophyCase>().IsOpen = true;

        // Act
        var response = await target.GetResponse("put leaflet in case");

        // Assert
        response.Should().Contain("Done");
        target.Context.Score.Should().Be(0);
    }

    [Test]
    public async Task PickingUpTreasure_AddPoints()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<BatRoom>();
        target.Context.Take(Repository.GetItem<Lantern>());
        Repository.GetItem<Lantern>().IsOn = true;

        // Act
        var response = await target.GetResponse("take figurine");
        Console.WriteLine(response);

        // Assert
        target.Context.Score.Should().Be(5);
    }

    [Test]
    public async Task PickingUpTreasure_NoExtraPointsForPickingItUpAgain()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<BatRoom>();
        target.Context.Take(Repository.GetItem<Lantern>());
        Repository.GetItem<Lantern>().IsOn = true;

        // Act
        await target.GetResponse("take figurine");
        await target.GetResponse("drop figurine");
        await target.GetResponse("take figurine");

        // Assert
        target.Context.Score.Should().Be(5);
    }

    [Test]
    public async Task DropStuffFromTheChasm_NeverSeenAgain()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Chasm>();
        target.Context.Take(Repository.GetItem<Leaflet>());
        target.Context.Take(Repository.GetItem<Lantern>());
        Repository.GetItem<Lantern>().IsOn = true;

        // Act
        var response = await target.GetResponse("drop leaflet");

        // Assert
        response.Should().Contain("The leaflet drops out of sight and into the chasm");
        target.Context.Items.Count.Should().Be(1);
        Repository.GetItem<Leaflet>().CurrentLocation.Should().BeNull();
        target.Context.HasItem<Leaflet>().Should().BeFalse();
    }
}
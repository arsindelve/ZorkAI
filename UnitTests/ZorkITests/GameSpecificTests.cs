using ZorkOne.GlobalCommand;

namespace UnitTests.ZorkITests;

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
        var target = GetTarget(new IntentParser(new ZorkOneGlobalCommandFactory()));
        target.Context.CurrentLocation = Repository.GetLocation<ForestPath>();

        // Act
        await target.GetResponse("climb up the tree");

        // Assert
        target.Context.CurrentLocation.Should().Be(Repository.GetLocation<UpATree>());
    }

    [Test]
    public async Task PrayAtTheAltar()
    {
        var target = GetTarget(new IntentParser(new ZorkOneGlobalCommandFactory()));
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
        var response = await target.GetResponse("put painting inside case");

        // Assert
        response.Should().Contain("Done");
        target.Context.Score.Should().Be(6);
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
        var response = await target.GetResponse("put painting inside case");

        // Assert
        response.Should().Contain("Done");
        target.Context.Score.Should().Be(6);
    }

    [Test]
    public async Task PuttingGarbageInTheTrophyCase_DoesNotIncreaseScore()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();
        target.Context.Take(Repository.GetItem<Leaflet>());
        Repository.GetItem<TrophyCase>().IsOpen = true;

        // Act
        var response = await target.GetResponse("put leaflet inside case");

        // Assert
        response.Should().Contain("Done");
        target.Context.Score.Should().Be(0);
    }
}
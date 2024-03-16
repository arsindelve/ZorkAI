namespace UnitTests;

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
        var target = GetTarget(new IntentParser());
        target.Context.CurrentLocation = Repository.GetLocation<ForestPath>();

        // Act
        await target.GetResponse("climb up the tree");
        
        // Assert
        target.Context.CurrentLocation.Should().Be(Repository.GetLocation<UpATree>());
    }
    
    [Test]
    public async Task PrayAtTheAltar()
    {
        var target = GetTarget(new IntentParser());
        target.Context.CurrentLocation = Repository.GetLocation<Altar>();

        // Act
        string? result = await target.GetResponse("pray");
        
        // Assert
        target.Context.CurrentLocation.Should().Be(Repository.GetLocation<ForestOne>());
        result.Should().Contain("sunlight");
    }
}
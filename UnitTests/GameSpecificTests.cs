using ZorkOne.Location;

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
    
}
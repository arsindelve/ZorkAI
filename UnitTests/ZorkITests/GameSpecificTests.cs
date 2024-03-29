using ZorkOne;
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

    [Test]
    public async Task Dam_Red_Button()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<MaintenanceRoom>();
        target.Context.Take(Repository.GetItem<Lantern>());
        Repository.GetItem<Lantern>().IsOn = true;

        // Act
        var response = await target.GetResponse("press the red button");

        // Assert
        response.Should().Contain("lights");
        Repository.GetLocation<MaintenanceRoom>().IsNoLongerDark.Should().BeTrue();
        target.Context.ItIsDarkHere.Should().BeFalse();
        Repository.GetItem<Lantern>().IsOn = false;
        target.Context.ItIsDarkHere.Should().BeFalse();

        // Act Again
        await target.GetResponse("press the red button");

        // Assert again
        target.Context.ItIsDarkHere.Should().BeTrue();
        Repository.GetLocation<MaintenanceRoom>().IsNoLongerDark.Should().BeFalse();
    }

    [Test]
    public async Task Dam_Yellow_Button_And_Brown_Button()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<MaintenanceRoom>();
        target.Context.Take(Repository.GetItem<Lantern>());
        Repository.GetItem<Lantern>().IsOn = true;
        Repository.GetItem<ControlPanel>().GreenBubbleGlowing.Should().BeFalse();

        // Act
        var response = await target.GetResponse("press the yellow button");

        // Assert
        response.Should().Contain("Click");
        Repository.GetItem<ControlPanel>().GreenBubbleGlowing.Should().BeTrue();

        // Act Again
        await target.GetResponse("press the yellow button");

        // Assert again. Still good. No effect, 
        Repository.GetItem<ControlPanel>().GreenBubbleGlowing.Should().BeTrue();

        // Act Again
        await target.GetResponse("press the brown button");
        Repository.GetItem<ControlPanel>().GreenBubbleGlowing.Should().BeFalse();
    }

    [Test]
    public async Task Clearing_MoveTheLeaves_RevealAGrate()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Clearing>();
        target.Context.CurrentLocation.HasItem<Grating>().Should().BeFalse();

        // Act
        var response = await target.GetResponse("move the leaves");

        // Assert
        response.Should().Contain("grating is revealed");
        target.Context.CurrentLocation.HasItem<Grating>().Should().BeTrue();
    }
    
    [Test]
    public async Task Clearing_TakeTheLeaves_RevealAGrate()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Clearing>();
        target.Context.CurrentLocation.HasItem<Grating>().Should().BeFalse();

        // Act
        var response = await target.GetResponse("take the leaves");

        // Assert
        response.Should().Contain("grating is revealed");
        target.Context.CurrentLocation.HasItem<Grating>().Should().BeTrue();
    }

    [Test]
    public async Task Clearing_CannotOpenLockedGrate()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Clearing>();

        // Act
        await target.GetResponse("move the leaves");
        var response = await target.GetResponse("open the grating");

        // Assert
        response.Should().Contain("locked");
        Repository.GetItem<Grating>().IsOpen.Should().BeFalse();
    }
    
    [Test]
    public async Task Clearing_CountTheLeaves()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Clearing>();

        // Act
        var response = await target.GetResponse("count the leaves");

        // Assert
        response.Should().Contain("69,105");
    }

    [Test]
    public async Task DomeRoom_TieRopeToRailing_DoNotHaveTheRope()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<DomeRoom>();
        ((ICanHoldItems)target.Context.CurrentLocation).ItemPlacedHere(Repository.GetItem<Rope>());
        target.Context.Take(Repository.GetItem<Lantern>());
        Repository.GetItem<Lantern>().IsOn = true;

        // Act
        var response = await target.GetResponse("tie rope to railing");

        // Assert
        response.Should().Contain("don't have the rope");
        Repository.GetItem<Rope>().TiedToRailing.Should().BeFalse();
    }

    [Test]
    public async Task DomeRoom_TieRopeToRailing_Success()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<DomeRoom>();
        target.Context.Take(Repository.GetItem<Lantern>());
        target.Context.Take(Repository.GetItem<Rope>());
        Repository.GetItem<Lantern>().IsOn = true;

        // Act
        var response = await target.GetResponse("tie rope to railing");

        // Assert
        response.Should().Contain("drops over the side");
        Repository.GetItem<Rope>().TiedToRailing.Should().BeTrue();
    }
    
    [Test]
    public async Task DomeRoom_TakingTheRopeUntiesIt()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<DomeRoom>();
        target.Context.Take(Repository.GetItem<Lantern>());
        target.Context.Take(Repository.GetItem<Rope>());
        Repository.GetItem<Lantern>().IsOn = true;

        // Act
        await target.GetResponse("tie rope to railing");
        target.Context.HasItem<Rope>().Should().BeFalse();
        var response = await target.GetResponse("take rope");

        // Assert
        response.Should().Contain("Taken");
        Repository.GetItem<Rope>().TiedToRailing.Should().BeFalse();
        target.Context.HasItem<Rope>().Should().BeTrue();
    }
    
    [Test]
    public async Task DomeRoom_Jump_Fatal()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<DomeRoom>();
        target.Context.Take(Repository.GetItem<Lantern>());
        target.Context.Take(Repository.GetItem<Rope>());
        Repository.GetItem<Lantern>().IsOn = true;

        // Act
        var response = await target.GetResponse("jump");

        // Assert
        response.Should().Contain("died");
        ((ZorkIContext)target.Context).DeathCounter.Should().Be(1);
    }
}
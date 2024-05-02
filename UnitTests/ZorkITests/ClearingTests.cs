namespace UnitTests.ZorkITests;

public class ClearingTests : EngineTestsBase
{
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
}
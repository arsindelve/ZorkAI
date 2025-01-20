using FluentAssertions;
using GameEngine;
using ZorkOne.Item;
using ZorkOne.Location;
using ZorkOne.Location.MazeLocation;

namespace ZorkOne.Tests;

public class ClearingAndGratingTests : EngineTestsBase
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

    [Test]
    public async Task Clearing_HaveKey_DontSpecify_UnlockTheGrating()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Clearing>();
        target.Context.Take(Repository.GetItem<SkeletonKey>());

        // Act
        await target.GetResponse("move leaves");
        var response = await target.GetResponse("unlock grate");

        // Assert
        response.Should().Contain("You can't reach the lock from here");
    }

    [Test]
    public async Task Clearing_HaveKey_Specify_UnlockTheGrating()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Clearing>();
        target.Context.Take(Repository.GetItem<SkeletonKey>());

        // Act
        await target.GetResponse("move leaves");
        var response = await target.GetResponse("unlock grate with the key");

        // Assert
        response.Should().Contain("You can't reach the lock from here");
    }

    [Test]
    public async Task Clearing_HaveKey_DontSpecifyTheKey_LockTheGrating()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Clearing>();
        target.Context.Take(Repository.GetItem<SkeletonKey>());

        // Act
        await target.GetResponse("move leaves");
        var response = await target.GetResponse("lock grate");

        // Assert
        response.Should().Contain("You can't lock it from this side");
    }

    [Test]
    public async Task Clearing_HaveKey_SpecifyTheKey_LockTheGrating()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Clearing>();
        target.Context.Take(Repository.GetItem<SkeletonKey>());

        // Act
        await target.GetResponse("move leaves");
        var response = await target.GetResponse("lock the grate with the key");

        // Assert
        response.Should().Contain("You can't lock it from this side");
    }

    [Test]
    public async Task GratingRoom_HaveKey_SpecifyTheKey_LockTheGrating()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<GratingRoom>();
        target.Context.Take(Repository.GetItem<SkeletonKey>());
        target.Context.Take(Repository.GetItem<Lantern>());
        Repository.GetItem<Lantern>().IsOn = true;

        // Act
        var response = await target.GetResponse("lock the grate with the key");

        // Assert
        response.Should().Contain("The grate is locked");
    }

    [Test]
    public async Task GratingRoom_Locked_Look()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<GratingRoom>();
        target.Context.Take(Repository.GetItem<SkeletonKey>());
        target.Context.Take(Repository.GetItem<Lantern>());
        Repository.GetItem<Lantern>().IsOn = true;

        // Act
        var response = await target.GetResponse("look");

        // Assert
        response.Should().Contain("skull-and-crossbones lock");
    }

    [Test]
    public async Task GratingRoom_Unlocked_Look()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<GratingRoom>();
        target.Context.Take(Repository.GetItem<SkeletonKey>());
        target.Context.Take(Repository.GetItem<Lantern>());
        Repository.GetItem<Lantern>().IsOn = true;

        // Act
        await target.GetResponse("unlock grate with the key");
        var response = await target.GetResponse("look");

        // Assert
        response.Should().Contain("Above you is a grating");
        response.Should().NotContain("skull-and-crossbones lock");
    }

    [Test]
    public async Task GratingRoom_Open_Look()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<GratingRoom>();
        target.Context.Take(Repository.GetItem<SkeletonKey>());
        target.Context.Take(Repository.GetItem<Lantern>());
        Repository.GetItem<Lantern>().IsOn = true;

        // Act
        await target.GetResponse("unlock grate with the key");
        await target.GetResponse("open grate");
        var response = await target.GetResponse("look");

        // Assert
        response.Should().Contain("Above you is an open grating");
        response.Should().Contain("sunlight pouring in");
        response.Should().NotContain("skull-and-crossbones lock");
    }

    [Test]
    public async Task GratingRoom_OpenWithoutUnlocking_Look()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<GratingRoom>();
        target.Context.Take(Repository.GetItem<SkeletonKey>());
        target.Context.Take(Repository.GetItem<Lantern>());
        Repository.GetItem<Lantern>().IsOn = true;

        // Act
        var response = await target.GetResponse("open grate");

        // Assert
        response.Should().Contain("locked");

        // Act
        response = await target.GetResponse("look");

        // Assert
        response.Should().Contain("skull-and-crossbones lock");
    }

    [Test]
    public async Task GratingRoom_HaveKey_Open_FirstTime_DumpLeavesOnMyHead()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<GratingRoom>();
        target.Context.Take(Repository.GetItem<SkeletonKey>());
        target.Context.Take(Repository.GetItem<Lantern>());
        Repository.GetItem<Lantern>().IsOn = true;

        // Act
        await target.GetResponse("unlock the grate with the key");
        var response = await target.GetResponse("open the grate");

        // Assert
        response.Should().Contain("reveal trees above you");
        response.Should().Contain("A pile of leaves falls onto your head and to the ground.");
    }

    [Test]
    public async Task GratingRoom_AlreadyTookTheLeaves_DoesNotDumpLeavesOnMyHead()
    {
        var target = GetTarget();
        target.Context.Take(Repository.GetItem<SkeletonKey>());
        target.Context.Take(Repository.GetItem<Lantern>());
        Repository.GetItem<Lantern>().IsOn = true;

        // Act
        target.Context.CurrentLocation = Repository.GetLocation<Clearing>();
        await target.GetResponse("move the leaves");
        target.Context.CurrentLocation = Repository.GetLocation<GratingRoom>();
        await target.GetResponse("unlock the grate with the key");
        var response = await target.GetResponse("open the grate");

        // Assert
        response.Should().Contain("reveal trees above you");
        response.Should().NotContain("A pile of leaves falls onto your head and to the ground.");
    }

    [Test]
    public async Task GratingRoom_HaveKey_Open_FirstTime_LeavesAreHereNow()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<GratingRoom>();
        target.Context.Take(Repository.GetItem<SkeletonKey>());
        target.Context.Take(Repository.GetItem<Lantern>());
        Repository.GetItem<Lantern>().IsOn = true;

        // Act
        await target.GetResponse("unlock the grate with the key");
        await target.GetResponse("open the grate");
        var response = await target.GetResponse("take leaves");

        // Assert
        response.Should().Contain("Taken");
    }

    [Test]
    public async Task GratingRoom_HaveKey_Open_SecondTime_DoesNotDumpLeavesOnMyHead()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<GratingRoom>();
        target.Context.Take(Repository.GetItem<SkeletonKey>());
        target.Context.Take(Repository.GetItem<Lantern>());
        Repository.GetItem<Lantern>().IsOn = true;

        // Act
        Console.WriteLine(await target.GetResponse("unlock the grate with the key"));
        Console.WriteLine(await target.GetResponse("open the grate"));
        Console.WriteLine(await target.GetResponse("close the grate"));
        var response = await target.GetResponse("open the grate");

        // Assert
        response.Should().Contain("reveal trees above you");
        response.Should().NotContain("A pile of leaves falls onto your head and to the ground.");
    }

    [Test]
    public async Task Clearing_GratingOpen_Look()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Clearing>();
        Repository.GetItem<Grating>().IsLocked = false;
        Repository.GetItem<Grating>().IsOpen = true;

        // Act
        await target.GetResponse("move the leaves");
        var response = await target.GetResponse("look");

        // Assert
        response.Should().Contain("There is an open grating, descending into darkness");
    }

    [Test]
    public async Task Clearing_GratingClosed_Look()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Clearing>();
        Repository.GetItem<Grating>().IsLocked = false;
        Repository.GetItem<Grating>().IsOpen = false;

        // Act
        await target.GetResponse("move the leaves");
        var response = await target.GetResponse("look");

        // Assert
        response.Should().Contain("securely fastened into the ground");
    }
}
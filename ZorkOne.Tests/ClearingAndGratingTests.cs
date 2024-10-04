using FluentAssertions;
using GameEngine;
using NUnit.Framework;
using UnitTests;
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
    
    // TODO: Open with leaves on top, dump on my head
    // TODO: Open it, no leaves, no dump
    // TODO: close it
    // TODO: open it without leaves on top
    // TODO: open without unlocking 
    // TODO: lock when open (it DOES lock and then you can't close it. BUG!!)
    
}
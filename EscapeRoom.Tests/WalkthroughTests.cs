using FluentAssertions;
using EscapeRoom.Item;
using EscapeRoom.Location;
using GameEngine;
using Model;

namespace EscapeRoom.Tests;

[TestFixture]
public class WalkthroughTests : EscapeRoomEngineTestsBase
{
    [SetUp]
    public void SetUp()
    {
        Repository.Reset();
    }

    [Test]
    public async Task CompleteWalkthrough_Should_WinGame()
    {
        // Arrange
        var engine = GetTarget();

        // Start at Reception
        engine.Context.CurrentLocation.Should().BeOfType<Reception>();

        // Step 1: Open the desk and take the leaflet
        var response = await engine.GetResponse("open desk");
        response.Should().Contain("leaflet");

        response = await engine.GetResponse("take leaflet");
        response.Should().Contain("Taken");
        engine.Context.HasItem<Leaflet>().Should().BeTrue();

        // Step 2: Go west to Office
        response = await engine.GetResponse("w");
        engine.Context.CurrentLocation.Should().BeOfType<Office>();

        // Step 3: Open the desk and take the flashlight
        response = await engine.GetResponse("open desk");
        response.Should().Contain("flashlight");

        response = await engine.GetResponse("take flashlight");
        response.Should().Contain("Taken");
        engine.Context.HasItem<Flashlight>().Should().BeTrue();

        // Step 4: Turn on the flashlight
        response = await engine.GetResponse("light flashlight");
        response.Should().Contain("on");
        Repository.GetItem<Flashlight>().IsOn.Should().BeTrue();

        // Step 5: Go east back to Reception
        response = await engine.GetResponse("e");
        engine.Context.CurrentLocation.Should().BeOfType<Reception>();

        // Step 6: Go north to Storage Closet (now we have light!)
        response = await engine.GetResponse("n");
        engine.Context.CurrentLocation.Should().BeOfType<StorageCloset>();

        // Step 7: Open the box and take the key
        response = await engine.GetResponse("open box");
        response.Should().Contain("key");

        response = await engine.GetResponse("take key");
        response.Should().Contain("Taken");
        engine.Context.HasItem<BrassKey>().Should().BeTrue();

        // Step 8: Go south back to Reception
        response = await engine.GetResponse("s");
        engine.Context.CurrentLocation.Should().BeOfType<Reception>();

        // Step 9: Go south to Exit Hallway
        response = await engine.GetResponse("s");
        engine.Context.CurrentLocation.Should().BeOfType<ExitHallway>();

        // Step 10: Unlock the door
        response = await engine.GetResponse("unlock door with key");
        response.Should().Contain("unlocked");

        // Step 11: Open the door - WIN!
        response = await engine.GetResponse("open door");
        response.Should().Contain("CONGRATULATIONS");

        // Verify win condition
        engine.Context.HasEscaped.Should().BeTrue();
        // Score breakdown: Flashlight(10) + Key(30) + Unlock(10) + Escape(50) = 100
        engine.Context.Score.Should().Be(100);
    }

    [Test]
    public async Task CannotOpenDoor_WhenLocked()
    {
        // Arrange
        var engine = GetTarget();

        // Go to exit hallway
        await engine.GetResponse("s");

        // Try to open the locked door
        var response = await engine.GetResponse("open door");
        response.Should().Contain("locked");

        // Should not have escaped
        engine.Context.HasEscaped.Should().BeFalse();
    }

    [Test]
    public async Task CannotUnlockDoor_WithoutKey()
    {
        // Arrange
        var engine = GetTarget();

        // Go to exit hallway
        await engine.GetResponse("s");

        // Try to unlock without key
        var response = await engine.GetResponse("unlock door");
        response.Should().Contain("don't have");

        // Door should still be locked
        Repository.GetItem<ExitDoor>().IsLocked.Should().BeTrue();
    }

    [Test]
    public async Task CannotSeeInDarkStorageCloset_WithoutLight()
    {
        // Arrange
        var engine = GetTarget();

        // Go north to Storage Closet without flashlight
        var response = await engine.GetResponse("n");
        engine.Context.CurrentLocation.Should().BeOfType<StorageCloset>();

        // Should see the dark description
        response.Should().Contain("dark");

        // Try to open the box in the dark - should not be able to see it
        response = await engine.GetResponse("open box");
        response.Should().NotContain("key");
    }

    [Test]
    public async Task CanSeeInStorageCloset_WithFlashlightOn()
    {
        // Arrange
        var engine = GetTarget();

        // Go west to Office
        await engine.GetResponse("w");

        // Get and turn on flashlight
        await engine.GetResponse("open desk");
        await engine.GetResponse("take flashlight");
        await engine.GetResponse("light flashlight");

        // Go east then north to Storage Closet
        await engine.GetResponse("e");
        var response = await engine.GetResponse("n");

        // Should see the normal description, not dark
        response.Should().NotContain("pitch dark");
        response.Should().Contain("Storage Closet");

        // Can now open the box and see the key
        response = await engine.GetResponse("open box");
        response.Should().Contain("key");
    }

    [Test]
    public async Task EnteringMaintenanceShaft_ShouldKillPlayer_AndRestartGame()
    {
        // Arrange
        var engine = GetTarget();

        // Start at Reception
        engine.Context.CurrentLocation.Should().BeOfType<Reception>();
        engine.Context.DeathCounter.Should().Be(0);

        // Go east to Lounge
        var response = await engine.GetResponse("e");
        engine.Context.CurrentLocation.Should().BeOfType<Lounge>();
        response.Should().Contain("DANGER");

        // Go south to the deadly Maintenance Shaft
        response = await engine.GetResponse("s");

        // Should contain death message
        response.Should().Contain("plummet");
        response.Should().Contain("You have died");

        // Death counter should be incremented
        engine.Context.DeathCounter.Should().Be(1);

        // Game should restart - player back at Reception
        engine.Context.CurrentLocation.Should().BeOfType<Reception>();
    }

    [Test]
    public async Task DyingMultipleTimes_ShouldIncrementDeathCounter()
    {
        // Arrange
        var engine = GetTarget();

        // Die once
        await engine.GetResponse("e"); // Go to Lounge
        await engine.GetResponse("s"); // Die in Maintenance Shaft
        engine.Context.DeathCounter.Should().Be(1);

        // Die again
        await engine.GetResponse("e"); // Go to Lounge
        await engine.GetResponse("s"); // Die in Maintenance Shaft
        engine.Context.DeathCounter.Should().Be(2);

        // Die a third time
        await engine.GetResponse("e"); // Go to Lounge
        await engine.GetResponse("s"); // Die in Maintenance Shaft
        engine.Context.DeathCounter.Should().Be(3);
    }
}

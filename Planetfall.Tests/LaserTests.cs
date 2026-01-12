using FluentAssertions;
using GameEngine;
using Model.Interface;
using Moq;
using Planetfall.Item.Kalamontee.Mech;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Location.Kalamontee.Mech;
using Planetfall.Location.Lawanda;

namespace Planetfall.Tests;

public class LaserTests : EngineTestsBase
{
    #region Dial Setting Tests
    [Test]
    public async Task SetDialToExistingSetting()
    {
        var target = GetTarget();
        Take<Laser>();

        var response = await target.GetResponse("set laser to 5");

        response.Should().Contain("That's where it's set now");
        GetItem<Laser>().Setting.Should().Be(5);
    }
    
    [Test]
    public async Task SetDialToNewSetting()
    {
        var target = GetTarget();
        Take<Laser>();

        var response = await target.GetResponse("set laser to 6");

        response.Should().Contain("now set to 6");
        GetItem<Laser>().Setting.Should().Be(6);
    }
    
    [Test]
    public async Task SetDialToTooLowNumber()
    {
        var target = GetTarget();
        Take<Laser>();

        var response = await target.GetResponse("set laser to 0");

        response.Should().Contain("he dial can only be set to numbers between 1 and 6");
        GetItem<Laser>().Setting.Should().Be(5);
    }
    
    [Test]
    public async Task SetDialToTooHighNumber()
    {
        var target = GetTarget();
        Take<Laser>();

        var response = await target.GetResponse("set laser to 90");

        response.Should().Contain("he dial can only be set to numbers between 1 and 6");
        GetItem<Laser>().Setting.Should().Be(5);
    }
    
    [Test]
    public async Task SetDialToTNonsense()
    {
        var target = GetTarget();
        Take<Laser>();

        var response = await target.GetResponse("set laser to bob");

        response.Should().Contain("he dial can only be set to numbers between 1 and 6");
        GetItem<Laser>().Setting.Should().Be(5);
    }

    [Test]
    public async Task ShootLaserWithoutBattery()
    {
        var target = GetTarget();
        Take<Laser>();
        GetItem<Laser>().Items.Clear();
        GetItem<OldBattery>().CurrentLocation = null;

        var response = await target.GetResponse("shoot laser");

        response.Should().Contain("Click");
    }
    
    [Test]
    public async Task ShootLaserWithDeadBattery()
    {
        var target = GetTarget();
        Take<Laser>();
        GetItem<OldBattery>().ChargesRemaining = 0;
        
        var response = await target.GetResponse("shoot laser");

        response.Should().Contain("Click");
    }
    
    [Test]
    public async Task ShootLaserNotHoldingIt()
    {
        var target = GetTarget();
        StartHere<ToolRoom>();
        
        var response = await target.GetResponse("shoot laser");

        response.Should().Contain("You're not holding the laser");
    }
    
    [Test]
    public async Task ShootLaserWithGoodBattery()
    {
        var target = GetTarget();
        Take<Laser>();
        GetItem<OldBattery>().ChargesRemaining = 5;
        
        var response = await target.GetResponse("shoot laser");

        response.Should().NotContain("Click");
        response.Should().Contain("beam of light");
    }
    
    [Test]
    public async Task ShootLaserWithGoodBatteryDepleatIt()
    {
        var target = GetTarget();
        Take<Laser>();
        GetItem<OldBattery>().ChargesRemaining = 2;
        
        var response = await target.GetResponse("shoot laser");
        response.Should().NotContain("Click");
        response.Should().Contain("beam of light");
        
        response = await target.GetResponse("shoot laser");
        response.Should().NotContain("Click");
        response.Should().Contain("beam of light");
        
        response = await target.GetResponse("shoot laser");
        response.Should().Contain("Click");
        response.Should().NotContain("beam of light");
    }
    
    [Test]
    [TestCase(1, "red")]
    [TestCase(2, "orange")]
    [TestCase(3, "yellow")]
    [TestCase(4, "green")]
    [TestCase(5, "blue")]
    [TestCase(6, "violet")]
    public async Task LasterColors(int dial, string color)
    {
        var target = GetTarget();
        Take<Laser>();
        GetItem<Laser>().Setting = dial;
        GetItem<OldBattery>().ChargesRemaining = 5;

        var response = await target.GetResponse("shoot laser");

        response.Should().NotContain("Click");
        response.Should().Contain("beam of light");
        response.Should().Contain(color);
    }

    #endregion

    #region Heating/Cooling System Tests

    [Test]
    public async Task ShootLaser_RegistersAsActor()
    {
        var target = GetTarget();
        Take<Laser>();
        GetItem<OldBattery>().ChargesRemaining = 10;

        target.Context.Actors.Should().NotContain(GetItem<Laser>());

        await target.GetResponse("shoot laser");

        target.Context.Actors.Should().Contain(GetItem<Laser>());
    }

    [Test]
    public async Task ShootLaser_SetsJustShotFlag()
    {
        var target = GetTarget();
        Take<Laser>();
        GetItem<OldBattery>().ChargesRemaining = 10;
        var laser = GetItem<Laser>();

        laser.JustShot.Should().BeFalse();

        await target.GetResponse("shoot laser");

        // JustShot is reset at end of turn by Act(), so check WarmthLevel instead
        laser.WarmthLevel.Should().Be(1);
    }

    [Test]
    public async Task ShootLaser_ThreeTimes_ShowsSlightlyWarmMessage()
    {
        var target = GetTarget();
        Take<Laser>();
        GetItem<OldBattery>().ChargesRemaining = 10;

        await target.GetResponse("shoot laser");
        await target.GetResponse("shoot laser");
        var response = await target.GetResponse("shoot laser");

        response.Should().Contain("slightly warm");
        GetItem<Laser>().WarmthLevel.Should().Be(3);
    }

    [Test]
    public async Task ShootLaser_SixTimes_ShowsSomewhatWarmMessage()
    {
        var target = GetTarget();
        Take<Laser>();
        GetItem<OldBattery>().ChargesRemaining = 10;

        for (int i = 0; i < 5; i++)
            await target.GetResponse("shoot laser");

        var response = await target.GetResponse("shoot laser");

        response.Should().Contain("somewhat warm");
        GetItem<Laser>().WarmthLevel.Should().Be(6);
    }

    [Test]
    public async Task ShootLaser_NineTimes_ShowsVeryWarmMessage()
    {
        var target = GetTarget();
        Take<Laser>();
        GetItem<OldBattery>().ChargesRemaining = 15;

        for (int i = 0; i < 8; i++)
            await target.GetResponse("shoot laser");

        var response = await target.GetResponse("shoot laser");

        response.Should().Contain("very warm");
        GetItem<Laser>().WarmthLevel.Should().Be(9);
    }

    [Test]
    public async Task ShootLaser_TwelveTimes_ShowsQuiteHotMessage()
    {
        var target = GetTarget();
        Take<Laser>();
        GetItem<OldBattery>().ChargesRemaining = 20;

        for (int i = 0; i < 11; i++)
            await target.GetResponse("shoot laser");

        var response = await target.GetResponse("shoot laser");

        response.Should().Contain("quite hot");
        GetItem<Laser>().WarmthLevel.Should().Be(12);
    }

    [Test]
    public async Task Laser_CoolsWhenNotFired()
    {
        var target = GetTarget();
        Take<Laser>();
        GetItem<OldBattery>().ChargesRemaining = 10;

        // Heat up to level 2
        await target.GetResponse("shoot laser");
        await target.GetResponse("shoot laser");
        GetItem<Laser>().WarmthLevel.Should().Be(2);

        // Wait (don't fire)
        await target.GetResponse("wait");
        GetItem<Laser>().WarmthLevel.Should().Be(1);

        await target.GetResponse("wait");
        GetItem<Laser>().WarmthLevel.Should().Be(0);
    }

    [Test]
    public async Task Laser_CoolingToThreshold3_ShowsCoolingMessage()
    {
        var target = GetTarget();
        Take<Laser>();
        GetItem<OldBattery>().ChargesRemaining = 10;

        // Heat up to level 4
        await target.GetResponse("shoot laser");
        await target.GetResponse("shoot laser");
        await target.GetResponse("shoot laser");
        await target.GetResponse("shoot laser");
        GetItem<Laser>().WarmthLevel.Should().Be(4);

        // Cool down to 3
        var response = await target.GetResponse("wait");

        response.Should().Contain("cooled");
        response.Should().Contain("slightly warm");
        GetItem<Laser>().WarmthLevel.Should().Be(3);
    }

    [Test]
    public async Task Laser_CoolingToThreshold6_ShowsCoolingMessage()
    {
        var target = GetTarget();
        Take<Laser>();
        GetItem<OldBattery>().ChargesRemaining = 10;

        // Heat up to level 7
        var laser = GetItem<Laser>();
        laser.WarmthLevel = 7;
        laser.JustShot = false;
        target.Context.RegisterActor(laser);

        // Cool down to 6
        var response = await target.GetResponse("wait");

        response.Should().Contain("cooled");
        response.Should().Contain("somewhat warm");
        laser.WarmthLevel.Should().Be(6);
    }

    [Test]
    public async Task Laser_CoolingToThreshold9_ShowsCoolingMessage()
    {
        var target = GetTarget();
        Take<Laser>();

        var laser = GetItem<Laser>();
        laser.WarmthLevel = 10;
        laser.JustShot = false;
        target.Context.RegisterActor(laser);

        var response = await target.GetResponse("wait");

        response.Should().Contain("cooled");
        response.Should().Contain("very warm");
        laser.WarmthLevel.Should().Be(9);
    }

    [Test]
    public async Task Laser_CoolingToThreshold12_ShowsCoolingMessage()
    {
        var target = GetTarget();
        Take<Laser>();

        var laser = GetItem<Laser>();
        laser.WarmthLevel = 13;
        laser.JustShot = false;
        target.Context.RegisterActor(laser);

        var response = await target.GetResponse("wait");

        response.Should().Contain("cooled");
        response.Should().Contain("quite hot");
        laser.WarmthLevel.Should().Be(12);
    }

    [Test]
    public async Task Laser_FullyCooled_RemovesFromActors()
    {
        var target = GetTarget();
        Take<Laser>();

        var laser = GetItem<Laser>();
        laser.WarmthLevel = 1;
        laser.JustShot = false;
        target.Context.RegisterActor(laser);
        target.Context.Actors.Should().Contain(laser);

        await target.GetResponse("wait");

        laser.WarmthLevel.Should().Be(0);
        target.Context.Actors.Should().NotContain(laser);
    }

    [Test]
    public async Task Laser_FullyCooled_NoMessage()
    {
        var target = GetTarget();
        Take<Laser>();

        var laser = GetItem<Laser>();
        laser.WarmthLevel = 1;
        laser.JustShot = false;
        target.Context.RegisterActor(laser);

        var response = await target.GetResponse("wait");

        // Should not have a cooling message when reaching 0
        response.Should().NotContain("cooled");
        response.Should().NotContain("warm");
        response.Should().NotContain("hot");
    }

    [Test]
    public async Task Laser_HeatCoolHeat_Sequence()
    {
        // Test the example from the spec: fire, fire, fire (3), fire (4), wait (3), wait (2), fire (3), fire (4)
        var target = GetTarget();
        Take<Laser>();
        GetItem<OldBattery>().ChargesRemaining = 20;
        var laser = GetItem<Laser>();

        // Turn 1: Fire laser → WARMTH-FLAG = 1
        await target.GetResponse("shoot laser");
        laser.WarmthLevel.Should().Be(1);

        // Turn 2: Fire laser → WARMTH-FLAG = 2
        await target.GetResponse("shoot laser");
        laser.WarmthLevel.Should().Be(2);

        // Turn 3: Fire laser → WARMTH-FLAG = 3 → "slightly warm now"
        var response = await target.GetResponse("shoot laser");
        laser.WarmthLevel.Should().Be(3);
        response.Should().Contain("slightly warm now");

        // Turn 4: Fire laser → WARMTH-FLAG = 4 (no message)
        response = await target.GetResponse("shoot laser");
        laser.WarmthLevel.Should().Be(4);
        response.Should().NotContain("warm");

        // Turn 5: Do something → WARMTH-FLAG = 3 → "cooled...slightly warm"
        response = await target.GetResponse("wait");
        laser.WarmthLevel.Should().Be(3);
        response.Should().Contain("cooled");
        response.Should().Contain("slightly warm");

        // Turn 6: Do something → WARMTH-FLAG = 2 (no message)
        response = await target.GetResponse("wait");
        laser.WarmthLevel.Should().Be(2);
        response.Should().NotContain("cooled");

        // Turn 7: Fire laser → WARMTH-FLAG = 3 → "slightly warm now"
        response = await target.GetResponse("shoot laser");
        laser.WarmthLevel.Should().Be(3);
        response.Should().Contain("slightly warm now");

        // Turn 8: Fire laser → WARMTH-FLAG = 4 (no message)
        response = await target.GetResponse("shoot laser");
        laser.WarmthLevel.Should().Be(4);
        response.Should().NotContain("warm");
    }

    [Test]
    public async Task Laser_NoMessageAtNonThresholdLevels()
    {
        var target = GetTarget();
        Take<Laser>();
        GetItem<OldBattery>().ChargesRemaining = 20;

        // Level 1 - no message
        var response = await target.GetResponse("shoot laser");
        response.Should().NotContain("warm");

        // Level 2 - no message
        response = await target.GetResponse("shoot laser");
        response.Should().NotContain("warm");

        // Level 4 - no message (3 was threshold)
        await target.GetResponse("shoot laser"); // 3
        response = await target.GetResponse("shoot laser"); // 4
        response.Should().NotContain("warm");

        // Level 5 - no message
        response = await target.GetResponse("shoot laser");
        response.Should().NotContain("warm");
    }

    [Test]
    public async Task Laser_InRoom_ShowsMessageWhenPlayerPresent()
    {
        var target = GetTarget();
        StartHere<ToolRoom>();
        var toolRoom = GetLocation<ToolRoom>();
        var laser = GetItem<Laser>();

        // Put laser in room (not in inventory)
        toolRoom.ItemPlacedHere(laser);
        laser.WarmthLevel = 2;
        laser.JustShot = true;
        target.Context.RegisterActor(laser);

        var response = await target.GetResponse("wait");

        response.Should().Contain("slightly warm");
    }

    [Test]
    public async Task Laser_InDifferentRoom_NoMessage()
    {
        var target = GetTarget();
        StartHere<RepairRoom>(); // Player is here
        var toolRoom = GetLocation<ToolRoom>();
        var laser = GetItem<Laser>();

        // Put laser in different room
        toolRoom.ItemPlacedHere(laser);
        laser.WarmthLevel = 2;
        laser.JustShot = true;
        target.Context.RegisterActor(laser);

        var response = await target.GetResponse("wait");

        // Laser still heats up
        laser.WarmthLevel.Should().Be(3);
        // But no message since player is not with laser
        response.Should().NotContain("warm");
    }

    [Test]
    public async Task Laser_StillCoolsWhenPlayerInDifferentRoom()
    {
        var target = GetTarget();
        StartHere<RepairRoom>(); // Player is here
        var toolRoom = GetLocation<ToolRoom>();
        var laser = GetItem<Laser>();

        // Put laser in different room
        toolRoom.ItemPlacedHere(laser);
        laser.WarmthLevel = 4;
        laser.JustShot = false;
        target.Context.RegisterActor(laser);

        await target.GetResponse("wait");
        laser.WarmthLevel.Should().Be(3);

        await target.GetResponse("wait");
        laser.WarmthLevel.Should().Be(2);

        await target.GetResponse("wait");
        laser.WarmthLevel.Should().Be(1);

        await target.GetResponse("wait");
        laser.WarmthLevel.Should().Be(0);
        target.Context.Actors.Should().NotContain(laser);
    }

    [Test]
    public async Task Laser_JustShotResetEachTurn()
    {
        var target = GetTarget();
        Take<Laser>();
        GetItem<OldBattery>().ChargesRemaining = 10;
        var laser = GetItem<Laser>();

        await target.GetResponse("shoot laser");
        // After Act() runs, JustShot should be reset
        laser.JustShot.Should().BeFalse();

        await target.GetResponse("wait");
        laser.JustShot.Should().BeFalse();
    }

    [Test]
    public void Act_DirectCall_WarmingUp()
    {
        Repository.Reset();
        var laser = GetItem<Laser>();
        var mockContext = new Mock<IContext>();
        mockContext.Setup(c => c.CurrentLocation).Returns(GetLocation<ToolRoom>());

        laser.WarmthLevel = 2;
        laser.JustShot = true;
        laser.CurrentLocation = mockContext.Object; // In player's inventory

        var result = laser.Act(mockContext.Object, null!).Result;

        laser.WarmthLevel.Should().Be(3);
        laser.JustShot.Should().BeFalse();
        result.Should().Contain("slightly warm");
    }

    [Test]
    public void Act_DirectCall_CoolingDown()
    {
        Repository.Reset();
        var laser = GetItem<Laser>();
        var mockContext = new Mock<IContext>();
        mockContext.Setup(c => c.CurrentLocation).Returns(GetLocation<ToolRoom>());

        laser.WarmthLevel = 4;
        laser.JustShot = false;
        laser.CurrentLocation = mockContext.Object; // In player's inventory

        var result = laser.Act(mockContext.Object, null!).Result;

        laser.WarmthLevel.Should().Be(3);
        result.Should().Contain("cooled");
        result.Should().Contain("slightly warm");
    }

    [Test]
    public void Act_DirectCall_FullyCooledRemovesActor()
    {
        Repository.Reset();
        var laser = GetItem<Laser>();
        var mockContext = new Mock<IContext>();
        mockContext.Setup(c => c.CurrentLocation).Returns(GetLocation<ToolRoom>());

        laser.WarmthLevel = 1;
        laser.JustShot = false;
        laser.CurrentLocation = mockContext.Object; // In player's inventory

        var result = laser.Act(mockContext.Object, null!).Result;

        laser.WarmthLevel.Should().Be(0);
        result.Should().BeEmpty(); // No message when reaching 0
        mockContext.Verify(c => c.RemoveActor(laser), Times.Once);
    }

    [Test]
    public void Act_DirectCall_NotWithPlayer_NoMessage()
    {
        Repository.Reset();
        var laser = GetItem<Laser>();
        var toolRoom = GetLocation<ToolRoom>();
        var repairRoom = GetLocation<RepairRoom>();
        var mockContext = new Mock<IContext>();
        mockContext.Setup(c => c.CurrentLocation).Returns(repairRoom); // Player in different room

        laser.WarmthLevel = 2;
        laser.JustShot = true;
        laser.CurrentLocation = toolRoom; // Laser in ToolRoom

        var result = laser.Act(mockContext.Object, null!).Result;

        laser.WarmthLevel.Should().Be(3); // Still heats up
        result.Should().BeEmpty(); // But no message
    }

    [Test]
    public async Task ShootLaserWithNoBattery_DoesNotRegisterAsActor()
    {
        var target = GetTarget();
        Take<Laser>();
        GetItem<Laser>().Items.Clear();

        await target.GetResponse("shoot laser");

        target.Context.Actors.Should().NotContain(GetItem<Laser>());
    }

    [Test]
    public async Task ShootLaserWithDeadBattery_DoesNotRegisterAsActor()
    {
        var target = GetTarget();
        Take<Laser>();
        GetItem<OldBattery>().ChargesRemaining = 0;

        await target.GetResponse("shoot laser");

        target.Context.Actors.Should().NotContain(GetItem<Laser>());
    }

    [Test]
    public async Task ShootLaserNotHolding_DoesNotRegisterAsActor()
    {
        var target = GetTarget();
        StartHere<ToolRoom>();

        await target.GetResponse("shoot laser");

        target.Context.Actors.Should().NotContain(GetItem<Laser>());
    }

    #endregion

    #region Shooting At Targets Tests

    [Test]
    public async Task ShootLaserWithLaser_RubberBarrelMessage()
    {
        var target = GetTarget();
        Take<Laser>();
        GetItem<OldBattery>().ChargesRemaining = 10;

        var response = await target.GetResponse("shoot laser with laser");

        response.Should().Contain("rubber barrel");
    }

    [Test]
    public async Task FireLaserAtLaser_RubberBarrelMessage()
    {
        var target = GetTarget();
        Take<Laser>();
        GetItem<OldBattery>().ChargesRemaining = 10;

        var response = await target.GetResponse("fire laser at laser");

        response.Should().Contain("rubber barrel");
    }

    [Test]
    public async Task ShootBatteryWithLaser_RubberBarrelMessage()
    {
        var target = GetTarget();
        Take<Laser>();
        GetItem<OldBattery>().ChargesRemaining = 10;

        var response = await target.GetResponse("shoot battery with laser");

        response.Should().Contain("rubber barrel");
    }

    [Test]
    public async Task ShootOldBatteryWithLaser_RubberBarrelMessage()
    {
        var target = GetTarget();
        Take<Laser>();
        GetItem<OldBattery>().ChargesRemaining = 10;

        var response = await target.GetResponse("shoot old battery with laser");

        response.Should().Contain("rubber barrel");
    }

    [Test]
    public async Task ShootItemInRoom_BeamStrikesMessage()
    {
        var target = GetTarget();
        StartHere<ToolRoom>();
        Take<Laser>();
        GetItem<OldBattery>().ChargesRemaining = 10;

        var response = await target.GetResponse("shoot flask with laser");

        response.Should().Contain("beam of light");
        response.Should().Contain("strikes the");
        response.Should().Contain("flask");
        response.Should().Contain("grows a bit warm");
    }

    [Test]
    public async Task ShootItemInInventory_BeamStrikesMessage()
    {
        var target = GetTarget();
        StartHere<ToolRoom>(); // Initialize ToolRoom which has magnet
        Take<Laser>();
        Take<Magnet>();
        GetItem<OldBattery>().ChargesRemaining = 10;

        var response = await target.GetResponse("shoot magnet with laser");

        response.Should().Contain("beam of light");
        response.Should().Contain("strikes the");
        response.Should().Contain("grows a bit warm");
    }

    [Test]
    public async Task ShootItem_ConsumesCharge()
    {
        var target = GetTarget();
        StartHere<ToolRoom>();
        Take<Laser>();
        var battery = GetItem<OldBattery>();
        battery.ChargesRemaining = 5;

        await target.GetResponse("shoot flask with laser");

        battery.ChargesRemaining.Should().Be(4);
    }

    [Test]
    public async Task ShootItem_RegistersAsActor()
    {
        var target = GetTarget();
        StartHere<ToolRoom>();
        Take<Laser>();
        GetItem<OldBattery>().ChargesRemaining = 10;

        target.Context.Actors.Should().NotContain(GetItem<Laser>());

        await target.GetResponse("shoot flask with laser");

        target.Context.Actors.Should().Contain(GetItem<Laser>());
    }

    [Test]
    public async Task ShootItem_IncreasesWarmthLevel()
    {
        var target = GetTarget();
        StartHere<ToolRoom>();
        Take<Laser>();
        GetItem<OldBattery>().ChargesRemaining = 10;
        var laser = GetItem<Laser>();

        laser.WarmthLevel.Should().Be(0);

        await target.GetResponse("shoot flask with laser");

        laser.WarmthLevel.Should().Be(1);
    }

    [Test]
    public async Task ShootItem_ShowsCorrectColor()
    {
        var target = GetTarget();
        StartHere<ToolRoom>();
        Take<Laser>();
        GetItem<OldBattery>().ChargesRemaining = 10;
        GetItem<Laser>().Setting = 1; // Red

        var response = await target.GetResponse("shoot flask with laser");

        response.Should().Contain("red beam of light");
    }

    [Test]
    public async Task ShootItem_NotHoldingLaser_CannotShoot()
    {
        var target = GetTarget();
        StartHere<ToolRoom>();
        // Don't take the laser - leave it in the room
        GetItem<OldBattery>().ChargesRemaining = 10;

        var response = await target.GetResponse("shoot flask with laser");

        response.Should().Contain("not holding the laser");
    }

    [Test]
    public async Task ShootItem_DeadBattery_Click()
    {
        var target = GetTarget();
        StartHere<ToolRoom>();
        Take<Laser>();
        GetItem<OldBattery>().ChargesRemaining = 0;

        var response = await target.GetResponse("shoot flask with laser");

        response.Should().Contain("Click");
    }

    [Test]
    public async Task ShootItem_NotInScope_CannotSee()
    {
        var target = GetTarget();
        StartHere<ToolRoom>(); // Flask is here, but let's try something not here
        Take<Laser>();
        GetItem<OldBattery>().ChargesRemaining = 10;

        var response = await target.GetResponse("shoot canteen with laser");

        response.Should().Contain("don't see any canteen");
    }

    [Test]
    public async Task ShootItem_RubberBarrel_DoesNotConsumeCharge()
    {
        var target = GetTarget();
        Take<Laser>();
        var battery = GetItem<OldBattery>();
        battery.ChargesRemaining = 5;

        await target.GetResponse("shoot laser with laser");

        battery.ChargesRemaining.Should().Be(5);
    }

    [Test]
    public async Task ShootItem_RubberBarrel_DoesNotRegisterAsActor()
    {
        var target = GetTarget();
        Take<Laser>();
        GetItem<OldBattery>().ChargesRemaining = 10;

        await target.GetResponse("shoot laser with laser");

        target.Context.Actors.Should().NotContain(GetItem<Laser>());
    }

    [Test]
    public async Task ShootFloyd_SpecialResponse()
    {
        var target = GetTarget();
        StartHere<RobotShop>(); // Floyd starts here
        Take<Laser>();
        GetItem<OldBattery>().ChargesRemaining = 10;
        GetItem<Floyd>().IsOn = true; // Floyd must be on to be in scope

        var response = await target.GetResponse("shoot floyd with laser");

        response.Should().Contain("beam of light");
        response.Should().Contain("strikes Floyd");
        response.Should().Contain("Yow");
        response.Should().Contain("eyes you warily");
    }

    #endregion

    #region "shoot laser at X" syntax tests (laser is NounOne)

    [Test]
    public async Task ShootLaserAtFlask_HitsFlask()
    {
        var target = GetTarget();
        StartHere<ToolRoom>(); // Flask is here
        Take<Laser>();
        GetItem<OldBattery>().ChargesRemaining = 10;

        var response = await target.GetResponse("shoot laser at flask");

        response.Should().Contain("beam of light");
        response.Should().Contain("strikes the");
        response.Should().Contain("flask");
        response.Should().Contain("grows a bit warm");
    }

    [Test]
    public async Task FireLaserAtFlask_HitsFlask()
    {
        var target = GetTarget();
        StartHere<ToolRoom>(); // Flask is here
        Take<Laser>();
        GetItem<OldBattery>().ChargesRemaining = 10;

        var response = await target.GetResponse("fire laser at flask");

        response.Should().Contain("beam of light");
        response.Should().Contain("strikes the");
        response.Should().Contain("flask");
        response.Should().Contain("grows a bit warm");
    }

    [Test]
    public async Task ShootLaserAtFloyd_SpecialResponse()
    {
        var target = GetTarget();
        StartHere<RobotShop>(); // Floyd starts here
        Take<Laser>();
        GetItem<OldBattery>().ChargesRemaining = 10;
        GetItem<Floyd>().IsOn = true; // Floyd must be on to be in scope

        var response = await target.GetResponse("shoot laser at floyd");

        response.Should().Contain("beam of light");
        response.Should().Contain("strikes Floyd");
        response.Should().Contain("Yow");
        response.Should().Contain("eyes you warily");
    }

    [Test]
    public async Task ShootLaserAtLaser_RubberBarrelMessage()
    {
        var target = GetTarget();
        Take<Laser>();
        GetItem<OldBattery>().ChargesRemaining = 10;

        var response = await target.GetResponse("fire laser at laser");

        response.Should().Contain("rubber barrel");
    }

    #endregion
}
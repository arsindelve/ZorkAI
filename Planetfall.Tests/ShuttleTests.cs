using FluentAssertions;
using GameEngine;
using Planetfall.Item.Feinstein;
using Planetfall.Item.Kalamontee.Admin;
using Planetfall.Location.Shuttle;

namespace Planetfall.Tests;

public class ShuttleTests : EngineTestsBase
{
    [Test]
    public async Task PushLeverWhenNotActive()
    {
        var target = GetTarget();
        StartHere<AlfieControlEast>();

        var response = await target.GetResponse("push lever");

        response.Should().Contain("A recorded voice says \"Shuttle controls are not currently activated.\"");
    }

    [Test]
    public async Task PullLeverWhenNotActive()
    {
        var target = GetTarget();
        StartHere<AlfieControlEast>();

        var response = await target.GetResponse("push lever");

        response.Should().Contain("A recorded voice says \"Shuttle controls are not currently activated.\"");
    }

    [Test]
    public async Task Activate_ThenWait_Deactivates()
    {
        var target = GetTarget();
        Take<ShuttleAccessCard>();
        StartHere<AlfieControlEast>();

        await target.GetResponse("slide shuttle access card through slot");
        GetLocation<AlfieControlEast>().Activated.Should().BeTrue();

        for (var i = 0; i < 15; i++)
            await target.GetResponse("wait");

        GetLocation<AlfieControlEast>().Activated.Should().BeFalse();
    }

    [Test]
    public async Task Look_WhileInStation_Outbound()
    {
        var target = GetTarget();
        StartHere<AlfieControlEast>();

        var response = await target.GetResponse("look");

        response.Should()
            .Contain(
                "Through the cabin window you can see parallel rails running along the floor of a long tunnel, vanishing in the distance");
    }

    [Test]
    public async Task Look_WhileInStation_Inbound()
    {
        var target = GetTarget();
        StartHere<AlfieControlWest>();

        var response = await target.GetResponse("look");

        response.Should().Contain("Through the cabin window you can see a featureless concrete wall");
    }

    [Test]
    public async Task Look_WhileCloseToStation_Outbound()
    {
        var target = GetTarget();
        StartHere<AlfieControlEast>().TunnelPosition = 190;

        var response = await target.GetResponse("look");

        response.Should()
            .Contain("Through the cabin window you can see parallel rails ending at a brightly lit station ahead");
    }

    [Test]
    public async Task Activate_WhileInStation_Inbound()
    {
        var target = GetTarget();
        Take<ShuttleAccessCard>();
        StartHere<AlfieControlWest>();

        var response = await target.GetResponse("slide shuttle access card through slot");

        response.Should().Contain("A recorded voice says \"Use other control cabin. Control activation overridden.\"");
    }

    [Test]
    public async Task Activate_WhileInStation_Outbound()
    {
        var target = GetTarget();
        Take<ShuttleAccessCard>();
        StartHere<AlfieControlEast>();

        var response = await target.GetResponse("slide shuttle access card through slot");

        response.Should().Contain("A recording of a deep male voice says \"Shuttle controls activated.\"");
        GetLocation<AlfieControlEast>().Activated.Should().BeTrue();
    }

    [Test]
    public async Task Activate_WhileInStation_Outbound_DoNotHave()
    {
        var target = GetTarget();
        StartHere<AlfieControlEast>();

        var response = await target.GetResponse("slide shuttle access card through slot");

        response.Should().NotContain("A recording of a deep male voice says \"Shuttle controls activated.\"");
        GetLocation<AlfieControlEast>().Activated.Should().BeFalse();
    }

    [Test]
    public async Task Activate_WhileInStation_Outbound_WrongCard()
    {
        var target = GetTarget();
        StartHere<AlfieControlEast>();
        Take<KitchenAccessCard>();

        var response = await target.GetResponse("slide kitchen access card through slot");

        response.Should().Contain("Inkorekt awtharazaashun kard");
        GetLocation<AlfieControlEast>().Activated.Should().BeFalse();
    }

    [Test]
    public async Task Activate_WhileInStation_Outbound_BeginToMove_PropertiesAndResponse()
    {
        var target = GetTarget();
        Take<ShuttleAccessCard>();
        StartHere<AlfieControlEast>();

        await target.GetResponse("slide shuttle access card through slot");
        var response = await target.GetResponse("push lever");

        response.Should().Contain("The lever is now in the upper position.");
        response.Should()
            .Contain(
                "The control cabin door slides shut and the shuttle car begins to move forward! The display changes to 5");
        GetLocation<AlfieControlEast>().Activated.Should().BeTrue();
        GetLocation<AlfieControlEast>().Speed.Should().Be(5);
        GetLocation<AlfieControlEast>().LeverPosition.Should().Be(ShuttleLeverPosition.Acceleration);
    }

    [Test]
    public async Task TryToDecelerate_WhileStopped()
    {
        var target = GetTarget();
        Take<ShuttleAccessCard>();
        StartHere<AlfieControlEast>();

        await target.GetResponse("slide shuttle access card through slot");
        var response = await target.GetResponse("pull lever");

        response.Should().Contain("The lever immediately pops back to the central position");
        GetLocation<AlfieControlEast>().Activated.Should().BeTrue();
        GetLocation<AlfieControlEast>().Speed.Should().Be(0);
        GetLocation<AlfieControlEast>().LeverPosition.Should().Be(ShuttleLeverPosition.Neutral);
    }

    [Test]
    public async Task Accelerate()
    {
        var target = GetTarget();
        Take<ShuttleAccessCard>();
        StartHere<AlfieControlEast>();

        await target.GetResponse("slide shuttle access card through slot");
        await target.GetResponse("push lever");
        var response = await target.GetResponse("wait");

        response.Should().Contain("The shuttle car continues to move. The display blinks, and now reads 10.");
        GetLocation<AlfieControlEast>().Activated.Should().BeTrue();
        GetLocation<AlfieControlEast>().Speed.Should().Be(10);
        GetLocation<AlfieControlEast>().LeverPosition.Should().Be(ShuttleLeverPosition.Acceleration);
    }

    [Test]
    public async Task Accelerate_Then_Neutral()
    {
        var target = GetTarget();
        Take<ShuttleAccessCard>();
        StartHere<AlfieControlEast>();

        await target.GetResponse("slide shuttle access card through slot");
        await target.GetResponse("push lever");
        var response = await target.GetResponse("pull lever");

        response.Should().Contain("The lever is now in the central position");
        response.Should().Contain("The shuttle car continues to move. The display still reads 5.");
        GetLocation<AlfieControlEast>().Activated.Should().BeTrue();
        GetLocation<AlfieControlEast>().Speed.Should().Be(5);
        GetLocation<AlfieControlEast>().LeverPosition.Should().Be(ShuttleLeverPosition.Neutral);
    }

    [Test]
    public async Task Accelerate_Then_Decelerate()
    {
        var target = GetTarget();
        Take<ShuttleAccessCard>();
        StartHere<AlfieControlEast>();

        await target.GetResponse("slide shuttle access card through slot");
        await target.GetResponse("push lever");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("pull lever");
        var response = await target.GetResponse("pull lever");

        response.Should().Contain("The lever is now in the lower position");
        response.Should().Contain("The shuttle car continues to move. The display blinks, and now reads 10.");
        GetLocation<AlfieControlEast>().Activated.Should().BeTrue();
        GetLocation<AlfieControlEast>().Speed.Should().Be(10);
        GetLocation<AlfieControlEast>().LeverPosition.Should().Be(ShuttleLeverPosition.Deceleration);
    }

    [Test]
    public async Task Accelerate_Then_Stop()
    {
        var target = GetTarget();
        Take<ShuttleAccessCard>();
        StartHere<AlfieControlEast>();

        await target.GetResponse("slide shuttle access card through slot");
        await target.GetResponse("push lever");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("pull lever");
        await target.GetResponse("pull lever");
        await target.GetResponse("wait");
        var response = await target.GetResponse("wait");

        response.Should().Contain("The shuttle car comes to a stop and the lever pops back to the central position");
        GetLocation<AlfieControlEast>().Activated.Should().BeTrue();
        GetLocation<AlfieControlEast>().Speed.Should().Be(0);
        GetLocation<AlfieControlEast>().LeverPosition.Should().Be(ShuttleLeverPosition.Neutral);
    }

    [Test]
    public async Task Accelerate_ThenPushLeverAgain()
    {
        var target = GetTarget();
        Take<ShuttleAccessCard>();
        StartHere<AlfieControlEast>();

        await target.GetResponse("slide shuttle access card through slot");
        await target.GetResponse("push lever");
        var response = await target.GetResponse("push lever");

        response.Should().Contain("The lever is already in the upper position");
        GetLocation<AlfieControlEast>().Activated.Should().BeTrue();
        GetLocation<AlfieControlEast>().Speed.Should().Be(10);
        GetLocation<AlfieControlEast>().LeverPosition.Should().Be(ShuttleLeverPosition.Acceleration);
    }

    [Test]
    public async Task Decelerate_ThenPullLeverAgain()
    {
        var target = GetTarget();
        Take<ShuttleAccessCard>();
        StartHere<AlfieControlEast>();

        await target.GetResponse("slide shuttle access card through slot");
        await target.GetResponse("push lever");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("pull lever");
        await target.GetResponse("pull lever");
        var response = await target.GetResponse("pull lever");

        response.Should().Contain("The lever is already in the lower position");
        GetLocation<AlfieControlEast>().Activated.Should().BeTrue();
        GetLocation<AlfieControlEast>().Speed.Should().Be(5);
        GetLocation<AlfieControlEast>().LeverPosition.Should().Be(ShuttleLeverPosition.Deceleration);
    }

    [Test]
    public async Task Decelerate_ThenPushLever()
    {
        var target = GetTarget();
        Take<ShuttleAccessCard>();
        StartHere<AlfieControlEast>();

        await target.GetResponse("slide shuttle access card through slot");
        await target.GetResponse("push lever");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("pull lever");
        await target.GetResponse("pull lever");
        var response = await target.GetResponse("push lever");

        response.Should().Contain("The lever is now in the central position");
        GetLocation<AlfieControlEast>().Activated.Should().BeTrue();
        GetLocation<AlfieControlEast>().Speed.Should().Be(10);
        GetLocation<AlfieControlEast>().LeverPosition.Should().Be(ShuttleLeverPosition.Neutral);
    }

    [Test]
    public async Task Move_ThenTryToOpenDoor()
    {
        var target = GetTarget();
        Take<ShuttleAccessCard>();
        StartHere<AlfieControlEast>();

        await target.GetResponse("slide shuttle access card through slot");
        await target.GetResponse("push lever");

        var response = await target.GetResponse("open door");

        response.Should()
            .Contain(
                "A recorded voice says \"Operator should remain in control cabin while shuttle car is between stations.\"");
    }

    [Test]
    public async Task Move_ThenTryToCloseDoor()
    {
        var target = GetTarget();
        Take<ShuttleAccessCard>();
        StartHere<AlfieControlEast>();

        await target.GetResponse("slide shuttle access card through slot");
        await target.GetResponse("push lever");

        var response = await target.GetResponse("close door");

        response.Should().Contain("It is closed");
    }

    [Test]
    public async Task Accelerate_Once_Then_Stop()
    {
        var target = GetTarget();
        Take<ShuttleAccessCard>();
        StartHere<AlfieControlEast>();

        await target.GetResponse("slide shuttle access card through slot");
        await target.GetResponse("push lever");
        await target.GetResponse("pull lever");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        var response = await target.GetResponse("pull lever");

        response.Should().Contain("The shuttle car comes to a stop and the lever pops back to the central position");
        GetLocation<AlfieControlEast>().Activated.Should().BeTrue();
        GetLocation<AlfieControlEast>().Speed.Should().Be(0);
        GetLocation<AlfieControlEast>().LeverPosition.Should().Be(ShuttleLeverPosition.Neutral);
    }

    [Test]
    public async Task MiddleOfTrack_CannotLeave()
    {
        var target = GetTarget();
        Take<ShuttleAccessCard>();
        StartHere<AlfieControlEast>();

        await target.GetResponse("slide shuttle access card through slot");
        await target.GetResponse("push lever");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("pull lever");
        await target.GetResponse("pull lever");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        var response = await target.GetResponse("west");

        response.Should().Contain("The door is closed");
        target.Context.CurrentLocation.Should().BeOfType<AlfieControlEast>();
    }

    [Test]
    [TestCase(3, "You pass a sign which says \"Limit 45.\"")]
    [TestCase(13,
        "The tunnel levels out and begins to slope upward. A sign flashes by which reads \"Hafwaa Mark -- Beegin Deeseluraashun.\"")]
    [TestCase(21, "You pass a sign, surrounded by blinking red lights, which says \"15.\"")]
    [TestCase(22, "You pass a sign, surrounded by blinking red lights, which says \"10.\"")]
    [TestCase(23, "You pass a sign, surrounded by blinking red lights, which says \"5.\"")]
    [TestCase(24,
        "The shuttle car is approaching a brightly lit area. As you near it, you make out the concrete platforms of a shuttle station")]
    public async Task Landmarks(int turns, string result)
    {
        var target = GetTarget();
        Take<ShuttleAccessCard>();
        StartHere<AlfieControlEast>();

        await target.GetResponse("slide shuttle access card through slot");
        await target.GetResponse("push lever");
        await target.GetResponse("pull lever");

        for (var i = 3; i < 25; i++)
        {
            var response = await target.GetResponse("wait");
            Console.Write(response);
            Console.WriteLine(Repository.GetLocation<AlfieControlEast>().TunnelPosition);
            response.Should().Contain("The shuttle car continues to move. The display still reads 5");
            if (i == turns)
                response.Should().Contain(result);
        }
    }

    [Test]
    public async Task FullTrip_ConstantSpeed_NoDeceleration()
    {
        var target = GetTarget();
        Take<ShuttleAccessCard>();
        StartHere<AlfieControlEast>();
        var controls = Repository.GetLocation<AlfieControlEast>();

        await target.GetResponse("slide shuttle access card through slot");

        controls.Speed.Should().Be(0);
        controls.LeverPosition.Should().Be(ShuttleLeverPosition.Neutral);
        controls.TunnelPosition.Should().Be(0);
        controls.Activated.Should().BeTrue();
        controls.DoorIsClosed.Should().BeFalse();

        await target.GetResponse("push lever");

        controls.Speed.Should().Be(5);
        controls.LeverPosition.Should().Be(ShuttleLeverPosition.Acceleration);
        controls.TunnelPosition.Should().Be(1);
        controls.Activated.Should().BeTrue();
        controls.DoorIsClosed.Should().BeTrue();

        await target.GetResponse("pull lever");

        controls.Speed.Should().Be(5);
        controls.LeverPosition.Should().Be(ShuttleLeverPosition.Neutral);
        controls.TunnelPosition.Should().Be(2);
        controls.Activated.Should().BeTrue();
        controls.DoorIsClosed.Should().BeTrue();

        var response = "";
        for (var i = 3; i < 26; i++)
        {
            controls.Speed.Should().Be(5);
            response = await target.GetResponse("wait") ?? string.Empty;
            Console.Write(response);
            Console.WriteLine(Repository.GetLocation<AlfieControlEast>().TunnelPosition);
        }

        response.Should().Contain(
            "The shuttle car rumbles through the station and smashes into the wall at the far end. You are thrown forward into the control panel. Both you and the shuttle car produce unhealthy crunching sounds as the cabin doors creak slowly open");

        controls.Speed.Should().Be(0);
        controls.LeverPosition.Should().Be(ShuttleLeverPosition.Neutral);
        controls.TunnelPosition.Should().Be(24);
        controls.Activated.Should().BeFalse();
        controls.DoorIsClosed.Should().BeFalse();

        var output = await target.GetResponse("look");
        output.Should().Contain(
            "This is a small control cabin. A control panel contains a slot, a lever, and a display. The lever can be set at a central position, or it could be pushed up to a position labelled \"+\", or pulled down to a position labelled \"-\". It is currently at the center setting. The display, a digital readout, currently reads 0. Through the cabin window you can see a featureless concrete wall");
        Console.Write(output);

        output = await target.GetResponse("pull lever");
        output.Should().Contain("not currently activated");
        Console.Write(output);

        output = await target.GetResponse("W");
        Console.Write(output);

        output = await target.GetResponse("N");
        Console.Write(output);
        output.Should().Contain("Lawanda Platform");

        output = await target.GetResponse("look");
        output.Should().Contain(
            "This is a wide, flat strip of concrete. Open shuttle cars lie to the north and south. A wide escalator, not currently operating, beckons upward at the east end of the platform. A faded sign reads \"Shutul Platform -- Lawanda Staashun.\"");
        Console.Write(output);
    }

    [Test]
    public async Task FullTrip_ConstantSpeed_PerfectLanding()
    {
        var target = GetTarget();
        Take<ShuttleAccessCard>();
        StartHere<AlfieControlEast>();
        var controls = Repository.GetLocation<AlfieControlEast>();

        await target.GetResponse("slide shuttle access card through slot");

        controls.Speed.Should().Be(0);
        controls.LeverPosition.Should().Be(ShuttleLeverPosition.Neutral);
        controls.TunnelPosition.Should().Be(0);
        controls.Activated.Should().BeTrue();
        controls.DoorIsClosed.Should().BeFalse();

        await target.GetResponse("push lever");

        controls.Speed.Should().Be(5);
        controls.LeverPosition.Should().Be(ShuttleLeverPosition.Acceleration);
        controls.TunnelPosition.Should().Be(1);
        controls.Activated.Should().BeTrue();
        controls.DoorIsClosed.Should().BeTrue();

        await target.GetResponse("pull lever");

        controls.Speed.Should().Be(5);
        controls.LeverPosition.Should().Be(ShuttleLeverPosition.Neutral);
        controls.TunnelPosition.Should().Be(2);
        controls.Activated.Should().BeTrue();
        controls.DoorIsClosed.Should().BeTrue();

        string response;
        for (var i = 3; i < 25; i++)
        {
            response = await target.GetResponse("wait") ?? "";
            Console.Write(response);
            Console.WriteLine(Repository.GetLocation<AlfieControlEast>().TunnelPosition);
        }

        response = await target.GetResponse("pull lever") ?? "";
        Console.Write(response);
        response.Should()
            .Contain(
                "The shuttle car glides into the station and comes to rest at the concrete platform. You hear the cabin doors slide open");

        controls.Speed.Should().Be(0);
        controls.LeverPosition.Should().Be(ShuttleLeverPosition.Neutral);
        controls.TunnelPosition.Should().Be(24);
        controls.Activated.Should().BeFalse();
        controls.DoorIsClosed.Should().BeFalse();

        var output = await target.GetResponse("look");
        output.Should().Contain(
            "This is a small control cabin. A control panel contains a slot, a lever, and a display. The lever can be set at a central position, or it could be pushed up to a position labelled \"+\", or pulled down to a position labelled \"-\". It is currently at the center setting. The display, a digital readout, currently reads 0. Through the cabin window you can see a featureless concrete wall");
        Console.Write(output);

        output = await target.GetResponse("pull lever");
        output.Should().Contain("not currently activated");
        Console.Write(output);

        output = await target.GetResponse("W");
        Console.Write(output);

        output = await target.GetResponse("N");
        Console.Write(output);
        output.Should().Contain("Lawanda Platform");

        output = await target.GetResponse("look");
        output.Should().Contain(
            "This is a wide, flat strip of concrete. Open shuttle cars lie to the north and south. A wide escalator, not currently operating, beckons upward at the east end of the platform. A faded sign reads \"Shutul Platform -- Lawanda Staashun.\"");
        Console.Write(output);
    }

    [Test]
    public async Task RoundTrip()
    {
        var target = GetTarget();
        Take<ShuttleAccessCard>();
        StartHere<AlfieControlEast>();
        
        Repository.GetItem<Chronometer>().CurrentTime = 5000;

        await target.GetResponse("slide shuttle access card through slot");
        await target.GetResponse("push lever");
        await target.GetResponse("pull lever");

        for (var i = 3; i < 25; i++) await target.GetResponse("wait");

        await target.GetResponse("pull lever");
        await target.GetResponse("look");
        await target.GetResponse("W");

        var output = await target.GetResponse("N");
        output.Should().Contain("Lawanda Platform");

        output = await target.GetResponse("look");
        output.Should().Contain(
            "This is a wide, flat strip of concrete. Open shuttle cars lie to the north and south. A wide escalator, not currently operating, beckons upward at the east end of the platform. A faded sign reads \"Shutul Platform -- Lawanda Staashun.\"");

        output = await target.GetResponse("N");
        output.Should().Contain("Shuttle Car Betty");
        Console.Write(output);

        output = await target.GetResponse("W");
        output.Should().Contain("Betty Control West");
        output.Should().Contain("vanishing in the distance");
        Console.Write(output);
        
        Repository.GetItem<Chronometer>().CurrentTime = 5000;

        await target.GetResponse("slide shuttle access card through slot");
        await target.GetResponse("push lever");
        await target.GetResponse("pull lever");

        for (var i = 3; i < 25; i++) Console.WriteLine(await target.GetResponse("wait"));

        output = await target.GetResponse("pull lever");
        Console.Write(output);
        output.Should()
            .Contain(
                "The shuttle car glides into the station and comes to rest at the concrete platform. You hear the cabin doors slide open");
        
        output = await target.GetResponse("look");
        output.Should().Contain(
            "This is a small control cabin. A control panel contains a slot, a lever, and a display. The lever can be set at a central position, or it could be pushed up to a position labelled \"+\", or pulled down to a position labelled \"-\". It is currently at the center setting. The display, a digital readout, currently reads 0. Through the cabin window you can see a featureless concrete wall");
        Console.Write(output);

        output = await target.GetResponse("E");
        Console.Write(output);
        output.Should().Contain("Shuttle Car Betty");
        
        output = await target.GetResponse("N");
        Console.Write(output);
        output.Should().Contain("An open shuttle car lies to the north.");
        output.Should().Contain("Kalamontee");
    }
    
    [Test]
    public async Task Activate_DuringEveningHours_RequiresAuthorization()
    {
        var target = GetTarget();
        Take<ShuttleAccessCard>();
        StartHere<AlfieControlEast>();
        
        Repository.GetItem<Chronometer>().CurrentTime = 6500;
        
        var response = await target.GetResponse("slide shuttle access card through slot");
        
        response.Should().Contain("A recorded voice explains that using the shuttle car during the evening hours requires special authorization.");
        
        GetLocation<AlfieControlEast>().Activated.Should().BeFalse();
    }
}

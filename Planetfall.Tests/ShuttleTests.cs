using FluentAssertions;
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
}
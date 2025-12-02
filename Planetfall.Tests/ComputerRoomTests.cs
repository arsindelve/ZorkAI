using FluentAssertions;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Location.Lawanda;

namespace Planetfall.Tests;

public class ComputerRoomTests : EngineTestsBase
{
    [Test]
    public async Task Floyd_IsPresent_AndTurnedOn_ShouldExpressConcern()
    {
        var target = GetTarget();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        var computerRoom = StartHere<ComputerRoom>();
        computerRoom.ItemPlacedHere(floyd);
        target.Context.RegisterActor(computerRoom);

        var response = await target.GetResponse("wait");

        response.Should().Contain(FloydConstants.ComputerBroken);
    }

    [Test]
    public async Task Floyd_IsNotTurnedOn_ShouldNotExpressConcern()
    {
        var target = GetTarget();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = false;
        var computerRoom = StartHere<ComputerRoom>();
        computerRoom.ItemPlacedHere(floyd);
        target.Context.RegisterActor(computerRoom);

        var response = await target.GetResponse("wait");

        response.Should().NotContain(FloydConstants.ComputerBroken);
    }

    [Test]
    public async Task Floyd_ExpressesConcern_OnlyOnce()
    {
        var target = GetTarget();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        var computerRoom = StartHere<ComputerRoom>();
        computerRoom.ItemPlacedHere(floyd);
        target.Context.RegisterActor(computerRoom);

        var response1 = await target.GetResponse("wait");
        response1.Should().Contain(FloydConstants.ComputerBroken);

        var response2 = await target.GetResponse("wait");
        response2.Should().NotContain(FloydConstants.ComputerBroken);
    }

    [Test]
    public async Task Floyd_NotInRoom_ShouldNotExpressConcern()
    {
        var target = GetTarget();
        GetItem<Floyd>().IsOn = true;
        var computerRoom = StartHere<ComputerRoom>();
        target.Context.RegisterActor(computerRoom);

        var response = await target.GetResponse("wait");

        response.Should().NotContain(FloydConstants.ComputerBroken);
    }

}

using FluentAssertions;
using GameEngine;
using ZorkOne.Location;
using ZorkOne.Location.MazeLocation;

namespace ZorkOne.Tests.Places;

/// <summary>
/// Regression guard for #284. The fix routes nameless "say …" speech to a present talkable NPC, but
/// it is presence-gated and Zork has no talkable NPCs, so the engine's "say &lt;magic word&gt;"
/// teleport puzzles must keep working untouched. These pin the two Temple/Treasure-Room teleports
/// (the LoudRoom "echo" puzzle is covered by LoudRoomTests).
/// </summary>
[TestFixture]
public class TempleMagicWordTests : EngineTestsBase
{
    [Test]
    public async Task SayTreasure_InTemple_TeleportsToTreasureRoom()
    {
        var target = GetTarget();
        StartHere<Temple>();

        var response = await target.GetResponse("say treasure");

        target.Context.CurrentLocation.Should().BeOfType<TreasureRoom>();
        response.Should().Contain("Treasure Room");
    }

    [Test]
    public async Task SayTemple_InTreasureRoom_TeleportsToTemple()
    {
        var target = GetTarget();
        StartHere<TreasureRoom>();

        var response = await target.GetResponse("say temple");

        target.Context.CurrentLocation.Should().BeOfType<Temple>();
        response.Should().Contain("Temple");
    }
}

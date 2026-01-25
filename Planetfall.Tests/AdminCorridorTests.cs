using FluentAssertions;
using Planetfall.Item.Kalamontee;
using Planetfall.Location.Kalamontee.Admin;

namespace Planetfall.Tests;

public class AdminCorridorTests : EngineTestsBase
{
    [Test]
    public async Task UseLadderOnRift_Extended_SpansRift()
    {
        var target = GetTarget();
        StartHere<AdminCorridor>();
        var ladder = GetItem<Ladder>();
        ladder.IsExtended = true;
        GetLocation<AdminCorridor>().ItemPlacedHere(ladder);

        var response = await target.GetResponse("use ladder on rift");

        response.Should().Contain("swings out across the rift");
        response.Should().Contain("spanning the precipice");
        ladder.IsAcrossRift.Should().BeTrue();
    }

    [Test]
    public async Task UseLadderOnChasm_Extended_SpansRift()
    {
        var target = GetTarget();
        StartHere<AdminCorridor>();
        var ladder = GetItem<Ladder>();
        ladder.IsExtended = true;
        GetLocation<AdminCorridor>().ItemPlacedHere(ladder);

        var response = await target.GetResponse("use ladder on chasm");

        response.Should().Contain("swings out across the rift");
        ladder.IsAcrossRift.Should().BeTrue();
    }

    [Test]
    public async Task UseLadderOnRift_NotExtended_FallsIntoRift()
    {
        var target = GetTarget();
        StartHere<AdminCorridor>();
        var ladder = GetItem<Ladder>();
        ladder.IsExtended = false;
        GetLocation<AdminCorridor>().ItemPlacedHere(ladder);

        var response = await target.GetResponse("use ladder on rift");

        response.Should().Contain("far too short");
        response.Should().Contain("plunges into the rift");
        response.Should().Contain("lost forever");
        ladder.CurrentLocation.Should().BeNull();
    }

    [Test]
    public async Task PlaceLadderAcrossRift_StillWorks()
    {
        var target = GetTarget();
        StartHere<AdminCorridor>();
        var ladder = GetItem<Ladder>();
        ladder.IsExtended = true;
        GetLocation<AdminCorridor>().ItemPlacedHere(ladder);

        var response = await target.GetResponse("place ladder across rift");

        response.Should().Contain("swings out across the rift");
        ladder.IsAcrossRift.Should().BeTrue();
    }

    [Test]
    public async Task CanGoNorth_AfterLadderSpansRift()
    {
        var target = GetTarget();
        StartHere<AdminCorridor>();
        var ladder = GetItem<Ladder>();
        ladder.IsExtended = true;
        ladder.IsAcrossRift = true;

        var response = await target.GetResponse("n");

        response.Should().Contain("swaying ladder");
        Context.CurrentLocation.Should().BeOfType<AdminCorridorNorth>();
    }

    [Test]
    public async Task CannotGoNorth_WithoutLadder()
    {
        var target = GetTarget();
        StartHere<AdminCorridor>();

        var response = await target.GetResponse("n");

        response.Should().Contain("too wide to jump");
        Context.CurrentLocation.Should().BeOfType<AdminCorridor>();
    }
}

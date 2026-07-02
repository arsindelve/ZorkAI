using FluentAssertions;
using Planetfall.Item.Lawanda;
using Planetfall.Location.Lawanda;

namespace Planetfall.Tests;

public class RepairRoomTests : EngineTestsBase
{
    [Test]
    public async Task ExamineRobot_ReturnsAchillesSpecificText_NotGenericFallback()
    {
        var target = GetTarget();
        StartHere<RepairRoom>();

        var response = await target.GetResponse("examine robot");

        response.Should().Contain("Achilles");
        response.Should().NotContain("nothing special");
    }

    [Test]
    public async Task ExamineDamagedRobot_ReturnsAchillesSpecificText()
    {
        var target = GetTarget();
        StartHere<RepairRoom>();

        var response = await target.GetResponse("examine damaged robot");

        response.Should().Contain("Achilles");
        response.Should().NotContain("nothing special");
    }

    [Test]
    public async Task ExamineBrokenRobot_ReturnsAchillesSpecificText()
    {
        var target = GetTarget();
        StartHere<RepairRoom>();

        var response = await target.GetResponse("examine broken robot");

        response.Should().Contain("Achilles");
        response.Should().NotContain("nothing special");
    }
}

using FluentAssertions;
using Planetfall.Item.Feinstein;
using Planetfall.Location.Kalamontee.Mech;

namespace Planetfall.Tests;

public class SlimeTests : EngineTestsBase
{
    // Verbs.TouchVerbs: the slime is touched in the same words as anything else.
    [TestCase("touch slime")]
    [TestCase("feel slime")]
    [TestCase("rub slime")]
    [TestCase("poke slime")]
    public async Task TouchingTheSlime_FeelsLikeSlime_HoweverThePlayerSaysIt(string input)
    {
        var target = GetTarget();
        // Not Deck Nine, where the ambassador wanders in at random.
        StartHere<ToolRoom>().ItemPlacedHere(GetItem<Slime>());

        var response = await target.GetResponse(input);

        response.Should().Contain("It feels like slime");
    }
}

using FluentAssertions;
using Planetfall.Item.Kalamontee;

namespace Planetfall.Tests;

public class TinCanTests : EngineTestsBase
{
    // Verbs.OpenItemVerbs: every way of trying to get a shut thing open gets the same refusal.
    [TestCase("open can")]
    [TestCase("pry can")]
    [TestCase("force can")]
    [TestCase("unseal can")]
    public async Task TheCan_NeedsACanOpener_HoweverOpeningItIsTried(string input)
    {
        var target = GetTarget();
        target.Context.ItemPlacedHere<TinCan>();

        var response = await target.GetResponse(input);

        response.Should().Contain("can opener");
    }
}

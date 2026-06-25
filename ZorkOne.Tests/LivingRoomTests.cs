using FluentAssertions;
using GameEngine;
using Model.Intent;
using ZorkOne.Location;

namespace ZorkOne.Tests;

/// <summary>
///     Issue #317: the Living Room description calls the inscription above the west door
///     "strange gothic lettering". When a player echoes that wording — e.g. "read gothic lettering"
///     — the AI parser folds the adjective into the noun (Noun = "gothic lettering"), so the bare
///     "lettering" entry in the handler's noun list never matched and the command silently failed.
///     The fix adds the adjective forms to the noun list so the player's most natural action works.
/// </summary>
[TestFixture]
public class LivingRoomTests : EngineTestsBase
{
    private const string Expected = "This space intentionally left blank";

    // Drives the location handler directly with the compound noun the AI parser actually produces,
    // since the unit-test parser would never assemble "gothic lettering" from these words.
    private async Task<string?> ReadLettering(string verb, string noun)
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>();

        var result = await Repository.GetLocation<LivingRoom>().RespondToSimpleInteraction(
            new SimpleIntent { Verb = verb, Noun = noun, OriginalInput = $"{verb} {noun}" },
            target.Context, Client.Object, null!);

        return result?.InteractionMessage;
    }

    [TestCase("read", "gothic lettering")]
    [TestCase("examine", "gothic lettering")]
    [TestCase("read", "strange gothic lettering")]
    public async Task ReadGothicLettering_Translates(string verb, string noun)
    {
        var message = await ReadLettering(verb, noun);

        message.Should().Contain(Expected);
    }

    // Regression: the bare wording must still work.
    [TestCase("read", "lettering")]
    [TestCase("examine", "engravings")]
    public async Task ReadBareLettering_StillTranslates(string verb, string noun)
    {
        var message = await ReadLettering(verb, noun);

        message.Should().Contain(Expected);
    }
}

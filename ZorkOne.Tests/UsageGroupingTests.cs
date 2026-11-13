using FluentAssertions;
using GameEngine;
using ZorkOne.Item;
using ZorkOne.Location;
using ZorkOne.Location.ForestLocation;

namespace ZorkOne.Tests;

/// <summary>
///     Objects that share a usage share a verb grouping (Model/Verbs.cs), so an object answers every
///     word of its grouping - not only the words its own handler was first written with. Each case runs
///     an established wording and another word from the same grouping on fresh games and requires the
///     same reply. Through the production grammar.
/// </summary>
public class UsageGroupingTests : EngineTestsBase
{
    private async Task<string?> ReplyTo(Action<GameEngine<ZorkI, ZorkIContext>> arrange, string[] lead,
        string command)
    {
        var target = GetTarget(Walkthrough.DeterministicFirst.Parser());
        arrange(target);

        foreach (var step in lead)
            await target.GetResponse(step);

        return await target.GetResponse(command);
    }

    private async Task SameReply(Action<GameEngine<ZorkI, ZorkIContext>> arrange, string[] lead,
        string established, string inOtherWords)
    {
        var expected = await ReplyTo(arrange, lead, established);
        var actual = await ReplyTo(arrange, lead, inOtherWords);

        expected.Should().NotBeNullOrWhiteSpace($"\"{established}\" is the wording the handler already answers");
        actual.Should().Be(expected, $"\"{inOtherWords}\" is the same action as \"{established}\"");
    }

    // Verbs.MoveAsideVerbs with Verbs.SearchVerbs: shifting the leaves, or going through them, shows
    // what is under them - the same as shifting the rug.
    [TestCase("slide leaves")]
    [TestCase("shift leaves")]
    [TestCase("search leaves")]
    [TestCase("check leaves")]
    public async Task DisturbingTheLeaves(string inOtherWords)
    {
        await SameReply(target => target.Context.CurrentLocation = Repository.GetLocation<Clearing>(), [],
            "move leaves", inOtherWords);
    }

    [TestCase("shift rug")]
    [TestCase("slide rug")]
    public async Task MovingTheRug(string inOtherWords)
    {
        await SameReply(target => target.Context.CurrentLocation = Repository.GetLocation<LivingRoom>(), [],
            "move rug", inOtherWords);
    }

    // Verbs.ForceOpenVerbs is built on Verbs.OpenItemVerbs.
    [TestCase("pry egg")]
    [TestCase("unseal egg")]
    public async Task ForcingTheEgg(string inOtherWords)
    {
        await SameReply(target => target.Context.CurrentLocation = Repository.GetLocation<UpATree>(),
            ["take egg"], "open egg", inOtherWords);
    }
}

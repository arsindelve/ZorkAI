using FluentAssertions;
using Model.Intent;
using Planetfall.Item.Feinstein;
using Planetfall.Item.Kalamontee;
using Planetfall.Item.Kalamontee.Mech;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Location.Feinstein;
using Planetfall.Location.Kalamontee;
using Planetfall.Location.Kalamontee.Admin;
using Planetfall.Location.Kalamontee.Mech;

namespace Planetfall.Tests;

/// <summary>
///     Objects that share a usage share a verb grouping (Model/Verbs.cs), so an object answers every
///     word of its grouping - not only the words its own handler was first written with. Each case runs
///     an established wording and another word from the same grouping on fresh games and requires the
///     same reply.
///     <para>
///         Through the production grammar, because the unit-test parser builds a two-noun intent only
///         for the sentences in its static table.
///     </para>
/// </summary>
public class UsageGroupingTests : EngineTestsBase
{
    private async Task<string?> ReplyTo(Action arrange, string[] lead, string command)
    {
        var target = GetTarget(Walkthrough.DeterministicFirst.Parser());
        arrange();

        foreach (var step in lead)
            await target.GetResponse(step);

        return await target.GetResponse(command);
    }

    private async Task SameReply(Action arrange, string[] lead, string established, string inOtherWords)
    {
        var expected = await ReplyTo(arrange, lead, established);
        var actual = await ReplyTo(arrange, lead, inOtherWords);

        expected.Should().NotBeNullOrWhiteSpace($"\"{established}\" is the wording the handler already answers");
        actual.Should().Be(expected, $"\"{inOtherWords}\" is the same action as \"{established}\"");
    }

    // Verbs.EmptyVerbs: tipping a container out.
    [TestCase("spill flask")]
    [TestCase("dump flask")]
    public async Task EmptyingTheFlask(string inOtherWords)
    {
        await SameReply(() =>
        {
            GetItem<Flask>().CurrentLocation = GetLocation<MachineShop>();
            Take<Flask>();
            StartHere<MachineShop>();
        }, ["put flask under spout", "press yellow button"], "empty flask", inOtherWords);
    }

    // Verbs.PutVerbs: putting a thing somewhere, under a spout or across a rift as much as into a box.
    [TestCase("shove flask under spout")]
    [TestCase("move flask under spout")]
    public async Task PuttingTheFlaskUnderTheSpout(string inOtherWords)
    {
        await SameReply(() =>
        {
            Take<Flask>();
            StartHere<MachineShop>();
        }, [], "put flask under spout", inOtherWords);
    }

    // Asked of the room directly: "across" is not a word the production grammar joins two nouns with,
    // so this sentence reaches the game through the AI parser, which the tests do not have.
    [TestCase("push")]
    [TestCase("shove")]
    [TestCase("move")]
    public async Task PuttingTheLadderAcrossTheRift(string inOtherWords)
    {
        async Task<string?> ReplyTo(string verb)
        {
            var target = GetTarget();
            var corridor = StartHere<AdminCorridor>();
            var ladder = GetItem<Ladder>();
            ladder.IsExtended = true;
            corridor.ItemPlacedHere(ladder);

            var result = await corridor.RespondToMultiNounInteraction(new MultiNounIntent
            {
                Verb = verb, NounOne = "ladder", NounTwo = "rift", Preposition = "across",
                OriginalInput = $"{verb} ladder across rift"
            }, target.Context);

            return result?.InteractionMessage;
        }

        var expected = await ReplyTo("place");

        expected.Should().Contain("swings out across the rift");
        (await ReplyTo(inOtherWords)).Should().Be(expected);
    }

    // Verbs.InsertVerbs: putting a thing into a slot. "push" and "slide" are deliberately not in it -
    // sliding a card through the slot is how the slot works, and must not get this refusal.
    [TestCase("stick upper elevator access card in slot")]
    [TestCase("feed upper elevator access card in slot")]
    public async Task PuttingACardInTheShallowSlot(string inOtherWords)
    {
        await SameReply(() => StartHere<UpperElevator>(), [],
            "put upper elevator access card in slot", inOtherWords);
    }

    // Verbs.SearchVerbs with Verbs.ExamineVerbs: the cubbyholes show what is in them either way.
    [TestCase("search cubbyholes")]
    [TestCase("inspect cubbyholes")]
    public async Task LookingInTheCubbyholes(string inOtherWords)
    {
        await SameReply(() => StartHere<PlanRoom>(), [], "examine cubbyholes", inOtherWords);
    }

    // Verbs.SearchVerbs with Verbs.OpenItemVerbs: for Floyd, opening him is searching him.
    [TestCase("open floyd")]
    [TestCase("force floyd")]
    [TestCase("check floyd")]
    public async Task SearchingFloyd(string inOtherWords)
    {
        await SameReply(() =>
        {
            StartHere<RobotShop>();
            GetItem<Floyd>().IsOn = true;
        }, [], "search floyd", inOtherWords);
    }

    // Verbs.CleanVerbs, and Verbs.CleanOffVerbs which is built on it.
    [TestCase("mop floor")]
    [TestCase("wipe floor")]
    public async Task CleaningTheDeck(string inOtherWords)
    {
        var response = await ReplyTo(() => StartHere<DeckNine>(), [], inOtherWords);

        response.Should().Contain("The floor is a bit shinier now");
    }

    [TestCase("clean slime", "wash slime")]
    [TestCase("clean slime", "remove slime")]
    [TestCase("taste slime", "lick slime")]
    [TestCase("taste slime", "swallow slime")]
    public async Task TheSlime(string established, string inOtherWords)
    {
        // Not Deck Nine, where the ambassador wanders in at random.
        await SameReply(() => StartHere<ToolRoom>().ItemPlacedHere(GetItem<Slime>()), [], established,
            inOtherWords);
    }
}

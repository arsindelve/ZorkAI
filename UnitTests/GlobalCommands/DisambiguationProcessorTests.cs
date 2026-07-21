using FluentAssertions;
using GameEngine.StaticCommand.Implementation;
using Model;
using Model.AIGeneration;
using Model.Interaction;
using Model.Interface;
using Moq;

namespace UnitTests.GlobalCommands;

/// <summary>
///     The processor takes the player's answer to a "which one do you mean?" prompt and rewrites it into a
///     full command. It matches answers by SUBSTRING, so several of a prompt's choices can match one reply
///     and the tie-breaking rules decide which button the player actually gets.
/// </summary>
public class DisambiguationProcessorTests
{
    private static Task<string> Answer(Dictionary<string, string> choices, string reply)
    {
        var result = new DisambiguationInteractionResult("Which one do you mean?", choices, "press the {0}");
        return new DisambiguationProcessor(result)
            .Process(reply, Mock.Of<IContext>(), Mock.Of<IGenerationClient>(), Runtime.Console);
    }

    [Test]
    public async Task LongerMatchWins_BecauseItIsTheMoreSpecificAnswer()
    {
        var choices = new Dictionary<string, string>
        {
            { "red", "red button" },
            { "red elevator button", "red elevator button" }
        };

        var response = await Answer(choices, "the red elevator button");

        response.Should().Be("press the red elevator button");
    }

    // Booth 2 labels its two buttons "1" and "3", so "one" answers the brown button while "tan" answers
    // the other one -- and a reply of "the tan one" contains BOTH keys, at equal length. The player named
    // the tan button first, and that is the one they meant. Getting this wrong teleports them to the
    // wrong booth, so it is worth pinning.
    //
    // Note the residual hazard this does NOT solve: "one" is both Booth 2's label for a button and an
    // English pronoun, so a reply that puts the pronoun first ("the one that is tan") still resolves to
    // the wrong button. No tie-breaking rule can separate those two senses -- it needs the key gone, the
    // same lesson DestinationNavigation already learned when it stopped emitting single-character keys.
    [Test]
    public async Task EqualLengthMatches_AreBrokenByWhereThePlayerMentionedThem()
    {
        var choices = new Dictionary<string, string>
        {
            { "brown", "brown button" },
            { "tan", "tan button" },
            { "one", "brown button" },
            { "three", "tan button" }
        };

        var response = await Answer(choices, "the tan one");

        response.Should().Be("press the tan button");
    }

    // The tie-break used to fall through to whatever order the caller happened to list its choices in,
    // which a Dictionary does not promise and which no test held in place: re-sorting a prompt's choices
    // could silently change which button a player got. Declaration order must not be able to decide.
    [Test]
    public async Task DeclarationOrderDoesNotDecideTheWinner()
    {
        var oneOrder = new Dictionary<string, string>
        {
            { "round", "round button" },
            { "white", "white button" }
        };

        var reversed = new Dictionary<string, string>
        {
            { "white", "white button" },
            { "round", "round button" }
        };

        var first = await Answer(oneOrder, "the round white button");
        var second = await Answer(reversed, "the round white button");

        first.Should().Be("press the round button");
        second.Should().Be(first);
    }

    [Test]
    public async Task AnUnrecognizedReply_IsPassedThroughUntouched()
    {
        var choices = new Dictionary<string, string> { { "red", "red button" } };

        var response = await Answer(choices, "go north");

        response.Should().Be("go north");
    }

    [Test]
    public async Task AnEmptyReply_YieldsNoCommand()
    {
        var choices = new Dictionary<string, string> { { "red", "red button" } };

        var response = await Answer(choices, "");

        response.Should().BeEmpty();
    }
}

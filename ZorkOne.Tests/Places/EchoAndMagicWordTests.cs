using FluentAssertions;
using GameEngine;
using Model.AIGeneration.Requests;
using Moq;
using ZorkOne.Item;
using ZorkOne.Location;
using ZorkOne.Location.ForestLocation;
using ZorkOne.Location.MazeLocation;

namespace ZorkOne.Tests.Places;

/// <summary>
/// Rooms whose handler intercepts a spoken word before the normal command pipeline sees it: the Loud
/// Room's echo puzzle, the Temple/Treasure Room "say treasure"/"say temple" teleports, and Inside the
/// Barrow's unconditional ending catch-all. Merged from the former LoudRoomTests /
/// TempleMagicWordTests / InsideTheBarrowTests, which already cross-referenced each other — the two
/// #354 free-command-word cases below are deliberate mirrors and belong side by side.
///
/// The Temple cases also guard #284: the fix routes nameless "say …" speech to a present talkable
/// NPC, but it is presence-gated and Zork has no talkable NPCs, so these teleports must keep working
/// untouched.
/// </summary>
[TestFixture]
public class EchoAndMagicWordTests : EngineTestsBase
{
    [Test]
    [TestCase("W")]
    [TestCase("walk w")]
    [TestCase("go west")]
    [TestCase("head 'west'")]
    [TestCase("move \"west\"")]
    public async Task LoudRoom_CanLeave(string command)
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<LoudRoom>();
        target.Context.ItemPlacedHere(Repository.GetItem<Torch>());

        var response = await target.GetResponse(command);
        Console.WriteLine(response);

        response.Should().Contain("Round Room");
    }

    [Test]
    public async Task LoudRoom_EchosLastWordsByDefault()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<LoudRoom>();

        var response = await target.GetResponse("hi");
        Console.WriteLine(response);

        response.Should().Contain("hi hi ...");
    }

    [Test]
    public async Task LoudRoom_EchoingAFreeCommandWord_StillCountsAsATurn()
    {
        // Issue #354 follow-up: the room's echo catch-all fires for ANY non-direction word,
        // including ones that happen to be global free-command names ("score"). Since the room -
        // not ScoreProcessor - actually produced the response, it must still count as a real turn
        // (Moves increments), exactly like the "hi" control case above.
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<LoudRoom>();

        var response = await target.GetResponse("score");
        Console.WriteLine(response);

        response.Should().Contain("score score ...");
        target.Moves.Should().Be(1);
    }

    [Test]
    public async Task LoudRoom_CanStillQuit()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<LoudRoom>();

        var response = await target.GetResponse("quit");
        Console.WriteLine(response);

        response.Should().Contain("Do you wish");
    }

    [Test]
    public async Task LoudRoom_EchosLastWorkByDefault_MultiPhrase()
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<LoudRoom>();

        var response = await target.GetResponse("hi there buddy");
        Console.WriteLine(response);

        response.Should().Contain("buddy buddy ...");
    }

    [Test]
    [TestCase("echo")]
    [TestCase("\"echo\"")]
    [TestCase("'echo'")]
    [TestCase("say the word 'echo'")]
    [TestCase("shout echo")]
    [TestCase("yell the word \"echo\"")]
    [TestCase("scream 'echo'")]
    public async Task LoudRoom_SimplestSolution(string input)
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<LoudRoom>();

        var response = await target.GetResponse(input);
        Console.WriteLine(response);

        response.Should().Contain("The acoustics of the room");
        Repository.GetLocation<LoudRoom>().EchoHasBeenSpoken.Should().BeTrue();
    }

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

    [Test]
    public async Task InsideTheBarrow_EchoingAFreeCommandWord_StillCountsAsATurn()
    {
        // Issue #354 follow-up: InsideTheBarrow's RespondToSpecificLocationInteraction is an
        // unconditional catch-all (the "you won ZORK I" ending narration fires for ANY input,
        // including a literal free-command word like "score"), same shape as the LoudRoom case above.
        // It must still count as a real turn.
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<InsideTheBarrow>();

        Client.Setup(c => c.GenerateNarration(It.IsAny<Request>(), It.IsAny<string>()))
            .ReturnsAsync("You have won the game! ");

        var response = await target.GetResponse("score");

        response.Should().Contain("You have won the game!");
        target.Moves.Should().Be(1);
    }
}

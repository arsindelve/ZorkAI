using FluentAssertions;
using GameEngine;
using Model.AIGeneration.Requests;
using Moq;
using ZorkOne.Location.ForestLocation;

namespace ZorkOne.Tests.Places;

[TestFixture]
public class InsideTheBarrowTests : EngineTestsBase
{
    [Test]
    public async Task EchoingAFreeCommandWord_StillCountsAsATurn()
    {
        // Issue #354 follow-up: InsideTheBarrow's RespondToSpecificLocationInteraction is an
        // unconditional catch-all (the "you won ZORK I" ending narration fires for ANY input,
        // including a literal free-command word like "score"), same shape as LoudRoomTests'
        // equivalent case. It must still count as a real turn.
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<InsideTheBarrow>();

        Client.Setup(c => c.GenerateNarration(It.IsAny<Request>(), It.IsAny<string>()))
            .ReturnsAsync("You have won the game! ");

        var response = await target.GetResponse("score");

        response.Should().Contain("You have won the game!");
        target.Moves.Should().Be(1);
    }
}

using ChatLambda;
using GameEngine;
using Model.AIGeneration;
using Model.Interface;
using Moq;
using Planetfall;
using Planetfall.Item.Feinstein;
using Planetfall.Location.Feinstein;

namespace IntegrationTests;

/// <summary>
/// Reproduces the production bug where talking to Ensign Blather does not work.
///
/// All talkable characters are gated through <see cref="IParseConversation.ParseAsync"/>
/// (the Floyd lambda's "RewriteSecondPerson" assistant). For input like
/// "blather, you clean the floor" that classifier returns "no" (it only recognizes
/// imperative commands), so <see cref="ConversationHandler.CheckForConversation"/> drops
/// the input and it never reaches Blather. We force that exact condition with a mocked
/// classifier and assert the player's directly-addressed input still reaches Blather.
/// </summary>
[TestFixture]
[Explicit]
public class BlatherConversationRoutingTests
{
    [Test]
    public async Task TalkingToBlatherByName_WhenClassifierDoesNotRecognizeACommand_StillReachesBlather()
    {
        Repository.Reset();

        var context = new PlanetfallContext();
        var lobby = Repository.GetLocation<ReactorLobby>();
        context.CurrentLocation = lobby;

        var blather = Repository.GetItem<Blather>();
        lobby.ItemPlacedHere(blather);

        // Reproduce production: RewriteSecondPerson returns "no" for "blather, you clean the
        // floor" => the handler currently considers it "not conversational".
        var parseConversation = new Mock<IParseConversation>();
        parseConversation
            .Setup(x => x.ParseAsync(It.IsAny<string>()))
            .ReturnsAsync((false, ""));

        var generationClient = new Mock<IGenerationClient>();
        generationClient.SetupGet(x => x.IsDisabled).Returns(false);

        var handler = new ConversationHandler(null, parseConversation.Object, generationClient.Object);

        var result = await handler.CheckForConversation("blather, you clean the floor", context);

        // Before the fix: null (the input is dropped and never reaches Blather).
        // After the fix: Blather responds in character via the ChatWithBlather lambda.
        result.Should().NotBeNull(
            "input that directly addresses Blather by name must reach him even when the " +
            "second-person rewriter doesn't recognize a command");
        result.Should().Contain("Blather");
    }
}

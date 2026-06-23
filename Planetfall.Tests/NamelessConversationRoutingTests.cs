using ChatLambda;
using FluentAssertions;
using Moq;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Location.Kalamontee.Mech;

namespace Planetfall.Tests;

/// <summary>
/// Regression tests for #284. When exactly one talkable NPC is present, conversational input that
/// names no one — bare quoted speech (<c>"you are a fool"</c>) and an untargeted <c>say …</c> — must
/// route to that NPC in their own voice instead of falling through to the third-person narrator.
///
/// Floyd is used as the present talker because his AI backend is injectable (<see cref="IChatWithFloyd"/>),
/// so the routed call is deterministic. Blather and the Ambassador travel the same
/// ConversationHandler path; that routing logic is proven game-agnostically in
/// UnitTests/Engine/ConversationHandlerTests.cs. These tests pin the end-to-end GameEngine wiring.
/// </summary>
public class NamelessConversationRoutingTests : EngineTestsBase
{
    private static Mock<IChatWithFloyd> FloydAnswering(string expectedPrompt, string reply)
    {
        var mock = new Mock<IChatWithFloyd>();
        mock.Setup(s => s.AskFloydAsync(expectedPrompt))
            .ReturnsAsync(new CompanionResponse(reply, null));
        return mock;
    }

    [Test]
    public async Task QuotedSpeech_WithSolePresentFloyd_RoutesToFloydInFirstPerson()
    {
        var target = GetTarget();
        StartHere<RobotShop>(); // RobotShop.Init places Floyd here, so he is the sole present talker.
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.ChatWithFloyd = FloydAnswering("you are a fool", "\"That not nice thing to say!\" Floyd pouts.").Object;

        // The quotes are stripped; the words inside reach Floyd.
        var response = await target.GetResponse("\"you are a fool\"");

        response.Should().Contain("That not nice thing to say!");
    }

    [Test]
    public async Task UntargetedSay_WithSolePresentFloyd_RoutesToFloydInFirstPerson()
    {
        var target = GetTarget();
        StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        floyd.ChatWithFloyd = FloydAnswering("hello", "\"Oh boy! Hello hello hello!\" Floyd burbles.").Object;

        // The "say" verb is stripped; "hello" reaches Floyd.
        var response = await target.GetResponse("say hello");

        response.Should().Contain("Hello hello hello!");
    }

    [Test]
    public async Task NamelessNonSpeechCommand_WithSolePresentFloyd_IsNotRoutedToHim()
    {
        // A real command that names no talker and is not speech reaches the nameless-speech branch
        // and must fall through to normal parsing — it must NOT be routed to the present NPC. Any
        // routed input would surface this sentinel; ordinary parsing of an unrecognized noun won't.
        var target = GetTarget();
        StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        var chat = new Mock<IChatWithFloyd>();
        chat.Setup(s => s.AskFloydAsync(It.IsAny<string>()))
            .ReturnsAsync(new CompanionResponse("FLOYD-WAS-WRONGLY-ENGAGED", null));
        floyd.ChatWithFloyd = chat.Object;

        var response = await target.GetResponse("examine the workbench");

        response.Should().NotContain("FLOYD-WAS-WRONGLY-ENGAGED");
        chat.Verify(s => s.AskFloydAsync(It.IsAny<string>()), Times.Never);
    }
}

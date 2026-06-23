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
    public async Task RealCommand_WithFloydPresent_IsNotHijacked()
    {
        // A real command that names no talker and is not speech must still be parsed normally even
        // with Floyd present — the nameless route must not swallow ordinary play.
        var target = GetTarget();
        StartHere<RobotShop>();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        // If routing fired, AskFloydAsync would be invoked; we never set it up, so any call would be
        // an obvious mismatch. The assertion below is the real guard.
        floyd.ChatWithFloyd = new Mock<IChatWithFloyd>(MockBehavior.Strict).Object;

        var response = await target.GetResponse("examine floyd");

        // Examining Floyd describes him; it does not produce a routed conversation reply.
        response.Should().Contain("robot");
    }
}

using Microsoft.Extensions.Logging;
using Model.AIGeneration.Requests;
using Model.Hints;
using Model.Intent;
using Model.Movement;
using OpenAI.Chat;
using ZorkAI.OpenAI;

namespace UnitTests;

[TestFixture]
public class ChatGPTClientBehaviorTests
{
    [Test]
    public async Task GenerateNarration_WhenDisabled_ReturnsFallbackWithoutCallingOpenAI()
    {
        var completion = new Mock<IChatCompletionClient>();
        var target = new ChatGPTClient(null, completion.Object) { IsDisabled = true };

        var result = await target.GenerateNarration(new EmptyRequest(), string.Empty);

        result.Should().Be("This action or command has no effect on the game. ");
        completion.VerifyNoOtherCalls();
    }

    [Test]
    public async Task GenerateNarration_BuildsContextAndInvokesCallback()
    {
        IReadOnlyList<ChatMessage>? messages = null;
        ChatCompletionOptions? options = null;
        var completion = new Mock<IChatCompletionClient>();
        completion.Setup(c => c.CompleteChatAsync(It.IsAny<IReadOnlyList<ChatMessage>>(),
                It.IsAny<ChatCompletionOptions>()))
            .Callback<IReadOnlyList<ChatMessage>, ChatCompletionOptions>((m, o) =>
            {
                messages = m;
                options = o;
            })
            .ReturnsAsync("A dry narrator response.");
        var generated = false;
        var target = new ChatGPTClient(null, completion.Object)
        {
            SystemPrompt = "Narrate.",
            LastFiveInputOutputs = [("look", "A room.", false), ("north", "You move.", false)],
            OnGenerate = () => generated = true
        };

        var result = await target.GenerateNarration(new CannotGoThatWayRequest("Kitchen", "west"), " Stay terse.");

        result.Should().Be("A dry narrator response.");
        generated.Should().BeTrue();
        messages.Should().HaveCount(3);
        MessageText(messages![0]).Should().Contain("Narrate. Stay terse.");
        MessageText(messages[1]).Should().Contain("Input: north. Output: You move.")
            .And.Contain("Input: look. Output: A room.");
        options!.Temperature.Should().Be(0.4f);
    }

    [Test]
    public async Task GenerateNarration_WhenProviderIsSilent_ReturnsNarratorFallback()
    {
        var completion = CompletionReturning(string.Empty);
        var target = new ChatGPTClient(null, completion.Object);

        var result = await target.GenerateNarration(new EmptyRequest(), string.Empty);

        result.Should().Be("The narrator is silent. ");
    }

    [Test]
    public async Task GenerateCompanionSpeech_WhenCompanionFails_FallsBackAndNormalizesQuotes()
    {
        var narrator = CompletionReturning("“Careful,” Floyd says.");
        var companion = new Mock<IChatCompletionClient>();
        companion.Setup(c => c.CompleteChatAsync(It.IsAny<IReadOnlyList<ChatMessage>>(),
                It.IsAny<ChatCompletionOptions>()))
            .ThrowsAsync(new InvalidOperationException("model unavailable"));
        var target = new ChatGPTClient(Mock.Of<ILogger>(), narrator.Object, companion.Object);

        var result = await target.GenerateCompanionSpeech(new CompanionRequest("React.", "You are Floyd."));

        result.Should().Be("\"Careful,\" Floyd says.");
        companion.Verify(c => c.CompleteChatAsync(It.IsAny<IReadOnlyList<ChatMessage>>(),
            It.IsAny<ChatCompletionOptions>()), Times.Once);
        narrator.Verify(c => c.CompleteChatAsync(It.IsAny<IReadOnlyList<ChatMessage>>(),
            It.IsAny<ChatCompletionOptions>()), Times.Once);
    }

    private static Mock<IChatCompletionClient> CompletionReturning(string response)
    {
        var completion = new Mock<IChatCompletionClient>();
        completion.Setup(c => c.CompleteChatAsync(It.IsAny<IReadOnlyList<ChatMessage>>(),
                It.IsAny<ChatCompletionOptions>()))
            .ReturnsAsync(response);
        return completion;
    }

    private static string MessageText(ChatMessage message) => message.Content[0].Text;
}

[TestFixture]
public class PronounResolverBehaviorTests
{
    [Test]
    public async Task ResolvePronouns_SkipsProvider_WhenThereIsNoPronoun()
    {
        var completion = new Mock<IChatCompletionClient>();
        var target = new PronounResolver(null, completion.Object);

        var result = await target.ResolvePronouns("take lamp", "look", "A lamp is here.");

        result.Should().BeNull();
        completion.VerifyNoOtherCalls();
    }

    [Test]
    public async Task ResolvePronouns_SkipsProvider_WhenThereIsNoContext()
    {
        var completion = new Mock<IChatCompletionClient>();
        var target = new PronounResolver(null, completion.Object);

        var result = await target.ResolvePronouns("take it", null, "  ");

        result.Should().BeNull();
        completion.VerifyNoOtherCalls();
    }

    [Test]
    public async Task ResolvePronouns_BuildsContextAndCleansProviderResponse()
    {
        IReadOnlyList<ChatMessage>? messages = null;
        ChatCompletionOptions? options = null;
        var completion = new Mock<IChatCompletionClient>();
        completion.Setup(c => c.CompleteChatAsync(It.IsAny<IReadOnlyList<ChatMessage>>(),
                It.IsAny<ChatCompletionOptions>()))
            .Callback<IReadOnlyList<ChatMessage>, ChatCompletionOptions>((m, o) =>
            {
                messages = m;
                options = o;
            })
            .ReturnsAsync("  \"turn brass lamp on\"  ");
        var target = new PronounResolver(null, completion.Object);

        var result = await target.ResolvePronouns("turn it on", "take brass lamp", "Taken.");

        result.Should().Be("turn brass lamp on");
        MessageText(messages![1]).Should().Contain("Last player command: \"take brass lamp\"")
            .And.Contain("Game response: \"Taken.\"")
            .And.Contain("Current command: turn it on");
        options!.Temperature.Should().Be(0f);
    }

    [Test]
    public async Task ResolvePronouns_WhenProviderFails_ReturnsNull()
    {
        var completion = new Mock<IChatCompletionClient>();
        completion.Setup(c => c.CompleteChatAsync(It.IsAny<IReadOnlyList<ChatMessage>>(),
                It.IsAny<ChatCompletionOptions>()))
            .ThrowsAsync(new InvalidOperationException("offline"));
        var target = new PronounResolver(Mock.Of<ILogger>(), completion.Object);

        var result = await target.ResolvePronouns("open it", "examine door", "It is locked.");

        result.Should().BeNull();
    }

    private static string MessageText(ChatMessage message) => message.Content[0].Text;
}

[TestFixture]
public class OpenAIParsingBehaviorTests
{
    [Test]
    public async Task Parser_ConvertsTaggedCompletionToIntent()
    {
        var completion = CompletionReturning("<intent>move</intent><direction>north</direction>");
        var target = new OpenAIParser(null, completion.Object);

        var result = await target.AskTheAIParser("head north", "A corridor", "session");

        result.Should().BeOfType<MoveIntent>().Which.Direction.Should().Be(Direction.N);
    }

    [Test]
    public async Task TakeAndDropParser_DeserializesItemsAndBuildsTheCorrectPrompt()
    {
        IReadOnlyList<ChatMessage>? messages = null;
        var completion = new Mock<IChatCompletionClient>();
        completion.Setup(c => c.CompleteChatAsync(It.IsAny<IReadOnlyList<ChatMessage>>(),
                It.IsAny<ChatCompletionOptions>()))
            .Callback<IReadOnlyList<ChatMessage>, ChatCompletionOptions>((m, _) => messages = m)
            .ReturnsAsync("{\"items\":[\"brass lantern\",\"sword\"]}");
        var target = new OpenAITakeAndDropListParser(null, completion.Object);

        var result = await target.GetListOfItemsToTake("grab both", "A brass lantern and sword are here.");

        result.Should().Equal("brass lantern", "sword");
        messages.Should().ContainSingle();
        messages![0].Content[0].Text.Should().Contain("A brass lantern and sword are here.")
            .And.Contain("grab both");
    }

    [Test]
    public async Task TakeAndDropParser_WhenItemsAreMissing_ReturnsEmptyList()
    {
        var target = new OpenAITakeAndDropListParser(null, CompletionReturning("{}").Object);

        var result = await target.GetListOfItemsToDrop("drop nothing", "Empty");

        result.Should().BeEmpty();
    }

    private static Mock<IChatCompletionClient> CompletionReturning(string response)
    {
        var completion = new Mock<IChatCompletionClient>();
        completion.Setup(c => c.CompleteChatAsync(It.IsAny<IReadOnlyList<ChatMessage>>(),
                It.IsAny<ChatCompletionOptions>()))
            .ReturnsAsync(response);
        return completion;
    }
}

[TestFixture]
public class OpenAiHintLanguageModelBehaviorTests
{
    [Test]
    public async Task Solve_IncludesKnowledgeContextHistoryAndQuestion()
    {
        IReadOnlyList<ChatMessage>? messages = null;
        var completion = new Mock<IChatCompletionClient>();
        completion.Setup(c => c.CompleteChatAsync(It.IsAny<IReadOnlyList<ChatMessage>>(),
                It.IsAny<ChatCompletionOptions>()))
            .Callback<IReadOnlyList<ChatMessage>, ChatCompletionOptions>((m, _) => messages = m)
            .ReturnsAsync("Use the ladder.");
        var target = new OpenAiHintLanguageModel(null, completion.Object);

        var result = await target.Solve("ladder docs", "at the rift",
            [new HintExchange("Where next?", "Look around.")], "How do I cross?", new HintPersona("Be dry."));

        result.Should().Be("Use the ladder.");
        messages![1].Content[0].Text.Should().Contain("ladder docs").And.Contain("at the rift")
            .And.Contain("Where next?").And.Contain("How do I cross?");
    }

    [Test]
    public async Task Reveal_WhenProviderFails_ReturnsCompleteSolution()
    {
        var completion = new Mock<IChatCompletionClient>();
        completion.Setup(c => c.CompleteChatAsync(It.IsAny<IReadOnlyList<ChatMessage>>(),
                It.IsAny<ChatCompletionOptions>()))
            .ThrowsAsync(new InvalidOperationException("offline"));
        var target = new OpenAiHintLanguageModel(Mock.Of<ILogger>(), completion.Object);

        var result = await target.Reveal("at the rift", "Use the ladder", [], "More?", new HintPersona("Be dry."));

        result.Should().Be("Use the ladder");
    }
}

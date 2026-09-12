using FluentAssertions;
using OpenAI.Chat;
using ZorkAI.OpenAI;

namespace UnitTests.SelfHosted;

/// <summary>
///     How the OpenAI client base reacts to broken endpoint configuration (issue #383).
///     <para>
///     The distinction under test: a client that <i>must</i> work fails loudly, and a client whose
///     contract is "degrade quietly when AI is unavailable" (<c>PronounResolver</c>, the hint model)
///     stays constructible. Resolving the settings in a field initializer broke the second group,
///     because a field initializer runs for every construction — including the injected-test-double
///     path — before the constructor can decide whether the failure matters.
///     </para>
/// </summary>
public class OpenAIClientBaseResilienceTests
{
    private string? _originalBaseUrl;
    private string? _originalKey;
    private string? _originalProvider;

    [SetUp]
    public void SetUp()
    {
        _originalProvider = Environment.GetEnvironmentVariable("ZORKAI_PROVIDER");
        _originalBaseUrl = Environment.GetEnvironmentVariable("OPENAI_BASE_URL");
        _originalKey = Environment.GetEnvironmentVariable("OPEN_AI_KEY");
    }

    [TearDown]
    public void TearDown()
    {
        Environment.SetEnvironmentVariable("ZORKAI_PROVIDER", _originalProvider);
        Environment.SetEnvironmentVariable("OPENAI_BASE_URL", _originalBaseUrl);
        Environment.SetEnvironmentVariable("OPEN_AI_KEY", _originalKey);
    }

    [Test]
    public void Should_StillConstructAnOptionalClient_When_TheProviderNameIsBogus()
    {
        Environment.SetEnvironmentVariable("ZORKAI_PROVIDER", "not-a-real-provider");

        var constructing = () => new PronounResolver();

        // Previously threw InvalidOperationException out of the field initializer, turning a typo in
        // an environment variable into a crash inside a feature that is meant to be optional.
        constructing.Should().NotThrow();
    }

    [Test]
    public void Should_StillConstructAnOptionalClient_When_TheBaseUrlIsNotAUrl()
    {
        Environment.SetEnvironmentVariable("OPENAI_BASE_URL", "not a url at all");

        var constructing = () => new PronounResolver();

        constructing.Should().NotThrow();
    }

    [Test]
    public void Should_StillHonorAnInjectedClient_When_TheEnvironmentIsBroken()
    {
        Environment.SetEnvironmentVariable("ZORKAI_PROVIDER", "not-a-real-provider");

        // A test double must never be able to fail on the host's configuration: the injected-seam
        // branch is taken before the environment is read at all.
        var constructing = () => new PronounResolver(null, new StubChatCompletionClient());

        constructing.Should().NotThrow();
    }

    [Test]
    public void Should_ThrowForARequiredClient_When_TheProviderNameIsBogus()
    {
        Environment.SetEnvironmentVariable("ZORKAI_PROVIDER", "not-a-real-provider");
        Environment.SetEnvironmentVariable("OPEN_AI_KEY", "sk-not-a-real-key");

        // The other half of the contract: a client the game cannot run without must still fail loudly
        // rather than silently becoming a no-op narrator.
        var constructing = () => new ChatGPTClient(null);

        constructing.Should().Throw<Exception>();
    }

    private sealed class StubChatCompletionClient : IChatCompletionClient
    {
        public Task<string> CompleteChatAsync(IReadOnlyList<ChatMessage> messages, ChatCompletionOptions options)
        {
            return Task.FromResult(string.Empty);
        }
    }
}

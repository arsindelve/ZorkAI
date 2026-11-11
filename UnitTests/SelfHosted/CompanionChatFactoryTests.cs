using FluentAssertions;
using GameEngine;
using Planetfall.AI;

namespace UnitTests.SelfHosted;

/// <summary>
///     Which backend Floyd, Blather and the Ambassador talk through: the cloud LangGraph Lambda, or a
///     local model (issue #383).
///     <para>
///     The point of these tests is the production guarantee. The choice is between invoking an AWS
///     Lambda and having no AWS at all, so it must hang on the explicit ZORKAI_SELF_HOSTED opt-in and
///     never on OPENAI_BASE_URL — a deployed function pointed at an OpenAI gateway must keep using
///     the LangGraph function, or Floyd loses the response metadata his Repair Room actions need.
///     </para>
/// </summary>
public class CompanionChatFactoryTests
{
    private string? _originalBaseUrl;
    private string? _originalProvider;
    private string? _originalSelfHosted;

    [SetUp]
    public void SetUp()
    {
        _originalBaseUrl = Environment.GetEnvironmentVariable("OPENAI_BASE_URL");
        _originalProvider = Environment.GetEnvironmentVariable("ZORKAI_PROVIDER");
        _originalSelfHosted = Environment.GetEnvironmentVariable(SelfHostedMode.EnvironmentVariableName);
    }

    [TearDown]
    public void TearDown()
    {
        Environment.SetEnvironmentVariable("OPENAI_BASE_URL", _originalBaseUrl);
        Environment.SetEnvironmentVariable("ZORKAI_PROVIDER", _originalProvider);
        Environment.SetEnvironmentVariable(SelfHostedMode.EnvironmentVariableName, _originalSelfHosted);
    }

    [Test]
    public void Should_UseTheLangGraphLambda_When_NothingIsConfigured()
    {
        Environment.SetEnvironmentVariable(SelfHostedMode.EnvironmentVariableName, null);
        Environment.SetEnvironmentVariable("OPENAI_BASE_URL", null);

        CompanionChatFactory.Floyd("prompt").Should().BeOfType<ChatWithFloyd>();
        CompanionChatFactory.Blather("prompt").Should().BeOfType<ChatWithBlather>();
        CompanionChatFactory.Ambassador("prompt").Should().BeOfType<ChatWithAmbassador>();
    }

    [Test]
    public void Should_StillUseTheLangGraphLambda_When_OnlyTheAiEndpointIsCustom()
    {
        // The regression this guards: a deployed Lambda whose OPENAI_BASE_URL points at a gateway or
        // proxy must not quietly switch Floyd onto a local-model backend.
        Environment.SetEnvironmentVariable(SelfHostedMode.EnvironmentVariableName, null);
        Environment.SetEnvironmentVariable("OPENAI_BASE_URL", "https://my-openai-gateway.internal/v1");

        CompanionChatFactory.Floyd("prompt").Should().BeOfType<ChatWithFloyd>();
        CompanionChatFactory.Blather("prompt").Should().BeOfType<ChatWithBlather>();
        CompanionChatFactory.Ambassador("prompt").Should().BeOfType<ChatWithAmbassador>();
    }

    [Test]
    public void Should_UseTheLocalModel_When_ExplicitlySelfHosted()
    {
        Environment.SetEnvironmentVariable(SelfHostedMode.EnvironmentVariableName, "true");
        Environment.SetEnvironmentVariable("OPENAI_BASE_URL", "http://localhost:11434/v1");

        CompanionChatFactory.Floyd("prompt").Should().BeOfType<LocalCompanionChat>();
        CompanionChatFactory.Blather("prompt").Should().BeOfType<LocalCompanionChat>();
        CompanionChatFactory.Ambassador("prompt").Should().BeOfType<LocalCompanionChat>();
    }
}

using FluentAssertions;
using ZorkAI.OpenAI;

namespace UnitTests.SelfHosted;

/// <summary>
///     Resolution matrix for the self-hosted endpoint settings (issue #383). All tests go through
///     the pure <see cref="OpenAIEndpointSettings.Resolve" /> overload with a dictionary lookup, so
///     nothing here touches real environment variables.
/// </summary>
public class OpenAIEndpointSettingsTests
{
    private static OpenAIEndpointSettings Resolve(Dictionary<string, string?> variables)
    {
        return OpenAIEndpointSettings.Resolve(name => variables.GetValueOrDefault(name));
    }

    [Test]
    public void Should_UseOfficialApi_When_NothingIsConfigured()
    {
        var settings = Resolve(new Dictionary<string, string?>());

        settings.Endpoint.Should().BeNull();
        settings.IsSelfHosted.Should().BeFalse();
        settings.CanCreateClient.Should().BeFalse();
    }

    [Test]
    public void Should_AllowClientCreation_When_OnlyApiKeyIsSet()
    {
        var settings = Resolve(new Dictionary<string, string?> { ["OPEN_AI_KEY"] = "sk-123" });

        settings.IsSelfHosted.Should().BeFalse();
        settings.CanCreateClient.Should().BeTrue();
        settings.ApiKey.Should().Be("sk-123");
    }

    [Test]
    public void Should_BeSelfHostedWithoutKey_When_BaseUrlIsSet()
    {
        var settings = Resolve(new Dictionary<string, string?>
        {
            ["OPENAI_BASE_URL"] = "http://localhost:1234/v1"
        });

        settings.IsSelfHosted.Should().BeTrue();
        settings.CanCreateClient.Should().BeTrue();
        settings.Endpoint.Should().Be(new Uri("http://localhost:1234/v1"));
    }

    [TestCase("lmstudio", "http://localhost:1234/v1")]
    [TestCase("ollama", "http://localhost:11434/v1")]
    [TestCase("koboldcpp", "http://localhost:5001/v1")]
    [TestCase("LMStudio", "http://localhost:1234/v1")]
    public void Should_FillInConventionalUrl_When_ProviderPresetIsUsed(string provider, string expectedUrl)
    {
        var settings = Resolve(new Dictionary<string, string?> { ["ZORKAI_PROVIDER"] = provider });

        settings.IsSelfHosted.Should().BeTrue();
        settings.Endpoint.Should().Be(new Uri(expectedUrl));
    }

    [Test]
    public void Should_StayOnOfficialApi_When_ProviderIsOpenAI()
    {
        var settings = Resolve(new Dictionary<string, string?>
        {
            ["ZORKAI_PROVIDER"] = "openai",
            ["OPEN_AI_KEY"] = "sk-123"
        });

        settings.IsSelfHosted.Should().BeFalse();
        settings.Endpoint.Should().BeNull();
    }

    [Test]
    public void Should_PreferExplicitUrl_When_BothUrlAndProviderAreSet()
    {
        var settings = Resolve(new Dictionary<string, string?>
        {
            ["ZORKAI_PROVIDER"] = "ollama",
            ["OPENAI_BASE_URL"] = "http://gpu-box:8080/v1"
        });

        settings.Endpoint.Should().Be(new Uri("http://gpu-box:8080/v1"));
    }

    [Test]
    public void Should_Throw_When_ProviderIsUnknown()
    {
        var act = () => Resolve(new Dictionary<string, string?> { ["ZORKAI_PROVIDER"] = "webserver" });

        act.Should().Throw<InvalidOperationException>().WithMessage("*webserver*");
    }

    [Test]
    public void Should_Throw_When_BaseUrlIsNotAValidUrl()
    {
        var act = () => Resolve(new Dictionary<string, string?> { ["OPENAI_BASE_URL"] = "localhost only" });

        act.Should().Throw<InvalidOperationException>().WithMessage("*not a valid absolute URL*");
    }

    [Test]
    public void Should_TrimModelOverride_And_TreatWhitespaceAsUnset()
    {
        Resolve(new Dictionary<string, string?> { ["OPENAI_MODEL"] = "  llama3.1  " })
            .ModelOverride.Should().Be("llama3.1");

        Resolve(new Dictionary<string, string?> { ["OPENAI_MODEL"] = "   " })
            .ModelOverride.Should().BeNull();
    }
}

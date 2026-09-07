using GameEngine.Web;
using Microsoft.Extensions.Logging;
using Model.Interface;

namespace UnitTests.Engine;

/// <summary>
///     The OPEN_AI_KEY env var was never declared in any serverless.template — it was set by hand on the
///     Zork and Planetfall Lambdas and survived only because CloudFormation does not manage properties a
///     template never declares. A brand-new game stack therefore deploys "successfully" and then 502s on
///     every request with "Missing environment variable OPEN_AI_KEY" (StationfallStack, release 2.0.9).
///     These cover the fallback that removes the manual step: env var wins when present, Secrets Manager
///     fills the gap when it isn't.
/// </summary>
[TestFixture]
public class OpenAiKeyResolverTests
{
    [SetUp]
    public void Setup()
    {
        // The env var is process-global state; snapshot it so a test can never leak into its neighbours
        // (or into the rest of the suite, which builds real clients).
        _originalKey = Environment.GetEnvironmentVariable(OpenAiKeyResolver.EnvironmentVariableName);
        _secretsManager = new Mock<ISecretsManager>();
        _logger = new Mock<ILogger>();
    }

    [TearDown]
    public void TearDown()
    {
        Environment.SetEnvironmentVariable(OpenAiKeyResolver.EnvironmentVariableName, _originalKey);
    }

    private string? _originalKey;
    private Mock<ISecretsManager> _secretsManager;
    private Mock<ILogger> _logger;

    [Test]
    public async Task Should_KeepTheEnvironmentVariable_When_ItIsAlreadySet()
    {
        // Zork and Planetfall run with the key already on the function. The fallback must not disturb
        // them, and must not spend a Secrets Manager call on every cold start.
        Environment.SetEnvironmentVariable(OpenAiKeyResolver.EnvironmentVariableName, "sk-existing-key");

        var resolved = await OpenAiKeyResolver.EnsureKeyAvailable(_secretsManager.Object, _logger.Object);

        resolved.Should().BeTrue();
        Environment.GetEnvironmentVariable(OpenAiKeyResolver.EnvironmentVariableName).Should().Be("sk-existing-key");
        _secretsManager.Verify(s => s.GetSecret(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task Should_FetchFromSecretsManager_When_EnvironmentVariableIsMissing()
    {
        // The Stationfall case: a fresh stack with no hand-applied env var.
        Environment.SetEnvironmentVariable(OpenAiKeyResolver.EnvironmentVariableName, null);
        _secretsManager.Setup(s => s.GetSecret(OpenAiKeyResolver.SecretName)).ReturnsAsync("sk-from-secrets");

        var resolved = await OpenAiKeyResolver.EnsureKeyAvailable(_secretsManager.Object, _logger.Object);

        resolved.Should().BeTrue();
        Environment.GetEnvironmentVariable(OpenAiKeyResolver.EnvironmentVariableName).Should().Be("sk-from-secrets");
        _secretsManager.Verify(s => s.GetSecret(OpenAiKeyResolver.SecretName), Times.Once);
    }

    [Test]
    public async Task Should_ReportUnresolved_When_SecretIsEmpty()
    {
        // An empty secret is not a key. Treat it as unresolved so the client's existing, explicit
        // "Missing environment variable OPEN_AI_KEY" throw is what the operator sees.
        Environment.SetEnvironmentVariable(OpenAiKeyResolver.EnvironmentVariableName, null);
        _secretsManager.Setup(s => s.GetSecret(OpenAiKeyResolver.SecretName)).ReturnsAsync("   ");

        var resolved = await OpenAiKeyResolver.EnsureKeyAvailable(_secretsManager.Object, _logger.Object);

        resolved.Should().BeFalse();
        Environment.GetEnvironmentVariable(OpenAiKeyResolver.EnvironmentVariableName).Should().BeNullOrEmpty();
    }

    [Test]
    public async Task Should_NotThrow_When_SecretsManagerFails()
    {
        // A Secrets Manager outage (or a missing secret) must not replace the clear downstream error
        // with an opaque startup crash from inside DI registration.
        Environment.SetEnvironmentVariable(OpenAiKeyResolver.EnvironmentVariableName, null);
        _secretsManager.Setup(s => s.GetSecret(OpenAiKeyResolver.SecretName))
            .ThrowsAsync(new InvalidOperationException("secret not found"));

        var resolved = await OpenAiKeyResolver.EnsureKeyAvailable(_secretsManager.Object, _logger.Object);

        resolved.Should().BeFalse();
        Environment.GetEnvironmentVariable(OpenAiKeyResolver.EnvironmentVariableName).Should().BeNullOrEmpty();
    }

    [Test]
    public async Task Should_TreatWhitespaceEnvironmentVariable_AsMissing()
    {
        // A blank env var is the same failure as no env var; fall back rather than hand the OpenAI
        // client a whitespace key that fails later and further away.
        Environment.SetEnvironmentVariable(OpenAiKeyResolver.EnvironmentVariableName, "  ");
        _secretsManager.Setup(s => s.GetSecret(OpenAiKeyResolver.SecretName)).ReturnsAsync("sk-from-secrets");

        var resolved = await OpenAiKeyResolver.EnsureKeyAvailable(_secretsManager.Object, _logger.Object);

        resolved.Should().BeTrue();
        Environment.GetEnvironmentVariable(OpenAiKeyResolver.EnvironmentVariableName).Should().Be("sk-from-secrets");
    }
}

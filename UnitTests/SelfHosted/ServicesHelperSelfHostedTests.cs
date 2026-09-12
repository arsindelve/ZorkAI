using ChatLambda;
using DynamoDb;
using EscapeRoom;
using FluentAssertions;
using GameEngine;
using GameEngine.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Model.Interface;
using SecretsManager;

namespace UnitTests.SelfHosted;

/// <summary>
///     The composition root that decides whether a game backend runs on AWS or entirely locally
///     (issue #383). These tests are the proof that a self-hosted backend resolves no AWS-backed
///     service — the console got this de-clouding first, and <see cref="ServicesHelper" /> is where
///     the four Lambdas inherit it.
///     <para>
///     Environment variables are process-global, so each test restores what it found. The assembly
///     runs with LevelOfParallelism(1), so there is no concurrent reader to race with.
///     </para>
/// </summary>
public class ServicesHelperSelfHostedTests
{
    private string? _originalBaseUrl;
    private string? _originalKey;
    private string? _originalProvider;
    private string? _originalSaveDir;
    private string? _originalSelfHosted;

    [SetUp]
    public void SetUp()
    {
        Repository.Reset();

        _originalBaseUrl = Environment.GetEnvironmentVariable("OPENAI_BASE_URL");
        _originalProvider = Environment.GetEnvironmentVariable("ZORKAI_PROVIDER");
        _originalKey = Environment.GetEnvironmentVariable("OPEN_AI_KEY");
        _originalSaveDir = Environment.GetEnvironmentVariable(SelfHostedStorage.SaveDirectoryVariable);
        _originalSelfHosted = Environment.GetEnvironmentVariable(SelfHostedMode.EnvironmentVariableName);
    }

    [TearDown]
    public void TearDown()
    {
        Environment.SetEnvironmentVariable("OPENAI_BASE_URL", _originalBaseUrl);
        Environment.SetEnvironmentVariable("ZORKAI_PROVIDER", _originalProvider);
        Environment.SetEnvironmentVariable("OPEN_AI_KEY", _originalKey);
        Environment.SetEnvironmentVariable(SelfHostedStorage.SaveDirectoryVariable, _originalSaveDir);
        Environment.SetEnvironmentVariable(SelfHostedMode.EnvironmentVariableName, _originalSelfHosted);
        Repository.Reset();
    }

    private static void GoSelfHosted()
    {
        Environment.SetEnvironmentVariable(SelfHostedMode.EnvironmentVariableName, "true");
        Environment.SetEnvironmentVariable("OPENAI_BASE_URL", "http://localhost:11434/v1");
        Environment.SetEnvironmentVariable("ZORKAI_PROVIDER", null);
        Environment.SetEnvironmentVariable("OPEN_AI_KEY", null);
    }

    private static void GoCloud()
    {
        Environment.SetEnvironmentVariable(SelfHostedMode.EnvironmentVariableName, null);
        Environment.SetEnvironmentVariable("OPENAI_BASE_URL", null);
        Environment.SetEnvironmentVariable("ZORKAI_PROVIDER", null);
        Environment.SetEnvironmentVariable("OPEN_AI_KEY", "sk-not-a-real-key");
    }

    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        ServicesHelper.ConfigureCommonServices(services);
        return services.BuildServiceProvider();
    }

    [Test]
    public void Should_ResolveLocalRepositories_When_SelfHosted()
    {
        GoSelfHosted();

        using var provider = BuildProvider();

        provider.GetRequiredService<ISessionRepository>().Should().BeOfType<FileSessionRepository>();
        provider.GetRequiredService<ISavedGameRepository>().Should().BeOfType<FileSavedGameRepository>();
        provider.GetRequiredService<ISecretsManager>().Should().BeOfType<LocalSecretsManager>();
        provider.GetRequiredService<IParseConversation>().Should().BeOfType<LocalParseConversation>();
    }

    [Test]
    public void Should_GiveTheLocalClassifierARealLogger_When_SelfHosted()
    {
        GoSelfHosted();

        using var provider = BuildProvider();

        // Its Logger defaults to NullLogger, so registering it by type would silently throw away the
        // diagnostics it emits when a local model returns unparseable output.
        var classifier = (LocalParseConversation)provider.GetRequiredService<IParseConversation>();

        classifier.Logger.Should().NotBeNull();
        classifier.Logger.Should().BeAssignableTo<ILogger<LocalParseConversation>>();
    }

    [Test]
    public void Should_NotRegisterTheLambdaClient_When_SelfHosted()
    {
        GoSelfHosted();

        var services = new ServiceCollection();
        services.AddLogging();
        ServicesHelper.ConfigureCommonServices(services);

        // Registering it would hand the AWS SDK a reason to go looking for credentials that a
        // self-hosted install does not have. The companions resolve their backend through
        // CompanionChatFactory instead.
        services.Should().NotContain(d => d.ServiceType == typeof(Amazon.Lambda.IAmazonLambda));
    }

    [Test]
    public void Should_RegisterTheAwsStack_When_NotSelfHosted()
    {
        GoCloud();

        var services = new ServiceCollection();
        services.AddLogging();
        ServicesHelper.ConfigureCommonServices(services);

        // Asserted on the descriptors rather than by resolving: constructing the real AWS clients
        // would reach for credentials this test deliberately does not have.
        Implementation<ISessionRepository>(services).Should().Be<DynamoDbSessionRepository>();
        Implementation<ISavedGameRepository>(services).Should().Be<DynamoDbSavedGameRepository>();
        Implementation<ISecretsManager>(services).Should().Be<AmazonSecretsManager>();
        Implementation<IParseConversation>(services).Should().Be<ParseConversation>();
        services.Should().Contain(d => d.ServiceType == typeof(Amazon.Lambda.IAmazonLambda));
    }

    [Test]
    public void Should_KeepTheAwsStack_When_OnlyTheAiEndpointIsCustom()
    {
        // The production guarantee. OPENAI_BASE_URL is a generic name that gateways, proxies and
        // Azure OpenAI all use, so pointing a DEPLOYED Lambda at one must not move every player's
        // session and saved game from DynamoDB onto the function's ephemeral filesystem. Only the
        // explicit ZORKAI_SELF_HOSTED opt-in may do that. See SelfHostedMode.
        Environment.SetEnvironmentVariable(SelfHostedMode.EnvironmentVariableName, null);
        Environment.SetEnvironmentVariable("OPENAI_BASE_URL", "https://my-openai-gateway.internal/v1");
        Environment.SetEnvironmentVariable("OPEN_AI_KEY", "sk-not-a-real-key");

        var services = new ServiceCollection();
        services.AddLogging();
        ServicesHelper.ConfigureCommonServices(services);

        Implementation<ISessionRepository>(services).Should().Be<DynamoDbSessionRepository>();
        Implementation<ISavedGameRepository>(services).Should().Be<DynamoDbSavedGameRepository>();
        Implementation<ISecretsManager>(services).Should().Be<AmazonSecretsManager>();
        Implementation<IParseConversation>(services).Should().Be<ParseConversation>();
    }

    [Test]
    public void Should_KeepCloudLoggingOn_When_OnlyTheAiEndpointIsCustom()
    {
        Environment.SetEnvironmentVariable(SelfHostedMode.EnvironmentVariableName, null);
        Environment.SetEnvironmentVariable("OPENAI_BASE_URL", "https://my-openai-gateway.internal/v1");
        Environment.SetEnvironmentVariable("OPEN_AI_KEY", "sk-not-a-real-key");

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped<ISecretsManager, LocalSecretsManager>();
        services.AddScoped<IParseConversation, LocalParseConversation>();
        ServicesHelper.ConfigureGameEngine<EscapeRoomGame, EscapeRoomContext>(services);
        using var provider = services.BuildServiceProvider();

        var engine = (GameEngine<EscapeRoomGame, EscapeRoomContext>)provider.GetRequiredService<IGameEngine>();

        engine.CloudLoggingEnabled.Should().BeTrue();
    }

    [TestCase("true")]
    [TestCase("TRUE")]
    [TestCase("1")]
    [TestCase("yes")]
    [TestCase("on")]
    public void Should_TreatAsSelfHosted_When_FlagIsAffirmative(string value)
    {
        SelfHostedMode.Resolve(_ => value).Should().BeTrue();
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("  ")]
    [TestCase("false")]
    [TestCase("0")]
    [TestCase("no")]
    [TestCase("maybe")]
    public void Should_TreatAsCloud_When_FlagIsAbsentOrNotAffirmative(string? value)
    {
        SelfHostedMode.Resolve(_ => value).Should().BeFalse();
    }

    [Test]
    public void Should_DisableCloudLogging_When_SelfHosted()
    {
        GoSelfHosted();

        var services = new ServiceCollection();
        services.AddLogging();
        ServicesHelper.ConfigureCommonServices(services);
        ServicesHelper.ConfigureGameEngine<EscapeRoomGame, EscapeRoomContext>(services);
        using var provider = services.BuildServiceProvider();

        var engine = (GameEngine<EscapeRoomGame, EscapeRoomContext>)provider.GetRequiredService<IGameEngine>();

        // Left at its default of true, InitializeEngine spins through CloudWatch credential retries
        // on a machine that has no AWS at all.
        engine.CloudLoggingEnabled.Should().BeFalse();
    }

    [Test]
    public void Should_LeaveCloudLoggingOn_When_NotSelfHosted()
    {
        GoCloud();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped<ISecretsManager, LocalSecretsManager>();
        services.AddScoped<IParseConversation, LocalParseConversation>();
        ServicesHelper.ConfigureGameEngine<EscapeRoomGame, EscapeRoomContext>(services);
        using var provider = services.BuildServiceProvider();

        var engine = (GameEngine<EscapeRoomGame, EscapeRoomContext>)provider.GetRequiredService<IGameEngine>();

        engine.CloudLoggingEnabled.Should().BeTrue();
    }

    private static Type? Implementation<TService>(IServiceCollection services)
    {
        return services.Single(d => d.ServiceType == typeof(TService)).ImplementationType;
    }
}

using Amazon;
using Amazon.Lambda;
using ChatLambda;
using DynamoDb;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Model.AIGeneration;
using Model.Interface;
using SecretsManager;
using ZorkAI.OpenAI;

namespace GameEngine.Web;

public static class ServicesHelper
{
    /// <summary>
    ///     Registers the services every game backend shares. In cloud mode this is the AWS stack that
    ///     has always been here. In self-hosted mode (issue #383) each AWS dependency is swapped for a
    ///     local one, so a backend can run with no AWS account, no OpenAI key and no network beyond the
    ///     configured model endpoint — the same de-clouding the console got, moved to the one place all
    ///     four Lambdas compose from.
    ///     <para>
    ///     The switch is <see cref="OpenAIEndpointSettings.IsSelfHosted" />, i.e. whether OPENAI_BASE_URL
    ///     or ZORKAI_PROVIDER names a custom endpoint. That is read here, in a composition root, and
    ///     never inside the engine — see <c>GameEngine.CloudLoggingEnabled</c> for why that distinction
    ///     matters.
    ///     </para>
    /// </summary>
    public static void ConfigureCommonServices(IServiceCollection services)
    {
        var settings = OpenAIEndpointSettings.FromEnvironment();

        if (settings.IsSelfHosted)
        {
            // File-backed state instead of DynamoDB. Singleton/scoped lifetimes deliberately match the
            // cloud registrations below so nothing else in the container has to care which mode it is.
            services.AddSingleton<ISessionRepository>(_ => new FileSessionRepository());
            services.AddScoped<ISavedGameRepository>(_ => new FileSavedGameRepository());
            services.AddScoped<ISecretsManager, LocalSecretsManager>();

            // Constructed by hand rather than by type so the classifier keeps the container's logger.
            // Its Logger property defaults to NullLogger, so a plain AddScoped<,>() would silently
            // discard the "classifier returned unparseable output" diagnostics it exists to emit.
            services.AddScoped<IParseConversation>(sp => new LocalParseConversation
            {
                Logger = (ILogger?)sp.GetService<ILogger<LocalParseConversation>>() ?? NullLogger.Instance
            });
        }
        else
        {
            services.AddSingleton<ISessionRepository, DynamoDbSessionRepository>();
            services.AddScoped<ISavedGameRepository, DynamoDbSavedGameRepository>();
            services.AddScoped<ISecretsManager, AmazonSecretsManager>();
            services.AddScoped<IParseConversation, ParseConversation>();

            // Only registered in cloud mode: this is the client for the Floyd LangGraph function. In
            // self-hosted mode the companions resolve their backend through CompanionChatFactory
            // instead, which needs no Lambda client — registering one here would just hand the AWS SDK
            // a reason to go looking for credentials that do not exist.
            services.AddSingleton<IAmazonLambda>(_ => new AmazonLambdaClient(RegionEndpoint.USEast1));
        }

        // Endpoint-agnostic: ChatGPTClient resolves its endpoint and model through
        // OpenAIEndpointSettings, so the same registration serves the real OpenAI API and any
        // OpenAI-compatible server.
        services.AddScoped<IGenerationClient, ChatGPTClient>();
    }

    /// <summary>
    ///     Whether the process is configured for self-hosted play.
    /// </summary>
    public static bool IsSelfHosted => OpenAIEndpointSettings.FromEnvironment().IsSelfHosted;

    /// <summary>
    ///     Registers a game's engine, wiring <c>CloudLoggingEnabled</c> from the self-hosted setting.
    ///     <para>
    ///     Each Startup used to register the engine by type — <c>AddScoped&lt;IGameEngine,
    ///     GameEngine&lt;X, Y&gt;&gt;()</c> — which leaves the flag at its default of true, so a
    ///     backend with no AWS would still spin through CloudWatch credential retries on every
    ///     <c>InitializeEngine</c>. Registering through a factory is the only way to set a property the
    ///     constructor does not take. The engine is built explicitly rather than via
    ///     <c>ActivatorUtilities</c> because <c>GameEngine</c> has two public constructors and we want
    ///     the three-argument one chosen deterministically, not by whatever else happens to be
    ///     registered.
    ///     </para>
    /// </summary>
    public static void ConfigureGameEngine<TGame, TContext>(IServiceCollection services)
        where TGame : IInfocomGame, new()
        where TContext : class, IContext, new()
    {
        var cloudLoggingEnabled = !IsSelfHosted;

        services.AddScoped<IGameEngine>(sp => new GameEngine<TGame, TContext>(
            sp.GetRequiredService<ILogger<GameEngine<TGame, TContext>>>(),
            sp.GetRequiredService<ISecretsManager>(),
            sp.GetRequiredService<IParseConversation>())
        {
            CloudLoggingEnabled = cloudLoggingEnabled
        });
    }
}

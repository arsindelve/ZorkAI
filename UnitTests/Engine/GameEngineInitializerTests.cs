using GameEngine.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Model.Interface;

namespace UnitTests.Engine;

/// <summary>
///     The hosted service that warms the engine at host startup. One fixture, because all three game
///     backends now share one implementation in <c>GameEngine/Web/</c> — each used to carry its own
///     copy of the class and its own copy of these tests.
/// </summary>
[TestFixture]
public class GameEngineInitializerTests
{
    private static ServiceProvider BuildProvider(Mock<IGameEngine> engine, bool validateScopes)
    {
        var services = new ServiceCollection();

        // Scoped, exactly as every game Startup registers it. That lifetime is the whole point: a
        // hosted service is a singleton and may not depend on a scoped service directly.
        services.AddScoped(_ => engine.Object);

        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = validateScopes });
    }

    private static GameEngineInitializer Create(ServiceProvider provider)
    {
        return new GameEngineInitializer(
            provider.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<GameEngineInitializer>.Instance);
    }

    [Test]
    public async Task StartAsync_Should_InitializeTheEngine()
    {
        var engine = new Mock<IGameEngine>();
        engine.Setup(e => e.InitializeEngine()).Returns(Task.CompletedTask);

        using var provider = BuildProvider(engine, validateScopes: false);

        await Create(provider).StartAsync(CancellationToken.None);

        engine.Verify(e => e.InitializeEngine(), Times.Once);
    }

    [Test]
    public async Task StartAsync_Should_Work_When_ScopeValidationIsOn()
    {
        // The regression. Injecting the scoped IGameEngine straight into this singleton threw
        //   "Cannot consume scoped service 'Model.Interface.IGameEngine' from singleton
        //    'Microsoft.Extensions.Hosting.IHostedService'"
        // whenever ValidateScopes was on — which ASP.NET Core does for itself under
        // ASPNETCORE_ENVIRONMENT=Development, so the backends simply could not start there.
        // Resolving through IServiceScopeFactory is the supported route and works either way.
        var engine = new Mock<IGameEngine>();
        engine.Setup(e => e.InitializeEngine()).Returns(Task.CompletedTask);

        using var provider = BuildProvider(engine, validateScopes: true);

        var starting = async () => await Create(provider).StartAsync(CancellationToken.None);

        await starting.Should().NotThrowAsync();
        engine.Verify(e => e.InitializeEngine(), Times.Once);
    }

    [Test]
    public async Task StopAsync_Should_CompleteSuccessfully()
    {
        var engine = new Mock<IGameEngine>();
        using var provider = BuildProvider(engine, validateScopes: true);

        var stopping = async () => await Create(provider).StopAsync(CancellationToken.None);

        await stopping.Should().NotThrowAsync();
    }

    [Test]
    public void Should_ImplementIHostedService()
    {
        var engine = new Mock<IGameEngine>();
        using var provider = BuildProvider(engine, validateScopes: true);

        Create(provider).Should().BeAssignableTo<IHostedService>();
    }
}

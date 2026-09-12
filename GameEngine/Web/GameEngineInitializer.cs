using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Model.Interface;

namespace GameEngine.Web;

/// <summary>
///     Warms the engine once at host startup so the first request does not pay for it.
///     <para>
///     <b>Why the scope.</b> A hosted service is a singleton, and <c>IGameEngine</c> is registered
///     scoped. Injecting it directly — which all three game Startups used to do, each with its own
///     copy of this class — made the host throw
///     <c>"Cannot consume scoped service 'Model.Interface.IGameEngine' from singleton
///     'Microsoft.Extensions.Hosting.IHostedService'"</c> whenever DI scope validation was on, i.e.
///     under <c>ASPNETCORE_ENVIRONMENT=Development</c>. The backends could not run in Development at
///     all. Resolving from an explicit scope is the supported way for a singleton to reach a scoped
///     service, and it works in every environment.
///     </para>
///     <para>
///     <b>What this actually warms.</b> Not the engine any request uses: <c>IGameEngine</c> is
///     scoped, so every request builds its own, and each controller action already calls
///     <c>InitializeEngine()</c> on it. What carries over is the <i>static</i>
///     <see cref="Repository" />, which the engine's constructor populates with the game's items and
///     locations and which every later engine then reuses. Keep that in mind before assuming this
///     saves a per-request cost — it does not; it moves one-time world construction off the first
///     request.
///     </para>
/// </summary>
public class GameEngineInitializer(
    IServiceScopeFactory scopeFactory,
    ILogger<GameEngineInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var engine = scope.ServiceProvider.GetRequiredService<IGameEngine>();

        await engine.InitializeEngine();

        logger.LogInformation("GameEngine initialized.");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

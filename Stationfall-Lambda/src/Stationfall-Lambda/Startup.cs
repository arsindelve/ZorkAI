using GameEngine;
using GameEngine.Web;
using Model.Interface;
using Stationfall;

namespace Stationfall_Lambda;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging();
        services.AddControllers();
        services.AddEndpointsApiExplorer();

        ServicesHelper.ConfigureGameEngine<StationfallGame, StationfallContext>(services);

        // NOTE: no IHintLanguageModel registration here, unlike Planetfall_Lambda. The hint subsystem
        // needs a game-specific IHintProvider, and Stationfall doesn't have one yet (Planetfall's is
        // Planetfall/Hints/PlanetfallHintProvider.cs). Register it here, and add the /Stationfall/hint
        // endpoint to StationfallController, once a StationfallHintProvider exists.

        // Register the hosted service that will initialize GameEngine asynchronously
        services.AddHostedService<GameEngineInitializer>();
        ServicesHelper.ConfigureCommonServices(services);
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseCors(b => b
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
        app.UseHttpsRedirection();
        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapGet("/",
                async context =>
                {
                    await context.Response.WriteAsync("Welcome to running ASP.NET Core on AWS Lambda");
                });
        });
    }
}

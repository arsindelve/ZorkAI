using GameEngine;
using GameEngine.Hints;
using GameEngine.Web;
using Model.Hints;
using Model.Interface;
using Planetfall;
using ZorkAI.OpenAI;

namespace Planetfall_Lambda;

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

        ServicesHelper.ConfigureGameEngine<PlanetfallGame, PlanetfallContext>(services);

        // Hint subsystem (v1: Planetfall, all-OpenAI). Stateless: the hint conversation is supplied by
        // the client on each request, so there's no server-side memory to register.
        services.AddScoped<IHintLanguageModel>(sp =>
            new OpenAiHintLanguageModel(sp.GetService<ILogger<OpenAiHintLanguageModel>>()));

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

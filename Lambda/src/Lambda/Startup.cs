using DynamoDb;
using GameEngine;
using GameEngine.Web;
using Model.AIGeneration;
using Model.Interface;
using OpenAI;
using ZorkOne;

namespace Lambda;

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
        services.AddSwaggerGen();
      
        services.AddScoped<IGameEngine, GameEngine<ZorkI, ZorkIContext>>();
        
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
        app.UseSwagger();
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

public class GameEngineInitializer(IGameEngine gameEngine) : IHostedService
{
    // Inject the GameEngine into the initializer

    // This will be called when the application starts
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Call the async initialization method on GameEngine
        await gameEngine.InitializeEngine();
        Console.WriteLine("GameEngine initialized!");
    }

    // Optional: This method is called during application shutdown
    public Task StopAsync(CancellationToken cancellationToken)
    {
        // Perform any cleanup or shutdown logic if necessary
        return Task.CompletedTask;
    }
}
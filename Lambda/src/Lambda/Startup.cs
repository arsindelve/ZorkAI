﻿using DynamoDb;
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

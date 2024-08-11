using DynamoDb;
using Microsoft.Extensions.DependencyInjection;
using Model.AIGeneration;
using Model.Interface;

namespace GameEngine.Web;

public static class ServicesHelper
{
    public static void ConfigureCommonServices(IServiceCollection services)
    {
        services.AddSingleton<ISessionRepository, DynamoDbSessionRepository>();
        services.AddScoped<ISavedGameRepository, DynamoDbSavedGameRepository>();
        services.AddScoped<IGenerationClient, ChatGPTClient>();
    }
}
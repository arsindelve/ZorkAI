using ChatLambda;
using DynamoDb;
using Microsoft.Extensions.DependencyInjection;
using Model.AIGeneration;
using Model.Interface;
using SecretsManager;

namespace GameEngine.Web;

public static class ServicesHelper
{
    public static void ConfigureCommonServices(IServiceCollection services)
    {
        services.AddSingleton<ISessionRepository, DynamoDbSessionRepository>();
        services.AddScoped<ISavedGameRepository, DynamoDbSavedGameRepository>();
        services.AddScoped<IGenerationClient, ChatGPTClient>();
        services.AddScoped<ISecretsManager, AmazonSecretsManager>();
        services.AddScoped<IParseConversation, ParseConversation>();
    }
}
using Amazon;
using Amazon.Lambda;
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
        services.AddSingleton<IAmazonLambda>(_ => new AmazonLambdaClient(RegionEndpoint.USEast1));
        services.AddScoped<IParseConversation, ParseConversation>();
    }
}
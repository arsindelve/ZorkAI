using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Model.Interface;

namespace SecretsManager;

public class AmazonSecretsManager : ISecretsManager
{
    public async Task<string> GetSecret(string secretName)
    {
        var region = "us-east-1";

        IAmazonSecretsManager client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(region));

        var request = new GetSecretValueRequest
        {
            SecretId = secretName,
            VersionStage = "AWSCURRENT"
        };

        var response = await client.GetSecretValueAsync(request);
        return response.SecretString;
    }
}
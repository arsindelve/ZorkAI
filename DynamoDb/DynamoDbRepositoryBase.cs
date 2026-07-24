using Amazon;
using Amazon.DynamoDBv2;

namespace DynamoDb;

public abstract class DynamoDbRepositoryBase
{
    protected readonly IAmazonDynamoDB Client;

    protected DynamoDbRepositoryBase(IAmazonDynamoDB? client = null)
    {
        if (client is not null)
        {
            Client = client;
            return;
        }

        var clientConfig = new AmazonDynamoDBConfig
        {
            // TODO: Read from env vars or config
            RegionEndpoint = RegionEndpoint.USEast1
        };

        Client = new AmazonDynamoDBClient(clientConfig);
    }
}

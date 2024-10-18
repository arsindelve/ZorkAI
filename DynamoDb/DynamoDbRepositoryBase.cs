using Amazon;
using Amazon.DynamoDBv2;

namespace DynamoDb;

public abstract class DynamoDbRepositoryBase
{
    protected readonly AmazonDynamoDBClient Client;

    protected DynamoDbRepositoryBase()
    {
        var clientConfig = new AmazonDynamoDBConfig
        {
            // TODO: Read from env vars or config
            RegionEndpoint = RegionEndpoint.USEast1
        };

        Client = new AmazonDynamoDBClient(clientConfig);
    }
}
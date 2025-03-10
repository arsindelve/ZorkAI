using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;

namespace IntegrationTests;

[TestFixture]
[Explicit]
[Ignore("")]
public class DynamoDbTests
{
    [SetUp]
    public void Setup()
    {
        var clientConfig = new AmazonDynamoDBConfig
        {
            // TODO: Read from env vars or config
            RegionEndpoint = RegionEndpoint.USEast1
        };

        _dynamoDbClient = new AmazonDynamoDBClient(clientConfig);
        _table = Table.LoadTable(_dynamoDbClient, TableName);
    }

    [TearDown]
    public void TearDown()
    {
        _dynamoDbClient.Dispose();
    }

    private AmazonDynamoDBClient _dynamoDbClient;
    private Table _table;
    private const string TableName = "zork_session_ondemand";

    [Test]
    public async Task DeleteAllItemsFromTable()
    {
        // Scan the table to retrieve all items
        var scanFilter = new ScanFilter();
        var search = _table.Scan(scanFilter);

        List<Document> documentList = new();

        do
        {
            var documentBatch = await search.GetNextSetAsync();
            documentList.AddRange(documentBatch);
        } while (!search.IsDone);

        // Delete each item
        foreach (var document in documentList) await _table.DeleteItemAsync(document);

        // Verify the table is empty
        var emptyScan = _table.Scan(scanFilter);
        var result = await emptyScan.GetNextSetAsync();
        result.Should().BeEmpty();
    }
}
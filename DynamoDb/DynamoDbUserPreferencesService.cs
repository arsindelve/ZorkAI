using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Model;
using Model.Interface;
using Newtonsoft.Json;

namespace DynamoDb;

/// <summary>
/// DynamoDB-based implementation of user preferences service for cloud applications
/// </summary>
public class DynamoDbUserPreferencesService : DynamoDbRepositoryBase, IUserPreferencesService
{
    private readonly string _tableName;
    private const string PreferencesPartitionKey = "USER_PREFERENCES";

    public DynamoDbUserPreferencesService(string tableName = "ZorkAI") : base()
    {
        _tableName = tableName;
    }

    public async Task<UserPreferences> GetPreferencesAsync(string? userId = null)
    {
        var effectiveUserId = userId ?? "default";
        
        try
        {
            var request = new GetItemRequest
            {
                TableName = _tableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "PK", new AttributeValue($"{PreferencesPartitionKey}#{effectiveUserId}") },
                    { "SK", new AttributeValue("PREFERENCES") }
                }
            };

            var response = await Client.GetItemAsync(request);
            
            if (response.Item.ContainsKey("PreferencesData"))
            {
                var json = response.Item["PreferencesData"].S;
                return JsonConvert.DeserializeObject<UserPreferences>(json) ?? UserPreferences.Default;
            }
        }
        catch (Exception)
        {
            // If there's any error loading preferences, use defaults
        }

        return UserPreferences.Default;
    }

    public async Task SavePreferencesAsync(UserPreferences preferences, string? userId = null)
    {
        var effectiveUserId = userId ?? "default";
        
        try
        {
            var json = JsonConvert.SerializeObject(preferences);
            var request = new PutItemRequest
            {
                TableName = _tableName,
                Item = new Dictionary<string, AttributeValue>
                {
                    { "PK", new AttributeValue($"{PreferencesPartitionKey}#{effectiveUserId}") },
                    { "SK", new AttributeValue("PREFERENCES") },
                    { "PreferencesData", new AttributeValue(json) },
                    { "LastModified", new AttributeValue(DateTime.UtcNow.ToString("O")) },
                    { "TTL", new AttributeValue { N = GetTtlTimestamp().ToString() } }
                }
            };

            await Client.PutItemAsync(request);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save preferences to DynamoDB: {ex.Message}", ex);
        }
    }

    public async Task ResetToDefaultsAsync(string? userId = null)
    {
        await SavePreferencesAsync(UserPreferences.Default, userId);
    }

    public async Task<string> ExportPreferencesAsync(string? userId = null)
    {
        var preferences = await GetPreferencesAsync(userId);
        return JsonConvert.SerializeObject(preferences, Formatting.Indented);
    }

    public async Task ImportPreferencesAsync(string preferencesJson, string? userId = null)
    {
        try
        {
            var preferences = JsonConvert.DeserializeObject<UserPreferences>(preferencesJson);
            if (preferences == null)
                throw new InvalidOperationException("Invalid preferences JSON format");

            await SavePreferencesAsync(preferences, userId);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse preferences JSON: {ex.Message}", ex);
        }
    }

    private static long GetTtlTimestamp()
    {
        // Set TTL for 5 years from now (preferences should persist for a long time)
        return DateTimeOffset.UtcNow.AddYears(5).ToUnixTimeSeconds();
    }
}
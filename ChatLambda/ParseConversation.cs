using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace ChatLambda;

public class ParseConversation : IParseConversation
{
    private const string FunctionName = "Floyd-LangGraphFunction-GefcykaLqwp-MySimpleLambda-lBRzg1jLKvXP";
    private readonly IAmazonLambda _lambdaClient;

    public ParseConversation(IAmazonLambda? lambdaClient)
    {
        _lambdaClient = lambdaClient ?? CreateDefaultLambdaClient();
    }

    private IAmazonLambda CreateDefaultLambdaClient()
    {
        return new AmazonLambdaClient(RegionEndpoint.USEast1);
    }

    /// <summary>
    /// Parses conversation input and returns whether the response is "No" and the actual response.
    /// </summary>
    /// <param name="input">The conversation input to parse</param>
    /// <returns>A tuple where Item1 is true if response is "No", Item2 is the response (empty if "No")</returns>
    public async Task<(bool isNo, string response)> ParseAsync(string input)
    {
        return await ParseAsync(input, CancellationToken.None);
    }

    public ILogger Logger { get; set; }

    /// <summary>
    /// Parses conversation input and returns whether the response is "No" and the actual response.
    /// </summary>
    /// <param name="input">The conversation input to parse</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>A tuple where Item1 is true if response is "No", Item2 is the response (empty if "No")</returns>
    private async Task<(bool isNo, string response)> ParseAsync(string input, CancellationToken cancellationToken)
    {
        Logger.LogDebug($"[PARSE CONVERSATION DEBUG] Starting ParseAsync with input: '{input}'");
        
        if (string.IsNullOrWhiteSpace(input))
        {
            Logger.LogDebug("[PARSE CONVERSATION DEBUG] Input is null or whitespace, throwing exception");
            throw new ArgumentException("Input cannot be empty", nameof(input));
        }

        try
        {
            // Create the request payload for RewriteSecondPerson endpoint
            var request = new
            {
                prompt = input,
                assistant = "RewriteSecondPerson"
            };

            // Serialize the request to JSON
            var requestJson = JsonSerializer.Serialize(request);
            Logger.LogDebug($"[PARSE CONVERSATION DEBUG] Request JSON: {requestJson}");

            // Set up the Lambda invoke request
            var invokeRequest = new InvokeRequest
            {
                FunctionName = FunctionName,
                Payload = requestJson
            };
            
            Logger.LogDebug($"[PARSE CONVERSATION DEBUG] Invoking Lambda function: {FunctionName}");

            // Invoke the Lambda function
            var response = await _lambdaClient.InvokeAsync(invokeRequest, cancellationToken);
            Logger.LogDebug($"[PARSE CONVERSATION DEBUG] Lambda response status code: {response.StatusCode}");

            // Process the response
            if (response.StatusCode != 200)
            {
                Logger.LogDebug($"[PARSE CONVERSATION DEBUG] Lambda invocation failed with status code: {response.StatusCode}");
                throw new Exception($"Lambda invocation failed with status code: {response.StatusCode}");
            }

            // Read the response payload
            using var reader = new StreamReader(response.Payload);
            var responseJson = await reader.ReadToEndAsync(cancellationToken);
            Logger.LogDebug($"[PARSE CONVERSATION DEBUG] Raw Lambda response JSON: {responseJson}");

            // Parse the Lambda response (body is a JSON string that needs to be deserialized)
            var lambdaResponse = JsonSerializer.Deserialize<LambdaResponseWrapper>(responseJson);
            Logger.LogDebug($"[PARSE CONVERSATION DEBUG] Deserialized Lambda response - StatusCode: {lambdaResponse?.StatusCode}, Body length: {lambdaResponse?.Body.Length ?? 0}");

            if (lambdaResponse == null)
            {
                Logger.LogDebug("[PARSE CONVERSATION DEBUG] Failed to deserialize Lambda response");
                throw new Exception("Failed to deserialize Lambda response");
            }

            // Parse the body content (which is a JSON string)
            Logger.LogDebug($"[PARSE CONVERSATION DEBUG] Lambda response body: {lambdaResponse.Body}");
            var bodyContent = JsonSerializer.Deserialize<BodyContent>(lambdaResponse.Body);
            Logger.LogDebug($"[PARSE CONVERSATION DEBUG] Deserialized body content - Results null: {bodyContent?.Results == null}");

            if (bodyContent?.Results == null)
            {
                Logger.LogDebug($"[PARSE CONVERSATION DEBUG] Failed to extract results from Lambda response. Body: {lambdaResponse.Body}");
                throw new Exception($"Failed to extract results from Lambda response. Body: {lambdaResponse.Body}");
            }
            
            Logger.LogDebug($"[PARSE CONVERSATION DEBUG] Results.Response: '{bodyContent.Results.Response}'");
            if (bodyContent.Results.Response == null)
            {
                Logger.LogDebug($"[PARSE CONVERSATION DEBUG] Failed to extract message from Lambda response. Results: {JsonSerializer.Serialize(bodyContent.Results)}");
                throw new Exception($"Failed to extract message from Lambda response. Results: {JsonSerializer.Serialize(bodyContent.Results)}");
            }

            var lambdaResponseText = bodyContent.Results.Response;
            Logger.LogDebug($"[PARSE CONVERSATION DEBUG] Lambda response text: '{lambdaResponseText}'");
            
            // Check if the response is "No" (case-insensitive)
            var trimmedResponse = lambdaResponseText.Trim();
            bool isNo = string.Equals(trimmedResponse, "No", StringComparison.OrdinalIgnoreCase);
            Logger.LogDebug($"[PARSE CONVERSATION DEBUG] Trimmed response: '{trimmedResponse}', isNo: {isNo}");
            
            var finalResult = (isNo, isNo ? string.Empty : lambdaResponseText);
            Logger.LogDebug($"[PARSE CONVERSATION DEBUG] Final result: isNo={finalResult.Item1}, response='{finalResult.Item2}'");
            
            return finalResult;
        }
        catch (Exception ex)
        {
            Logger.LogDebug($"[PARSE CONVERSATION DEBUG] Exception occurred: {ex.Message}");
            Logger.LogDebug($"[PARSE CONVERSATION DEBUG] Stack trace: {ex.StackTrace}");
            throw new Exception($"Error communicating with Parse Lambda: {ex.Message}", ex);
        }
    }

    // Records for JSON deserialization (same as ChatWithFloyd)
    private record LambdaResponseWrapper(
        [property: JsonPropertyName("statusCode")]
        int StatusCode,
        [property: JsonPropertyName("body")] string Body
    );

    private record BodyContent(
        [property: JsonPropertyName("results")]
        Results Results
    );

    [UsedImplicitly]
    private record Results(
        [property: JsonPropertyName("single_message")]
        string Response,
        [property: JsonPropertyName("metadata")]
        Metadata? Metadata
    );

    [UsedImplicitly]
    private record Metadata(
        [property: JsonPropertyName("assistant_type")]
        string AssistantType,
        [property: JsonPropertyName("parameters")]
        Dictionary<string, object>? Parameters
    );
}
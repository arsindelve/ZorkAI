using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using JetBrains.Annotations;

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

    /// <summary>
    /// Parses conversation input and returns whether the response is "No" and the actual response.
    /// </summary>
    /// <param name="input">The conversation input to parse</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>A tuple where Item1 is true if response is "No", Item2 is the response (empty if "No")</returns>
    private async Task<(bool isNo, string response)> ParseAsync(string input, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[PARSE CONVERSATION DEBUG] Starting ParseAsync with input: '{input}'");
        
        if (string.IsNullOrWhiteSpace(input))
        {
            Console.WriteLine("[PARSE CONVERSATION DEBUG] Input is null or whitespace, throwing exception");
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
            Console.WriteLine($"[PARSE CONVERSATION DEBUG] Request JSON: {requestJson}");

            // Set up the Lambda invoke request
            var invokeRequest = new InvokeRequest
            {
                FunctionName = FunctionName,
                Payload = requestJson
            };
            Console.WriteLine($"[PARSE CONVERSATION DEBUG] Invoking Lambda function: {FunctionName}");

            // Invoke the Lambda function
            var response = await _lambdaClient.InvokeAsync(invokeRequest, cancellationToken);
            Console.WriteLine($"[PARSE CONVERSATION DEBUG] Lambda response status code: {response.StatusCode}");

            // Process the response
            if (response.StatusCode != 200)
            {
                Console.WriteLine($"[PARSE CONVERSATION DEBUG] Lambda invocation failed with status code: {response.StatusCode}");
                throw new Exception($"Lambda invocation failed with status code: {response.StatusCode}");
            }

            // Read the response payload
            using var reader = new StreamReader(response.Payload);
            var responseJson = await reader.ReadToEndAsync(cancellationToken);
            Console.WriteLine($"[PARSE CONVERSATION DEBUG] Raw Lambda response JSON: {responseJson}");

            // Parse the Lambda response
            var lambdaResponse = JsonSerializer.Deserialize<LambdaResponse>(responseJson);
            Console.WriteLine($"[PARSE CONVERSATION DEBUG] Deserialized Lambda response - StatusCode: {lambdaResponse?.StatusCode}, Body length: {lambdaResponse?.Body?.Length ?? 0}");

            if (lambdaResponse == null)
            {
                Console.WriteLine("[PARSE CONVERSATION DEBUG] Failed to deserialize Lambda response");
                throw new Exception("Failed to deserialize Lambda response");
            }

            // Parse the body content
            Console.WriteLine($"[PARSE CONVERSATION DEBUG] Lambda response body: {lambdaResponse.Body}");
            var bodyContent = JsonSerializer.Deserialize<BodyContent>(lambdaResponse.Body);
            Console.WriteLine($"[PARSE CONVERSATION DEBUG] Deserialized body content - Results null: {bodyContent?.Results == null}");

            if (bodyContent?.Results == null)
            {
                Console.WriteLine($"[PARSE CONVERSATION DEBUG] Failed to extract results from Lambda response. Body: {lambdaResponse.Body}");
                throw new Exception($"Failed to extract results from Lambda response. Body: {lambdaResponse.Body}");
            }
            
            Console.WriteLine($"[PARSE CONVERSATION DEBUG] Results.Response: '{bodyContent.Results.Response}'");
            if (bodyContent.Results.Response == null)
            {
                Console.WriteLine($"[PARSE CONVERSATION DEBUG] Failed to extract message from Lambda response. Results: {JsonSerializer.Serialize(bodyContent.Results)}");
                throw new Exception($"Failed to extract message from Lambda response. Results: {JsonSerializer.Serialize(bodyContent.Results)}");
            }

            var lambdaResponseText = bodyContent.Results.Response;
            Console.WriteLine($"[PARSE CONVERSATION DEBUG] Lambda response text: '{lambdaResponseText}'");
            
            // Check if the response is "No" (case-insensitive)
            var trimmedResponse = lambdaResponseText.Trim();
            bool isNo = string.Equals(trimmedResponse, "No", StringComparison.OrdinalIgnoreCase);
            Console.WriteLine($"[PARSE CONVERSATION DEBUG] Trimmed response: '{trimmedResponse}', isNo: {isNo}");
            
            var finalResult = (isNo, isNo ? string.Empty : lambdaResponseText);
            Console.WriteLine($"[PARSE CONVERSATION DEBUG] Final result: isNo={finalResult.Item1}, response='{finalResult.Item2}'");
            
            return finalResult;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PARSE CONVERSATION DEBUG] Exception occurred: {ex.Message}");
            Console.WriteLine($"[PARSE CONVERSATION DEBUG] Stack trace: {ex.StackTrace}");
            throw new Exception($"Error communicating with Parse Lambda: {ex.Message}", ex);
        }
    }

    // Records for JSON deserialization (same as ChatWithFloyd)
    private record LambdaResponse(
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
        string Response
    );
}
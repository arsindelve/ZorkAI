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
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Input cannot be empty", nameof(input));

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

            // Set up the Lambda invoke request
            var invokeRequest = new InvokeRequest
            {
                FunctionName = FunctionName,
                Payload = requestJson
            };

            // Invoke the Lambda function
            var response = await _lambdaClient.InvokeAsync(invokeRequest, cancellationToken);

            // Process the response
            if (response.StatusCode != 200)
                throw new Exception($"Lambda invocation failed with status code: {response.StatusCode}");

            // Read the response payload
            using var reader = new StreamReader(response.Payload);
            var responseJson = await reader.ReadToEndAsync(cancellationToken);

            // Parse the Lambda response
            var lambdaResponse = JsonSerializer.Deserialize<LambdaResponse>(responseJson);

            if (lambdaResponse == null) throw new Exception("Failed to deserialize Lambda response");


            // Parse the body content
            var bodyContent = JsonSerializer.Deserialize<BodyContent>(lambdaResponse.Body);

            if (bodyContent?.Results == null)
                throw new Exception($"Failed to extract results from Lambda response. Body: {lambdaResponse.Body}");
            
            if (bodyContent.Results.Response == null)
                throw new Exception($"Failed to extract message from Lambda response. Results: {JsonSerializer.Serialize(bodyContent.Results)}");

            var lambdaResponseText = bodyContent.Results.Response;
            
            // Check if the response is "No" (case-insensitive)
            bool isNo = string.Equals(lambdaResponseText.Trim(), "No", StringComparison.OrdinalIgnoreCase);
            
            return (isNo, isNo ? string.Empty : lambdaResponseText);
        }
        catch (Exception ex)
        {
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
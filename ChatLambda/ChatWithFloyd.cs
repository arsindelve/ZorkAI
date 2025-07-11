using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon;
using Amazon.Lambda;
using Amazon.Lambda.Model;

namespace ChatLambda;

public class ChatWithFloyd
{
    private const string FunctionName = "Floyd-LangGraphFunction-GefcykaLqwp-MySimpleLambda-lBRzg1jLKvXP";
    private readonly IAmazonLambda _lambdaClient;

    public ChatWithFloyd(IAmazonLambda? lambdaClient)
    {
        _lambdaClient = lambdaClient ?? CreateDefaultLambdaClient();
    }

    private IAmazonLambda CreateDefaultLambdaClient()
    {
        // This will use the AWS credentials from the environment or AWS credential profile
        // It will automatically use credentials from:
        // - Environment variables (AWS_ACCESS_KEY_ID, AWS_SECRET_ACCESS_KEY)
        // - AWS credential profiles
        // - EC2 Instance Profiles if running on EC2
        // - ECS Task Role if running in ECS
        return new AmazonLambdaClient(RegionEndpoint.USEast1); // Adjust the region as needed
    }

    // Example usage:   
    /// <summary>
    /// Sends a question to the Floyd Lambda function and returns the response.
    /// </summary>
    /// <param name="prompt">The question to ask Floyd</param>
    /// <returns>Floyd's response as a string</returns>
    public async Task<string> AskFloydAsync(string prompt)
    {
        return await AskFloydAsync(prompt, CancellationToken.None);
    }

    /// <summary>
    /// Sends a question to the Floyd Lambda function and returns the response.
    /// </summary>
    /// <param name="prompt">The question to ask Floyd</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>Floyd's response as a string</returns>
    private async Task<string> AskFloydAsync(string prompt, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            throw new ArgumentException("Question cannot be empty", nameof(prompt));

        try
        {
            // Create the request payload
            var request = new
            {
                prompt = prompt,
                assistant = "router",
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

            if (bodyContent?.Results.Response == null)
                throw new Exception("Failed to extract message from Lambda response");

            return bodyContent.Results.Response;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error communicating with Floyd Lambda: {ex.Message}", ex);
        }
    }

    // Records for JSON deserialization
    private record LambdaResponse(
        [property: JsonPropertyName("statusCode")]
        int StatusCode,
        [property: JsonPropertyName("body")] string Body
    );

    private record BodyContent(
        [property: JsonPropertyName("results")]
        Results Results
    );

    private record Results(
        [property: JsonPropertyName("single_message")]
        string Response
    );
}
using Amazon.Lambda;

namespace ChatLambda;

public class ChatWithAmbassador(IAmazonLambda? lambdaClient) : ChatWithCompanion(lambdaClient)
{
    protected override string AssistantName => "ambassador";

    /// <summary>
    /// Sends a question to the Ambassador Lambda function and returns the response.
    /// </summary>
    /// <param name="prompt">The question to ask Ambassador</param>
    /// <returns>Ambassador's response including message and metadata</returns>
    public async Task<CompanionResponse> AskAmbassadorAsync(string prompt)
    {
        return await AskCompanionAsync(prompt);
    }
}
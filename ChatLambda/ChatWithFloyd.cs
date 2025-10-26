using Amazon.Lambda;

namespace ChatLambda;

public class ChatWithFloyd(IAmazonLambda? lambdaClient) : ChatWithCompanion(lambdaClient)
{
    protected override string AssistantName => "floyd";

    /// <summary>
    /// Sends a question to the Floyd Lambda function and returns the response.
    /// </summary>
    /// <param name="prompt">The question to ask Floyd</param>
    /// <returns>Floyd's response including message and metadata</returns>
    public async Task<CompanionResponse> AskFloydAsync(string prompt)
    {
        return await AskCompanionAsync(prompt);
    }
}
using Amazon.Lambda;

namespace ChatLambda;

public class ChatWithBlather(IAmazonLambda? lambdaClient) : ChatWithCompanion(lambdaClient)
{
    protected override string AssistantName => "blather";

    /// <summary>
    /// Sends a question to the Blather Lambda function and returns the response.
    /// </summary>
    /// <param name="prompt">The question to ask Blather</param>
    /// <returns>Blather's response as a string</returns>
    public async Task<string> AskBlatherAsync(string prompt)
    {
        return await AskCompanionAsync(prompt);
    }
}
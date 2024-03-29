using System.Diagnostics;
using Azure;
using Azure.AI.OpenAI;
using Model;
using Model.AIGeneration;
using Model.AIGeneration.Requests;

namespace OpenAI;

/// <summary>
///     Represents a client for interacting with OpenAI API to generate text.
/// </summary>
public class ChatGPTClient : IGenerationClient
{
    private readonly OpenAIClient _client;

    public ChatGPTClient()
    {
        var key = Environment.GetEnvironmentVariable("OPEN_AI_KEY");

        if (string.IsNullOrEmpty(key))
            throw new Exception("Missing environment variable OPEN_AI_KEY");

        _client = new OpenAIClient(key);
    }

    /// <summary>
    ///     Completes a chat conversation using the OpenAI API.
    /// </summary>
    /// <param name="request">The request object containing the system and user messages for the chat conversation.</param>
    /// <returns>The generated response message from the chat conversation.</returns>
    public async Task<string> CompleteChat(Request request)
    {
        return "Generated Result";

        Debug.WriteLine($"Sending request of type: {request.GetType().Name} ");

        var chatCompletionsOptions = new ChatCompletionsOptions
        {
            // gpt-3.5-turbo
            // gpt-4-turbo-preview
            DeploymentName = "gpt-4-turbo-preview",
            Messages =
            {
                new ChatRequestSystemMessage(request.SystemMessage),
                new ChatRequestUserMessage(request.UserMessage)
            }
        };

        Debug.WriteLine(request.UserMessage);

        Response<ChatCompletions> response = await _client.GetChatCompletionsAsync(chatCompletionsOptions);
        var responseMessage = response.Value.Choices[0].Message;
        return responseMessage.Content;
    }
}
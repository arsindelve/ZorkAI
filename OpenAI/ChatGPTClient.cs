using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Logging;
using Model.AIGeneration;
using Model.AIGeneration.Requests;

namespace OpenAI;

/// <summary>
///     Represents a client for interacting with OpenAI API to generate text.
/// </summary>
public class ChatGPTClient : IGenerationClient
{
    private readonly OpenAIClient _client;
    private readonly ILogger? _logger;

    public ChatGPTClient(ILogger? logger)
    {
        _logger = logger;
        var key = Environment.GetEnvironmentVariable("OPEN_AI_KEY");

        if (string.IsNullOrEmpty(key))
            throw new Exception("Missing environment variable OPEN_AI_KEY");

        _client = new OpenAIClient(key);
    }

    public Action? OnGenerate { get; set; }

    public List<(string, string, bool)> LastFiveInputOutputs { get; set; } = new();

    /// <summary>
    ///     Completes a chat conversation using the OpenAI API.
    /// </summary>
    /// <param name="request">The request object containing the system and user messages for the chat conversation.</param>
    /// <returns>The generated response message from the chat conversation.</returns>
    public async Task<string> CompleteChat(Request request)
    {
        _logger?.LogDebug($"Sending request of type: {request.GetType().Name} ");

        var chatCompletionsOptions = new ChatCompletionsOptions
        {
            // gpt-3.5-turbo
            // gpt-4-turbo-preview
            DeploymentName = "gpt-4o",
            Messages =
            {
                new ChatRequestSystemMessage(request.SystemMessage)
            }
        };

        var reverse = LastFiveInputOutputs.ToList();
        reverse.Reverse();

        // This will get the most recent generated inputs and outputs, stopping when we hit 
        // a non-generated response. We're going to pass those to the AI, as it will create
        // a conversational back-and-forth. 
        List<(string, string)> lastGeneratedResults = reverse
            
            .TakeWhile(s => s.Item3)
            .Select(s => (s.Item1, s.Item2))
            .ToList();
        
        // Add the history
        lastGeneratedResults.Reverse();

        foreach ((string input, string output) next in lastGeneratedResults)
        {
            chatCompletionsOptions.Messages.Add(new ChatRequestUserMessage(next.input));
            chatCompletionsOptions.Messages.Add(new ChatRequestAssistantMessage(next.output));
        }

        // Add the most recent request
        chatCompletionsOptions.Messages.Add(new ChatRequestUserMessage(request.UserMessage));

        _logger?.LogDebug(request.UserMessage);

        Response<ChatCompletions> response = await _client.GetChatCompletionsAsync(chatCompletionsOptions);
        var responseMessage = response.Value.Choices[0].Message;

        OnGenerate?.Invoke();

        return responseMessage.Content;
    }
}
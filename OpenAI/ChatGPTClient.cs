using System.Text;
using Azure;
using Azure.AI.OpenAI;
using CloudWatch;
using CloudWatch.Model;
using Microsoft.Extensions.Logging;
using Model.AIGeneration;
using Model.AIGeneration.Requests;

namespace OpenAI;

/// <summary>
///     Represents a client for interacting with OpenAI API to generate text.
/// </summary>
public class ChatGPTClient(ILogger? logger) : OpenAIClientBase(logger), IGenerationClient
{
    protected override string DeploymentName => "gpt-4o";

    public Action? OnGenerate { get; set; }

    public string? SystemPrompt { private get; set; }

    public List<(string, string, bool)> LastFiveInputOutputs { get; set; } = new();

    public Guid TurnCorrelationId { get; set; }

    public ICloudWatchLogger<GenerationLog>? CloudWatchLogger { get; set; }

    /// <summary>
    ///     Completes a chat conversation using the OpenAI API.
    /// </summary>
    /// <param name="request">The request object containing the system and user messages for the chat conversation.</param>
    /// <param name="systemPromptAddendum"></param>
    /// <returns>The generated response message from the chat conversation.</returns>
    public async Task<string> GenerateNarration(Request request, string systemPromptAddendum)
    {
        Logger?.LogDebug($"Sending request of type: {request.GetType().Name} ");

        var chatCompletionsOptions = GetChatCompletionsOptions(SystemPrompt + systemPromptAddendum, request.Temperature);

        var reverse = LastFiveInputOutputs.ToList();
        reverse.Reverse();

        StringBuilder lastInputs = new();
        lastInputs.AppendLine(
            "For reference and context, here are the player's last five interactions with the narrator:");

        foreach (var tuple in reverse) lastInputs.AppendLine($"Input: {tuple.Item1}. Output: {tuple.Item2}");

        chatCompletionsOptions.Messages.Add(new ChatRequestSystemMessage(lastInputs.ToString()));

        // Add the most recent request
        chatCompletionsOptions.Messages.Add(new ChatRequestUserMessage(request.UserMessage));

        Logger?.LogDebug(request.UserMessage);

        Response<ChatCompletions> response = await Client.GetChatCompletionsAsync(chatCompletionsOptions);
        var responseMessage = response.Value.Choices[0].Message;

        OnGenerate?.Invoke();
        Log(request, responseMessage, SystemPrompt + systemPromptAddendum);

        return responseMessage.Content;
    }

    private void Log(Request request, ChatResponseMessage responseMessage, string systemMessage)
    {
        if (!string.IsNullOrEmpty(request.UserMessage))
            CloudWatchLogger?.WriteLogEvents(new GenerationLog
            {
                SystemPrompt = systemMessage,
                Temperature = request.Temperature,
                LanguageModel = DeploymentName,
                UserPrompt = request.UserMessage,
                Response = responseMessage.Content,
                TurnCorrelationId = TurnCorrelationId.ToString()
            });
    }

    public async Task<string> GenerateCompanionSpeech(CompanionRequest request)
    {
        var chatCompletionsOptions = GetChatCompletionsOptions(request.SystemMessage, request.Temperature);
        chatCompletionsOptions.Messages.Add(new ChatRequestUserMessage(request.UserMessage));

        Response<ChatCompletions> response = await Client.GetChatCompletionsAsync(chatCompletionsOptions);
        var responseMessage = response.Value.Choices[0].Message;
        
        Log(request, responseMessage, request.SystemMessage);

        return responseMessage.Content;
    }
}
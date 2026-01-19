using System.Text;
using CloudWatch;
using CloudWatch.Model;
using Microsoft.Extensions.Logging;
using Model.AIGeneration;
using Model.AIGeneration.Requests;
using OpenAI.Chat;

namespace ZorkAI.OpenAI;

/// <summary>
///     Represents a client for interacting with ZorkAI.OpenAI API to generate text.
/// </summary>
public class ChatGPTClient(ILogger? logger) : OpenAIClientBase(logger), IGenerationClient
{
    protected override string ModelName => "gpt-4o";

    public Action? OnGenerate { get; set; }

    public bool IsDisabled { get; set; }

    public string? SystemPrompt { private get; set; }

    public List<(string, string, bool)> LastFiveInputOutputs { get; set; } = new();

    public Guid TurnCorrelationId { get; set; }

    public ICloudWatchLogger<GenerationLog>? CloudWatchLogger { get; set; }

    /// <summary>
    ///     Completes a chat conversation using the ZorkAI.OpenAI API.
    /// </summary>
    /// <param name="request">The request object containing the system and user messages for the chat conversation.</param>
    /// <param name="systemPromptAddendum"></param>
    /// <returns>The generated response message from the chat conversation.</returns>
    public async Task<string> GenerateNarration(Request request, string systemPromptAddendum)
    {
        if (IsDisabled)
            return "This action or command has no effect on the game. ";

        Logger?.LogDebug($"Sending request of type: {request.GetType().Name} ");

        var messages = new List<ChatMessage>();

        // Add system prompt
        messages.Add(new SystemChatMessage(SystemPrompt + systemPromptAddendum));

        // Add context from last interactions
        var reverse = LastFiveInputOutputs.ToList();
        reverse.Reverse();

        StringBuilder lastInputs = new();
        lastInputs.AppendLine(
            "For reference and context, here are the player's last five interactions with the narrator:");

        foreach (var tuple in reverse) lastInputs.AppendLine($"Input: {tuple.Item1}. Output: {tuple.Item2}");

        messages.Add(new SystemChatMessage(lastInputs.ToString()));

        // Add the most recent request
        messages.Add(new UserChatMessage(request.UserMessage));

        Logger?.LogDebug(request.UserMessage);

        var options = new ChatCompletionOptions
        {
            Temperature = request.Temperature
        };

        ChatCompletion completion = await Client!.CompleteChatAsync(messages, options);
        var responseContent = completion.Content[0].Text;

        if (string.IsNullOrEmpty(responseContent))
            return "The narrator is silent. ";

        OnGenerate?.Invoke();
        Log(request, responseContent, SystemPrompt + systemPromptAddendum);

        return responseContent;
    }

    private void Log(Request request, string responseContent, string systemMessage)
    {
        if (!string.IsNullOrEmpty(request.UserMessage))
            CloudWatchLogger?.WriteLogEvents(new GenerationLog
            {
                SystemPrompt = systemMessage,
                Temperature = request.Temperature,
                LanguageModel = ModelName,
                UserPrompt = request.UserMessage,
                Response = responseContent,
                TurnCorrelationId = TurnCorrelationId.ToString()
            });
    }

    public async Task<string> GenerateCompanionSpeech(CompanionRequest request)
    {
        if (IsDisabled)
            return "This action or command has no effect on the game. ";

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(request.SystemMessage),
            new UserChatMessage(request.UserMessage)
        };

        var options = new ChatCompletionOptions
        {
            Temperature = request.Temperature
        };

        ChatCompletion completion = await Client!.CompleteChatAsync(messages, options);
        var responseContent = completion.Content[0].Text;

        if (string.IsNullOrEmpty(responseContent))
            return "Your companion says nothing. ";

        Log(request, responseContent, request.SystemMessage);

        return responseContent;
    }
}

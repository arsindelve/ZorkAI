using System.Diagnostics;
using System.Dynamic;
using Amazon.BedrockRuntime;
using CloudWatch;
using CloudWatch.Model;
using Model.AIGeneration;
using Model.AIGeneration.Requests;
using Newtonsoft.Json;

namespace Bedrock;

/// <summary>
///     This is a response generator for Anthropic "Claude" via AWS Bedrock. It will
///     use the default AWS CLI credentials, so no additional environment variables or keys
///     are required.
/// </summary>
public class ClaudeFourGenerationClient : ClaudeClientBase, IGenerationClient
{
    public Action? OnGenerate { get; set; }

    public required string SystemPrompt { private get; set; }
    
    public List<(string, string, bool)> LastFiveInputOutputs { get; set; } = new();
    
    public Guid TurnCorrelationId { get; set; }
    
    public ICloudWatchLogger<GenerationLog>? Logger { get; set; }

    public async Task<string> CompleteChat(Request request)
    {
        Debug.WriteLine($"Sending request of type: {request.GetType().Name} ");
        Debug.WriteLine($"Prompt says: {request.UserMessage}");

        var jsonString = BuildPayload(request);

        var generatedText = "";
        try
        {
            generatedText = await GenerateResponse(jsonString);
        }
        catch (AmazonBedrockRuntimeException e)
        {
            Console.WriteLine(e.Message);
        }

        OnGenerate?.Invoke();
        return generatedText ?? string.Empty;
    }

    private string BuildPayload(Request request)
    {
        dynamic message;
        dynamic textContent;

        // This will get the most recent generated inputs and outputs, stopping when we hit 
        // a non-generated response. We're going to pass those to the AI, as it will create
        // a conversational back-and-forth. 
        var lastGeneratedResults = LastFiveInputOutputs
            .TakeWhile(s => s.Item3)
            .Select(s => (s.Item1, s.Item2))
            .ToList();

        dynamic payload = new ExpandoObject();

        payload.anthropic_version = AnthropicVersion;
        payload.max_tokens = 1024;
        payload.temperature = 0.45;
        payload.system = SystemPrompt;

        payload.messages = new List<dynamic>();

        foreach ((string input, string output) next in lastGeneratedResults)
        {
            // Add the adventurer input
            message = new ExpandoObject();
            message.role = "user";
            message.content = new List<dynamic>();
            textContent = new ExpandoObject();
            textContent.type = "text";
            textContent.text = next.input;
            message.content.Add(textContent);
            payload.messages.Add(message);

            // Add the AI generated response 
            message = new ExpandoObject();
            message.role = "assistant";
            message.content = new List<dynamic>();
            textContent = new ExpandoObject();
            textContent.type = "text";
            textContent.text = next.output;
            message.content.Add(textContent);
            payload.messages.Add(message);
        }

        message = new ExpandoObject();
        message.role = "user";
        message.content = new List<dynamic>();

        textContent = new ExpandoObject();
        textContent.type = "text";
        textContent.text = request.UserMessage ?? string.Empty;

        message.content.Add(textContent);
        payload.messages.Add(message);

        string jsonString = JsonConvert.SerializeObject(payload, Formatting.Indented);
        return jsonString;
    }
}
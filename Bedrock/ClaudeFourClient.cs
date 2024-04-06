using System.Diagnostics;
using System.Dynamic;
using System.Net;
using Amazon;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Amazon.Util;
using Model.AIGeneration;
using Model.AIGeneration.Requests;
using Newtonsoft.Json;

namespace Bedrock;

/// <summary>
///     This is a response generator for Anthropic "Claude" via AWS Bedrock. It will
///     use the default AWS CLI credentials, so no additional environment variables or keys
///     are required.
/// </summary>
public class ClaudeFourClient : IGenerationClient
{
    //private const string ClaudeModelId = "anthropic.claude-3-sonnet-20240229-v1:0";
    private const string ClaudeModelId = "anthropic.claude-3-haiku-20240307-v1:0";
    private const string AnthropicVersion = "bedrock-2023-05-31";

    public Action? OnGenerate { get; set; }

    public List<(string, string, bool)> LastFiveInputOutputs { get; set; } = new();

    public async Task<string> CompleteChat(Request request)
    {
        Debug.WriteLine($"Sending request of type: {request.GetType().Name} ");
        Debug.WriteLine($"Prompt says: {request.UserMessage}");

        AmazonBedrockRuntimeClient client = new(RegionEndpoint.USEast1);

        var jsonString = BuildPayload(request);

        var generatedText = "";
        try
        {
            var response = await client.InvokeModelAsync(new InvokeModelRequest
            {
                ModelId = ClaudeModelId,
                Body = AWSSDKUtils.GenerateMemoryStreamFromString(jsonString),
                ContentType = "application/json",
                Accept = "application/json"
            });

            if (response.HttpStatusCode == HttpStatusCode.OK)
            {
                using var streamReader = new StreamReader(response.Body);
                var content = await streamReader.ReadToEndAsync();
                var responseContent = JsonConvert.DeserializeObject<dynamic>(content) ??
                                      throw new InvalidOperationException();

                generatedText = responseContent.content[0].text;
            }
        }
        catch (AmazonBedrockRuntimeException e)
        {
            Console.WriteLine(e.Message);
        }

        OnGenerate?.Invoke();
        return generatedText;
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
        payload.system = request.SystemMessage ?? string.Empty;

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
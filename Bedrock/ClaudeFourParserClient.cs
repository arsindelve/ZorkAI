using System.Dynamic;
using Model;
using Newtonsoft.Json;

namespace Bedrock;

public interface IClaudeFourParserClient
{
    string LanguageModel { get; }
    Task<string?> GetResponse(string location, string input);
}

internal class ClaudeFourParserClient : ClaudeClientBase, IClaudeFourParserClient
{
    public Task<string?> GetResponse(string location, string input)
    {
        var request = string.Format(ParsingHelper.Prompt, location, input);
        return GenerateResponse(BuildPayload(request));
    }

    public string LanguageModel => ClaudeModelId;

    private string BuildPayload(string request)
    {
        dynamic payload = new ExpandoObject();

        payload.anthropic_version = AnthropicVersion;
        payload.max_tokens = 1024;
        payload.temperature = 0;

        dynamic message = new ExpandoObject();

        payload.messages = new List<dynamic>();

        message.role = "user";
        message.content = new List<dynamic>();

        dynamic textContent = new ExpandoObject();
        textContent.type = "text";
        textContent.text = request;

        message.content.Add(textContent);
        payload.messages.Add(message);

        string jsonString = JsonConvert.SerializeObject(payload, Formatting.Indented);
        return jsonString;
    }
}
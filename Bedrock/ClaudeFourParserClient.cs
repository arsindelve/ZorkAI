using System.Dynamic;
using Newtonsoft.Json;

namespace Bedrock;

public interface IClaudeFourParserClient
{
    Task<string?> GetResponse(string location, string input);
}

internal class ClaudeFourParserClient : ClaudeClientBase, IClaudeFourParserClient
{
    private const string Prompt =
        """
        You are a parser for an interactive fiction game. The player is in this location: "{0}"
        
        I need to know the player's intent. Given the sentence "{1}":
        
        1. Tell me in <intent> tags if:
            a) If the player is expressing a desire to move, enter, go in, or travel somewhere, put "Move"
            b) If the player wants to enter a vehicle or sub-location, put "Board"
            c) If the player wants to exit a vehicle or sub-location, put "Disembark"
            d) Something else, put "Act"
             
        2. In <verb> tags, put the single most important verb I need to know, which best expresses the player's intention. If there is a simpler, more common synonym for the verb, use that instead.
        
        3. For each noun in the sentence that relates to the main verb, place each noun in a set of <noun> tags. If there is an adjective immediately preceding a noun, include it in separate <adjective> tags outside the <noun> tags. 
        Use context to distinguish between adjectives and nouns. Tag words as adjectives only if they immediately precede a noun and fit common adjective patterns.
        
        4. If there are two nouns, in <preposition> tags, put the preposition which connects the nouns. Otherwise, omit these tags.
        
        5. If the sentence expresses a desire to move in a certain direction or fo a certain way based on their current location, put in <direction> tags the exact word from this list which 
        best describes where they want to go: "in, out, enter, exit, up, down, east, west, north, south, north-west, north-east, south-west, or south-east." If the sentence includes a term like 
        'follow' or 'go towards' combined with a specific location described in the player's current environment, use the corresponding direction. If you cannot match any of these words, put "other."
        
        
        
        Do not provide any analysis or explanation, just the tags.
        """;

    public Task<string?> GetResponse(string location, string input)
    {
        var request = string.Format(Prompt, location, input);
        return GenerateResponse(BuildPayload(request));
    }

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
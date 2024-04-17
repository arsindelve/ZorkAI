using HtmlAgilityPack;
using Model;
using Model.AIParsing;
using Model.Intent;
using Microsoft.Extensions.Logging;

namespace Bedrock;

public class ClaudeFourParser : ClaudeClientBase, IAIParser
{
    private readonly ILogger? _logger;
    private readonly IClaudeFourParserClient _client;

    /// <summary>
    ///     Constructor for unit testing.
    /// </summary>
    /// <param name="client"></param>
    public ClaudeFourParser(IClaudeFourParserClient client)
    {
        _client = client;
    }

    public ClaudeFourParser(ILogger? logger)
    {
        _logger = logger;
        _client = new ClaudeFourParserClient();
    }

    public async Task<IntentBase> AskTheAIParser(string input, string locationDescription, string sessionId)
    {
        var response = await _client.GetResponse(locationDescription, input);
        _logger?.LogDebug($"Response from Claude Parser: {response}");

        MoveIntent? moveIntent = DetermineMoveIntent(response?.ToLowerInvariant());
        if (moveIntent != null)
            return moveIntent;

        IntentBase? actionIntent = DetermineActionIntent(response?.ToLowerInvariant());
        if (actionIntent != null)
            return actionIntent;

        return new NullIntent();
    }

    private IntentBase? DetermineActionIntent(string? response)
    {
        string? intentTag = ExtractElementsByTag(response, "intent").SingleOrDefault();
        if (string.IsNullOrEmpty(intentTag))
            return null;
         
        if (intentTag != "act")
            return null;
        
        string? verbTag = ExtractElementsByTag(response, "verb").SingleOrDefault();
        if (string.IsNullOrEmpty(verbTag))
            return null;

        var nouns = ExtractElementsByTag(response, "noun");
        if (!nouns.Any())
            return null;

        if (nouns.Count == 1)
            return new SimpleIntent { Verb = verbTag, Noun = nouns.Single(), OriginalInput = response ?? ""};

        return null;

    }

    private MoveIntent? DetermineMoveIntent(string? response)
    {
        var intentTag = ExtractElementsByTag(response, "intent").SingleOrDefault();
        if (string.IsNullOrEmpty(intentTag))
            return null;

        if (intentTag != "move")
            return null;

        var directionTag = ExtractElementsByTag(response, "direction").SingleOrDefault();
        if (string.IsNullOrEmpty(directionTag))
            return null;

        var direction = DirectionParser.ParseDirection(directionTag);

        return direction == Direction.Unknown ? null : new MoveIntent { Direction = direction };
    }


    private static List<string> ExtractElementsByTag(string? response, string tag)
    {
        var list = new List<string>();
        var doc = new HtmlDocument();
        doc.LoadHtml(response);

        var nodes = doc.DocumentNode.SelectNodes($"//{tag}");
        if (nodes != null)
            foreach (var node in nodes)
                list.Add(node.InnerText.Trim());

        return list;
    }
}
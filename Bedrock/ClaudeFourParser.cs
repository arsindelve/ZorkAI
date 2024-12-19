using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Model.AIParsing;
using Model.Intent;
using Model.Movement;

namespace Bedrock;

public class ClaudeFourParser : ClaudeClientBase, IAIParser
{
    private readonly IClaudeFourParserClient _client;
    private readonly ILogger? _logger;

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
        _logger?.LogDebug($"Response from Claude Parser: \n\t{response}");

        var boardIntent = DetermineBoardIntent(response?.ToLower());
        if (boardIntent != null)
            return boardIntent;

        var disembarkIntent = DetermineDisembarkIntent(response?.ToLower());
        if (disembarkIntent != null)
            return disembarkIntent;

        var moveIntent = DetermineMoveIntent(response?.ToLowerInvariant());
        if (moveIntent != null)
            return moveIntent;

        var actionIntent = DetermineActionIntent(response?.ToLowerInvariant(), input);
        if (actionIntent != null)
            return actionIntent;

        return new NullIntent();
    }

    public string LanguageModel => _client.LanguageModel;

    private ExitSubLocationIntent? DetermineDisembarkIntent(string? response)
    {
        var intentTag = ExtractElementsByTag(response, "intent").SingleOrDefault();
        if (string.IsNullOrEmpty(intentTag))
            return null;

        if (intentTag != "disembark")
            return null;

        var nouns = ExtractElementsByTag(response, "noun");
        if (!nouns.Any())
            return null;

        return new ExitSubLocationIntent { NounOne = nouns.First(), NounTwo = nouns.LastOrDefault()};
    }

    private EnterSubLocationIntent? DetermineBoardIntent(string? response)
    {
        var intentTag = ExtractElementsByTag(response, "intent").SingleOrDefault();
        if (string.IsNullOrEmpty(intentTag))
            return null;

        if (intentTag != "board")
            return null;

        var nouns = ExtractElementsByTag(response, "noun");
        if (!nouns.Any())
            return null;

        return new EnterSubLocationIntent { Noun = nouns.First() };
    }

    private IntentBase? DetermineActionIntent(string? response, string originalInput)
    {
        var intentTag = ExtractElementsByTag(response, "intent").SingleOrDefault();
        if (string.IsNullOrEmpty(intentTag))
        {
            _logger?.LogDebug("No intent tag was found trying to make an act intent");
            return null;
        }

        if (intentTag != "act")
        {
            _logger?.LogDebug("The intent tag was not 'act' trying to make an act intent");
            return null;
        }

        var verbTag = ExtractElementsByTag(response, "verb").SingleOrDefault();
        if (string.IsNullOrEmpty(verbTag))
        {
            _logger?.LogDebug("No verb was found trying to make an act intent");
            return null;
        }

        var prepositionTag = ExtractElementsByTag(response, "preposition").SingleOrDefault();

        var nouns = ExtractElementsByTag(response, "noun");
        if (!nouns.Any())
            return null;

        if (nouns.Count == 1)
        {
            var adjectives = ExtractElementsByTag(response, "adjective").FirstOrDefault();
            return new SimpleIntent
            {
                Verb = verbTag,
                Noun = nouns.Single(),
                Adverb = prepositionTag,
                Adjective = adjectives,
                OriginalInput = originalInput
            };
        }

        if (nouns.Count == 2)
        {
            if (string.IsNullOrEmpty(prepositionTag))
            {
                _logger?.LogDebug("No preposition was found trying to make a MultiNoun intent");
                // TODO: Claude is inconsistent giving us the preposition. For now, hardcode the most common one if we don't get it. 
                prepositionTag = "with";
            }

            return new MultiNounIntent
            {
                NounOne = nouns[0],
                NounTwo = nouns[1],
                Verb = verbTag,
                Preposition = prepositionTag,
                OriginalInput = originalInput
            };
        }

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
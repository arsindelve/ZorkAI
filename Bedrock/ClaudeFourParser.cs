using Microsoft.Extensions.Logging;
using Model;
using Model.AIParsing;
using Model.Intent;

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

    public string LanguageModel => _client.LanguageModel;

    public async Task<IntentBase> AskTheAIParser(string input, string locationDescription, string sessionId)
    {
        var response = await _client.GetResponse(locationDescription, input);
        _logger?.LogDebug($"Response from Claude Parser: \n\t{response}");

        return ParsingHelper.GetIntent(input, response, _logger);
    }
}
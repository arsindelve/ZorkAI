using Amazon;
using Amazon.LexRuntimeV2;
using Amazon.LexRuntimeV2.Model;

namespace Lex;

/// <summary>
///     This class uses the AWS Lex product to take the user's input, and determine
///     the user's intent.
/// </summary>
internal class LexClient
{
    private readonly AmazonLexRuntimeV2Client _client = new(RegionEndpoint.USEast1);

    /// <summary>
    ///     Sends a text message to the AWS Lex service and recognizes the user's input,
    ///     determining the user's intent.
    /// </summary>
    /// <param name="messageToSend">The text message to send to Lex.</param>
    /// <param name="sessionId">The unique session ID.</param>
    /// <returns>
    ///     A task representing the asynchronous operation. The result of the task is an object of type
    ///     <see cref="RecognizeTextResponse" />.
    /// </returns>
    internal async Task<RecognizeTextResponse> SendTextMsgToLex(string messageToSend, string sessionId)
    {
        var lexTextRequest = new RecognizeTextRequest
        {
            BotId = "EEKHVST8KF",
            BotAliasId = "2RSS2SDO8I",
            Text = messageToSend,
            SessionId = sessionId,
            LocaleId = "en_US"
        };

        var lexTextResponse = await _client.RecognizeTextAsync(lexTextRequest);
        return lexTextResponse;
    }
}
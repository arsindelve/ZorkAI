using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CloudWatch.Model;

public record TurnLog : ITurnBasedLog
{
    public required string SessionId { get; init; }
    public required string Location { get; init; }
    public required int Score { get; init; }
    public required int Moves { get; init; }
    public required string Input { get; init; }
    public required string Response { get; init; }
    public string? TurnCorrelationId { get; set; }

    /// <summary>
    ///     The shape the parser gave this turn's input — the intent subtype plus the fields that
    ///     actually drive dispatch (verb/noun/adverb, or noun one/noun two/preposition). Parse
    ///     metadata only: nothing here is raw player text beyond what <see cref="Input" /> already
    ///     captures (issue #578).
    ///     <para>
    ///     Null when the turn never reached the parser — a stateful processor answering its own
    ///     prompt, an empty command, a location's raw-input interaction, a conversation — which is
    ///     itself the diagnostic fact.
    ///     </para>
    /// </summary>
    public string? ParsedIntent { get; init; }

    /// <summary>
    ///     Which branch of the engine ended this turn: a handler, one of the no-handler deflections,
    ///     or a parse that produced nothing usable. Serialized by name so a log reader sees
    ///     "NounNotPresent" rather than "12".
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public TurnTerminalPath TerminalPath { get; init; }
}

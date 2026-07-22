using System.Text;
using Microsoft.Extensions.Logging;
using Model.Intent;
using Newtonsoft.Json;

namespace Model;

/// <summary>
/// Structured-output contract for the AI parser (see also <see cref="ParsingHelper" />).
///
/// The parser used to ask gpt-4o for free-text pseudo-XML tags and HTML-parse them, which made the parse
/// fragile: malformed / missing / duplicate tags produced a silent NullIntent or an HTTP 500, and the
/// intent/direction could be any string the model felt like. This moves the *wire format* to an OpenAI
/// Structured Output constrained by a strict JSON schema:
///
///   - the STRUCTURE is guaranteed valid JSON with exactly these fields (no malformed/duplicate tags);
///   - <c>intent</c> is constrained to the closed set the engine dispatches on;
///   - <c>direction</c> is constrained to the known movement words (plus "other");
///   - <c>verb</c> and <c>nouns</c> stay free strings — the verb vocabulary is not yet centralized, and
///     forcing the noun onto a known object would produce confident-wrong parses (better to let it be
///     free and validate against scope downstream).
///
/// The validated JSON is then rendered to the canonical tag string via <see cref="ToTagString" /> and fed
/// through the existing <see cref="ParsingHelper.GetIntent" />, so every accumulated intent-construction
/// rule (#256 multi-command, #268 goto, #423 look-with-noun, the move/place safety nets) is reused
/// unchanged — the model simply can no longer hand us something those rules can't cope with.
/// </summary>
public static class StructuredIntentParsing
{
    /// <summary>The intent categories <see cref="ParsingHelper.GetIntent" /> recognises. Closed set.</summary>
    public static readonly string[] IntentValues =
        ["move", "goto", "board", "disembark", "take", "drop", "look", "inventory", "act"];

    /// <summary>
    /// The movement words <see cref="Model.Movement.DirectionParser.ParseDirection" /> resolves, plus
    /// "other" for a move whose direction the model can't pin down. Closed set.
    /// </summary>
    public static readonly string[] DirectionValues =
    [
        "in", "out", "enter", "exit", "up", "down", "east", "west", "north", "south",
        "north-west", "north-east", "south-west", "south-east", "other"
    ];

    /// <summary>
    /// The strict JSON schema the model must satisfy. Strict mode requires every property to be listed in
    /// <c>required</c> and <c>additionalProperties:false</c>; optional fields are expressed as nullable.
    /// </summary>
    public static string JsonSchema =>
        $$"""
          {
            "type": "object",
            "additionalProperties": false,
            "required": ["intent", "verb", "nouns", "preposition", "direction", "adjective"],
            "properties": {
              "intent": { "type": "string", "enum": [{{QuotedList(IntentValues)}}] },
              "verb": { "type": ["string", "null"] },
              "nouns": { "type": "array", "items": { "type": "string" } },
              "preposition": { "type": ["string", "null"] },
              "direction": { "type": ["string", "null"], "enum": [{{QuotedList(DirectionValues)}}, null] },
              "adjective": { "type": ["string", "null"] }
            }
          }
          """;

    /// <summary>
    /// System prompt for the structured parse. Mirrors the intent/verb/noun rules of the original
    /// tag-based <see cref="ParsingHelper.SystemPrompt" />, but asks for the schema fields instead of tags.
    /// The {0} / {1} placeholders (location description, player sentence) are substituted by
    /// <see cref="BuildSystemPrompt" /> — NOT by string.Format, which would choke on the literal { } in the
    /// JSON examples below.
    /// </summary>
    public static readonly string SystemPrompt =
        """
        You are a parser for an interactive fiction game. The player is in this location: "{0}"

        Determine the player's intent for the sentence "{1}" and return it as a JSON object with these fields:

        "intent" — one of:
            "move"      the player wants to move/go/travel in a cardinal or relative DIRECTION (north, up, in, out...)
            "goto"      the player wants to travel to a SPECIFIC NAMED place or room ("go to the kitchen",
                        "walk into the shuttle"). Put the destination in "nouns", NORMALIZING a colloquial
                        place-name to the common room noun (e.g. "the galley" -> "kitchen"; "the loo" -> "bathroom").
            "board"     the player wants to enter a vehicle or sub-location
            "disembark" the player wants to exit a vehicle or sub-location
            "take"      the player wants to take or pick up one or more items (EXCEPTION: if "take" is used
                        WITH a tool via "with"/"using", use "act" instead)
            "drop"      the player wants to drop one or more items
            "look"      the player wants to "look"/"look around" or asks "where am I?"
            "inventory" the player wants to know what they are carrying
            "act"       anything else

        IMPORTANT: "move" is only for TRAVEL by a movement verb (go/walk/run/head/enter/climb...). A
        press/push/turn/type/set command is intent=act EVEN when it names a direction word: in
        "press the up button" / "press up", the "up" is part of the object being pressed, NOT a movement.
        Put such commands as "act" with the direction word kept in the noun, and leave "direction" null.

        "verb" — the single most important verb expressing the player's intention; prefer a simple, common
            synonym. Turn something on -> "activate"; turn something off -> "deactivate"; wear/put on
            clothing -> "don"; take off/remove clothing -> "doff". Null if there is no meaningful verb.

        "nouns" — an array of the noun phrase(s) that are arguments of the verb, head noun with any leading
            adjectives, WITHOUT the preposition. Keep compound nouns together ("pile of plastic", "air pump",
            "id card"). Empty array if none. At most two.

        "preposition" — if there are two nouns, the preposition connecting them ("with", "to", "in", "under",
            "through"); otherwise null.

        "direction" — if the sentence expresses movement, the exact word from this list that best fits:
            "in, out, enter, exit, up, down, east, west, north, south, north-west, north-east, south-west,
            south-east". Use "follow"/"go towards" + a location in the room to pick the direction. If you
            cannot match any, use "other". Null if not a movement.

        "adjective" — a distinguishing adjective for a single noun, if any; otherwise null.

        Examples:
        "type 1" -> {"intent":"act","verb":"type","nouns":["1"],"preposition":null,"direction":null,"adjective":null}
        "drop the sword" -> {"intent":"drop","verb":"drop","nouns":["sword"],"preposition":null,"direction":null,"adjective":null}
        "take the sword" -> {"intent":"take","verb":"take","nouns":["sword"],"preposition":null,"direction":null,"adjective":null}
        "look under the rug" -> {"intent":"act","verb":"look","nouns":["rug"],"preposition":"under","direction":null,"adjective":null}
        "turn on lamp" -> {"intent":"act","verb":"activate","nouns":["lamp"],"preposition":null,"direction":null,"adjective":null}
        "press the up button" -> {"intent":"act","verb":"press","nouns":["up button"],"preposition":null,"direction":null,"adjective":null}
        "press up" -> {"intent":"act","verb":"press","nouns":["up"],"preposition":null,"direction":null,"adjective":null}
        "put on the hat" -> {"intent":"act","verb":"don","nouns":["hat"],"preposition":null,"direction":null,"adjective":null}
        "inflate the pile of plastic with the air pump" -> {"intent":"act","verb":"inflate","nouns":["pile of plastic","air pump"],"preposition":"with","direction":null,"adjective":null}
        "tie the rope to the railing" -> {"intent":"act","verb":"tie","nouns":["rope","railing"],"preposition":"to","direction":null,"adjective":null}
        "exit the boat" -> {"intent":"disembark","verb":"exit","nouns":["boat"],"preposition":null,"direction":null,"adjective":null}
        "go to the galley" -> {"intent":"goto","verb":"go","nouns":["kitchen"],"preposition":null,"direction":null,"adjective":null}
        "walk north" -> {"intent":"move","verb":"walk","nouns":[],"preposition":null,"direction":"north","adjective":null}
        "where am I?" -> {"intent":"look","verb":null,"nouns":[],"preposition":null,"direction":null,"adjective":null}
        "what am I carrying?" -> {"intent":"inventory","verb":null,"nouns":[],"preposition":null,"direction":null,"adjective":null}
        """;

    /// <summary>
    /// Substitutes the location description and the player's sentence into <see cref="SystemPrompt" />.
    /// Uses <c>string.Replace</c> deliberately, NOT <c>string.Format</c>: the prompt embeds literal { }
    /// from its JSON examples, which string.Format treats as malformed format items and throws on.
    /// </summary>
    public static string BuildSystemPrompt(string locationDescription, string input) =>
        SystemPrompt.Replace("{0}", locationDescription).Replace("{1}", input);

    /// <summary>
    /// Renders a validated <see cref="ParsedIntent" /> to the canonical tag string consumed by
    /// <see cref="ParsingHelper.GetIntent" />. Lowercased so downstream tag matching (and the integration
    /// tests that assert on <c>Message</c>) behave exactly as they did with the old tag-based parser.
    /// Because the JSON was schema-validated, this string is always well-formed — no duplicate or missing
    /// tags — which is precisely what removes the old fragility.
    /// </summary>
    private static string QuotedList(string[] values) => string.Join(", ", values.Select(v => $"\"{v}\""));

    public static string ToTagString(ParsedIntent parsed)
    {
        var sb = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(parsed.Intent))
            sb.Append($"<intent>{parsed.Intent.Trim().ToLowerInvariant()}</intent>\n");

        if (!string.IsNullOrWhiteSpace(parsed.Verb))
            sb.Append($"<verb>{parsed.Verb.Trim().ToLowerInvariant()}</verb>\n");

        if (!string.IsNullOrWhiteSpace(parsed.Adjective))
            sb.Append($"<adjective>{parsed.Adjective.Trim().ToLowerInvariant()}</adjective>\n");

        foreach (var noun in parsed.Nouns ?? [])
            if (!string.IsNullOrWhiteSpace(noun))
                sb.Append($"<noun>{noun.Trim().ToLowerInvariant()}</noun>\n");

        if (!string.IsNullOrWhiteSpace(parsed.Preposition))
            sb.Append($"<preposition>{parsed.Preposition.Trim().ToLowerInvariant()}</preposition>\n");

        // "other" is the model's "I know it's a move but can't name the direction" value; emit it so
        // DetermineMoveIntent's #268 safety net (named-place move -> goto) can still see it.
        if (!string.IsNullOrWhiteSpace(parsed.Direction))
            sb.Append($"<direction>{parsed.Direction.Trim().ToLowerInvariant()}</direction>\n");

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// Turns a raw structured-output response body into an <see cref="IntentBase" />: deserialize the JSON,
    /// render it to the canonical tag form, and run it through <see cref="ParsingHelper.GetIntent" />.
    /// Invalid or empty JSON — a refusal, or a truncated max-tokens response — degrades to
    /// <see cref="NullIntent" /> rather than throwing, matching the tolerance of the tag-based path this
    /// replaced. Extracted from the OpenAI client so it is unit-testable without the network.
    /// </summary>
    public static IntentBase ParseResponse(string? responseContent, string input, ILogger? logger = null)
    {
        ParsedIntent? parsed;
        try
        {
            parsed = JsonConvert.DeserializeObject<ParsedIntent>(responseContent ?? string.Empty);
        }
        catch (JsonException ex)
        {
            logger?.LogWarning("Structured parse returned invalid JSON for input '{Input}': {Message}", input,
                ex.Message);
            return new NullIntent();
        }

        if (parsed is null)
        {
            logger?.LogWarning("Structured parse returned null JSON for input '{Input}'", input);
            return new NullIntent();
        }

        return ParsingHelper.GetIntent(input, ToTagString(parsed), logger);
    }
}

/// <summary>
/// Deserialization target for the structured parse response. Mirrors <see cref="StructuredIntentParsing.JsonSchema" />.
/// </summary>
public class ParsedIntent
{
    [JsonProperty("intent")] public string Intent { get; set; } = "act";

    [JsonProperty("verb")] public string? Verb { get; set; }

    [JsonProperty("nouns")] public List<string>? Nouns { get; set; } = [];

    [JsonProperty("preposition")] public string? Preposition { get; set; }

    [JsonProperty("direction")] public string? Direction { get; set; }

    [JsonProperty("adjective")] public string? Adjective { get; set; }
}

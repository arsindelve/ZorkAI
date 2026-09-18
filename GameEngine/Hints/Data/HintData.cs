using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameEngine.Hints.Data;

// =====================================================================================
// The generated hint corpus for one game, as data. Produced by the generator (from the game's
// verified walkthroughs and live state), reviewed once, checked in as an embedded resource, and
// loaded by DataPuzzleGraph / DataHintCorpus. Nothing in here is hand-authored engine logic.
// =====================================================================================

public sealed class HintData
{
    public string Game { get; set; } = "";

    /// <summary>The walkthroughs this was generated from, for provenance.</summary>
    public List<string> GeneratedFrom { get; set; } = new();

    public List<HintNodeData> Nodes { get; set; } = new();

    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public string ToJson() => JsonSerializer.Serialize(this, Options);

    public static HintData FromJson(string json) =>
        JsonSerializer.Deserialize<HintData>(json, Options) ?? throw new InvalidDataException("Empty hint data.");
}

/// <summary>One puzzle: a segment of the walkthrough the player can be stuck on.</summary>
public sealed class HintNodeData
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Location { get; set; } = "";

    /// <summary>Serves only an optional goal: never done in the minimal walkthrough.</summary>
    public bool Optional { get; set; }

    public List<string> Prerequisites { get; set; } = new();

    /// <summary>The words players use for this puzzle — the nouns of its own commands.</summary>
    public List<string> Aliases { get; set; } = new();

    /// <summary>All of these must hold for the puzzle to count as done. Evaluated over the live state.</summary>
    public List<StatePredicate> Done { get; set; } = new();

    /// <summary>The ladder: nudge, approach, solution.</summary>
    public List<string> Rungs { get; set; } = new();

    /// <summary>The verified commands of the segment (provenance; rung C is written from these).</summary>
    public List<string> Commands { get; set; } = new();

    /// <summary>The moves that led here from the previous segment (provenance).</summary>
    public List<string> Approach { get; set; } = new();
}

/// <summary>
///     A single observation over the flattened live state: <c>Path</c> is an object and property as
///     <see cref="StateFlattener" /> names them ("Item:Magnet.HasEverBeenPickedUp"), <c>Op</c> is one of
///     <c>eq</c>, <c>gte</c> (integers) or <c>contains</c> (string lists), and <c>Value</c> the expected value.
/// </summary>
public sealed class StatePredicate
{
    public string Path { get; set; } = "";
    public string Op { get; set; } = "eq";
    public string Value { get; set; } = "";

    public bool Holds(IReadOnlyDictionary<string, string> flat)
    {
        if (!flat.TryGetValue(Path, out var actual)) return false;

        return Op switch
        {
            "gte" => int.TryParse(actual, out var a) && int.TryParse(Value, out var v) && a >= v,
            "contains" => actual.StartsWith("list:", StringComparison.Ordinal) &&
                          actual[5..].Split('|', StringSplitOptions.RemoveEmptyEntries).Contains(Value),
            _ => actual == Value
        };
    }

    public override string ToString() => $"{Path} {Op} {Value}";
}

using System.Text.Json.Serialization;

namespace UnitTests.IntentMapping;

/// <summary>
/// Root configuration object for intent mappings loaded from JSON.
/// </summary>
public class IntentMappingConfiguration
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0";

    [JsonPropertyName("gameName")]
    public string GameName { get; set; } = "base";

    [JsonPropertyName("extends")]
    public string? Extends { get; set; }

    [JsonPropertyName("globalCommands")]
    public List<GlobalCommandMapping> GlobalCommands { get; set; } = [];

    [JsonPropertyName("simpleIntents")]
    public List<SimpleIntentMapping> SimpleIntents { get; set; } = [];

    [JsonPropertyName("multiNounIntents")]
    public List<MultiNounIntentMapping> MultiNounIntents { get; set; } = [];

    [JsonPropertyName("enterSubLocationIntents")]
    public List<EnterSubLocationMapping> EnterSubLocationIntents { get; set; } = [];

    [JsonPropertyName("exitSubLocationIntents")]
    public List<ExitSubLocationMapping> ExitSubLocationIntents { get; set; } = [];
}

/// <summary>
/// Mapping for global commands (look, inventory, wait, etc.)
/// </summary>
public class GlobalCommandMapping
{
    [JsonPropertyName("inputs")]
    public List<string> Inputs { get; set; } = [];

    [JsonPropertyName("processorType")]
    public string ProcessorType { get; set; } = string.Empty;
}

/// <summary>
/// Mapping for simple verb+noun intents.
/// </summary>
public class SimpleIntentMapping
{
    [JsonPropertyName("inputs")]
    public List<string> Inputs { get; set; } = [];

    [JsonPropertyName("verb")]
    public string Verb { get; set; } = string.Empty;

    [JsonPropertyName("noun")]
    public string Noun { get; set; } = string.Empty;

    [JsonPropertyName("adverb")]
    public string? Adverb { get; set; }

    [JsonPropertyName("adjective")]
    public string? Adjective { get; set; }

    [JsonPropertyName("originalInput")]
    public string? OriginalInput { get; set; }
}

/// <summary>
/// Mapping for multi-noun intents (put X in Y, give X to Y, etc.)
/// </summary>
public class MultiNounIntentMapping
{
    [JsonPropertyName("inputs")]
    public List<string> Inputs { get; set; } = [];

    [JsonPropertyName("verb")]
    public string Verb { get; set; } = string.Empty;

    [JsonPropertyName("nounOne")]
    public string NounOne { get; set; } = string.Empty;

    [JsonPropertyName("nounTwo")]
    public string NounTwo { get; set; } = string.Empty;

    [JsonPropertyName("preposition")]
    public string Preposition { get; set; } = string.Empty;

    [JsonPropertyName("originalInput")]
    public string? OriginalInput { get; set; }
}

/// <summary>
/// Mapping for entering sub-locations (get in boat, enter vehicle, etc.)
/// </summary>
public class EnterSubLocationMapping
{
    [JsonPropertyName("inputs")]
    public List<string> Inputs { get; set; } = [];

    [JsonPropertyName("noun")]
    public string Noun { get; set; } = string.Empty;
}

/// <summary>
/// Mapping for exiting sub-locations (leave boat, get out of bed, etc.)
/// </summary>
public class ExitSubLocationMapping
{
    [JsonPropertyName("inputs")]
    public List<string> Inputs { get; set; } = [];

    [JsonPropertyName("nounOne")]
    public string NounOne { get; set; } = string.Empty;

    [JsonPropertyName("nounTwo")]
    public string? NounTwo { get; set; }
}

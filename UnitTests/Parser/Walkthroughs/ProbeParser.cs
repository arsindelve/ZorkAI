using CloudWatch;
using CloudWatch.Model;
using Model.Intent;
using Model.Interface;

namespace UnitTests.Parser.Walkthroughs;

/// <summary>
///     Sits between a walkthrough's engine and its parser. Ordinarily it only passes everything through
///     and remembers what the engine was handed. A tool studying the walkthrough can also make it hand
///     the engine an intent of its own for one turn, which is how "would this other reading have done the
///     same thing?" is asked of the game itself.
/// </summary>
public sealed class ProbeParser(IIntentParser inner) : IIntentParser
{
    /// <summary>When set, the engine is handed this for whatever is typed, and the real parser is not asked.</summary>
    public IntentBase? Forced { get; set; }

    /// <summary>Every intent handed to the engine since this was last cleared, in order.</summary>
    public List<IntentBase> Handed { get; } = [];

    /// <summary>
    ///     The sentence the engine last asked about - not always the one typed: "again" is the sentence
    ///     before it, and a pronoun is rewritten before parsing.
    /// </summary>
    public string? LastSentence { get; set; }

    public Guid? TurnCorrelationId
    {
        get => inner.TurnCorrelationId;
        set => inner.TurnCorrelationId = value;
    }

    public ICloudWatchLogger<GenerationLog>? Logger
    {
        get => inner.Logger;
        set => inner.Logger = value;
    }

    public IntentBase? DetermineGlobalIntentType(string? input) => inner.DetermineGlobalIntentType(input);

    public IntentBase? DetermineSystemIntentType(string? input) => inner.DetermineSystemIntentType(input);

    public async Task<IntentBase> DetermineComplexIntentType(string? input, string locationDescription,
        string sessionId)
    {
        var intent = Forced ?? await inner.DetermineComplexIntentType(input, locationDescription, sessionId);
        LastSentence = input;
        Handed.Add(intent);
        return intent;
    }

    public Task<string?> ResolvePronounsAsync(string input, string? lastInput, string? lastResponse)
    {
        return inner.ResolvePronounsAsync(input, lastInput, lastResponse);
    }
}

namespace Model.Hints;

// The language-model seam for the hint engine. Lives in Model (like IGenerationClient in
// Model.AIGeneration) so both the engine (GameEngine.Hints) and the OpenAI implementation
// (ZorkAI.OpenAI) can see it without a project-reference cycle.

/// <summary>How a free-text player question is routed by the hint engine.</summary>
public enum HintIntent
{
    Progress,   // "what do I do?" — puzzle hint
    Mechanic,   // "why am I sick / why can't I carry this?"
    Lore,       // "why is everything deserted / who was X?"
    OutOfScope  // not a game question
}

/// <summary>The voice the phrasing model speaks in. v1: one snarky-narrator persona for all games.</summary>
public sealed record HintPersona(string SystemPrompt);

/// <summary>
///     Minimal language-model seam the hint engine uses. Implemented over OpenAI for v1 (per the
///     locked build decision); stubbed deterministically in the eval harness.
/// </summary>
public interface IHintLanguageModel
{
    /// <summary>Classify a free-text player question into a hint intent.</summary>
    Task<HintIntent> ClassifyIntent(string question);

    /// <summary>Render a single hint rung in the persona's voice (must not exceed the rung's content).</summary>
    Task<string> PhraseRung(string rung, HintPersona persona);

    /// <summary>Render a grounded lore/mechanic answer in the persona's voice from retrieved source text.</summary>
    Task<string> PhraseLore(string question, string groundedSource, HintPersona persona);
}

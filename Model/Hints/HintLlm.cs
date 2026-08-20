namespace Model.Hints;

// The language-model seam for the two-tier hint engine. Lives in Model (like IGenerationClient in
// Model.AIGeneration) so both the engine (GameEngine.Hints) and the OpenAI implementation
// (ZorkAI.OpenAI) can see it without a project-reference cycle.

/// <summary>
///     The voice both LLM calls speak in (v1: one snarky-narrator persona per game), plus the small amount
///     of game identity the shared solver prompt needs. <see cref="IHintLanguageModel" /> implementations
///     live in a shared library, so anything game-specific — which game this is, and which flags of the
///     player's situation actually change an answer — must arrive here rather than be hardcoded there.
/// </summary>
/// <param name="SystemPrompt">The persona's voice, used verbatim by the revealer (LLM 2).</param>
/// <param name="GameName">
///     The game being solved, as the player would name it (e.g. "Planetfall"). Empty by default: an
///     un-customized persona asserts no game identity at all, and the prompt drops the naming clause
///     rather than substituting a placeholder.
/// </param>
/// <param name="StateGrounding">
///     A short, game-specific list of what the player's situation contains and what actually matters in it
///     (e.g. "what's done, Floyd alive/dead, their health") — grounding for the solver, never an excuse to
///     dodge the question.
/// </param>
public sealed record HintPersona(
    string SystemPrompt,
    string GameName = "",
    string StateGrounding = "what they have done so far, and their current condition");

/// <summary>One turn of the hint conversation — what the player asked and what was revealed back.</summary>
public sealed record HintExchange(string Question, string Revealed);

/// <summary>
///     The two-tier hint LLM seam. Implemented over OpenAI for v1; stubbed deterministically in tests.
/// </summary>
public interface IHintLanguageModel
{
    /// <summary>
    ///     LLM 1 — the solver. Given the full game docs, the player's live context, and the conversation
    ///     so far (so it can resolve follow-ups like "it" / "more"), work out the COMPLETE solution to
    ///     whatever the player is currently facing (or the honest truth, e.g. "that's a dead end"). Reason
    ///     only over the provided docs/context — never invent facts.
    /// </summary>
    Task<string> Solve(string docs, string playerContext, IReadOnlyList<HintExchange> history, string question,
        HintPersona persona);

    /// <summary>
    ///     LLM 2 — the revealer. Given the player's context, the complete solution from <see cref="Solve" />,
    ///     and the conversation so far, decide what to reveal NEXT: the least that helps, pacing up from the
    ///     chat history (first ask = gentle nudge; repeated asks = progressively more). Never dump the whole
    ///     solution unless the player has clearly pushed for it.
    /// </summary>
    Task<string> Reveal(string playerContext, string solution, IReadOnlyList<HintExchange> history,
        string question, HintPersona persona);
}

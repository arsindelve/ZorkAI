namespace Model.Hints;

// The language-model seam for the hint engine. Lives in Model (like IGenerationClient in
// Model.AIGeneration) so both the engine (GameEngine.Hints) and the OpenAI implementation
// (ZorkAI.OpenAI) can see it without a project-reference cycle.

/// <summary>
///     The voice the model speaks in, plus the small amount of game identity the shared prompts need.
///     <see cref="IHintLanguageModel" /> implementations live in a shared library, so anything
///     game-specific — which game this is, and which flags of the player's situation actually change an
///     answer — must arrive here rather than be hardcoded there.
/// </summary>
/// <param name="SystemPrompt">The persona's voice, used verbatim by the phrasing calls.</param>
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

/// <summary>
///     One turn of the hint conversation — what the player asked and what was revealed back. The client
///     replays these on every request, echoing the <see cref="Topic" />, <see cref="Rung" /> and
///     <see cref="Kind" /> the engine returned for that turn. That is how a ladder resumes where it left
///     off, and how "more" knows whether it is continuing a puzzle hint, a lore answer, or a fallback
///     conversation — without any server-side memory. All three are null for exchanges recorded before
///     they existed; the engine then trusts the router's reading of the message.
/// </summary>
public sealed record HintExchange(
    string Question,
    string Revealed,
    string? Topic = null,
    int? Rung = null,
    string? Kind = null);

/// <summary>How a free-text player message is routed by the hint engine.</summary>
public enum HintIntent
{
    /// <summary>"What do I do?" / "how do I open X?" — a puzzle hint.</summary>
    Progress,

    /// <summary>"Why am I sick / tired / hungry?" — something happening to the player, explained from state.</summary>
    Mechanic,

    /// <summary>"Why did the ship explode? Who is X? What is this place?" — the world and its story.</summary>
    Lore,

    /// <summary>Not a question about the game.</summary>
    OutOfScope
}

/// <summary>A puzzle the router may attach a question to (the catalog it chooses from).</summary>
public sealed record HintTopic(string Id, string Title, string Location);

/// <summary>
///     The router's reading of a message: what kind of question it is, whether it carries on the previous
///     exchange ("more", "I still don't get it", "how do I open it?"), and — for progress questions about a
///     specific puzzle — which one. <see cref="TopicId" /> is null for open-ended asks ("what do I do?")
///     and for <see cref="Unlisted" /> ones: a specific object, place or action the catalog has no puzzle
///     for (a dead end, a red herring, something the authored ladders don't cover). Those are answered by
///     the fallback solver over the full docs, never by the active blocker's ladder.
/// </summary>
public sealed record RoutedIntent(HintIntent Intent, bool ContinuesThread, string? TopicId, bool Unlisted = false)
{
    /// <summary>"What do I do?" — progress, fresh, no particular puzzle.</summary>
    public static readonly RoutedIntent OpenEnded = new(HintIntent.Progress, false, null);

    /// <summary>"More" — progress, continuing whatever was last discussed.</summary>
    public static readonly RoutedIntent More = new(HintIntent.Progress, true, null);
}

/// <summary>
///     The language-model seam. Every call is bounded by what it is handed: the phraser only ever holds
///     one rung, the lore answerer only the tier-gated source. Every call fails in the direction that
///     cannot spoil: the router and the lore/solve/reveal calls return null/empty when the model is
///     unavailable (the engine declines); the phraser returns the authored rung it was handed.
///     Implemented over OpenAI; stubbed deterministically in tests.
/// </summary>
public interface IHintLanguageModel
{
    /// <summary>
    ///     Classify a player message and, for progress questions, pick the puzzle it is about. Null when the
    ///     model is unavailable or produced nothing usable — the engine must then decline rather than guess.
    /// </summary>
    Task<RoutedIntent?> Route(string question, IReadOnlyList<HintExchange> history, IReadOnlyList<HintTopic> topics);

    /// <summary>
    ///     Deliver a single authored hint rung in the persona's voice. The model holds nothing but this rung
    ///     and the player's key state, so it cannot reveal more than the rung says.
    /// </summary>
    Task<string> PhraseRung(string rung, string keyState, IReadOnlyList<HintExchange> history, string question,
        HintPersona persona);

    /// <summary>
    ///     Answer a lore or mechanic question from the supplied source text only. Returns empty when the
    ///     model is unavailable; the engine then declines rather than improvising.
    /// </summary>
    Task<string> AnswerLore(string question, string groundedSource, string keyState,
        IReadOnlyList<HintExchange> history, HintPersona persona);

    /// <summary>
    ///     Fallback solver, used only when no authored ladder covers the question: reason over the full game
    ///     docs and the player's complete situation to work out the answer (or the honest truth — "that's a
    ///     dead end"). Never invents beyond the docs.
    /// </summary>
    Task<string> Solve(string docs, string playerContext, IReadOnlyList<HintExchange> history, string question,
        HintPersona persona);

    /// <summary>
    ///     Fallback revealer, paired with <see cref="Solve" />: given the complete solution and the conversation,
    ///     reveal only the next appropriate amount. Returns empty on failure — never the raw solution.
    /// </summary>
    Task<string> Reveal(string playerContext, string solution, IReadOnlyList<HintExchange> history,
        string question, HintPersona persona);
}

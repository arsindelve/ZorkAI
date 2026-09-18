using Model.Hints;

namespace Planetfall.Tests.Hints;

/// <summary>
///     Deterministic stand-in for OpenAI in the Planetfall hint tests: routes however the test says and
///     echoes what it is handed, so the deterministic pipeline (localization, blocker inference, rung
///     selection, tier gating) is asserted without network. Per Docs/hints/planetfall/04: no AI in the
///     assertion path.
/// </summary>
internal sealed class RoutingStubLlm : IHintLanguageModel
{
    public RoutedIntent Routed { get; set; } = RoutedIntent.OpenEnded;
    public string? LastRung;
    public string? LastLoreSource;
    public string? LastKeyState;

    public Task<RoutedIntent> Route(string question, IReadOnlyList<HintExchange> history,
        IReadOnlyList<HintTopic> topics)
    {
        return Task.FromResult(Routed);
    }

    public Task<string> PhraseRung(string rung, string keyState, IReadOnlyList<HintExchange> history,
        string question, HintPersona persona)
    {
        LastRung = rung;
        LastKeyState = keyState;
        return Task.FromResult(rung);
    }

    public Task<string> AnswerLore(string question, string groundedSource, string keyState,
        IReadOnlyList<HintExchange> history, HintPersona persona)
    {
        LastLoreSource = groundedSource;
        LastKeyState = keyState;
        return Task.FromResult(groundedSource);
    }

    public Task<string> Solve(string docs, string playerContext, IReadOnlyList<HintExchange> history,
        string question, HintPersona persona)
    {
        return Task.FromResult("SOLVED");
    }

    public Task<string> Reveal(string playerContext, string solution, IReadOnlyList<HintExchange> history,
        string question, HintPersona persona)
    {
        return Task.FromResult("REVEALED");
    }
}

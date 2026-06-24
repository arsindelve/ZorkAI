using GameEngine.Hints;
using Model.Hints;
using Model.Interface;

namespace UnitTests.Hints;

/// <summary>Records what each tier was called with, and returns canned outputs.</summary>
internal sealed class RecordingLlm : IHintLanguageModel
{
    public string? LastDocs, LastSolveContext, LastSolveQuestion;
    public string? LastRevealContext, LastSolution, LastRevealQuestion;
    public IReadOnlyList<HintExchange> LastSolveHistory = new List<HintExchange>();
    public IReadOnlyList<HintExchange> LastHistory = new List<HintExchange>();
    public string SolveResult = "THE COMPLETE SOLUTION";

    public Task<string> Solve(string docs, string playerContext, IReadOnlyList<HintExchange> history,
        string question, HintPersona persona)
    {
        LastDocs = docs;
        LastSolveContext = playerContext;
        LastSolveHistory = history.ToList();
        LastSolveQuestion = question;
        return Task.FromResult(SolveResult);
    }

    public Task<string> Reveal(string playerContext, string solution, IReadOnlyList<HintExchange> history,
        string question, HintPersona persona)
    {
        LastRevealContext = playerContext;
        LastSolution = solution;
        LastHistory = history.ToList();
        LastRevealQuestion = question;
        // The "revealed" amount scales with how many times they've already asked — proves history drives pacing.
        return Task.FromResult($"reveal#{history.Count}");
    }
}

internal sealed class FakeProvider : IHintProvider
{
    public string Docs { get; set; } = "DOCS";
    public string PlayerContext { get; set; } = "CONTEXT";
    public List<IProactiveRule> Proactive { get; } = new();

    public string KeyState { get; set; } = "KEYSTATE";
    public string DescribePlayerContext(IContext state) => PlayerContext;
    public string DescribeKeyState(IContext state) => KeyState;
    public HintPersona Persona => new("test narrator");
    public IReadOnlyList<IProactiveRule> ProactiveRules => Proactive;
}

internal sealed class FakeProactive : IProactiveRule
{
    private readonly ProactiveNudge _nudge;
    public FakeProactive(ProactiveNudge nudge) => _nudge = nudge;
    public ProactiveNudge? Evaluate(IContext liveState) => _nudge;
}

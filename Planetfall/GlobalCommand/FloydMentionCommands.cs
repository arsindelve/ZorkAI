using System.Text.RegularExpressions;
using GameEngine.StaticCommand;
using Model.AIGeneration;
using Planetfall.Item.Kalamontee.Mech.FloydPart;

namespace Planetfall.GlobalCommand;

/// <summary>
///     Canned, deterministic answers to the player mentioning Floyd by name (issue #552).
///     <para>
///     Both halves used to fall through to the AI narrator, which improvised a different joke every
///     time - "off on his own little adventure", "cosmic coffee break" - including after his death,
///     where any joke at all is the wrong register (#545). Matching here, in a Planetfall-owned
///     global command, means the answer is fixed, free of LLM cost, and lands before the AI parser
///     and the conversation handler ever see the input.
///     </para>
/// </summary>
internal static class FloydMentionCommands
{
    /// <summary>
    ///     The word "floyd" on its own, not as part of a longer word. Only the NAME is matched:
    ///     "robot" and "B-19-7" are things the player can see and read in the Robot Shop, and the
    ///     deactivated-robot description they produce is part of the intended discovery flow.
    /// </summary>
    private static readonly Regex NamesFloyd = new(@"\bfloyd\b", RegexOptions.IgnoreCase);

    /// <summary>
    ///     CI-critical exemption. "activate floyd" is the walkthrough spine, and it is necessarily
    ///     typed BEFORE any met-flag flips - if the fourth-wall intercept swallowed it, Floyd would
    ///     be unactivatable by name and every walkthrough test would break.
    /// </summary>
    private static readonly Regex ActivatesFloyd =
        new(@"\b(activate|turn\s+on|start|switch\s+on|power\s+(on|up))\b", RegexOptions.IgnoreCase);

    /// <summary>
    ///     Where-is-Floyd-shaped queries. Deliberately anchored at the start of the command: an
    ///     unanchored "where" would hijack conversation aimed AT Floyd that merely contains the word
    ///     ("floyd, do you know where the card is").
    /// </summary>
    private static readonly Regex AsksWhereFloydIs =
        new(@"^\s*(where|find|locate)\b", RegexOptions.IgnoreCase);

    /// <summary>
    ///     Returns the command that answers this mention of Floyd, or null to let normal parsing
    ///     handle the input.
    /// </summary>
    internal static IGlobalCommand? Match(string? input)
    {
        if (string.IsNullOrWhiteSpace(input) || !NamesFloyd.IsMatch(input))
            return null;

        var floyd = Repository.GetItem<Floyd>();

        // Before the player can know the name, ANY mention of it is a returning fan talking - where
        // is he, examine him, take him, greet him, ask him about Lazarus. Activation is the one
        // phrasing that has to keep working.
        if (!floyd.PlayerKnowsFloydByName)
            return ActivatesFloyd.IsMatch(input)
                ? null
                : new SimpleResponseCommand(FloydConstants.NobodyHereByThatName);

        // Once they know him, only the where-is queries are claimed. Examining him, talking to him
        // and every other verb keep the behavior they already have.
        return AsksWhereFloydIs.IsMatch(input) ? new WhereIsFloydProcessor() : null;
    }
}

/// <summary>
///     Answers "where is Floyd" from Floyd's actual state rather than from the narrator's
///     imagination (issue #552). Needs the context - "right here" is a question about the player's
///     room - so unlike the fourth-wall line it can't be resolved to a fixed string at match time.
/// </summary>
internal class WhereIsFloydProcessor : IGlobalCommand
{
    public Task<string> Process(string? input, IContext context, IGenerationClient client, Runtime runtime)
    {
        var floyd = Repository.GetItem<Floyd>();

        // Death first: his body is still lying in the Bio Lock, so the in-the-room checks below
        // would otherwise answer "right here" about a corpse.
        if (floyd.HasDied)
            return Task.FromResult(FloydConstants.WhereIsFloydDead);

        if (!floyd.IsInTheRoom(context))
            return Task.FromResult(FloydConstants.WhereIsFloydAbsent);

        // IsAlive rather than a bare IsOn. The HasDied return above already makes the two
        // equivalent here, but #545's lesson is that a liveness guard should be right in its own
        // right, not merely shadowed by an earlier return in the same method.
        return Task.FromResult(floyd.IsAlive
            ? FloydConstants.WhereIsFloydHereAndOn
            : FloydConstants.WhereIsFloydHereAndOff);
    }
}

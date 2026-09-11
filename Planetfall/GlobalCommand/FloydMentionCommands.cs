using System.Text.RegularExpressions;
using Model.AIGeneration;
using Planetfall.Item.Feinstein;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Utilities;

namespace Planetfall.GlobalCommand;

/// <summary>
///     Canned, deterministic answers to the player mentioning Floyd by name (issue #552).
///     <para>
///     Both halves used to fall through to the AI narrator, which improvised a different joke every
///     time - "off on his own little adventure", "cosmic coffee break" - including after his death,
///     where any joke at all is the wrong register (#545). Matching here, in a Planetfall-owned
///     global command, means the answer is fixed and free of LLM cost.
///     </para>
///     <para>
///     Everything here matches RAW player text, before the parser has resolved a verb or a noun.
///     That buys determinism and zero cost, and it is why every matcher below is anchored and
///     narrow: an unanchored match on raw text claims sentences that were never about Floyd. The
///     review that followed the first cut of this file found five separate bugs of exactly that
///     shape, so the rule for anything added here is "match the whole command, or don't match".
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
    ///     typed BEFORE any met-flag flips - if the intercept swallowed it, Floyd would be
    ///     unactivatable by name and every walkthrough test would break.
    ///     <para>
    ///     Floyd must be the OBJECT of the activation, and the particle may sit either side of him:
    ///     the production parser normalizes "turn X on" to the verb "activate"
    ///     (<c>Model/ParsingHelper.cs</c>), but that happens downstream of here, so this has to
    ///     recognize the raw English itself. Two bugs lived in the earlier version: it required the
    ///     particle to be adjacent (so "turn floyd on" was swallowed - a command that scores the +2
    ///     on main), and it tested the whole sentence for an activation word (so "floyd, let's start
    ///     singing" escaped the intercept entirely).
    ///     </para>
    /// </summary>
    private static readonly Regex ActivatesFloyd = new(
        @"^\s*(please\s+)?((activate|start|boot)\s+(the\s+)?floyd\b"
        + @"|(turn|switch|power)\s+((on|up)\s+)?(the\s+)?floyd(\s+(on|up))?\b)",
        RegexOptions.IgnoreCase);

    /// <summary>
    ///     Where-is-Floyd queries, matched against the WHOLE command rather than its first word.
    ///     Anchoring only the leading "where" was not enough: "where is floyd's card" and "where is
    ///     the survival kit, floyd" both led with a where-word and merely mentioned him, and both
    ///     were answered with his location. Floyd is the companion who carries the elevator and
    ///     mini-booth cards, so questions about his possessions are exactly what players ask.
    /// </summary>
    private static readonly Regex AsksWhereFloydIs = new(
        @"^(where(s)?\s+(is\s+|are\s+|was\s+|did\s+|has\s+|do\s+|does\s+)?(the\s+)?(floyd|robot)(\s+(go|gone|at|now))?"
        + @"|(find|locate)\s+(the\s+)?(floyd|robot))$",
        RegexOptions.IgnoreCase);

    /// <summary>
    ///     Returns the command that answers this mention of Floyd, or null to let normal parsing
    ///     handle the input.
    /// </summary>
    internal static IGlobalCommand? Match(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        var namesFloyd = NamesFloyd.IsMatch(input);

        // The where-is branch also answers for "robot", so that asking after a dead companion by the
        // synonym cannot slip past into the narrator's improvisation - the #545 tonal bug this whole
        // change exists to close. The PRE-meeting branch below stays name-only, so "examine robot" /
        // "where is the robot" in the Robot Shop keep their part in the discovery flow.
        var asksWhereHeIs = AsksWhereFloydIs.IsMatch(Normalize(input));

        if (!namesFloyd && !asksWhereHeIs)
            return null;

        // "blather, where is floyd" is Blather's line to answer. He is the NPC the whole opening
        // sequence is built around and he is standing right there; claiming it handed the player an
        // out-of-fiction wink instead of an in-fiction reply.
        if (AddressesAnotherCharacter(input))
            return null;

        var floyd = Repository.GetItem<Floyd>();

        // Before the player can know the name, ANY mention of it is a returning fan talking - where
        // is he, examine him, take him, greet him, ask him about Lazarus. Activation is the one
        // phrasing that has to keep working.
        if (!floyd.PlayerKnowsFloydByName)
            return namesFloyd && !ActivatesFloyd.IsMatch(input)
                ? new FreeResponseCommand(FloydConstants.NobodyHereByThatName)
                : null;

        // Once they know him, only the where-is queries are claimed. Examining him, talking to him
        // and every other verb keep the behavior they already have.
        return asksWhereHeIs ? new WhereIsFloydProcessor() : null;
    }

    /// <summary>
    ///     True when the command opens by addressing one of Planetfall's other talkable characters.
    ///     Only a LEADING name counts as address, the same rule ConversationHandler applies.
    /// </summary>
    private static bool AddressesAnotherCharacter(string input)
    {
        IItem[] others = [Repository.GetItem<Blather>(), Repository.GetItem<Ambassador>()];

        return others
            .SelectMany(other => other.NounsForMatching)
            .Any(noun => Regex.IsMatch(input, @"^\s*(hey\s+|yo\s+|the\s+)*" + Regex.Escape(noun) + @"\b",
                RegexOptions.IgnoreCase));
    }

    /// <summary>
    ///     Punctuation out, whitespace collapsed, so "where's floyd?" and "Where Is Floyd" both reach
    ///     the whole-command matcher in the one shape it has to describe.
    /// </summary>
    private static string Normalize(string input) =>
        Regex.Replace(input.StripNonChars().Trim(), @"\s+", " ");
}

/// <summary>
///     The fourth-wall line, as a FREE command. It is a parser-level "no such character" refusal,
///     not an in-world action, so it must not advance Context.Moves or tick Planetfall's survival
///     clocks (issue #354). It cost a full turn in the first cut of this file, which on Deck Nine
///     meant eight mentions burned eight of the twelve turns the player has to reach the escape pod.
/// </summary>
internal class FreeResponseCommand(string response) : IFreeGlobalCommand
{
    public Task<string> Process(string? input, IContext context, IGenerationClient client, Runtime runtime)
        => Task.FromResult(response);
}

/// <summary>
///     Answers "where is Floyd" from Floyd's actual state rather than from the narrator's
///     imagination (issue #552). Free for the same reason as <see cref="FreeResponseCommand" />:
///     asking after your companion is a status query, exactly like the sibling meta-commands
///     (diagnose, look, score, inventory, time), and must not cost survival time.
/// </summary>
internal class WhereIsFloydProcessor : IFreeGlobalCommand
{
    public Task<string> Process(string? input, IContext context, IGenerationClient client, Runtime runtime)
    {
        var floyd = Repository.GetItem<Floyd>();

        // Death first: his body is still lying in the Bio Lock, so the in-the-room checks below
        // would otherwise answer "right here" about a corpse.
        if (floyd.HasDied)
            return Task.FromResult(FloydConstants.WhereIsFloydDead);

        // A scripted errand nulls CurrentLocation while HasDied is still false - most painfully the
        // Bio Lab sacrifice, where the generic absent line told the player Floyd was "off exploring
        // somewhere" while they listened to him being torn apart. Must precede the absent branch.
        if (floyd.IsAwayOnScriptedSequence)
            return Task.FromResult(FloydConstants.WhereIsFloydOutOfSight);

        if (!floyd.IsInTheRoom(context))
            // Wandering and switched-off-and-left-behind are different facts. "He'll turn up" is
            // false of a robot the player personally deactivated.
            return Task.FromResult(floyd.IsOn
                ? FloydConstants.WhereIsFloydAbsent
                : FloydConstants.WhereIsFloydAbsentAndOff);

        // He is here. The wake-up countdown is its own state: PlayerKnowsFloydByName flips the
        // moment the switch is flipped, but IsOn does not flip until he wakes three turns later, so
        // without this the boot-up got the copy written for a Floyd the player deactivated.
        if (!floyd.IsOn && floyd.TurnOnCountdown > 0 && !floyd.HasEverBeenOn)
            return Task.FromResult(FloydConstants.WhereIsFloydStillBooting);

        // IsAlive rather than a bare IsOn: #545's lesson is that a liveness guard should be right in
        // its own right, not merely shadowed by an earlier return in the same method.
        return Task.FromResult(floyd.IsAlive
            ? FloydConstants.WhereIsFloydHereAndOn
            : FloydConstants.WhereIsFloydHereAndOff);
    }
}

using FluentAssertions;
using GameEngine;
using GameEngine.Diagnostics;

namespace Stationfall.Tests;

/// <summary>
///     The second measurement: not just whether a noun is recognised, but whether the verbs the
///     <i>original</i> specifically answers for are answered here too.
///     <para>
///         The battery is not invented. Each object's ZIL action routine names the verbs it handles, and
///         those are what is driven below — 133 object/verb pairs across the opening. A verb the original
///         did not handle is out of scope: there the original gives a generic refusal and so may we.
///     </para>
/// </summary>
[TestFixture]
public class VerbSweepTests : EngineTestsBase
{
    /// <summary>
    ///     ZIL verb → the phrasing a player would actually type.
    /// </summary>
    private static string Command(string verb, string noun) => verb switch
    {
        "examine" => $"examine {noun}",
        "read" => $"read {noun}",
        "take" => $"take {noun}",
        "drop" => $"drop {noun}",
        "open" => $"open {noun}",
        "close" => $"close {noun}",
        "eat" => $"eat {noun}",
        "empty" => $"empty {noun}",
        "pour" => $"pour {noun}",
        "touch" => $"touch {noun}",
        "taste" => $"taste {noun}",
        "smell" => $"smell {noun}",
        "search" => $"search {noun}",
        "look-inside" => $"look in {noun}",
        "count" => $"count {noun}",
        "listen" => $"listen to {noun}",
        "on" => $"turn on {noun}",
        "off" => $"turn off {noun}",
        "shake" => $"shake {noun}",
        "wear" => $"wear {noun}",
        "remove" => $"remove {noun}",
        "crumple" => $"crumple {noun}",
        "mung" => $"destroy {noun}",
        _ => $"{verb} {noun}"
    };

    /// <summary>
    ///     Objects reachable in the opening without solving anything, with the verbs their own action
    ///     routine in ship.zil handles. The companion and the other robots are swept separately — their
    ///     routines carry a great deal of behaviour that only becomes reachable much later.
    /// </summary>
    private static readonly (string[] Travel, string Noun, string[] Verbs)[] Targets =
    [
        ([], "assignment", ["crumple", "examine", "mung", "read"]),
        ([], "authorization", ["crumple", "examine", "mung", "read"]),
        ([], "activation", ["crumple", "examine", "mung", "read"]),
        ([], "chronometer", ["examine", "read"]),
        ([], "uniform", ["close", "examine", "open"]),
        (["south"], "pallets", ["close", "count", "look-inside", "mung", "open", "read", "search", "take"]),
        (["north", "east", "east"], "spacetruck", ["close", "examine", "look-inside", "open", "search"]),
        ([], "hatch", ["open", "close", "examine"]),
        // The hatch verbs leave it CLOSED (open, then close, then examine), so it has to be reopened
        // before boarding - otherwise every target below is swept from the cargo bay, where none of
        // them are in scope, and the sweep reports the whole truck as missing.
        (["open hatch", "in"], "kit", ["empty", "open"]),
        ([], "thermos", ["empty", "examine", "open", "pour", "look-inside"]),
        ([], "radio", ["examine", "listen", "off", "on"]),
        ([], "soup", ["eat", "empty", "examine", "pour", "taste", "touch"]),
        ([], "gray goo", ["drop", "eat", "remove", "take"])
    ];

    /// <summary>
    ///     Each target gets a FRESH engine. A sweep that reuses one game invalidates its own later
    ///     probes: emptying the Thermos destroys the soup, so every soup verb afterwards reports a gap
    ///     that does not exist. Replaying the route per target costs a little and buys a number that
    ///     means something.
    /// </summary>
    [Test]
    public async Task Report_WhichAuthoredVerbsTheOpeningStillLeavesToTheNarrator()
    {
        var gaps = new List<string>();
        var pairs = 0;

        foreach (var (travel, noun, verbs) in Targets)
        foreach (var verb in verbs)
        {
            pairs++;

            var target = GetTarget();
            foreach (var step in Route(travel, noun))
                await target.GetResponse(step);

            LeakRecorder.Clear();
            await target.GetResponse(Command(verb, noun));

            if (LeakRecorder.CompletenessLeaks.Count > 0)
                gaps.Add(Command(verb, noun));
        }

        TestContext.Out.WriteLine($"=== {gaps.Count} of {pairs} authored object/verb pairs fall through ===");
        foreach (var gap in gaps.Order())
            TestContext.Out.WriteLine($"  {gap}");

        Assert.Pass($"{gaps.Count}/{pairs}");
    }

    /// <summary>
    ///     The full route from the start of the game to where this noun is in scope. Cumulative,
    ///     because each target starts from a fresh game.
    /// </summary>
    private static IEnumerable<string> Route(string[] travel, string noun)
    {
        var toTheTruck = new[] { "east", "east", "open hatch", "in" };

        if (noun is "kit" or "thermos" or "radio" or "soup" or "gray goo")
        {
            foreach (var step in toTheTruck) yield return step;
            if (noun is not ("kit" or "radio")) yield return "open kit";
            if (noun is "soup") yield return "open thermos";
            yield break;
        }

        if (noun is "spacetruck" or "hatch")
        {
            yield return "east";
            yield return "east";
            yield break;
        }

        foreach (var step in travel) yield return step;
    }
}

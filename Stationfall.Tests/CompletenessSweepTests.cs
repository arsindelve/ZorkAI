using FluentAssertions;
using GameEngine;
using GameEngine.Diagnostics;

namespace Stationfall.Tests;

/// <summary>
///     Measures how finished the opening is, rather than asserting a guess about it.
///     <para>
///         Every command below is something a curious player would plainly try. Any of them that falls
///         through to the narrator is a gap: the engine will improvise an answer that reads exactly like
///         a real one, so an unfinished object is worse than a missing one — the player cannot tell.
///     </para>
/// </summary>
[TestFixture]
public class CompletenessSweepTests : EngineTestsBase
{
    /// <summary>
    ///     Nouns every room in the original answers for, wherever you are standing.
    /// </summary>
    private static readonly string[] UniversalNouns =
        ["walls", "wall", "floor", "ceiling", "air", "me", "hands"];

    /// <summary>
    ///     The route through the opening, with the nouns each room's own prose puts in front of the
    ///     player. A player who reads the description and types one of these words back has done
    ///     nothing unusual, and deserves an authored answer.
    /// </summary>
    private static readonly (string Travel, string[] Nouns)[] OpeningRooms =
    [
        ("", ["door", "slot", "corridor", "deck"]),
        ("south", ["pallets", "boxes", "forms"]),
        ("north", []),
        ("east", ["entrance"]),
        ("north", ["keypad", "slot", "first bin", "second bin", "third bin", "floyd", "rex", "helen"]),
        ("south", []),
        ("east", ["spacetruck", "hatch"])
    ];

    private async Task<List<NarrationLeak>> SweepTheOpening()
    {
        var target = GetTarget();

        foreach (var (travel, nouns) in OpeningRooms)
        {
            if (!string.IsNullOrEmpty(travel))
                await target.GetResponse(travel);

            foreach (var noun in nouns.Concat(UniversalNouns))
                await target.GetResponse($"examine {noun}");
        }

        return LeakRecorder.CompletenessLeaks.ToList();
    }

    [Test]
    public async Task Report_WhatTheOpeningStillLeavesToTheNarrator()
    {
        var leaks = await SweepTheOpening();

        var byNoun = leaks
            .GroupBy(l => $"{l.Location} :: {l.Input}")
            .OrderBy(g => g.Key)
            .ToList();

        TestContext.Out.WriteLine($"=== {leaks.Count} narrator fall-throughs across the opening ===");

        var universal = leaks.Count(l => UniversalNouns.Any(n => l.Input.EndsWith(n, StringComparison.OrdinalIgnoreCase)));
        TestContext.Out.WriteLine($"  {universal} from nouns every room should answer for (walls/floor/ceiling/air/me/hands)");
        TestContext.Out.WriteLine($"  {leaks.Count - universal} from nouns specific to one room");
        TestContext.Out.WriteLine("--- room-specific gaps ---");
        foreach (var group in byNoun.Where(g => !UniversalNouns.Any(n => g.First().Input.EndsWith(n, StringComparison.OrdinalIgnoreCase))))
            TestContext.Out.WriteLine($"  {group.Key}");

        // Reporting only. The assertion that this must reach zero is the test below, which is the one
        // that gates "the opening is finished".
        Assert.Pass($"{leaks.Count} gaps recorded.");
    }

    /// <summary>
    ///     The gate. While this passes, nothing in the opening is being improvised by the narrator.
    ///     A new object or room description that names something it does not answer for will fail here,
    ///     which is the point: content and its coverage land together or not at all.
    /// </summary>
    [Test]
    public async Task TheOpeningLeavesNothingToTheNarrator()
    {
        var leaks = await SweepTheOpening();

        leaks.Should().BeEmpty(
            "every noun the opening's own prose puts in front of the player must have an authored answer");
    }
}

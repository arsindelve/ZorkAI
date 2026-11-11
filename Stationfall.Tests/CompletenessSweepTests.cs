using FluentAssertions;
using GameEngine;
using GameEngine.Diagnostics;
using GameEngine.Item.ItemProcessor;
using Model.AIParsing;
using Model.Intent;
using Moq;

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

    /// <summary>
    ///     The same route, asking to pick each noun up instead of looking at it — the other half of what
    ///     a curious player does with a thing the prose just named (issue #577).
    ///     <para>
    ///         Deliberately NOT driven through <c>GetResponse</c>: TestParser resolves every "take X" to a
    ///         SimpleIntent, while production's parser frequently tags it as a TakeIntent and the engine
    ///         dispatches that to <see cref="TakeOrDropInteractionProcessor" /> instead. Sweeping only the
    ///         shape the test parser produces is what let the take path go unmeasured while the examine
    ///         sweep read zero. This drives the TakeIntent overload exactly as GameEngine.cs does, so the
    ///         gaps it records are the ones a real player hits.
    ///     </para>
    ///     <para>
    ///         Runs on its own engine: a take that succeeds moves an object out of the room, which would
    ///         quietly change what the examine sweep above is looking at.
    ///     </para>
    /// </summary>
    private async Task<List<NarrationLeak>> SweepTheOpeningForTakes()
    {
        var target = GetTarget();

        // What the real take/drop list-parser does with a noun the room description does not offer as
        // a takeable object; the processor then falls back to resolving the player's own noun.
        var takeAndDropParser = new Mock<IAITakeAndAndDropParser>();
        takeAndDropParser.Setup(s => s.GetListOfItemsToTake(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync([]);
        var processor = new TakeOrDropInteractionProcessor(takeAndDropParser.Object);

        foreach (var (travel, nouns) in OpeningRooms)
        {
            if (!string.IsNullOrEmpty(travel))
                await target.GetResponse(travel);

            foreach (var noun in nouns.Concat(UniversalNouns))
            {
                target.Context.LastInput = $"take {noun}";
                await processor.Process(
                    new TakeIntent { Noun = noun, OriginalInput = $"take {noun}" },
                    target.Context, LeakRecorder);
            }
        }

        return LeakRecorder.CompletenessLeaks.ToList();
    }

    [Test]
    public async Task Report_WhatTheOpeningStillLeavesToTheNarrator()
    {
        var examineLeaks = await SweepTheOpening();
        var takeLeaks = await SweepTheOpeningForTakes();

        Report("examine", examineLeaks);
        Report("take", takeLeaks);

        // Reporting only. The assertion that these must reach zero is the pair of tests below, which
        // are what gate "the opening is finished".
        Assert.Pass($"{examineLeaks.Count + takeLeaks.Count} gaps recorded.");
    }

    private static void Report(string verb, List<NarrationLeak> leaks)
    {
        var byNoun = leaks
            .GroupBy(l => $"{l.Location} :: {l.Input}")
            .OrderBy(g => g.Key)
            .ToList();

        TestContext.Out.WriteLine($"=== {leaks.Count} narrator fall-throughs across the opening ({verb}) ===");

        var universal = leaks.Count(l => UniversalNouns.Any(n => l.Input.EndsWith(n, StringComparison.OrdinalIgnoreCase)));
        TestContext.Out.WriteLine($"  {universal} from nouns every room should answer for (walls/floor/ceiling/air/me/hands)");
        TestContext.Out.WriteLine($"  {leaks.Count - universal} from nouns specific to one room");
        TestContext.Out.WriteLine("--- room-specific gaps ---");
        foreach (var group in byNoun.Where(g => !UniversalNouns.Any(n => g.First().Input.EndsWith(n, StringComparison.OrdinalIgnoreCase))))
            TestContext.Out.WriteLine($"  {group.Key}");
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

    /// <summary>
    ///     The same gate for the other half of the sweep. "Examine it" and "take it" are the two things
    ///     a player does with a noun the prose just handed them; a zero that only counts the first is a
    ///     floor, not a ceiling (issue #577).
    /// </summary>
    [Test]
    public async Task TheOpeningLeavesNothingToTheNarrator_WhenYouTryToPickThingsUp()
    {
        var leaks = await SweepTheOpeningForTakes();

        leaks.Should().BeEmpty(
            "every noun the opening's own prose puts in front of the player must say why it can't be taken");
    }
}

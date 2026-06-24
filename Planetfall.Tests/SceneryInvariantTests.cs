using System.Reflection;
using FluentAssertions;
using GameEngine;
using GameEngine.Item;
using GameEngine.Location;
using Model.AIGeneration;
using Model.AIParsing;
using Model.Intent;
using Model.Interaction;
using Moq;
using Planetfall.Location.Lawanda;

namespace Planetfall.Tests;

/// <summary>
///     Deterministic invariant gate for the room-scenery mechanism (issue #315). These tests scan
///     <em>every</em> Planetfall location that declares <see cref="SceneryItem" />s, so they cover the
///     whole scenery sweep automatically as more rooms are filled in — no AI, no per-room boilerplate.
/// </summary>
public class SceneryInvariantTests : EngineTestsBase
{
    // Scenery is a protected virtual on the base; fetch the PropertyInfo from the declaring type and
    // invoke it on each derived instance so virtual dispatch returns the override's data.
    private static readonly PropertyInfo SceneryProperty =
        typeof(LocationBase).GetProperty("Scenery", BindingFlags.NonPublic | BindingFlags.Instance)!;

    private static IEnumerable<(Type type, IReadOnlyList<SceneryItem> scenery)> LocationsWithScenery()
    {
        foreach (var type in typeof(MiniaturizationBooth).Assembly.GetTypes()
                     .Where(t => t is { IsClass: true, IsAbstract: false, IsGenericType: false }
                                 && t.IsSubclassOf(typeof(LocationBase))))
        {
            var instance = (LocationBase)Activator.CreateInstance(type)!;
            var scenery = (IReadOnlyList<SceneryItem>)SceneryProperty.GetValue(instance)!;
            if (scenery.Count > 0)
                yield return (type, scenery);
        }
    }

    [Test]
    public void EverySceneryEntry_DeclaresNounsAndAnExamineDescription()
    {
        foreach (var (type, scenery) in LocationsWithScenery())
        foreach (var item in scenery)
        {
            item.Nouns.Should().NotBeEmpty($"{type.Name} scenery must declare at least one noun");
            item.Nouns.Should().OnlyContain(n => !string.IsNullOrWhiteSpace(n) && n == n.ToLowerInvariant().Trim(),
                $"{type.Name} scenery nouns must be lowercase, trimmed and non-empty (the parser lowercases input)");
            item.ExaminationDescription.Trim().Should()
                .NotBeEmpty($"{type.Name} scenery must have a non-empty examine description");
        }
    }

    [Test]
    public void NoNoun_IsDeclaredOnMoreThanOneSceneryEntry_InTheSameRoom()
    {
        foreach (var (type, scenery) in LocationsWithScenery())
        {
            var nouns = scenery.SelectMany(s => s.Nouns.Select(n => n.ToLowerInvariant())).ToList();
            nouns.Should().OnlyHaveUniqueItems(
                $"{type.Name} declares the same scenery noun on more than one entry, making the match ambiguous");
        }
    }

    [Test]
    public void NoSceneryNoun_CollidesWithARealItemInTheSameRoom()
    {
        foreach (var (type, scenery) in LocationsWithScenery())
        {
            // Init seeds the room's real items; a scenery noun must never overlap one of them, or the
            // real object (checked first) would shadow the scenery, or disambiguation would fire.
            Repository.Reset();
            var instance = (LocationBase)Activator.CreateInstance(type)!;
            instance.Init();

            var realItemNouns = instance.GetAllItemsRecursively
                .SelectMany(i => i.NounsForMatching)
                .Select(n => n.ToLowerInvariant())
                .ToHashSet();

            foreach (var noun in scenery.SelectMany(s => s.Nouns))
                realItemNouns.Should().NotContain(noun.ToLowerInvariant(),
                    $"{type.Name} scenery noun '{noun}' collides with a real item in the room; " +
                    "remove the scenery entry or give the real item the examine behavior instead");
        }
    }

    [Test]
    public async Task EverySceneryNoun_ResolvesToItsText_ThroughTheRealOverrideChain()
    {
        // Bypass the parser (the test parser only knows real item nouns) and call the room's real
        // RespondToSimpleInteraction directly. This catches the trap a structural test cannot: a room
        // whose override forgets to call base, leaving its scenery dead.
        var engine = GetTarget();
        var client = Mock.Of<IGenerationClient>();
        var factory = new ItemProcessorFactory(Mock.Of<IAITakeAndAndDropParser>());

        foreach (var (type, scenery) in LocationsWithScenery())
        {
            var loc = (LocationBase)Activator.CreateInstance(type)!;
            loc.Init();
            engine.Context.CurrentLocation = loc;

            foreach (var s in scenery)
            {
                var noun = s.Nouns[0];

                var examine = await loc.RespondToSimpleInteraction(
                    new SimpleIntent { Verb = "examine", Noun = noun }, engine.Context, client, factory);
                examine.Should().BeOfType<PositiveInteractionResult>(
                    $"examine '{noun}' in {type.Name} must resolve as scenery — does the room's " +
                    "RespondToSimpleInteraction override call base?");
                examine.InteractionMessage.Should().Be(s.ExaminationDescription, $"{type.Name} examine '{noun}'");

                if (s.CannotBeTakenReason is not null)
                {
                    var take = await loc.RespondToSimpleInteraction(
                        new SimpleIntent { Verb = "take", Noun = noun }, engine.Context, client, factory);
                    take.InteractionMessage.Should().Be(s.CannotBeTakenReason, $"{type.Name} take '{noun}'");
                }
            }
        }
    }
}

using FluentAssertions;
using GameEngine;
using Planetfall.Hints;
using Planetfall.Item.Kalamontee.Mech;

namespace Planetfall.Tests.Hints.Oracle;

/// <summary>The real provider over a real game: the knowledge is embedded, and the situation is read from live state.</summary>
[TestFixture]
public class PlanetfallOracleProviderTests : EngineTestsBase
{
    [SetUp]
    public void SetUp() => GetTarget();

    [Test]
    public void TheGameBible_IsEmbedded()
    {
        PlanetfallOracleProvider.IsAvailable.Should().BeTrue();
        var bible = new PlanetfallOracleProvider().GameKnowledge;
        bible.Length.Should().BeGreaterThan(50_000);
        bible.Should().Contain("Floyd").And.Contain("Feinstein").And.Contain("Lawanda");
        bible.Should().NotContain("class ").And.NotContain("namespace "); // understanding, not source
    }

    [Test]
    public void AFreshGame_HasNothingChanged()
    {
        var situation = new PlanetfallOracleProvider().DescribeSituation(Context);
        situation.Should().Contain("Location: Deck Nine");
        situation.Should().Contain("Floyd: never switched on");
        situation.Should().Contain("Day 1");
        situation.Should().NotContain("What is different in the world");
    }

    [Test]
    public void WhatThePlayerHasDone_ShowsUpAsFacts_WithoutAnyPuzzleKnowledgeHere()
    {
        var magnet = Repository.GetItem<Magnet>();
        magnet.HasEverBeenPickedUp = true;
        Context.ItemPlacedHere(magnet);

        var situation = new PlanetfallOracleProvider().DescribeSituation(Context);

        situation.Should().Contain($"Carrying: {magnet.Name}");
        situation.Should().Contain("Magnet.HasEverBeenPickedUp = True");
    }
}

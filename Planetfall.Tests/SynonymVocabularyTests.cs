using FluentAssertions;
using GameEngine;
using Planetfall.Item.Feinstein;
using Planetfall.Item.Lawanda.BioLab;
using Planetfall.Item.Lawanda.Lab;

namespace Planetfall.Tests;

/// <summary>
/// Issue #228: the C# port's NounsForMatching arrays had dropped some of the original
/// Infocom (SYNONYM ...) vocabulary, so perfectly reasonable references silently failed to
/// resolve. These tests pin the restored gameplay-relevant synonyms for Planetfall.
/// </summary>
public class SynonymVocabularyTests : EngineTestsBase
{
    // In the Bio Lab chase a panicked player naturally types "attack monster" / "kill monster".
    // The original gives each mutant MONSTER as a synonym; restore it for every mutant.
    [Test]
    public void MutantGrue_RecognizesMonsterSynonym()
    {
        GetTarget();
        Repository.GetItem<MutantGrue>().HasMatchingNoun("monster").HasItem.Should().BeTrue();
    }

    [Test]
    public void MutantTroll_RecognizesMonsterSynonym()
    {
        GetTarget();
        Repository.GetItem<MutantTroll>().HasMatchingNoun("monster").HasItem.Should().BeTrue();
    }

    [Test]
    public void RatAnt_RecognizesMonsterSynonym()
    {
        GetTarget();
        Repository.GetItem<RatAnt>().HasMatchingNoun("monster").HasItem.Should().BeTrue();
    }

    [Test]
    public void Triffid_RecognizesMonsterSynonym()
    {
        GetTarget();
        Repository.GetItem<Triffid>().HasMatchingNoun("monster").HasItem.Should().BeTrue();
    }

    // Zork-muscle-memory players type "lantern" for the portable lamp.
    [TestCase("lamp")]
    [TestCase("light")]
    [TestCase("lantern")]
    public void Lamp_RecognizesOriginalSynonyms(string noun)
    {
        GetTarget();
        Repository.GetItem<Lamp>().HasMatchingNoun(noun).HasItem.Should().BeTrue();
    }

    // The pile of computer output is described throughout as "the printout".
    [TestCase("output")]
    [TestCase("computer output")]
    [TestCase("printout")]
    public void ComputerOutput_RecognizesOriginalSynonyms(string noun)
    {
        GetTarget();
        Repository.GetItem<ComputerOutput>().HasMatchingNoun(noun).HasItem.Should().BeTrue();
    }
}

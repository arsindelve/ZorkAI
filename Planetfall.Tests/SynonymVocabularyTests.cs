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
    // The original gives each mutant MONSTER as a synonym, shared via MutantBase.
    private static readonly MutantBase[] Mutants =
        [new MutantGrue(), new MutantTroll(), new RatAnt(), new Triffid()];

    [TestCaseSource(nameof(Mutants))]
    public void Mutants_RecognizeMonsterSynonym(MutantBase mutant)
    {
        mutant.HasMatchingNoun("monster").HasItem.Should().BeTrue();
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

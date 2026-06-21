using FluentAssertions;
using GameEngine;
using ZorkOne.Item;
using ZorkOne.Location;

namespace ZorkOne.Tests.Things;

/// <summary>
/// Issue #228: the C# port's NounsForMatching arrays had dropped some of the original
/// Infocom (SYNONYM ...) vocabulary, so perfectly reasonable references silently failed to
/// resolve. This is the systematic generalization of the #23 sceptre fix. These tests pin the
/// restored synonyms so the gameplay-relevant words keep resolving.
/// </summary>
public class SynonymVocabularyTests : EngineTestsBase
{
    [TestCase("leaflet")]
    [TestCase("mail")]
    [TestCase("booklet")]
    [TestCase("pamphlet")]
    [TestCase("advertisement")]
    public void Leaflet_RecognizesOriginalSynonyms(string noun)
    {
        GetTarget();
        Repository.GetItem<Leaflet>().HasMatchingNoun(noun).HasItem.Should().BeTrue();
    }

    [TestCase("map")]
    [TestCase("ancient map")]
    [TestCase("parchment")]
    public void Map_RecognizesOriginalSynonyms(string noun)
    {
        GetTarget();
        Repository.GetItem<Map>().HasMatchingNoun(noun).HasItem.Should().BeTrue();
    }

    [TestCase("skeleton")]
    [TestCase("bones")]
    [TestCase("body")]
    [TestCase("remains")]
    public void Skeleton_RecognizesOriginalSynonyms(string noun)
    {
        GetTarget();
        Repository.GetItem<Skeleton>().HasMatchingNoun(noun).HasItem.Should().BeTrue();
    }

    [Test]
    public async Task ReadMail_ResolvesToTheLeaflet()
    {
        var target = GetTarget();
        var leaflet = Repository.GetItem<Leaflet>();
        target.Context.Take(leaflet);

        var response = await target.GetResponse("read mail");

        response.Should().Contain("WELCOME TO ZORK!");
    }

    // The original DAM object (SYNONYM DAM GATE GATES) is examinable. In this port the dam is a
    // location rather than an item, so "examine dam" had nothing to resolve to. It is now handled
    // at the location level rather than mis-attributed to the unrelated Hades Gate item.
    [TestCase("examine dam")]
    [TestCase("examine gates")]
    public async Task ExamineDam_AtTheDam_DescribesTheSluiceGates(string input)
    {
        var target = GetTarget();
        target.Context.CurrentLocation = Repository.GetLocation<Dam>();

        var response = await target.GetResponse(input);

        response.Should().Contain("sluice gates");
    }
}

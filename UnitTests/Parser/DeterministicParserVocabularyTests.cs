using GameEngine;
using Model;
using Model.Intent;
using Model.Movement;

namespace UnitTests.Parser;

/// <summary>
///     Exhaustive tests of the <see cref="DeterministicParser" /> against each game's whole harvested
///     vocabulary: every noun, every verb, every room name. Hand-picked examples prove the grammar works
///     for the words someone thought of; these prove it for the words the games actually contain, and they
///     keep proving it as items, rooms and verb families are added.
/// </summary>
[TestFixture("ZorkOne")]
[TestFixture("Planetfall")]
[TestFixture("Stationfall")]
[TestFixture("EscapeRoom")]
public class DeterministicParserVocabularyTests(string game)
{
    private DeterministicParser _parser = null!;
    private string[] _nouns = null!;
    private string[] _verbs = null!;

    [SetUp]
    public void SetUp()
    {
        Repository.Reset();
        _parser = new DeterministicParser(game);
        _nouns = Repository.GetNouns(game).Concat(Repository.GetSceneryNouns(game))
            .Select(noun => noun.ToLowerInvariant()).Distinct().Order().ToArray();
        _verbs = Repository.GetVerbs(game).SelectMany(family => family)
            .Where(verb => !verb.Contains(' ')).Select(verb => verb.ToLowerInvariant()).Distinct().Order().ToArray();
    }

    [Test]
    public void TheVocabularyIsNotEmpty()
    {
        _nouns.Should().NotBeEmpty();
        _verbs.Should().NotBeEmpty();
    }

    [Test]
    public void EveryNounTheGameDeclares_CanBeTheObjectOfAVerb()
    {
        var failures = new List<string>();

        foreach (var noun in _nouns)
            if (_parser.Parse($"examine {noun}") is not SimpleIntent { Verb: "examine" } simple ||
                !simple.MatchNoun([noun]))
                failures.Add(noun);

        failures.Should().BeEquivalentTo(Array.Empty<string>(),
            "\"examine <noun>\" must resolve to that noun for every noun the game declares");
    }

    [Test]
    public void EveryNounTheGameDeclares_SurvivesALeadingArticle()
    {
        var failures = new List<string>();

        foreach (var noun in _nouns)
            if (_parser.Parse($"examine the {noun}") is not SimpleIntent simple || !simple.MatchNoun([noun]))
                failures.Add(noun);

        failures.Should().BeEquivalentTo(Array.Empty<string>());
    }

    [Test]
    public void EveryVerbTheEngineShares_IsUnderstoodWithAnyNoun()
    {
        // A plain one-word noun, so the verb is the only thing that varies.
        var noun = _nouns.First(candidate => !candidate.Contains(' ') && candidate.All(char.IsLetter) &&
                                             DirectionParser.ParseDirection(candidate) == Direction.Unknown);
        var failures = new List<string>();

        foreach (var verb in SingleWords(declaration => IsEngineLevel(declaration.DeclaredOn)))
            if (!IsUnderstood(verb, noun, _parser.Parse($"{verb} {noun}")))
                failures.Add($"{verb} {noun}");

        failures.Should().BeEquivalentTo(Array.Empty<string>(),
            "a verb the thesaurus declares is answered for any object");
    }

    [Test]
    public void EveryVerbAnObjectDeclares_IsUnderstoodOnThatObjectsOwnNouns()
    {
        var owners = Repository.GetNounOwners(game);
        var failures = new List<string>();

        foreach (var (family, declaredOn) in Repository.GetVerbDeclarations(game)
                     .Where(declaration => !IsEngineLevel(declaration.DeclaredOn)))
        foreach (var noun in owners.Where(owner => declaredOn.IsAssignableFrom(owner.Owner))
                     .SelectMany(owner => owner.Nouns).Select(noun => noun.ToLowerInvariant()).Distinct())
        foreach (var verb in family.Where(word => !word.Contains(' ')))
            if (!IsUnderstood(verb, noun, _parser.Parse($"{verb} {noun}")))
                failures.Add($"{verb} {noun} ({declaredOn.Name})");

        failures.Should().BeEquivalentTo(Array.Empty<string>(),
            "a verb an item or room declares must resolve when aimed at that item's or room's own nouns");
    }

    [Test]
    public void AVerbOnlyOneObjectDeclares_IsLeftToTheAiOnEveryOtherNoun()
    {
        var owners = Repository.GetNounOwners(game);
        var shared = SingleWords(declaration => IsEngineLevel(declaration.DeclaredOn))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var failures = new List<string>();

        foreach (var group in Repository.GetVerbDeclarations(game)
                     .Where(declaration => !IsEngineLevel(declaration.DeclaredOn))
                     .SelectMany(declaration => declaration.Family.Where(word => !word.Contains(' '))
                         .Select(verb => (Verb: verb.ToLowerInvariant(), declaration.DeclaredOn)))
                     .Where(pair => !shared.Contains(pair.Verb))
                     .GroupBy(pair => pair.Verb))
        {
            var itsNouns = owners
                .Where(owner => group.Any(pair => pair.DeclaredOn.IsAssignableFrom(owner.Owner)))
                .SelectMany(owner => owner.Nouns).Select(noun => noun.ToLowerInvariant()).ToHashSet();

            foreach (var noun in _nouns.Where(noun => !itsNouns.Contains(noun)))
                if (_parser.Parse($"{group.Key} {noun}") is not null)
                    failures.Add($"{group.Key} {noun}");
        }

        failures.Should().BeEquivalentTo(Array.Empty<string>(),
            "known is not answered: a verb only one object declares does nothing to any other object, so " +
            "the grammar must hand it to the AI, which can reduce it to a verb that object does answer");
    }

    private static bool IsEngineLevel(Type type)
    {
        return type.Assembly == typeof(Verbs).Assembly || type.Assembly == typeof(DeterministicParser).Assembly;
    }

    private IEnumerable<string> SingleWords(Func<(VerbFamily Family, Type DeclaredOn), bool> which)
    {
        return Repository.GetVerbDeclarations(game).Where(which)
            .SelectMany(declaration => declaration.Family)
            .Where(word => !word.Contains(' '))
            .Select(word => word.ToLowerInvariant())
            .Distinct();
    }

    private static bool IsUnderstood(string verb, string noun, IntentBase? result)
    {
        return verb switch
        {
            // Leaving something is the AI's to resolve; getting into something is its own intent.
            "exit" or "leave" or "disembark" => result is null,
            "enter" or "board" => result is EnterSubLocationIntent board && board.Noun == noun,
            _ => result is SimpleIntent simple && simple.Verb == verb && simple.Noun == noun
        };
    }

    [Test]
    public void EveryRoomNameAndAlias_IsAGoToDestination()
    {
        var failures = new List<string>();

        foreach (var name in Repository.GetDestinationNames(game).Select(name => name.Trim().ToLowerInvariant()).Distinct())
            if (_parser.Parse($"go to {name}") is not GoToDestinationIntent)
                failures.Add(name);

        failures.Should().BeEquivalentTo(Array.Empty<string>(),
            "\"go to <room>\" must be recognised for every room name and alias the game declares");
    }
}

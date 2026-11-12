using System.Reflection;
using System.Reflection.Emit;
using GameEngine;
using Model;
using Model.Intent;

namespace UnitTests;

[TestFixture]
public class VerbFamilyTests
{
    private static readonly VerbFamily Shift = new("move", "slide", "pull");

    [Test]
    public void TheFirstWordIsTheCanonicalName()
    {
        Shift.Name.Should().Be("move");
        Shift.ToString().Should().Be("move");
    }

    [Test]
    public void Matches_IgnoresCase_AndRejectsEverythingElse()
    {
        Shift.Matches("PULL").Should().BeTrue();
        Shift.Matches("push").Should().BeFalse();
        Shift.Matches(null).Should().BeFalse();
    }

    [Test]
    public void ASpreadBuildsOnAnotherFamily_WithoutRepeatingSharedWords()
    {
        var built = new VerbFamily([..Verbs.KillVerbs, ..Verbs.BreakVerbs, "kick"]);

        built.Should().Contain(["kill", "smash", "kick"]);
        built.Count(word => word == "destroy").Should().Be(1, "\"destroy\" is in both source families");
    }

    [Test]
    public void AFamilyNeedsAtLeastOneRealVerb()
    {
        var empty = () => new VerbFamily();
        var blank = () => new VerbFamily("take", " ");

        empty.Should().Throw<ArgumentException>();
        blank.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Intents_MatchTheirVerbAgainstAFamily()
    {
        var simple = new SimpleIntent { Verb = "Slide", Noun = "rug" };
        var multi = new MultiNounIntent
            { Verb = "pull", NounOne = "rug", NounTwo = "hook", Preposition = "with", OriginalInput = "" };

        simple.MatchVerb(Shift).Should().BeTrue();
        simple.Match(Shift, ["rug"]).Should().BeTrue();
        simple.Match(Shift, ["lamp"]).Should().BeFalse();
        multi.MatchVerb(Shift).Should().BeTrue();
        multi.Match(Shift, ["rug"], ["hook"], ["with"]).Should().BeTrue();
        multi.MatchVerb(Verbs.DropVerbs).Should().BeFalse();
    }
}

/// <summary>
///     <see cref="Repository.GetVerbs" /> is to verbs what <see cref="Repository.GetNouns" /> is to nouns.
///     These pin where it looks, because a place it does not look is a verb the vocabulary silently lacks.
/// </summary>
[TestFixture]
public class GetVerbsTests
{
    private static HashSet<string> Words(string game)
    {
        return Repository.GetVerbs(game).SelectMany(family => family).ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    [TestCase("ZorkOne")]
    [TestCase("Planetfall")]
    [TestCase("Stationfall")]
    [TestCase("EscapeRoom")]
    public void EveryGame_GetsTheSharedThesaurus_IncludingWhatTheEngineProcessorsAnswer(string game)
    {
        var families = Repository.GetVerbs(game);

        families.Should().Contain(Verbs.TakeVerbs).And.Contain(Verbs.ExamineVerbs);
        families.Should().Contain(Verbs.SmellVerbs).And.Contain(Verbs.DoffVerbs).And.Contain(Verbs.PutVerbs);
    }

    [Test]
    public void AGroupingIsInAGamesVocabulary_OnlyIfThatGameOrTheEngineUsesIt()
    {
        // Every grouping is declared in one file for all games. What puts one in a game's vocabulary is
        // that game's code matching on it.
        Repository.GetVerbs("Planetfall").Should().Contain(Verbs.SetVerbs, "the laser and the conference room door use it");
        Repository.GetVerbs("ZorkOne").Should().NotContain(Verbs.SetVerbs, "nothing in Zork I sets a dial");

        Repository.GetVerbs("ZorkOne").Should().Contain(Verbs.WaveVerbs, "the rainbow rooms use it");
        Repository.GetVerbs("Planetfall").Should().NotContain(Verbs.WaveVerbs);
    }

    [Test]
    public void AGroupingTheEngineUses_IsAnsweredForAnyObject_AndOneAGameClassUsesIsAnsweredByThatClass()
    {
        var owners = Repository.GetVerbOwners("ZorkOne");

        owners.Where(owned => ReferenceEquals(owned.Family, Verbs.TakeVerbs)).Select(owned => owned.Owner)
            .Should().Contain((Type?)null, "the take processor matches on it for every item");

        owners.Where(owned => ReferenceEquals(owned.Family, Verbs.WaveVerbs)).Select(owned => owned.Owner!.Name)
            .Should().BeEquivalentTo("OnTheRainbow", "EndOfRainbow");
    }

    [Test]
    public void AGame_GetsTheFamiliesItsOwnItemsAndRoomsDeclare()
    {
        Words("ZorkOne").Should().Contain("rearrange", "the rug declares it");
        Words("Planetfall").Should().Contain("lubricate", "Floyd declares it");
        Words("Stationfall").Should().Contain("requisition", "the ship's robots declare it");
    }

    [Test]
    public void AGame_DoesNotGetAnotherGamesFamilies()
    {
        Words("Planetfall").Should().NotContain("rearrange");
        Words("ZorkOne").Should().NotContain("lubricate").And.NotContain("requisition");
    }

    [Test]
    public void AFamilyDeclaredOnAGenericBaseClass_IsFoundThroughItsSubclasses()
    {
        // SlotBase<TAccessCard, TAccessSlot> declares the card-sliding verbs. An open generic type has no
        // statics to read; only its closed forms do, and those are reached as a subclass's base type.
        Words("Planetfall").Should().Contain(["swipe", "scan", "glide"]);
    }

    [Test]
    public void TheVerbsTheHandTypedListsMissed_AreNowKnown()
    {
        // Each of these was answered by a handler but absent from every verb list the parsers consulted.
        Words("ZorkOne").Should().Contain(["pull", "burn", "cut", "shake", "sit", "cross"]);
        Words("Planetfall").Should().Contain(["pull", "slide", "deactivate", "collapse", "shake", "kiss"]);
    }
}

/// <summary>
///     The rule that makes <see cref="Repository.GetVerbs" /> complete: a <see cref="VerbFamily" /> is only
///     ever created in a static field initialiser. One built inline inside a method would match perfectly
///     well, and no reflection over fields would ever find it - which is exactly the drift the type exists
///     to rule out. The compiler cannot express "static fields only", so this reads the compiled IL instead.
/// </summary>
[TestFixture]
public class VerbFamilyDeclarationTests
{
    private static readonly string[] AssembliesThatAnswerVerbs =
        ["Model", "GameEngine", "StellarPatrol", "ZorkOne", "ZorkTwo", "Planetfall", "Stationfall", "EscapeRoom"];

    private const BindingFlags Everything = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static |
                                            BindingFlags.Instance | BindingFlags.DeclaredOnly;

    private static IEnumerable<Type> AllTypes()
    {
        return AssembliesThatAnswerVerbs.Select(Assembly.Load).SelectMany(assembly => assembly.GetTypes());
    }

    [Test]
    public void AVerbFamilyIsOnlyEverCreatedInAStaticFieldInitialiser()
    {
        var offenders = new List<string>();

        foreach (var type in AllTypes().Where(type => type != typeof(VerbFamily)))
        foreach (var method in type.GetMethods(Everything).Cast<MethodBase>().Concat(type.GetConstructors(Everything)))
        {
            // A static field initialiser compiles into the type's static constructor.
            if (method is ConstructorInfo { IsStatic: true })
                continue;

            if (CreatesAVerbFamily(method))
                offenders.Add($"{type.FullName}.{method.Name}");
        }

        // BeEquivalentTo, not BeEmpty: it names every offender, where BeEmpty reports only the first.
        offenders.Should().BeEquivalentTo(Array.Empty<string>(),
            "a VerbFamily must be a static readonly field so the vocabulary harvest can find it; these " +
            "methods build one inline instead");
    }

    private static readonly string[] Games = ["ZorkOne", "Planetfall", "Stationfall", "EscapeRoom"];

    [Test]
    public void AFamilyAGameClassDeclares_IsInThatGamesVocabulary()
    {
        foreach (var game in Games)
        {
            var harvested = Repository.GetVerbs(game)
                .Select(family => string.Join('|', family))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var declared = Assembly.Load(game).GetTypes()
                .Where(type => !type.ContainsGenericParameters)
                .SelectMany(type => type.GetFields(Everything))
                .Where(field => field.IsStatic && field.FieldType == typeof(VerbFamily))
                .Select(field => (Field: $"{field.DeclaringType!.FullName}.{field.Name}",
                    Words: string.Join('|', (VerbFamily)field.GetValue(null)!)));

            declared.Where(family => !harvested.Contains(family.Words)).Select(family => family.Field)
                .Should().BeEquivalentTo(Array.Empty<string>(), $"every family {game} declares must reach its vocabulary");
        }
    }

    /// <summary>
    ///     A grouping nothing matches on is vocabulary no game has: it looks like a supported verb to
    ///     anyone reading <see cref="Verbs" />, and no parser will ever be told about it.
    /// </summary>
    [Test]
    public void EveryGroupingInTheThesaurus_IsUsedByTheEngineOrSomeGame()
    {
        var used = Games.SelectMany(Repository.GetVerbs).ToHashSet();

        var unused = typeof(Verbs).GetNestedTypes().Append(typeof(Verbs))
            .SelectMany(type => type.GetFields(Everything))
            .Where(field => field.IsStatic && field.FieldType == typeof(VerbFamily))
            .Where(field => !used.Contains((VerbFamily)field.GetValue(null)!))
            .Select(field => $"{field.DeclaringType!.Name}.{field.Name}");

        unused.Should().BeEquivalentTo(Array.Empty<string>(),
            "a grouping in Verbs.cs that no handler matches on is dead vocabulary");
    }

    /// <summary>
    ///     The same words declared in two places are one family written twice, and the two copies drift the
    ///     first time someone adds a synonym to one of them. Once a second object answers a set of words it
    ///     moves to <see cref="Verbs" /> and both use it from there.
    /// </summary>
    [Test]
    public void NoTwoFamiliesHoldTheSameWords()
    {
        var repeated = DeclaredFamilies()
            .GroupBy(family => family.Words, StringComparer.OrdinalIgnoreCase)
            .Where(sameWords => sameWords.Count() > 1)
            .Select(sameWords => $"{sameWords.Key}: {string.Join(", ", sameWords.Select(family => family.Field).Order())}")
            .Order();

        repeated.Should().BeEquivalentTo(Array.Empty<string>(),
            "words that more than one class answers are declared once, in Model/Verbs.cs, and used from there");
    }

    /// <summary>
    ///     A verb the engine answers is answered in every game, which makes it part of the shared thesaurus.
    ///     It is declared in <see cref="Verbs" /> and nowhere else: a processor or the parser that kept its
    ///     own list would be a second place to look, and a second place for the lists to drift apart.
    /// </summary>
    [Test]
    public void TheEngineDeclaresNoFamiliesOfItsOwn()
    {
        var declaredInTheEngine = DeclaredFamilies()
            .Select(family => family.Field)
            .Where(field => field.StartsWith("GameEngine.", StringComparison.Ordinal))
            .Order();

        declaredInTheEngine.Should().BeEquivalentTo(Array.Empty<string>(),
            "engine-level verb families are declared in Model/Verbs.cs and used from there");
    }

    /// <summary>
    ///     A collection of verbs - two words or more - is a piece of the thesaurus and is declared in
    ///     <see cref="Verbs" />, whoever answers it. The only family a game class declares for itself is a
    ///     single verb that nothing else uses.
    /// </summary>
    [Test]
    public void ACollectionOfVerbsIsDeclaredInVerbsCs()
    {
        var collectionsDeclaredElsewhere = DeclaredFamilies()
            .Where(family => !family.Field.StartsWith("Model.Verbs", StringComparison.Ordinal))
            .Where(family => family.Words.Contains('/'))
            .Select(family => $"{family.Field}: {family.Words}")
            .Order();

        collectionsDeclaredElsewhere.Should().BeEquivalentTo(Array.Empty<string>(),
            "a family of more than one verb is declared in Model/Verbs.cs and used from there");
    }

    /// <summary>
    ///     Every static <see cref="VerbFamily" /> field, once each, with its words in a fixed order so that
    ///     the same set written in a different order still compares equal.
    /// </summary>
    private static IEnumerable<(string Field, string Words)> DeclaredFamilies()
    {
        var seen = new HashSet<string>();

        foreach (var type in AllTypes())
            // As the harvest does: a generic base class only has statics in its closed forms, and those are
            // only reachable as some concrete subclass's base type.
            for (var current = type; current is not null; current = current.BaseType)
            {
                if (current.ContainsGenericParameters)
                    continue;

                var declaredOn = current.IsGenericType ? current.GetGenericTypeDefinition() : current;

                foreach (var field in current.GetFields(Everything))
                {
                    if (!field.IsStatic || field.FieldType != typeof(VerbFamily))
                        continue;

                    var name = $"{declaredOn.FullName}.{field.Name}";
                    if (seen.Add(name))
                        yield return (name, string.Join('/',
                            ((VerbFamily)field.GetValue(null)!).Select(word => word.ToLowerInvariant()).Order()));
                }
            }
    }

    [Test]
    public void AGenericClassThatDeclaresAFamily_HasASubclassTheHarvestCanReachItThrough()
    {
        var types = AllTypes().ToList();

        var unreachable = types
            .Where(type => type.IsGenericTypeDefinition &&
                           type.GetFields(Everything).Any(field => field.FieldType == typeof(VerbFamily)))
            .Where(generic => !types.Any(type => !type.ContainsGenericParameters && DerivesFrom(type, generic)))
            .Select(type => type.FullName);

        unreachable.Should().BeEmpty(
            "an open generic type has no static fields to read, so a family declared on one is only " +
            "harvested through a concrete subclass");
    }

    private static bool DerivesFrom(Type type, Type genericDefinition)
    {
        for (var current = type.BaseType; current is not null; current = current.BaseType)
            if (current.IsGenericType && current.GetGenericTypeDefinition() == genericDefinition)
                return true;

        return false;
    }

    private static readonly Dictionary<short, OpCode> OpCodesByValue = typeof(OpCodes)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Select(field => (OpCode)field.GetValue(null)!)
        .ToDictionary(code => code.Value);

    /// <summary>
    ///     Walks the method's IL instruction by instruction looking for <c>newobj VerbFamily::.ctor</c>.
    ///     Walking properly (rather than scanning for the opcode byte) means an operand that happens to
    ///     contain the same byte can never be mistaken for an instruction.
    /// </summary>
    private static bool CreatesAVerbFamily(MethodBase method)
    {
        var il = method.GetMethodBody()?.GetILAsByteArray();
        if (il is null)
            return false;

        var typeArguments = method.DeclaringType is { IsGenericType: true } declaring
            ? declaring.GetGenericArguments()
            : null;
        var methodArguments = method is MethodInfo { IsGenericMethod: true } ? method.GetGenericArguments() : null;

        var position = 0;
        while (position < il.Length)
        {
            var code = il[position] == 0xFE
                ? OpCodesByValue[unchecked((short)(0xFE00 | il[position + 1]))]
                : OpCodesByValue[il[position]];
            position += code.Size;

            if (code == OpCodes.Newobj)
            {
                var token = BitConverter.ToInt32(il, position);
                var target = method.Module.ResolveMethod(token, typeArguments, methodArguments);
                if (target?.DeclaringType == typeof(VerbFamily))
                    return true;
            }

            position += code.OperandType switch
            {
                OperandType.InlineNone => 0,
                OperandType.ShortInlineBrTarget or OperandType.ShortInlineI or OperandType.ShortInlineVar => 1,
                OperandType.InlineVar => 2,
                OperandType.InlineI8 or OperandType.InlineR => 8,
                OperandType.InlineSwitch => 4 + 4 * BitConverter.ToInt32(il, position),
                _ => 4
            };
        }

        return false;
    }
}

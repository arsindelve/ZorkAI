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
    public void EveryGame_GetsTheSharedThesaurus_AndTheEngineProcessorsOwnFamilies(string game)
    {
        var families = Repository.GetVerbs(game);

        families.Should().Contain(Verbs.TakeVerbs).And.Contain(Verbs.ExamineVerbs);
        // Declared by SmellInteractionProcessor and ClothingOnAndOffProcessor, not by Verbs.cs.
        Words(game).Should().Contain(["sniff", "doff", "slip off"]);
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

    [Test]
    public void EveryDeclaredFamily_IsInTheHarvestOfTheGameThatDeclaresIt()
    {
        var shared = new[] { "Model", "GameEngine", "StellarPatrol" };

        foreach (var game in new[] { "ZorkOne", "Planetfall", "Stationfall", "EscapeRoom" })
        {
            var harvested = Repository.GetVerbs(game)
                .Select(family => string.Join('|', family))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var declared = shared.Append(game)
                .Select(Assembly.Load)
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => !type.ContainsGenericParameters)
                .SelectMany(type => type.GetFields(Everything))
                .Where(field => field.IsStatic && field.FieldType == typeof(VerbFamily))
                .Select(field => (Field: $"{field.DeclaringType!.FullName}.{field.Name}",
                    Words: string.Join('|', (VerbFamily)field.GetValue(null)!)));

            declared.Where(family => !harvested.Contains(family.Words)).Select(family => family.Field)
                .Should().BeEmpty($"every family {game} declares must reach its vocabulary");
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

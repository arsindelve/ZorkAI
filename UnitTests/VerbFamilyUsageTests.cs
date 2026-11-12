using System.Reflection;
using GameEngine;
using Model;

namespace UnitTests;

/// <summary>
///     <see cref="VerbFamilyUsage" /> is how the parser knows which object answers which verb grouping,
///     so what it has to get right is whose code a read belongs to - including the places the compiler
///     moves code out of the class the programmer wrote it in.
/// </summary>
[TestFixture]
public class VerbFamilyUsageTests
{
    private static readonly IReadOnlyList<(FieldInfo Family, Type UsedBy)> Uses =
        VerbFamilyUsage.In(typeof(VerbFamilyUsageTests).Assembly);

    private static IEnumerable<VerbFamily> ReadBy(Type user)
    {
        return Uses.Where(use => use.UsedBy == user).Select(use => (VerbFamily)use.Family.GetValue(null)!);
    }

    [Test]
    public void AReadInAnOrdinaryMethod_BelongsToItsClass()
    {
        ReadBy(typeof(PlainHandler)).Should().BeEquivalentTo([Verbs.TakeVerbs]);
    }

    [Test]
    public void AReadInsideALambda_BelongsToTheClassTheLambdaWasWrittenIn()
    {
        ReadBy(typeof(LambdaHandler)).Should().BeEquivalentTo([Verbs.DropVerbs]);
    }

    [Test]
    public void AReadInsideAnAsyncMethod_BelongsToItsClass()
    {
        ReadBy(typeof(AsyncHandler)).Should().BeEquivalentTo([Verbs.GiveVerbs]);
    }

    [Test]
    public void AReadInsideAnIterator_BelongsToItsClass()
    {
        ReadBy(typeof(IteratorHandler)).Should().BeEquivalentTo([Verbs.ShowVerbs]);
    }

    [Test]
    public void AReadInAGenericClass_BelongsToTheGenericDefinition_WhichEverySubclassDerivesFrom()
    {
        ReadBy(typeof(GenericHandler<>)).Should().BeEquivalentTo([Verbs.ThrowVerbs]);

        VerbFamilyUsage.IsOrDerivesFrom(typeof(ConcreteHandler), typeof(GenericHandler<>)).Should().BeTrue();
        VerbFamilyUsage.IsOrDerivesFrom(typeof(PlainHandler), typeof(GenericHandler<>)).Should().BeFalse();
    }

    [Test]
    public void AClassThatMatchesOnNothing_UsesNothing()
    {
        ReadBy(typeof(Bystander)).Should().BeEmpty();
    }

    [Test]
    public void AFamilyAClassDeclaresForItself_IsOneItReads()
    {
        ReadBy(typeof(SelfDeclaring)).Should().ContainSingle().Which.Should().Contain("salute");
    }

    // ---------------------------------------------------------------- the code under inspection

    private class PlainHandler
    {
        public bool Answers(string verb) => Verbs.TakeVerbs.Contains(verb);
    }

    private class LambdaHandler
    {
        public Func<string, bool> Answers(string prefix)
        {
            // Capturing a local forces the compiler to move this lambda into a closure class.
            return verb => Verbs.DropVerbs.Contains(prefix + verb);
        }
    }

    private class AsyncHandler
    {
        public async Task<bool> Answers(string verb)
        {
            await Task.Yield();
            return Verbs.GiveVerbs.Contains(verb);
        }
    }

    private class IteratorHandler
    {
        public IEnumerable<string> Words()
        {
            foreach (var word in Verbs.ShowVerbs)
                yield return word;
        }
    }

    private class GenericHandler<T>
    {
        public bool Answers(string verb, T _) => Verbs.ThrowVerbs.Contains(verb);
    }

    private class ConcreteHandler : GenericHandler<int>;

    private class Bystander
    {
        public string Name => "nothing to do with verbs";
    }

    private class SelfDeclaring
    {
        private static readonly VerbFamily SaluteVerb = new("salute");

        public bool Answers(string verb) => SaluteVerb.Contains(verb);
    }
}

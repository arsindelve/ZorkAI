using GameEngine;
using Model.Interface;
using UnitTests;
using ZorkOne.GlobalCommand;

namespace ZorkOne.Tests.Walkthrough;

/// <summary>
///     Every walkthrough in this project, replayed with the production <see cref="DeterministicParser" />
///     layered ahead of the test parser (see <see cref="DeterministicFirstTestParser" />). The base
///     fixtures prove the game's handlers; these prove that when the grammar answers a command, the engine
///     does exactly what it does for the parse the walkthrough was written against - across every
///     route, including the deaths and dead ends, where the unusual commands are.
/// </summary>
internal static class DeterministicFirst
{
    public static IIntentParser Parser() => new DeterministicFirstTestParser(new ZorkOneGlobalCommandFactory(), "ZorkOne");
}

[TestFixture]
public sealed class WalkthroughByLocationNameDeterministicFirst : WalkthroughByLocationName
{
    protected override IIntentParser? BuildParser() => DeterministicFirst.Parser();
}

[TestFixture]
public sealed class WalkthroughTestOneDeterministicFirst : WalkthroughTestOne
{
    protected override IIntentParser? BuildParser() => DeterministicFirst.Parser();
}

[TestFixture]
public sealed class WalkthroughTestThreeDeterministicFirst : WalkthroughTestThree
{
    protected override IIntentParser? BuildParser() => DeterministicFirst.Parser();
}

[TestFixture]
public sealed class WalkthroughTestTwoDeterministicFirst : WalkthroughTestTwo
{
    protected override IIntentParser? BuildParser() => DeterministicFirst.Parser();
}

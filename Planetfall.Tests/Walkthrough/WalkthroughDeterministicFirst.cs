using GameEngine;
using Model.Interface;
using Planetfall.GlobalCommand;
using UnitTests;

namespace Planetfall.Tests.Walkthrough;

/// <summary>
///     Re-runs the winning <see cref="WalkthroughTestOne" /> command sequence with the production
///     <see cref="DeterministicParser" /> layered ahead of the TestParser (see
///     <see cref="DeterministicFirstTestParser" />). Planetfall is where most room-specific handlers live,
///     so this is the end-to-end proof that when the deterministic parser answers, it produces an intent
///     the engine handles exactly as before - across a full real game, not isolated parses.
/// </summary>
[TestFixture]
public sealed class WalkthroughTestOneDeterministicFirst : WalkthroughTestOne
{
    protected override IIntentParser? BuildParser() =>
        new DeterministicFirstTestParser(new PlanetfallGlobalCommandFactory(), "Planetfall");
}

/// <summary>
///     The same game played by room name ("go to mech corridor north") through the deterministic-first
///     parser. This is the route that exercises the parser's "go to &lt;room&gt;" gate against real
///     navigation: every destination must be recognised as a room and resolved by the engine.
/// </summary>
[TestFixture]
public sealed class WalkthroughByLocationNameDeterministicFirst : WalkthroughByLocationName
{
    protected override IIntentParser? BuildParser() =>
        new DeterministicFirstTestParser(new PlanetfallGlobalCommandFactory(), "Planetfall");
}

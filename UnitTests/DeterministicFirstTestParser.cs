using GameEngine;
using Model.Intent;
using Model.Interface;

namespace UnitTests;

/// <summary>
/// A test parser that layers the PRODUCTION <see cref="DeterministicParser" /> (Layer 1) ahead of the
/// deterministic <see cref="TestParser" />, mirroring how production runs the deterministic parser ahead of
/// a fallback — but with the TestParser standing in for the live AI so the whole thing stays deterministic.
///
/// Its purpose is coverage: driving full game walkthroughs through this parser exercises the real
/// DeterministicParser on real command sequences (not just isolated unit tests). A walkthrough that passes
/// with this parser proves the DeterministicParser's outputs are correct in situ — when it resolves a
/// command it produces an intent the engine handles identically to the fallback, and everything else falls
/// through cleanly.
/// </summary>
public class DeterministicFirstTestParser : TestParser
{
    private readonly DeterministicParser _deterministic;

    public DeterministicFirstTestParser(IGlobalCommandFactory gameSpecificCommandFactory, string gameName = "ZorkOne")
        : base(gameSpecificCommandFactory, gameName)
    {
        _deterministic = new DeterministicParser(gameName);
    }

    public override Task<IntentBase> DetermineComplexIntentType(string? input, string locationDescription,
        string sessionId)
    {
        if (!string.IsNullOrWhiteSpace(input) && _deterministic.Parse(input) is { } deterministic)
            return Task.FromResult(deterministic);

        return base.DetermineComplexIntentType(input, locationDescription, sessionId);
    }
}

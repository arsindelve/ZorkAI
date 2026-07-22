using GameEngine;
using Model.Intent;
using Model.Interface;

namespace UnitTests;

/// <summary>
/// Diagnostic parser: the production <see cref="DeterministicParser" /> with NO fallback — a miss becomes
/// <see cref="NullIntent" /> instead of deferring to the TestParser's grammar. Running the game/walkthrough
/// tests through this SURFACES every standard-grammar command the deterministic parser does not yet handle
/// (each such gap is a command production would otherwise have to send to the AI, or a hidden bug). Pronoun
/// resolution is left to the base TestParser so this isolates intent-parsing gaps only.
/// </summary>
public class DeterministicOnlyTestParser : TestParser
{
    private readonly DeterministicParser _deterministic;

    public DeterministicOnlyTestParser(IGlobalCommandFactory gameSpecificCommandFactory, string gameName = "ZorkOne")
        : base(gameSpecificCommandFactory, gameName)
    {
        _deterministic = new DeterministicParser(gameName);
    }

    public override Task<IntentBase> DetermineComplexIntentType(string? input, string locationDescription,
        string sessionId)
    {
        return Task.FromResult(_deterministic.Parse(input) ?? new NullIntent());
    }
}

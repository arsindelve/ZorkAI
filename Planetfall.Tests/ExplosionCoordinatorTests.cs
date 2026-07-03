using FluentAssertions;
using Planetfall.Location.Feinstein;

namespace Planetfall.Tests;

/// <summary>
/// Issue #354 follow-up: ExplosionCoordinator gates each narration beat of the Feinstein explosion
/// sequence on an exact Context.Moves value via a switch statement, and only removes itself as an
/// actor in the death cases. Free commands (look/score/inventory/time) run actors without advancing
/// Moves, so without a guard, the same narration beat would re-fire on every consecutive free command
/// issued while Moves is frozen at the triggering value.
/// </summary>
public class ExplosionCoordinatorTests : EngineTestsBase
{
    [Test]
    public async Task Narration_DoesNotRepeat_OnConsecutiveFreeCommands()
    {
        var engine = GetTarget();
        StartHere<DeckEight>();

        var explosionCoordinator = new ExplosionCoordinator();
        engine.Context.RegisterActor(explosionCoordinator);

        // One turn away from the explosion beat - "wait" below advances Moves to the trigger value.
        engine.Context.Moves = ExplosionCoordinator.TurnWhenFeinsteinBlowsUp - 1;

        var explosionResponse = await engine.GetResponse("wait");
        explosionResponse.Should().Contain("A massive explosion rocks the ship");

        // Free command: Moves stays frozen at TurnWhenFeinsteinBlowsUp, but actors still run.
        var lookResponse = await engine.GetResponse("look");
        lookResponse.Should().NotContain("A massive explosion rocks the ship");

        // Same again - the guard must hold across repeated free commands, not just once.
        var scoreResponse = await engine.GetResponse("score");
        scoreResponse.Should().NotContain("A massive explosion rocks the ship");
    }
}

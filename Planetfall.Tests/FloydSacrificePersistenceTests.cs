using FluentAssertions;
using GameEngine;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Item.Lawanda.Lab;
using Planetfall.Location.Lawanda;
using Planetfall.Location.Lawanda.Lab;
using static Planetfall.Location.Lawanda.Lab.BioLockStateMachineManager;

namespace Planetfall.Tests;

/// <summary>
///     Issue #493 (same player-visible failure as closed #365 / AB-052). Floyd's Bio-Lab sacrifice — the
///     only way to obtain the miniaturization access card that gates Station 384 — cannot be performed in
///     the deployed (stateless) game. The whole sequence's state lives in <c>BioLockEast.StateMachine</c>,
///     which was a get-only property. The session serializer deliberately drops read-only properties
///     (<c>DoNotSerializeReadOnlyPropertiesResolver</c>), so the state machine was never written to the
///     base64 session and re-initialised to a fresh <c>new()</c> on every turn: Floyd never offered the
///     plan and opening the inner door was always an instant "wrong time" death.
///
///     This is the same class of bug fixed in #472 (disambiguation state that lived only in memory). Like
///     those tests, every test here serializes + deserializes the game between turns, mirroring the Lambda
///     save/restore round-trip. A single-process test passes with or without the fix — the object simply
///     survives in memory between turns in-process — so only a round-tripping test can prove it.
/// </summary>
public class FloydSacrificePersistenceTests : EngineTestsBase
{
    private GameEngine<PlanetfallGame, PlanetfallContext> RoundTrip(
        GameEngine<PlanetfallGame, PlanetfallContext> engine)
    {
        var saved = engine.SaveGame();
        var restored = GetTarget();
        restored.RestoreGame(saved);
        return restored;
    }

    [Test]
    public void StateMachine_Fields_SurviveSaveRestore()
    {
        var engine = GetTarget();
        var bioLockEast = StartHere<BioLockEast>();
        bioLockEast.StateMachine.HasWaitedOneTurnInBioLockEast = true;
        bioLockEast.StateMachine.FloydHasSaidNeedToGetCard = true;
        bioLockEast.StateMachine.FloydWaitingForDoorOpenCount = 3;
        bioLockEast.StateMachine.LabSequenceState = FloydLabSequenceState.DoorClosedFloydFighting;
        bioLockEast.StateMachine.FloydFightingTurnCount = 1;

        RoundTrip(engine);

        // Before the fix the get-only StateMachine property was dropped by the serializer, so every one
        // of these reset to its default on restore.
        var machine = Repository.GetLocation<BioLockEast>().StateMachine;
        machine.HasWaitedOneTurnInBioLockEast.Should().BeTrue();
        machine.FloydHasSaidNeedToGetCard.Should().BeTrue();
        machine.FloydWaitingForDoorOpenCount.Should().Be(3);
        machine.LabSequenceState.Should().Be(FloydLabSequenceState.DoorClosedFloydFighting);
        machine.FloydFightingTurnCount.Should().Be(1);
    }

    [Test]
    public async Task NeedToGetCard_FiresAfterConcern_AcrossSaveRestore()
    {
        var engine = GetTarget();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        var bioLockEast = StartHere<BioLockEast>();
        bioLockEast.ItemPlacedHere(floyd);
        bioLockEast.StateMachine.HasWaitedOneTurnInBioLockEast = true; // Skip the one-turn delay
        engine.Context.RegisterActor(bioLockEast);
        GetLocation<ComputerRoom>().FloydHasExpressedConcern = true;

        // The player saved after Floyd expressed concern and reached the window; restore, then wait.
        var restored = RoundTrip(engine);
        var response = await restored.GetResponse("wait");

        // Before the fix, HasWaitedOneTurnInBioLockEast reset to false every turn, so the state machine
        // returned on the "first-turn is silent" branch forever and the plan dialogue never appeared.
        response.Should().Contain(FloydConstants.NeedToGetCard);
    }

    [Test]
    public async Task OpenDoor_WhenFloydReady_StartsSequence_AcrossSaveRestore()
    {
        var engine = GetTarget();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        var bioLockEast = StartHere<BioLockEast>();
        bioLockEast.ItemPlacedHere(floyd);
        bioLockEast.StateMachine.FloydHasSaidNeedToGetCard = true;
        engine.Context.RegisterActor(bioLockEast);

        var restored = RoundTrip(engine);
        var response = await restored.GetResponse("open door");

        // Before the fix FloydHasSaidNeedToGetCard reset to false, so isFloydReady was always false and
        // opening the inner door hit the fallback "wrong time" branch — instant death — instead of
        // starting the sacrifice.
        response.Should().Contain(FloydConstants.InTheLabOne);
        response.Should().NotContain("devour you");
        Repository.GetLocation<BioLockEast>().StateMachine.LabSequenceState
            .Should().Be(FloydLabSequenceState.FloydRushedInNeedToCloseDoor);
    }

    [Test]
    public async Task FullSacrificeChoreography_CompletesAcrossSaveRestore()
    {
        var engine = GetTarget();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        var bioLockEast = StartHere<BioLockEast>();
        bioLockEast.ItemPlacedHere(floyd);
        bioLockEast.StateMachine.HasWaitedOneTurnInBioLockEast = true; // Skip the one-turn delay
        bioLockEast.StateMachine.FloydHasSaidNeedToGetCard = true;
        engine.Context.RegisterActor(bioLockEast);

        // open door -> Floyd rushes in
        engine = RoundTrip(engine);
        var open = await engine.GetResponse("open door");
        open.Should().Contain(FloydConstants.InTheLabOne);

        // close door -> Floyd fighting behind the door
        engine = RoundTrip(engine);
        var close = await engine.GetResponse("close door");
        close.Should().Contain("And not a moment too soon!");

        // wait -> Floyd knocks (InTheLabThree), state advances to NeedToReopenDoor
        engine = RoundTrip(engine);
        await engine.GetResponse("wait");

        // open door -> Floyd stumbles back with the card
        engine = RoundTrip(engine);
        var reopen = await engine.GetResponse("open door");
        reopen.Should().Contain(FloydConstants.FloydReturnsWithCard);

        // close door -> sacrifice completes: body + card placed, +2 awarded
        engine = RoundTrip(engine);
        var final = await engine.GetResponse("close door");

        final.Should().Contain("Floyd staggers to the ground");

        var restoredBioLockEast = Repository.GetLocation<BioLockEast>();
        restoredBioLockEast.StateMachine.LabSequenceState.Should().Be(FloydLabSequenceState.Completed);

        var miniCard = Repository.GetItem<MiniaturizationAccessCard>();
        miniCard.CurrentLocation.Should().Be(restoredBioLockEast);
        Repository.GetItem<Floyd>().HasDied.Should().BeTrue();
        Repository.GetItem<Floyd>().CurrentLocation.Should().Be(restoredBioLockEast);
        engine.Context.Score.Should().Be(2);
    }

    [Test]
    public async Task AfterSacrifice_DeadFloydStaysSilentOnReentry_AcrossSaveRestore()
    {
        // Issue #493 review follow-up: once the sacrifice is Completed, leaving and walking back into Bio
        // Lock East must not make Floyd's corpse ask you to open the door again. The Completed state (and
        // the guard that silences the actor) has to survive the stateless save/restore boundary, so this
        // exercises the exact deployed path: round-trip the game, leave, round-trip, return.
        var engine = GetTarget();
        var floyd = GetItem<Floyd>();
        floyd.IsOn = true;
        var bioLockEast = StartHere<BioLockEast>();
        bioLockEast.ItemPlacedHere(floyd);
        bioLockEast.StateMachine.HasWaitedOneTurnInBioLockEast = true;
        bioLockEast.StateMachine.FloydHasSaidNeedToGetCard = true;
        bioLockEast.StateMachine.LabSequenceState = FloydLabSequenceState.DoorReopenedNeedToCloseAgain;
        GetItem<BioLockInnerDoor>().IsOpen = true;
        engine.Context.RegisterActor(bioLockEast);

        var complete = await engine.GetResponse("close door"); // completes the sacrifice
        complete.Should().Contain("Floyd staggers to the ground");

        engine = RoundTrip(engine);
        await engine.GetResponse("west"); // leave
        engine = RoundTrip(engine);
        var back = await engine.GetResponse("east"); // and come back
        var wait = await engine.GetResponse("wait");

        back.Should().NotContain(FloydConstants.OpenTheDoor);
        wait.Should().NotContain(FloydConstants.OpenTheDoor);
        wait.Should().NotContain(FloydConstants.Sulk);
        Repository.GetLocation<BioLockEast>().StateMachine.LabSequenceState
            .Should().Be(FloydLabSequenceState.Completed);
    }
}

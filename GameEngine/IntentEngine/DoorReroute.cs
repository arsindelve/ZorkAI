using Model.AIGeneration;
using Model.Intent;
using Model.Interface;
using Model.Item;

namespace GameEngine.IntentEngine;

/// <summary>
/// Shared "enter/exit/go-through &lt;door&gt;" routing for <see cref="EnterSubLocationEngine"/> and
/// <see cref="ExitSubLocationEngine"/> (issue #262).
///
/// A door is whatever a room's map <i>declares</i> as the <see cref="Model.Movement.MovementParameters.GatingItem"/>
/// of one of its exits — a window, a bulkhead, a trap door, a grating. We do NOT guess doorness from
/// the item's type; we ask the current location which direction the resolved noun gates
/// (<see cref="Model.Location.ILocation.DirectionGatedBy"/>) and walk that way, letting the map's own
/// open-check / CustomFailureMessage apply. Because the map names the door, a carried sack or a
/// wall-mounted trophy case is simply not anyone's gating item and falls through to the caller's
/// refusal — no portability/container exclusions needed.
///
/// From a given room a door gates exactly one passage, so "enter", "exit" and "go through" all mean
/// the same thing here: traverse it. There is deliberately no In/Out direction — the map says which
/// way the door leads.
/// </summary>
internal static class DoorReroute
{
    /// <summary>
    /// If <paramref name="resolvedNoun"/> is the item gating one of the current location's exits,
    /// walk through it via <see cref="MoveEngine"/> and return the result. Otherwise returns null,
    /// leaving the caller to give its own "you can't go through that" message.
    /// </summary>
    public static async Task<(InteractionResult? resultObject, string ResultMessage)?> TryTraverse(
        IItem? resolvedNoun, IContext context, IGenerationClient generationClient)
    {
        if (resolvedNoun is null)
            return null;

        var direction = context.CurrentLocation.DirectionGatedBy(resolvedNoun, context);
        if (direction is null)
            return null;

        return await new MoveEngine().Process(
            new MoveIntent { Direction = direction.Value }, context, generationClient);
    }
}

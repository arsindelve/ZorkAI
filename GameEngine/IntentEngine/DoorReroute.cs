using Model.AIGeneration;
using Model.Intent;
using Model.Interface;
using Model.Item;
using Model.Movement;

namespace GameEngine.IntentEngine;

/// <summary>
/// Shared "enter/exit &lt;door&gt;" routing for <see cref="EnterSubLocationEngine"/> and
/// <see cref="ExitSubLocationEngine"/> (issue #262). Kept in one place so the two engines can't
/// drift apart on the (subtle) definition of a door.
///
/// A <b>passage door</b> is a fixed openable that gates a room exit: it is
/// <see cref="IOpenAndClose"/>, NOT something you carry (<see cref="ICanBeTakenAndDropped"/>), and
/// NOT a container (<see cref="ICanContainItems"/>). The container exclusion matters because
/// <see cref="Repository.GetItemInScope"/> also searches inventory and the room, so without it an
/// openable that is merely <i>present</i> — a carried sack, or the Living Room's wall-mounted trophy
/// case — would resolve here and hijack the room's exit (e.g. "enter case" teleporting you down the
/// trap door). "enter/exit &lt;door&gt;" means "go through it", so we defer to <see cref="MoveEngine"/>
/// in the given direction and let the location map's own open-check / CustomFailureMessage apply,
/// instead of a generic narrator-mock refusal.
///
/// NOTE: this confirms the noun is <i>a</i> passage door and that the room has <i>that</i> exit, not
/// that <i>this</i> door gates <i>that</i> exit. It is safe as long as no room we expose an In/Out
/// door-alias in has two distinct passage doors sharing a noun. (BioLabLocation has two "door"s, but
/// it is deliberately NOT aliased.) Tying a door to its gating direction would remove that caveat.
/// </summary>
internal static class DoorReroute
{
    /// <summary>
    /// If <paramref name="resolvedNoun"/> is a passage door and the current location has an exit in
    /// <paramref name="direction"/>, walk through it via <see cref="MoveEngine"/> and return the
    /// result. Otherwise returns null, leaving the caller to give its own "you can't go through that"
    /// message.
    /// </summary>
    public static async Task<(InteractionResult? resultObject, string ResultMessage)?> TryProcess(
        IItem? resolvedNoun, Direction direction, IContext context, IGenerationClient generationClient)
    {
        if (resolvedNoun is not IOpenAndClose
            || resolvedNoun is ICanBeTakenAndDropped
            || resolvedNoun is ICanContainItems)
            return null;

        if (context.CurrentLocation.Navigate(direction, context) is null)
            return null;

        return await new MoveEngine().Process(
            new MoveIntent { Direction = direction }, context, generationClient);
    }
}

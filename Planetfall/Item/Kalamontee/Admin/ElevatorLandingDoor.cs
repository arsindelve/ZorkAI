using Newtonsoft.Json;

namespace Planetfall.Item.Kalamontee.Admin;

/// <summary>
///     One shaft's door <i>as it stands in one particular room</i> — the panel in the Elevator Lobby,
///     or the one at the far end of the shaft. A landing door is a real, distinct object that lives in
///     exactly one room, but it owns no state of its own: what it reports is the shaft's.
///     <para>
///         Before this existed, each shaft had a single door object seeded into two rooms, and the two
///         <c>Init()</c>s fought over its <see cref="ItemBase.CurrentLocation" /> — the loser could not
///         resolve the door as an object at all, so <c>enter blue door</c> in the lobby and
///         <c>examine door</c> at the Tower Core found nothing to act on (issue #532). One object cannot
///         honestly be in two places; giving each room its own object is what makes
///         <c>CurrentLocation</c> true again, and with it every scope check built on it.
///     </para>
///     <para>
///         Splitting identity is only safe because state is <i>not</i> split with it:
///         <see cref="IsOpen" /> reads <see cref="IsOpenHere" /> — this room's vantage point on the one
///         shaft-wide flag — and writes through to <see cref="ShaftDoor" />. That is what retires the
///         workaround #505 needed: the lobby used to intercept examine/open/close because the door
///         object it had reported the raw flag, which is only true from inside the car. A landing door
///         reports the effective state directly, so the ordinary processors are right again and no room
///         has to answer for its door by hand.
///     </para>
/// </summary>
public abstract class ElevatorLandingDoor : ElevatorDoorBase
{
    /// <summary>
    ///     The object holding the shaft's one open/closed flag — the door inside the car, which is where
    ///     the flag lived before the split, so saved games keep pointing at it.
    /// </summary>
    protected abstract ElevatorDoorBase ShaftDoor { get; }

    /// <summary>
    ///     Whether the shaft counts as open <i>from this room</i>: the flag alone cannot say, because it
    ///     is shared by both ends and the arrival path leaves it open at whichever end the car parked at.
    ///     Implementations delegate to the car's own <c>IsOpenAtTheLobby</c> / <c>IsOpenAtTheFarEnd</c>.
    /// </summary>
    protected abstract bool IsOpenHere { get; }

    // No backing field, and nothing to serialize: the flag belongs to ShaftDoor, which is serialized in
    // its own right. These MUST be Newtonsoft's JsonIgnore, not System.Text.Json's - saves go through
    // JsonConvert (GameEngine.SaveGame), which ignores the STJ attribute entirely and would happily
    // write a landing door's derived IsOpen into the blob and then push it back through the setter on
    // restore, overwriting the shaft flag with this room's view of it.
    //
    // Note that get and set are deliberately not inverses: a landing door shows whether the shaft is
    // open *at this end*, but opening one opens the shaft. Nothing in the game exercises the setter -
    // ElevatorDoorBase refuses open and close unconditionally - and it exists only to satisfy
    // IOpenAndClose.
    [JsonIgnore]
    public override bool IsOpen
    {
        get => IsOpenHere;
        set => ShaftDoor.IsOpen = value;
    }

    [JsonIgnore]
    public override bool HasEverBeenOpened
    {
        get => ShaftDoor.HasEverBeenOpened;
        set => ShaftDoor.HasEverBeenOpened = value;
    }
}

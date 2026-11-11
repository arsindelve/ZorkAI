namespace Planetfall.Item;

/// <summary>
///     A door <i>as seen from one particular room</i>.
///     <para>
///         Every elevator-door bug this game has had — #450, #454, #456, #505, #512, #518, #523, #532 —
///         is the same defect. A door's state is answered by several surfaces of a room: the exit in its
///         <c>Map</c>, the sentence in its description, <c>examine door</c>, and <c>open/close door</c>.
///         Each surface used to re-derive that state independently, one of them would be written as a
///         plain literal or read a flag that meant something else, and the room would then contradict
///         itself — describing a door as open while walking that way answered "The door is closed."
///     </para>
///     <para>
///         So a room states its door exactly once, as a Doorway, and asks this object for each surface:
///         <see cref="StateWord" /> for the description and <see cref="Passage" /> for the map. They
///         cannot disagree, because there is only one fact. A new room gets the invariant for free;
///         drifting from it now takes deliberate effort rather than a copied string.
///     </para>
///     <para>
///         The verbs are deliberately <i>not</i> here. A Doorway used to be able to answer
///         examine/open/close itself, for a room whose vantage point differed from the door's raw flag —
///         which was really a symptom of one door object standing in two rooms at once, the bug behind
///         #532. Once each room has its own door, that door's own state is the room's answer and the
///         ordinary processors are correct; a room that intercepts its own door's verbs is stating it
///         twice again.
///     </para>
/// </summary>
public sealed class Doorway
{
    /// <summary>
    ///     What every one of these passages says when the door is shut. Shared so the map and any custom
    ///     refusal stay in step.
    /// </summary>
    public const string ClosedMessage = "The door is closed. ";

    /// <param name="door">
    ///     The door, which owns all of the wording <i>and</i> the state. A Doorway deliberately takes no
    ///     "but from here it's really closed" override: the elevator shafts used to need one because a
    ///     single door object stood in two rooms at once, and the room that lost the coin toss then also
    ///     lost the object from scope entirely (issue #532). The fix was to give each room its own door
    ///     whose <see cref="IOpenAndClose.IsOpen" /> already means "open from here" — so if a room seems
    ///     to need an override, what it actually needs is its own door.
    /// </param>
    public Doorway(IDoor door)
    {
        Door = door;
    }

    public IDoor Door { get; }

    /// <summary>
    ///     The single fact every surface below is derived from.
    /// </summary>
    public bool IsOpen => Door.IsOpen;

    /// <summary>
    ///     The word a room description must use for this door: "open" or "closed".
    /// </summary>
    public string StateWord => IsOpen ? "open" : "closed";

    /// <summary>
    ///     The passage through this door, gated on the same predicate the description reports. Declaring
    ///     the door as the <see cref="MovementParameters.GatingItem" /> also lets "enter/exit door"
    ///     resolve to this exit (DoorReroute, issue #262).
    /// </summary>
    public MovementParameters Passage(ILocation? destination, string closedMessage = ClosedMessage)
    {
        return new MovementParameters
        {
            GatingItem = Door,
            CanGo = _ => IsOpen,
            Location = destination,
            CustomFailureMessage = closedMessage
        };
    }
}

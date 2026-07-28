namespace Planetfall.Item;

/// <summary>
///     A door <i>as seen from one particular room</i>: the door object paired with the predicate that
///     says whether it counts as open from where the asking room stands.
///     <para>
///         Every elevator-door bug this game has had — #450, #454, #456, #505, #512, #518, #523 — is the
///         same defect. A door's state is answered by up to four surfaces of a room: the exit in its
///         <c>Map</c>, the sentence in its description, <c>examine door</c>, and <c>open/close door</c>.
///         Each surface used to re-derive that state independently, one of them would be written as a
///         plain literal or read the raw flag, and the room would then contradict itself — describing a
///         door as open while walking that way answered "The door is closed."
///     </para>
///     <para>
///         So a room states its door exactly once, as a Doorway, and asks this object for each surface:
///         <see cref="StateWord" /> for the description, <see cref="Passage" /> for the map, and
///         <see cref="Answer" /> for the verbs. They cannot disagree, because there is only one
///         predicate. A new room gets the invariant for free; drifting from it now takes deliberate
///         effort rather than a copied string.
///     </para>
/// </summary>
public sealed class Doorway
{
    /// <summary>
    ///     What every one of these passages says when the door is shut. Shared so the map and any custom
    ///     refusal stay in step.
    /// </summary>
    public const string ClosedMessage = "The door is closed. ";

    // ExamineInteractionProcessor answers a wider set than Verbs.ExamineVerbs lists - "check", "look in"
    // and "peek at" appear only in its own switch. Matching just the array would let "check blue door"
    // reach the door item and report the raw flag, reopening the contradiction for that one phrasing.
    private static readonly string[] ExamineDoorVerbs = [..Verbs.ExamineVerbs, "check", "look in", "peek at"];

    private readonly Func<bool> _isOpenHere;

    /// <param name="door">The door, which owns all of the wording.</param>
    /// <param name="isOpenHere">
    ///     Whether the door counts as open from the room holding this Doorway. Omit it when the door's own
    ///     flag is the whole truth, which is the usual case; supply it when the flag is shared between two
    ///     vantage points and therefore cannot answer "open <i>here</i>" on its own.
    /// </param>
    public Doorway(IDoor door, Func<bool>? isOpenHere = null)
    {
        Door = door;
        _isOpenHere = isOpenHere ?? (() => door.IsOpen);
    }

    public IDoor Door { get; }

    /// <summary>
    ///     The single fact every surface below is derived from.
    /// </summary>
    public bool IsOpen => _isOpenHere();

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

    /// <summary>
    ///     Answers "examine/open/close &lt;door&gt;" from <see cref="IsOpen" /> instead of the raw flag,
    ///     re-stating exactly what the examine and open/close processors would say. Returns null for any
    ///     other verb, any other noun, or any case where the door would really open or close — those must
    ///     reach the normal processors so the state change actually happens.
    ///     <para>
    ///         Only a room whose vantage point differs from the flag needs to call this; elsewhere the
    ///         processors already read the one true state. Nothing is lost by intercepting for such a room:
    ///         a door with a shared flag is one the player cannot work by hand anyway, so these paths never
    ///         mutate state for it.
    ///     </para>
    /// </summary>
    public InteractionResult? Answer(SimpleIntent action, IContext context)
    {
        if (!action.MatchNounAndAdjective(Door.NounsForMatching))
            return null;

        if (action.MatchVerb(ExamineDoorVerbs))
            return new PositiveInteractionResult(Door.DescribeAs(IsOpen));

        if (action.MatchVerb(Verbs.OpenVerbs))
        {
            if (IsOpen)
                return new PositiveInteractionResult(Door.AlreadyOpen);

            var refusal = Door.CannotBeOpenedDescription(context);
            return refusal is null ? null : new PositiveInteractionResult(refusal);
        }

        if (action.MatchVerb(Verbs.CloseVerbs))
        {
            if (!IsOpen)
                return new PositiveInteractionResult(Door.AlreadyClosed);

            var refusal = Door.CannotBeClosedDescription(context);
            return refusal is null ? null : new PositiveInteractionResult(refusal);
        }

        return null;
    }
}

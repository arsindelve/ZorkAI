using GameEngine.Item;
using Model.Interface;
using Model.Item;

namespace Stationfall.Item.Duffy;

/// <summary>
///     Shared behavior for the three Stellar Patrol forms the player starts the game carrying. In the
///     original these are physical "feelies" from the game package, so they begin in inventory rather
///     than lying in a room (ship.zil:36-61).
///     Each form is accepted by exactly one slot; see <c>FormSlot</c> for the routing.
/// </summary>
public abstract class FormBase : ItemBase, ICanBeTakenAndDropped, ICanBeExamined, ICanBeRead
{
    /// <summary>
    ///     The form's serial designation, e.g. "QX-17-T".
    /// </summary>
    public abstract string FormNumber { get; }

    /// <summary>
    ///     The form's full title, without the serial number.
    /// </summary>
    public abstract string FormTitle { get; }

    /// <summary>
    ///     Set once the form has been fed into (and swallowed by) the correct slot. A consumed form is
    ///     gone from play, so this keeps it from being re-used or re-listed.
    /// </summary>
    public bool Consumed { get; set; }

    public override int Size => 1;

    public override string[] NounsForMatching =>
        [FormNumber.ToLowerInvariant(), $"{FormTitle.ToLowerInvariant()} form", FormTitle.ToLowerInvariant(), "form"];

    public virtual string ExaminationDescription => ReadDescription;

    public virtual string ReadDescription =>
        $"{FormTitle} Form {FormNumber}: a triumph of Stellar Patrol bureaucracy, printed in " +
        "eleven colors on paper thick enough to stop a low-caliber slug. ";

    /// <summary>
    ///     "An Assignment Completion Form", but "A Robot Use Authorization Form".
    /// </summary>
    private string Article => "AEIOU".Contains(FormTitle[0]) ? "An" : "A";

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return $"{Article} {FormTitle} Form {FormNumber} lies here. ";
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return $"{Article} {FormTitle} Form {FormNumber}";
    }
}

using Newtonsoft.Json;

namespace Stationfall.Item.Duffy;

/// <summary>
///     The spacetruck's hatch <i>as it stands in one particular room</i> — seen from the cargo bay,
///     from inside the cab, or from the docking bay. It must be shut before launch (the original vents
///     the cabin and kills a player who leaves it open, ship.zil:1164-1166) and cannot be opened at all
///     in deep space (ship.zil:1042-1048).
///     <para>
///         There is one hatch in the fiction but three objects here, because one object cannot honestly
///         be in three places: a single instance seeded into three rooms has its
///         <see cref="ItemBase.CurrentLocation" /> overwritten by whichever <c>Init()</c> runs last, and
///         every scope check built on that then rejects it in the other two. Splitting identity is what
///         makes <c>CurrentLocation</c> true again. State is deliberately <i>not</i> split with it —
///         see <see cref="IsOpen" />.
///     </para>
///     <para>
///         This is what retires the raw-string interception the truck and the docking bay used to need.
///         They matched verb-plus-noun against the input by hand precisely because the hatch object was
///         out of scope there; with a real object in each room the ordinary processors work, and every
///         verb they support — not just the two that were hand-coded — works from all three rooms.
///     </para>
/// </summary>
public abstract class SpacetruckHatchBase : OpenAndCloseContainerBase, ICanBeExamined
{
    /// <summary>
    ///     The object holding the one open/closed flag: the cargo bay's hatch, which is where the flag
    ///     lived before the split, so saved games keep pointing at it.
    /// </summary>
    protected virtual SpacetruckHatchBase FlagHolder => Repository.GetItem<SpacetruckHatch>();

    public override string[] NounsForMatching => ["hatch", "spacetruck hatch", "truck hatch", "door"];

    public override bool IsTransparent => true;

    public string ExaminationDescription =>
        $"A broad loading hatch in the side of the spacetruck. It is {(IsOpen ? "open" : "closed")}. ";

    // No backing field and nothing to serialize here: the flag belongs to the holder, which is
    // serialized in its own right. This must be Newtonsoft's JsonIgnore, not System.Text.Json's - saves
    // go through JsonConvert (GameEngine.SaveGame), which ignores the STJ attribute entirely and would
    // write each room's copy into the blob, then push it back through the setter on restore.
    [JsonIgnore]
    public override bool IsOpen
    {
        get => FlagHolder.RawIsOpen;
        set => FlagHolder.RawIsOpen = value;
    }

    /// <summary>
    ///     The flag itself, reached only through <see cref="IsOpen" />. Overridden by the holder to be
    ///     a real stored property; every other hatch inherits this and never uses it.
    /// </summary>
    protected virtual bool RawIsOpen
    {
        get => false;
        set { }
    }

    public override string NowOpen(ILocation currentLocation)
    {
        return "The hatch swings open. ";
    }

    public override string NowClosed(ILocation currentLocation)
    {
        return "The hatch swings shut and seals with a pneumatic sigh. ";
    }

    /// <summary>
    ///     Once the truck is under way there is nothing outside but vacuum, so the hatch stays shut
    ///     until it docks (ship.zil:1042-1048).
    /// </summary>
    public override string? CannotBeOpenedDescription(IContext context)
    {
        return Repository.GetLocation<Spacetruck>().IsInFlight
            ? "You can't open the hatch in deep space! "
            : null;
    }

    public override void Init()
    {
    }
}

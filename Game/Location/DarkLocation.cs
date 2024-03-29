using Model.Location;

namespace Game.Location;

/// <summary>
///     Represents a dark location in the game, where interactions may be different
///     when the adventurer is not carrying an active light source.
/// </summary>
public abstract class DarkLocation : BaseLocation, IDarkLocation
{
    public virtual string DarkDescription => "It is pitch black. You are likely to be eaten by a grue. ";

    public virtual bool IsNoLongerDark { get; set; }
}
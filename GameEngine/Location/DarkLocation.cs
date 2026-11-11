using Model.Interface;
using Model.Location;

namespace GameEngine.Location;

/// <summary>
///     Represents a dark location in the game, where interactions may be different
///     when the adventurer is not carrying an active light source.
/// </summary>
public abstract class DarkLocation : LocationBase, IDarkLocation
{
    // The default dark-room text belongs to the game, not the engine: each game supplies its own
    // wording (e.g. Zork's "eaten by a grue") via IInfocomGame so no game-specific flavor is
    // hardcoded here. A specific location may still override this for bespoke dark-room wording.
    public virtual string GetDarkDescription(IContext context) => context.Game.DarkLocationDescription;

    public virtual bool IsNoLongerDark { get; set; }
}
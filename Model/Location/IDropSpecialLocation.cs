using Model.Interaction;
using Model.Interface;
using Model.Item;

namespace Model.Location;

/// <summary>
/// Interface for locations where dropping things requires special handling. Maybe it goes into hot lava,
/// or down into a rift never to be seen again. 
/// </summary>
public interface IDropSpecialLocation
{
    /// <summary>
    /// Handle the dropping of an item from this special location. 
    /// </summary>
    /// <param name="item"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    InteractionResult DropSpecial(IItem item, IContext context);
}
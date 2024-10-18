using Model.Interaction;
using Model.Interface;

namespace Model.Item;

/// <summary>
///     Marker interface for objects that can handed/given/offered things. Almost
///     always this is a person/monster/robot/etc. 
/// </summary>
public interface ICanBeGivenThings
{
    InteractionResult OfferThisThing(IItem item, IContext context);
}
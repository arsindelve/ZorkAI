using GameEngine;
using GameEngine.Item;
using Model.Interface;

namespace ZorkOne.Item;

// IAmPointyAndPunctureThings: the axe is one of the sharp items that punctures the
// inflatable boat on boarding (ZIL RBOAT-FUNCTION, zork1/1actions.zil:2787-2799).
public class BloodyAxe : ItemBase, ICanBeTakenAndDropped, IWeapon, IAmPointyAndPunctureThings
{
    public override string[] NounsForMatching => ["axe", "bloody axe"];

    public override string? CannotBeTakenDescription
    {
        get
        {
            var troll = Repository.GetItem<Troll>();

            if (troll.ItemBeingHeld == this && !troll.IsUnconscious)
                return "The troll swings it out of your reach. ";

            return null;
        }
    }

    public override string? OnBeingTaken(IContext context, ICanContainItems? previousLocation)
    {
        Repository.GetItem<Troll>().ItemBeingHeld = null;
        return base.OnBeingTaken(context, previousLocation);
    }

    string ICanBeTakenAndDropped.OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a bloody axe here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return ((ICanBeTakenAndDropped)this).OnTheGroundDescription(currentLocation);
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A bloody axe ";
    }
}
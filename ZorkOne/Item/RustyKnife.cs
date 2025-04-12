using GameEngine.Item;
using Model.Interface;

namespace ZorkOne.Item;

public class RustyKnife : ItemBase, ICanBeTakenAndDropped, IWeapon, IAmPointyAndPunctureThings
{
    public override string[] NounsForMatching => ["knife", "rusty knife", "rusty"];

    string ICanBeTakenAndDropped.OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a rusty knife here. ";
    }

    string ICanBeTakenAndDropped.NeverPickedUpDescription(ILocation currentLocation)
    {
        return "Beside the skeleton is a rusty knife. ";
    }

    public override string? OnBeingTaken(IContext context, ICanContainItems? previousLocation)
    {
        return context.HasItem<Sword>()
            ? "As you touch the rusty knife, your sword gives a single pulse of blinding blue light. "
            : base.OnBeingTaken(context, previousLocation);
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "a rusty knife";
    }
}
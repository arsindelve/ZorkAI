using GameEngine.Item;

namespace ZorkOne.Item;

public class ViscousMaterial : ItemBase, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["viscous material", "viscous", "material", "gunk"];

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a viscous material here. ";
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A viscous material";
    }
}
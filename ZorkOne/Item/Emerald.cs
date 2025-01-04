using GameEngine.Item;

namespace ZorkOne.Item;

public class Emerald : ItemBase, ICanBeTakenAndDropped, IGivePointsWhenPlacedInTrophyCase,
    IGivePointsWhenFirstPickedUp
{
    public override string[] NounsForMatching => ["emerald", "large emerald"];

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a large emerald here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return OnTheGroundDescription(currentLocation);
    }

    int IGivePointsWhenFirstPickedUp.NumberOfPoints => 5;

    int IGivePointsWhenPlacedInTrophyCase.NumberOfPoints => 10;

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A large emerald ";
    }
}
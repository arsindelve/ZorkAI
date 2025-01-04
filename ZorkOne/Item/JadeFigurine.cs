using GameEngine.Item;

namespace ZorkOne.Item;

public class JadeFigurine : ItemBase, ICanBeTakenAndDropped, IGivePointsWhenPlacedInTrophyCase,
    IGivePointsWhenFirstPickedUp
{
    public override string[] NounsForMatching => ["jade", "jade figurine", "figurine"];

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is an exquisite jade figurine here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return OnTheGroundDescription(currentLocation);
    }

    int IGivePointsWhenFirstPickedUp.NumberOfPoints => 5;

    int IGivePointsWhenPlacedInTrophyCase.NumberOfPoints => 5;

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A jade figurine";
    }
}
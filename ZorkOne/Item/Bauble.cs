using GameEngine.Item;

namespace ZorkOne.Item;

public class Bauble : ItemBase, ICanBeTakenAndDropped, IGivePointsWhenPlacedInTrophyCase, IGivePointsWhenFirstPickedUp
{
    public override string[] NounsForMatching => ["brass", "bauble", "brass bauble"];

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a beautiful brass bauble here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return OnTheGroundDescription(currentLocation);
    }

    int IGivePointsWhenFirstPickedUp.NumberOfPoints => 1;

    int IGivePointsWhenPlacedInTrophyCase.NumberOfPoints => 1;

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A beautiful brass bauble";
    }
}
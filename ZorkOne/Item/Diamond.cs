using GameEngine.Item;

namespace ZorkOne.Item;

public class Diamond : ItemBase, ICanBeTakenAndDropped, IGivePointsWhenPlacedInTrophyCase,
    IGivePointsWhenFirstPickedUp
{
    public override string[] NounsForMatching => ["diamond", "huge diamond"];

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a huge diamond here. ";
    }

    int IGivePointsWhenFirstPickedUp.NumberOfPoints => 10;

    int IGivePointsWhenPlacedInTrophyCase.NumberOfPoints => 10;

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A huge diamond";
    }
}
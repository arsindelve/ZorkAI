using GameEngine.Item;

namespace ZorkOne.Item;

public class PlatinumBar : ItemBase, ICanBeTakenAndDropped, IGivePointsWhenFirstPickedUp,
    IGivePointsWhenPlacedInTrophyCase
{
    public override string[] NounsForMatching => ["bar", "platinum", "platinum bar"];
    
    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "On the ground is a large platinum bar here";
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A platinum bar";
    }
    
    public override string NeverPickedUpDescription(ILocation currentLocation) => OnTheGroundDescription(currentLocation);

    int IGivePointsWhenFirstPickedUp.NumberOfPoints => 10;

    int IGivePointsWhenPlacedInTrophyCase.NumberOfPoints => 5;
}

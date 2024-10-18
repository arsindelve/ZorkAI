using GameEngine.Item;

namespace ZorkOne.Item;

public class Emerald : ItemBase, ICanBeTakenAndDropped, IGivePointsWhenPlacedInTrophyCase,
    IGivePointsWhenFirstPickedUp
{
    public override string[] NounsForMatching => ["emerald", "large emerald"];
    
    public string OnTheGroundDescription(ILocation currentLocation) => "There is a large emerald here. ";

    public override string GenericDescription(ILocation? currentLocation) => "A large emerald ";

    public override string NeverPickedUpDescription(ILocation currentLocation) => OnTheGroundDescription(currentLocation);

    int IGivePointsWhenFirstPickedUp.NumberOfPoints => 5;

    int IGivePointsWhenPlacedInTrophyCase.NumberOfPoints => 10;
}
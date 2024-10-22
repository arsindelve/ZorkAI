using GameEngine.Item;

namespace ZorkOne.Item;

public class SilverChalice : ItemBase, ICanBeTakenAndDropped, IGivePointsWhenFirstPickedUp,
    IGivePointsWhenPlacedInTrophyCase
{
    public override string[] NounsForMatching =>
        ["silver", "chalice", "silver chalice"];

    public override string GenericDescription(ILocation? currentLocation) => "A chalice";
    
    public string OnTheGroundDescription(ILocation currentLocation) => "There is a silver chalice here";

    public override string NeverPickedUpDescription(ILocation currentLocation) => OnTheGroundDescription(currentLocation);

    int IGivePointsWhenFirstPickedUp.NumberOfPoints => 10;

    int IGivePointsWhenPlacedInTrophyCase.NumberOfPoints => 5;
}
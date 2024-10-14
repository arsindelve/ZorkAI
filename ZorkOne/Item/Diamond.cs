using GameEngine.Item;

namespace ZorkOne.Item;

public class Diamond : ItemBase, ICanBeTakenAndDropped, IGivePointsWhenPlacedInTrophyCase,
    IGivePointsWhenFirstPickedUp
{
    public override string[] NounsForMatching => ["diamond", "huge diamond"];

    public override string GenericDescription(ILocation? currentLocation) => "A huge diamond";

    public string OnTheGroundDescription(ILocation currentLocation) => "There is a huge diamond here. ";

    int IGivePointsWhenFirstPickedUp.NumberOfPoints => 10;

    int IGivePointsWhenPlacedInTrophyCase.NumberOfPoints => 10;
}
using GameEngine.Item;

namespace ZorkOne.Item;

public class PotOfGold : ItemBase, ICanBeTakenAndDropped, IGivePointsWhenPlacedInTrophyCase,
    IGivePointsWhenFirstPickedUp
{
    public override string[] NounsForMatching => ["pot", "gold", "pot of gold"];

    public override string GenericDescription(ILocation? currentLocation) => "A pot of gold ";

    public string OnTheGroundDescription(ILocation currentLocation) => "There is a pot of gold here. ";

    public override string NeverPickedUpDescription(ILocation currentLocation) =>
        "At the end of the rainbow is a pot of gold. ";

    int IGivePointsWhenFirstPickedUp.NumberOfPoints => 10;

    int IGivePointsWhenPlacedInTrophyCase.NumberOfPoints => 10;
}
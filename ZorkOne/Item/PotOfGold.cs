using GameEngine.Item;

namespace ZorkOne.Item;

public class PotOfGold : ItemBase, ICanBeTakenAndDropped, IGivePointsWhenPlacedInTrophyCase,
    IGivePointsWhenFirstPickedUp
{
    public override string[] NounsForMatching => ["pot", "gold", "pot of gold"];

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a pot of gold here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return "At the end of the rainbow is a pot of gold. ";
    }

    int IGivePointsWhenFirstPickedUp.NumberOfPoints => 10;

    int IGivePointsWhenPlacedInTrophyCase.NumberOfPoints => 10;

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A pot of gold ";
    }
}
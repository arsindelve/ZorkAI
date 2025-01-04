using GameEngine.Item;

namespace ZorkOne.Item;

public class SapphireBracelet : ItemBase, ICanBeTakenAndDropped, IGivePointsWhenFirstPickedUp,
    IGivePointsWhenPlacedInTrophyCase
{
    public override string[] NounsForMatching =>
        ["bracelet", "sapphire bracelet", "sapphire", "sapphire-encrusted bracelet"];

    public override int Size => 1;

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a sapphire-encrusted bracelet here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return OnTheGroundDescription(currentLocation);
    }

    int IGivePointsWhenFirstPickedUp.NumberOfPoints => 5;

    int IGivePointsWhenPlacedInTrophyCase.NumberOfPoints => 5;

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A sapphire-encrusted bracelet ";
    }
}
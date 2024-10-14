using GameEngine.Item;

namespace ZorkOne.Item;

public class SapphireBracelet : ItemBase, ICanBeTakenAndDropped, IGivePointsWhenFirstPickedUp,
    IGivePointsWhenPlacedInTrophyCase
{
    public override string[] NounsForMatching =>
        ["bracelet", "sapphire bracelet", "sapphire", "sapphire-encrusted bracelet"];

    public override string GenericDescription(ILocation? currentLocation) => "A sapphire-encrusted bracelet ";

    public string OnTheGroundDescription(ILocation currentLocation) => "There is a sapphire-encrusted bracelet here. ";

    public override string NeverPickedUpDescription(ILocation currentLocation) => OnTheGroundDescription(currentLocation);

    public override int Size => 1;

    int IGivePointsWhenFirstPickedUp.NumberOfPoints => 5;

    int IGivePointsWhenPlacedInTrophyCase.NumberOfPoints => 5;
}
using GameEngine.Item;

namespace ZorkOne.Item;

public class Trident : ItemBase, ICanBeTakenAndDropped, IGivePointsWhenPlacedInTrophyCase,
    IGivePointsWhenFirstPickedUp
{
    public override string[] NounsForMatching => ["trident", "crystal trident", "poseidon's trident", "poseidon's own trident"];

    public override string GenericDescription(ILocation? currentLocation) => "A crystal trident";

    public string OnTheGroundDescription(ILocation currentLocation) => "There is a crystal trident here. ";

    public override string NeverPickedUpDescription(ILocation currentLocation) => "On the shore lies Poseidon's own crystal trident. ";

    public override int Size => 5;

    int IGivePointsWhenFirstPickedUp.NumberOfPoints => 4;

    int IGivePointsWhenPlacedInTrophyCase.NumberOfPoints => 11;
}
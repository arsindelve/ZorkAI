using GameEngine.Item;

namespace ZorkOne.Item;

public class Trident : ItemBase, ICanBeTakenAndDropped, IGivePointsWhenPlacedInTrophyCase,
    IGivePointsWhenFirstPickedUp
{
    public override string[] NounsForMatching =>
        ["trident", "crystal trident", "poseidon's trident", "poseidon's own trident"];

    public override int Size => 5;

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a crystal trident here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return "On the shore lies Poseidon's own crystal trident. ";
    }

    int IGivePointsWhenFirstPickedUp.NumberOfPoints => 4;

    int IGivePointsWhenPlacedInTrophyCase.NumberOfPoints => 11;

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A crystal trident";
    }
}
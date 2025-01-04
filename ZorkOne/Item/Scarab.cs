using GameEngine.Item;

namespace ZorkOne.Item;

public class Scarab : ItemBase, ICanBeTakenAndDropped, IGivePointsWhenFirstPickedUp, IGivePointsWhenPlacedInTrophyCase
{
    public override string[] NounsForMatching => ["scarab", "jeweled scarab", "beautiful jeweled scarab"];

    public override int Size => 1;

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a beautiful jeweled scarab here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return OnTheGroundDescription(currentLocation);
    }

    int IGivePointsWhenFirstPickedUp.NumberOfPoints => 5;

    int IGivePointsWhenPlacedInTrophyCase.NumberOfPoints => 5;

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A beautiful jeweled scarab";
    }
}
using GameEngine.Item;

namespace ZorkOne.Item;

public class Scarab : ItemBase, ICanBeTakenAndDropped, IGivePointsWhenFirstPickedUp, IGivePointsWhenPlacedInTrophyCase
{
    public override string[] NounsForMatching => ["scarab", "jeweled scarab", "beautiful jeweled scarab"];

    public string OnTheGroundDescription(ILocation currentLocation) => "There is a beautiful jeweled scarab here. ";

    public override string NeverPickedUpDescription(ILocation currentLocation) => OnTheGroundDescription(currentLocation);

    public override string GenericDescription(ILocation? currentLocation) => "A beautiful jeweled scarab";

    public override int Size => 1;

    int IGivePointsWhenFirstPickedUp.NumberOfPoints => 5;

    int IGivePointsWhenPlacedInTrophyCase.NumberOfPoints => 5;
}
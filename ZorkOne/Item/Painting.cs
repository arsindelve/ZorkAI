using GameEngine.Item;

namespace ZorkOne.Item;

public class Painting : ItemBase, ICanBeTakenAndDropped, IGivePointsWhenPlacedInTrophyCase,
    IGivePointsWhenFirstPickedUp
{
    public override string[] NounsForMatching => ["painting"];

    public override string GenericDescription(ILocation? currentLocation) => "A painting";

    public override int Size => 4;

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "A painting by a neglected artist is here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return
            "Fortunately, there is still one chance for you to be a vandal, for on the far wall is a painting of unparalleled beauty. ";
    }

    int IGivePointsWhenFirstPickedUp.NumberOfPoints => 4;
    
    int IGivePointsWhenPlacedInTrophyCase.NumberOfPoints => 6;
}
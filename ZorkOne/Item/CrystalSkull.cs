using GameEngine.Item;

namespace ZorkOne.Item;

public class CrystalSkull : ItemBase, ICanBeTakenAndDropped, IGivePointsWhenPlacedInTrophyCase,
    IGivePointsWhenFirstPickedUp
{
    public override string[] NounsForMatching => ["skull", "crystal skull"];

    public override int Size => 1;

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a crystal skull here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return
            "Lying in one corner of the room is a beautifully carved crystal skull. It appears to be grinning at you rather nastily. ";
    }

    int IGivePointsWhenFirstPickedUp.NumberOfPoints => 10;

    int IGivePointsWhenPlacedInTrophyCase.NumberOfPoints => 10;

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A crystal skull";
    }
}
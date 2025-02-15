using GameEngine.Item;

namespace ZorkOne.Item;

public class Sword : ItemBase, ICanBeTakenAndDropped, IWeapon, IAmPointyAndPunctureThings
{
    public override string[] NounsForMatching => ["sword", "glamdring", "orcrist"];

    public override int Size => 4;

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a sword here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return "Above the trophy case hangs an elvish sword of great antiquity.";
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A sword";
    }
}
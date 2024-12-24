using GameEngine.Item;

namespace ZorkOne.Item;

public class Sword : ItemBase, ICanBeTakenAndDropped, IWeapon
{
    public override string[] NounsForMatching => ["sword", "glamdring", "orcrist", "glowing sword"];

    public override string GenericDescription(ILocation? currentLocation) => "A sword";

    public override int Size => 4;

    public string OnTheGroundDescription(ILocation currentLocation) => "There is a sword here. ";

    public override string NeverPickedUpDescription(ILocation currentLocation) =>
        "Above the trophy case hangs an elvish sword of great antiquity.";
}
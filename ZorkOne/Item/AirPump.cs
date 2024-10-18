using GameEngine.Item;

namespace ZorkOne.Item;

public class AirPump : ItemBase, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["pump", "air pump", "hand-held air pump"];

    public override string GenericDescription(ILocation? currentLocation) => "A hand-held air pump";
    
    public string OnTheGroundDescription(ILocation currentLocation) => "There is a hand-held air pump here. ";

    public override string NeverPickedUpDescription(ILocation currentLocation) => OnTheGroundDescription(currentLocation);

    public override int Size => 5;
}
using GameEngine.Item;

namespace ZorkOne.Item;

public class Coal : ItemBase, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["pile of coal", "coal", "pile", "small pile of coal"];

    public override string GenericDescription(ILocation? currentLocation) => "A small pile of coal ";
    
    public string OnTheGroundDescription(ILocation currentLocation) => "There is a small pile of coal here. ";

    public override string NeverPickedUpDescription(ILocation currentLocation) => OnTheGroundDescription(currentLocation);

    public override int Size => 5;
}
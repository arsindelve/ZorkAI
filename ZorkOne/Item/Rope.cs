using GameEngine.Item;
using Model.Interface;

namespace ZorkOne.Item;

public class Rope : ItemBase, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["rope"];

    public override int Size => 2;

    public bool TiedToRailing { get; set; }

    string ICanBeTakenAndDropped.OnTheGroundDescription(ILocation currentLocation)
    {
        return TiedToRailing
            ? "Hanging down from the railing is a rope which ends about ten feet from the floor below. "
            : "There is a rope here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return "A large coil of rope is lying in the corner.";
    }

    public override string? OnBeingTaken(IContext context, ICanContainItems? previousLocation)
    {
        TiedToRailing = false;
        return base.OnBeingTaken(context, previousLocation);
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A rope";
    }
}
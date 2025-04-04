using GameEngine.Location;
using Model.Location;

namespace Planetfall.Item.Lawanda;

public class Pliers : ItemBase, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["wide-nosed pliers", "pliers"];

    public string OnTheGroundDescription(ILocation? currentLocation)
    {
        return "There is a pair of wide-nosed pliers here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return OnTheGroundDescription(currentLocation);
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A pair of wide-nosed pliers";
    }
}

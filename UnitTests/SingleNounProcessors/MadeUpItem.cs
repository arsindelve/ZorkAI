using Model.Interface;
using Model.Item;
using Model.Location;

namespace UnitTests.SingleNounProcessors;

internal class MadeUpItem : ICanBeEaten, ICanBeTakenAndDropped
{
    public string EatenDescription => "";

    public string OnTheGroundDescription(ILocation currentLocation) => "";

    public string? NeverPickedUpDescription(ILocation currentLocation) => "";

    public bool HasEverBeenPickedUp => false;

    public string OnBeingTaken(IContext context)
    {
        return "";
    }

    public void OnFailingToBeTaken(IContext context)
    {
    }
}
using Model.Interface;
using Model.Item;
using Model.Location;

namespace UnitTests.SingleNounProcessors;

internal class MadeUpItem : ICanBeEaten, ICanBeTakenAndDropped
{
    public string OnEating(IContext context)
    {
        return "";
    }

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "";
    }

    public string? NeverPickedUpDescription(ILocation currentLocation)
    {
        return "";
    }

    public bool HasEverBeenPickedUp => false;

    public string OnBeingTaken(IContext context, ICanContainItems? previousLocation)
    {
        return "";
    }

    public void OnFailingToBeTaken(IContext context)
    {
    }
}
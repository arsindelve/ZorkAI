using Model.Interface;
using Model.Item;

namespace UnitTests.SingleNounProcessors;

internal class MadeUpItem : IInteractionTarget, ICanBeEaten, ICanBeTakenAndDropped
{
    public string EatenDescription => "";

    public string OnTheGroundDescription => "";

    public string? NeverPickedUpDescription => "";

    public bool HasEverBeenPickedUp => false;

    public string OnBeingTaken(IContext context)
    {
        return "";
    }

    public void OnFailingToBeTaken(IContext context)
    {
    }
}
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Location.Lawanda.Lab;

namespace Planetfall.Item.Lawanda.Lab;

internal class BioLockInnerDoor : SimpleDoor
{
    public override string[] NounsForMatching =>
    [
        "door"
    ];

    public override string OnOpening(IContext context)
    {
        var bioLockEast = Repository.GetLocation<BioLockEast>();
        var floyd = Repository.GetItem<Floyd>();

        var isFloydReady = bioLockEast.StateMachine.FloydHasSaidNeedToGetCard && floyd.IsHereAndIsOn(context);

        return bioLockEast.StateMachine.HandleDoorOpening(isFloydReady, context, bioLockEast);
    }

    public override string NowClosed(ILocation currentLocation)
    {
        var bioLockEast = Repository.GetLocation<BioLockEast>();
        var result = bioLockEast.StateMachine.HandleDoorClosing();

        // If we get a custom message back, return it; otherwise use default
        return !string.IsNullOrEmpty(result) ? result : base.NowClosed(currentLocation);
    }
}
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
        return bioLockEast.StateMachine.HandleDoorOpening(context);
    }

    public override string OnClosing(IContext context)
    {
        var bioLockEast = Repository.GetLocation<BioLockEast>();
        return bioLockEast.StateMachine.HandleDoorClosing(context);
    }
}
using Model.AIGeneration;
using Planetfall.Location.Lawanda.Lab;

namespace Planetfall.Item.Lawanda.Lab;

internal class BioLockInnerDoor : SimpleDoor, ITurnBasedActor
{
    public override string[] NounsForMatching => ["lab door", "door"];

    public override string[] NounsForPreciseMatching => ["lab door", "lab"];

    [UsedImplicitly] public int TurnsSinceOpening { get; set; }

    public override string CannotBeOpenedDescription(IContext context)
    {
        var outerDoor = Repository.GetItem<BioLockOuterDoor>();
        if (outerDoor.IsOpen)
        {
            return "A very bored-sounding recorded voice explains that, in order to prevent contamination, " +
                   "both lock doors cannot be open simultaneously. ";
        }

        return base.CannotBeOpenedDescription(context);
    }

    public override string OnOpening(IContext context)
    {
        var outerDoor = Repository.GetItem<BioLockOuterDoor>();
        if (outerDoor.IsOpen)
        {
            return CannotBeOpenedDescription(context);
        }

        var bioLockEast = Repository.GetLocation<BioLockEast>();
        var stateMachineResult = bioLockEast.StateMachine.HandleDoorOpening(context);

        context.RegisterActor(this);
        TurnsSinceOpening = 1;

        return stateMachineResult;
    }

    public override string OnClosing(IContext context)
    {
        var bioLockEast = Repository.GetLocation<BioLockEast>();
        return bioLockEast.StateMachine.HandleDoorClosing(context);
    }

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        if (TurnsSinceOpening == 1)
        {
            TurnsSinceOpening++;
        }
        else if (TurnsSinceOpening > 1)
        {
            TurnsSinceOpening = 0;
            IsOpen = false;
            context.RemoveActor(this);

            if (context.CurrentLocation is BioLockEast or BioLockWest or BioLabLocation)
            {
                return Task.FromResult("The door at the eastern end of the bio-lock closes silently. ");
            }
        }

        return Task.FromResult(string.Empty);
    }
}
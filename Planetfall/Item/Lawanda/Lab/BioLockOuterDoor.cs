using Model.AIGeneration;
using Planetfall.Location.Lawanda.Lab;

namespace Planetfall.Item.Lawanda.Lab;

internal class BioLockOuterDoor : SimpleDoor, ITurnBasedActor
{
    [UsedImplicitly] public int TurnsSinceOpening { get; set; }

    public override string[] NounsForMatching =>
    [
        "bio-lock door", "door", "biolock", "bio lock", "bio lock door", "bio-lock", "bio door"
    ];

    public override string CannotBeOpenedDescription(IContext context)
    {
        var innerDoor = Repository.GetItem<BioLockInnerDoor>();
        if (innerDoor.IsOpen)
        {
            return "A very bored-sounding recorded voice explains that, in order to prevent contamination, " +
                   "both lock doors cannot be open simultaneously. ";
        }

        return base.CannotBeOpenedDescription(context);
    }

    public override string OnOpening(IContext context)
    {
        var innerDoor = Repository.GetItem<BioLockInnerDoor>();
        if (innerDoor.IsOpen)
        {
            return CannotBeOpenedDescription(context);
        }

        context.RegisterActor(this);
        TurnsSinceOpening = 1;
        return base.OnOpening(context);
    }

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        if (TurnsSinceOpening == 1)
        {
            TurnsSinceOpening++;
        }
        else
        {
            TurnsSinceOpening = 0;
            IsOpen = false;
            context.RemoveActor(this);

            if (context.CurrentLocation is MainLab or BioLockEast or BioLockWest)
            {
                return Task.FromResult("The door at the western end of the bio-lock closes silently.");
            }
        }

        return Task.FromResult(string.Empty);
    }
}
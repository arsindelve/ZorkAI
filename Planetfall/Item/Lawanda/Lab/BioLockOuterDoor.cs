using Model.AIGeneration;
using Planetfall.Location.Lawanda.Lab;

namespace Planetfall.Item.Lawanda.Lab;

internal class BioLockOuterDoor : SimpleDoor, ITurnBasedActor
{
    [UsedImplicitly] public int TurnsSinceOpening { get; set; }

    public override string[] NounsForMatching =>
    [
        "bio-lock door", "door", "biolock", "bio lock", "bio lock door", "bio-lock", "bio door", "biolock door"
    ];

    public override string OnOpening(IContext context)
    {
        context.RegisterActor(this);
        TurnsSinceOpening = 1;
        return base.OnOpening(context);
    }

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        if (TurnsSinceOpening <= 2)
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
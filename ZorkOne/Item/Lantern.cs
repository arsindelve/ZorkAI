using System.Diagnostics;

namespace ZorkOne.Item;

public class Lantern : ItemBase, ICanBeExamined, ICanBeTakenAndDropped, ICanBeTurnedOnAndOff, IAmALightSource,
    ITurnBasedActor
{
    public int TurnsWhileOn { get; set; }

    public bool BurnedOut { get; set; }

    public override string[] NounsForMatching => ["lantern", "lamp", "light"];

    public override string InInventoryDescription => $"A brass lantern {(IsOn ? "(providing light)" : string.Empty)}";

    public override int Size => 3;

    string ICanBeExamined.ExaminationDescription => IsOn ? "The lamp is on." : "The lamp is turned off.";

    string ICanBeTakenAndDropped.OnTheGroundDescription => "There is a brass lantern (battery-powered) here.";

    public override string NeverPickedUpDescription => "A battery-powered brass lantern is on the trophy case.";

    public bool IsOn { get; set; }

    string ICanBeTurnedOnAndOff.NowOnText => "The brass lantern is now on.";

    string ICanBeTurnedOnAndOff.NowOffText => "The brass lantern is now off.";

    string ICanBeTurnedOnAndOff.AlreadyOffText => "It is already off.";

    string ICanBeTurnedOnAndOff.AlreadyOnText => "It is already on.";

    public string CannotBeTurnedOnText => BurnedOut ? "A burned-out lamp won't light. " : string.Empty;

    public void OnBeingTurnedOn(IContext context)
    {
        context.RegisterActor(this);
    }

    public void OnBeingTurnedOff(IContext context)
    {
        context.RemoveActor(this);
    }

    public string Act(IContext context)
    {
        Debug.WriteLine($"Lantern counter: {TurnsWhileOn}");
        TurnsWhileOn++;

        switch (TurnsWhileOn)
        {
            case 200:
                return "The lamp appears a bit dimmer. ";

            case 300:
                return "The lamp is definitely dimmer now. ";

            case 370:
                return "The lamp is nearly out. ";

            case 375:
                IsOn = false;
                BurnedOut = true;
                return "You'd better have more light than from the brass lantern. ";

            default:
                return string.Empty;
        }
    }
}
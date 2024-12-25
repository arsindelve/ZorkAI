using Model.AIGeneration;

namespace Planetfall.Item.Kalamontee.Admin;

public class KitchenDoor : ItemBase, ICanBeExamined, IOpenAndClose, ITurnBasedActor
{
    // ReSharper disable once MemberCanBePrivate.Global
    public int TurnsOpen { get; set; }

    public override string[] NounsForMatching => ["kitchen door", "door", "kitchen card door"];

    public string ExaminationDescription => $"The door is {(IsOpen ? "open" : "closed")}. ";

    public bool IsOpen { get; set; }

    public string NowOpen(ILocation currentLocation)
    {
        return "The kitchen door quietly slides open. ";
    }

    public string NowClosed(ILocation currentLocation)
    {
        return "The kitchen door slides quietly closed. ";
    }

    public string AlreadyOpen => "A light flashes \"Pleez yuuz kitcin akses kard.\" ";

    public string AlreadyClosed => "It's already closed. ";

    public bool HasEverBeenOpened { get; set; }

    public string CannotBeOpenedDescription(IContext context)
    {
        return "A light flashes \"Pleez yuuz kitcin akses kard.\" ";
    }

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        TurnsOpen++;

        if (TurnsOpen != 3) return Task.FromResult(string.Empty);
        
        TurnsOpen = 0;
        context.RemoveActor(this);
        IsOpen = false;
        return Task.FromResult(NowClosed(context.CurrentLocation));
    }
}
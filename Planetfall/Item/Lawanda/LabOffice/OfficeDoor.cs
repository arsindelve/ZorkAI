using Planetfall.Command;

namespace Planetfall.Item.Lawanda.LabOffice;

public class OfficeDoor : ItemBase, IOpenAndClose, ICanBeExamined
{
    public override string[] NounsForMatching => ["door", "office door"];

    public bool IsOpen { get; set; }

    public bool HasEverBeenOpened { get; set; }

    /// <summary>
    /// Flag to prevent duplicate mist message on the turn the door is opened.
    /// Set to true in OnOpening, checked and cleared by FungicideTimer.Act().
    /// </summary>
    [UsedImplicitly]
    public bool JustOpenedThisTurn { get; set; }

    public string AlreadyOpen => "The door is already open. ";

    public string AlreadyClosed => "The door is already closed. ";

    public string ExaminationDescription =>
        IsOpen
            ? "The office door is open. "
            : "The office door is closed. ";

    public string NowOpen(ILocation currentLocation) => "The office door is now open. ";

    public string NowClosed(ILocation currentLocation) => "Closed. ";

    public string? CannotBeOpenedDescription(IContext context) => null;

    public override string OnOpening(IContext context)
    {
        var timer = Repository.GetItem<FungicideTimer>();
        if (timer.IsActive)
        {
            JustOpenedThisTurn = true;
            return "\nThrough the open doorway you can see the Bio Lab. It seems to be filled with a light mist. " +
                   "Horrifying biological nightmares stagger about making choking noises. ";
        }

        var deathResult = new DeathProcessor().Process(
            "Mutated monsters from the Bio Lab pour into the office. You are devoured. ",
            context);
        return "\n" + deathResult.InteractionMessage;
    }

    public string OnClosing(IContext context) => string.Empty;
}
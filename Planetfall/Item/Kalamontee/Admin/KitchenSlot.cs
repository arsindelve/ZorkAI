using Model.AIGeneration;

namespace Planetfall.Item.Kalamontee.Admin;

internal class KitchenSlot : SlotBase<KitchenAccessCard, KitchenSlot, KitchenDoor>;

internal abstract class SlotBase<TAccessCard, TAccessSlot, TDoor> : ItemBase, ICanBeExamined
    where TAccessCard : ItemBase, new()
    where TAccessSlot : ItemBase, new()
    where TDoor : ItemBase, IOpenAndClose, ITurnBasedActor, new()
{
    public override string[] NounsForMatching => ["kitchen slot", "slot", "kitchen card slot"];

    public string ExaminationDescription =>
        "The slot is about ten centimeters wide, but only about two centimeters deep. It is surrounded on its " +
        "long sides by parallel ridges of metal. ";

    
    // TODO: Add matching nouns for disambiguation, which defaults to nouns for matching 
    // TODO: Remove "card" from Nouns for matching, and require one of the other nouns
    // TODO: Train Claude to treat certain nouns as being "together" "Green button" "upper card" etc. 
    // TODO: Fix disambiguation in general. 
    
    public override InteractionResult RespondToMultiNounInteraction(MultiNounIntent action, IContext context)
    {
        string[] verbs = ["insert", "put", "place"];
        if (action.Match<TAccessCard, TAccessSlot>(verbs, ["in", "into"]))
            return new PositiveInteractionResult(
                "The slot is shallow, so you can't put anything in it. It may be possible to slide " +
                "something through the slot, though. ");

        verbs = ["slide", "swipe", "scan", "pass", "insert", "glide", "draw", "push"];
        if (action.Match<TAccessCard, TAccessSlot>(verbs, ["across", "through", "in", "into"]))
        {
            var door = Repository.GetItem<TDoor>();
            door.IsOpen = true;
            context.RegisterActor(door);
            return new PositiveInteractionResult(door.OnOpening(context));
        }

        return base.RespondToMultiNounInteraction(action, context);
    }
}

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
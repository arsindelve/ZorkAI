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
            return new PositiveInteractionResult(door.NowOpen(context.CurrentLocation));
        }

        return base.RespondToMultiNounInteraction(action, context);
    }
}
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


    public override InteractionResult RespondToMultiNounInteraction(MultiNounIntent action, IContext context)
    {
        string[] verbs = ["insert", "put", "place"];
        if (action.Match<TAccessCard, TAccessSlot>(verbs, ["in", "into"]))
            return new PositiveInteractionResult(
                "The slot is shallow, so you can't put anything in it. It may be possible to slide " +
                "something through the slot, though. ");

        verbs = ["slide", "swipe", "scan", "pass", "glide", "draw", "push"];
        string[] prepositions = ["across", "through"];
        if (action.Match<TAccessCard, TAccessSlot>(verbs, prepositions))
        {
            var door = Repository.GetItem<TDoor>();
            door.IsOpen = true;
            context.RegisterActor(door);
            return new PositiveInteractionResult(door.NowOpen(context.CurrentLocation));
        }

        var nounOne = Repository.GetItem(action.NounOne);

        // Right idea, wrong card. 
        if (action.MatchVerb(verbs) && action.MatchPreposition(prepositions) && nounOne is AccessCard)
            return new PositiveInteractionResult(
                "A sign flashes \"Inkorekt awtharazaashun kard...akses deeniid.\"");

        return base.RespondToMultiNounInteraction(action, context);
    }
}
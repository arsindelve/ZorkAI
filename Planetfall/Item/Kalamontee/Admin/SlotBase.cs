namespace Planetfall.Item.Kalamontee.Admin;

internal abstract class SlotBase<TAccessCard, TAccessSlot> : ItemBase, ICanBeExamined
    where TAccessCard : AccessCard, new()
    where TAccessSlot : ItemBase, new()
{
    public string ExaminationDescription =>
        "The slot is about ten centimeters wide, but only about two centimeters deep. It is surrounded on its " +
        "long sides by parallel ridges of metal. ";

    protected abstract InteractionResult OnSuccessfulSlide(IContext context);

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
            return OnSuccessfulSlide(context);

        var nounOne = Repository.GetItem(action.NounOne);

        // Right idea, wrong card. 
        if (action.MatchVerb(verbs) && action.MatchPreposition(prepositions) && nounOne is AccessCard)
            return new PositiveInteractionResult(
                "A sign flashes \"Inkorekt awtharazaashun kard...akses deeniid.\"");

        return base.RespondToMultiNounInteraction(action, context);
    }
}
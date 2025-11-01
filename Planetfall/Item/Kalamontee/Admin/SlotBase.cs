using Planetfall.Item.Kalamontee.Mech;
using Planetfall.Item.Kalamontee.Mech.FloydPart;

namespace Planetfall.Item.Kalamontee.Admin;

/// <summary>
/// Represents an abstract base class for a slot that interacts with a specific type of access card.
/// Derived classes are expected to implement functionality specific to their use case.
/// </summary>
/// <typeparam name="TAccessCard">
/// The type of access card that this slot can interact with. Must inherit from <see cref="AccessCard"/>.
/// </typeparam>
/// <typeparam name="TAccessSlot">
/// The type of slot that interacts with the access card. Must inherit from <see cref="ItemBase"/>.
/// </typeparam>
internal abstract class SlotBase<TAccessCard, TAccessSlot> : ItemBase, ICanBeExamined
    where TAccessCard : AccessCard, new()
    where TAccessSlot : ItemBase, new()
{
    public string ExaminationDescription =>
        "The slot is about ten centimeters wide, but only about two centimeters deep. It is surrounded on its " +
        "long sides by parallel ridges of metal. ";

    /// <summary>
    /// Handles the operations to be performed upon a successful slide action of the correct card in this slot.
    /// </summary>
    /// <param name="context">The context in which the slide action is performed, providing the necessary environment or state information.</param>
    /// <returns>An <see cref="InteractionResult"/> object representing the outcome of the successful slide action.</returns>
    protected abstract InteractionResult OnSuccessfulSlide(IContext context, string? afterMessage);

    public override async Task<InteractionResult?> RespondToMultiNounInteraction(MultiNounIntent action,
        IContext context)
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
            if (!context.HasItem<TAccessCard>())
                return new NoNounMatchInteractionResult();

            string? afterMessage = Repository.GetItem<Floyd>().OffersLowerElevatorCard(context);
            
            return OnSuccessfulSlide(context, afterMessage);
        }

        var nounOne = Repository.GetItem(action.NounOne);

        // Right idea, wrong card. 
        if (action.MatchVerb(verbs) && action.MatchPreposition(prepositions) && nounOne is AccessCard)
        {
            if (!context.HasMatchingNoun(action.NounOne).HasItem)
                return new NoNounMatchInteractionResult();

            return new PositiveInteractionResult(
                "A sign flashes \"Inkorekt awtharazaashun kard...akses deeniid.\"");
        }

        return await base.RespondToMultiNounInteraction(action, context);
    }
}
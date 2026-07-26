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

        verbs = ["slide", "swipe", "scan", "pass", "glide", "draw", "push", "use"];
        string[] prepositions = ["across", "through", "on", "in"];

        if (action.Match<TAccessCard, TAccessSlot>(verbs, prepositions))
        {
            // Possession must be container-aware, exactly like the wrong-card branch below (which uses
            // HasMatchingNoun). context.HasItem<T>() is top-level-only, so a card in the Patrol uniform
            // pocket - the game's starting container, and where the ID card ships - failed this check
            // and the turn fell through to the narrator with no hint that pocketing the card was the
            // problem (issue #503). The check itself must stay: the card is resolved by TYPE from the
            // repository, so this is the only thing stopping a card lying across the map from working.
            if (!context.IsCarrying<TAccessCard>())
                return new NoNounMatchInteractionResult();

            // The original slides a card you are merely carrying: SLOT-F performs an implicit take
            // (globals.zil:1375) before any of the per-card branches run, so mirror that and put the
            // card in hand either way.
            var card = Repository.GetItem<TAccessCard>();
            if (!context.Items.Contains(card))
                context.ItemPlacedHere(card);

            // Deliberate deviation from the original (issue #211 follow-up): the ZIL's WRONG-CARD
            // (globals.zil:1438) doesn't distinguish "wrong card" from "corrupted card" - a scrambled
            // card gets the exact same "incorrect authorization" message as a card that was never
            // valid here at all. Since scrambling is silent and permanent, that leaves the player no
            // way to tell "wrong card" (try a different one) apart from "your card is ruined" (the
            // magnet did this - stop carrying it with your cards). We judged that ambiguity unfair and
            // gave the scrambled case its own message instead of matching the original verbatim.
            if (card.Scrambled)
                return new PositiveInteractionResult(
                    "A sign flashes \"Damejd kard...akses deeniid.\"");

            string? afterMessage = Repository.GetItem<Floyd>().OffersLowerElevatorCard(context);

            return OnSuccessfulSlide(context, afterMessage);
        }

        var nounOne = Repository.GetItem(action.NounOne);

        // Right idea, wrong card. NOTE (found while fixing #503, left alone deliberately): this global
        // Repository.GetItem(noun) returns the first LOADED item whose nouns loosely match, and the ID
        // card's bare noun "card" swallows every "<x> access card" phrase - so once the ID card exists
        // (it starts in the uniform pocket) this branch can resolve to the ID card, which isn't an
        // AccessCard, and the wrong-card message never fires. That's a separate noun-resolution defect,
        // not the possession bug above; fixing it here is not safe by inspection, because the obvious
        // scope-aware replacements resolve through CurrentLocation, which for an inventory item can be
        // stale (Floyd's Init re-seeds the lower elevator card singleton and steals its CurrentLocation).
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
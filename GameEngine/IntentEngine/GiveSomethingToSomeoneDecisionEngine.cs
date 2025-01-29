using GameEngine.Item;
using Model.Interface;
using Model.Item;

namespace GameEngine.IntentEngine;

/// <summary>
///     Given an intent and a recipient, is it my intention with this text input
///     to give something to the recipient?
/// </summary>
/// <typeparam name="TRecipient"></typeparam>
public class GiveSomethingToSomeoneDecisionEngine<TRecipient> where TRecipient : ItemBase, ICanBeGivenThings, new()
{
    /// <summary>
    /// Determines if the action being taken involves giving an item to the specified recipient.
    /// If so, it processes the action and returns the result of the interaction.
    /// </summary>
    /// <param name="action">The multi-noun intent that specifies the action and potential nouns involved.</param>
    /// <param name="recipient">The recipient to whom the item may be given as part of the verb action.</param>
    /// <param name="context">The current interaction context, including items and state information.</param>
    /// <returns>
    /// An <see cref="InteractionResult"/> if the action involves giving an item and can be resolved,
    /// or null if the action does not match a "give" type verb or cannot be completed.
    /// </returns>
    public InteractionResult? AreWeGivingSomethingToSomeone(MultiNounIntent action, TRecipient recipient,
        IContext context)
    {
        if (!action.MatchVerb(Verbs.GiveVerbs))
            return null;

        // Recipient can be in either noun...."give thing to recipient" or "offer recipient the thing"
        string? otherThingNoun;
        if (action.MatchNounOne(recipient.NounsForMatching))
        {
            otherThingNoun = action.NounTwo;
        }
        else
        {
            if (action.MatchNounTwo(recipient.NounsForMatching))
                otherThingNoun = action.NounOne;
            else
                return null;
        }

        var thing = Repository.GetItem(otherThingNoun);
        if (thing is null)
            return null;

        if (!context.Items.Contains(thing))
            return new PositiveInteractionResult($"You don't have the {thing.NounsForMatching[0]}! ");

        return recipient.OfferThisThing(thing, context);
    }
}
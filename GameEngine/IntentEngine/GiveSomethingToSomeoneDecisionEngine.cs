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
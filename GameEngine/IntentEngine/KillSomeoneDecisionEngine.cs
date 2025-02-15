using GameEngine.Item;
using Model.Interface;
using Model.Item;

namespace GameEngine.IntentEngine;

/// <summary>
///     Given an intent and a type of foe, is it my intention with this input
///     to attack and kill the foe?
/// </summary>
/// <param name="combatEngine">
///     If the adventurer DOES want to kill the TFoe,
///     this is the combat engine that will take over.
/// </param>
/// <typeparam name="TFoe">The type of foe....Blather, cyclops, troll, Floyd (why would you do that?)</typeparam>
public class KillSomeoneDecisionEngine<TFoe>(ICombatEngine combatEngine) where TFoe : ItemBase, ICanBeAttacked, new()
{
    private readonly TFoe _foe = Repository.GetItem<TFoe>();

    /// <summary>
    /// Determines whether the intent provided by the player is to kill someone,
    /// validates the provided context and action, and performs the necessary actions
    /// to carry out the intent if it is valid.
    /// </summary>
    /// <param name="action">
    /// The player's intent, including the verb, nouns, and context of the action.
    /// This should contain the information related to what the player wants to do,
    /// such as killing a foe with a specific weapon.
    /// </param>
    /// <param name="context">
    /// The current context in which the action is being evaluated. This includes the
    /// items available, current location details, and any relevant information about
    /// the environment surrounding the intent.
    /// </param>
    /// <returns>
    /// An <see cref="InteractionResult"/> object if the action is valid and performed successfully,
    /// or null if the action is invalid or cannot be completed in the current context.
    /// </returns>
    public InteractionResult? DoYouWantToKillSomeone(MultiNounIntent action, IContext context)
    {
        string[] prepositions = ["with", "using", "by", "to"];

        if (!action.MatchNounOne(_foe.NounsForMatching) &&
            !action.MatchNounTwo(_foe.NounsForMatching))
            return null;

        var nounTwo = Repository.GetItem(action.NounTwo);
        {
            if (nounTwo is not IWeapon)
                return null;

            if (nounTwo.CurrentLocation != context)
                return new PositiveInteractionResult($"You don't have the {nounTwo.Name}. ");
        }

        if (!action.MatchVerb(Verbs.KillVerbs))
            return null;

        return !prepositions.Contains(action.Preposition.ToLowerInvariant().Trim())
            ? null
            : combatEngine.Attack(context, nounTwo as IWeapon);
    }

    /// <summary>
    /// Evaluates the player's intent to kill someone without specifying a weapon, identifies if the action can be valid
    /// based on the provided context, and executes the action if possible. The method will determine if the player
    /// has no weapons, exactly one weapon, or multiple weapons and responds accordingly.
    /// </summary>
    /// <param name="action">
    /// The intent of the player, including the relevant verb and noun, representing the action of killing and the target
    /// of the kill attempt.
    /// </param>
    /// <param name="context">
    /// The current gameplay context, including the inventory, location details, and environmental factors that may affect
    /// the player's ability to perform the action.
    /// </param>
    /// <returns>
    /// An <see cref="InteractionResult"/> object indicating the outcome of the interaction, such as successfully attacking
    /// the target or prompting the player for additional input, or null if the action is invalid.
    /// </returns>
    public InteractionResult? DoYouWantToKillSomeoneButYouDidNotSpecifyAWeapon(SimpleIntent action, IContext context)
    {
        InteractionResult? result = null;

        if (!action.MatchVerb(Verbs.KillVerbs))
            return result;

        // Got some weapons on you? 
        switch (context.GetItems<IWeapon>().Count)
        {
            case 0:
                if (!action.MatchNoun(_foe.NounsForMatching) &&
                    !action.MatchNoun(_foe.NounsForMatching))
                    return null;

                // Bare-knuckle brawl, old-school! 
                return combatEngine.Attack(context, null);

            case 1:
            {
                // Assume they want to kill the foe with the only weapon they have
                var weaponName = context.GetItems<IWeapon>().Cast<IItem>().Single().NounsForMatching.First();
                var multiNounIntent = new MultiNounIntent
                {
                    Verb = action.Verb,
                    NounOne = action.Noun ?? "",
                    Preposition = "with",
                    OriginalInput = action.OriginalInput ?? "",
                    NounTwo = weaponName
                };
                var withWeaponResponse = DoYouWantToKillSomeone(multiNounIntent, context);

                if (withWeaponResponse is null)
                    return result;

                result = new PositiveInteractionResult($"(with the {weaponName})\n" +
                                                       withWeaponResponse.InteractionMessage);
                break;
            }
            default:
                result = new PositiveInteractionResult("You'll need to specify which weapon you want to use. ");
                break;
        }

        return result;
    }
}
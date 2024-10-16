using GameEngine.Item;
using Model.Interface;
using Model.Item;

namespace GameEngine.IntentEngine;

/// <summary>
///     Given an intent and a type of foe, is it my intention with this text input
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

    public InteractionResult? DoYouWantToKillSomeoneButYouDidNotSpecifyAWeapon(SimpleIntent action, IContext context)
    {
        InteractionResult? result = null;

        if (!action.MatchVerb(Verbs.KillVerbs))
            return result;

        // Got some weapons on you? 
        switch (context.Items.OfType<IWeapon>().Count())
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
                var weaponName = context.Items.OfType<IWeapon>().Cast<IItem>().Single().NounsForMatching.First();
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
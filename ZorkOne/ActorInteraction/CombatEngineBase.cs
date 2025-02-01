using Model.Interface;
using ZorkOne.Command;

namespace ZorkOne.ActorInteraction;

internal abstract class CombatEngineBase<T>(IRandomChooser chooser) where T : ITurnBasedActor
{
    private static bool SmallWound(ZorkIContext zorkContext, bool fatal)
    {
        if (zorkContext.LightWoundCounter > 0)
            fatal = true;
        else
            zorkContext.LightWoundCounter = 30;
        
        return fatal;
    }

    private static (CombatOutcome outcome, string text) DropWeapon(IContext context, ZorkIContext zorkContext,
        (CombatOutcome outcome, string text) attack)
    {
        var weapon = zorkContext.GetWeapon();
        if (weapon is null)
            // this should not happen based on logic above.
            return attack;
        context.Drop(weapon);
        attack.text = attack.text.Replace("{weapon}", weapon.Name);
        return attack;
    }

    protected string Go(List<(CombatOutcome outcome, string text)> possibleOutcomes, ZorkIContext zorkContext)
    {
        var fatal = false;
       
        var attack = chooser.Choose(possibleOutcomes);

        switch (attack.outcome)
        {
            case CombatOutcome.Miss:
                break;

            case CombatOutcome.Disarm:
            {
                attack = DropWeapon(zorkContext, zorkContext, attack);
                break;
            }
            case CombatOutcome.SmallWound:
                fatal = SmallWound(zorkContext, fatal);
                break;

            case CombatOutcome.Stun:
                zorkContext.IsStunned = true;
                break;

            case CombatOutcome.Fatal:
                fatal = true;
                break;
        }

        if (fatal)
        {
            zorkContext.RemoveActor<T>();
            return new DeathProcessor()
                .Process(
                    $"{attack.text} It appears that that last blow was too much for you. I'm afraid you are dead. \n",
                    zorkContext).InteractionMessage;
        }

        return "\n" + attack.text;
    }
}
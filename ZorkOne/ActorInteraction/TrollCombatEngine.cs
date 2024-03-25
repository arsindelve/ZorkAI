using ZorkOne.Command;

namespace ZorkOne.ActorInteraction;

internal class TrollCombatEngine
{
    private readonly List<(CombatOutcome outcome, string text)> _haveWeaponOutcomes =
    [
        (CombatOutcome.Miss, "The troll's swing almost knocks you over as you barely parry in time."),
        (CombatOutcome.DropWeapon, "The axe hits your {weapon} and knocks it spinning."),
        (CombatOutcome.DropWeapon, "The troll swings, you parry, but the force of his blow knocks your {weapon} away.")
    ];

    private readonly List<(CombatOutcome outcome, string text)> _outcomes =
    [
        (CombatOutcome.Miss, "The axe crashes against the rock, throwing sparks! "),
        (CombatOutcome.Miss, "The axe sweeps past as you jump aside. "),
        (CombatOutcome.Fatal, "The troll's axe removes your head. "),
        (CombatOutcome.Fatal, "The troll's axe stroke cleaves you from the nave to the chops. "),
        (CombatOutcome.Miss, "The troll's axe barely misses your ear. "),
        (CombatOutcome.Miss, "The troll's swing almost knocks you over as you barely parry in time."),
        (CombatOutcome.SmallWound, "The axe gets you right in the side. Ouch!"),
        (CombatOutcome.Miss, "The troll swings his axe, but it misses."),
        (CombatOutcome.Stun, "The troll hits you with a glancing blow, and you are momentarily stunned."),
        (CombatOutcome.Miss, "The troll swings; the blade turns on your armor but crashes broadside into your head."),
        (CombatOutcome.Miss, "The troll swings his axe, but it misses."),
        (CombatOutcome.SmallWound, "The troll swings his axe, and it nicks your arm as you dodge. "),
        (CombatOutcome.SmallWound, "The flat of the troll's axe skins across your forearm. "),
        (CombatOutcome.Stun, "The troll's mighty blow drops you to your knees. "),
        (CombatOutcome.Miss, "You stagger back under a hail of axe strokes. "),
        (CombatOutcome.Fatal,
            "The flat of the troll's axe hits you delicately on the head, knocking you out. The troll scratches his " +
            "head ruminatively:  Might you be magically protected, he wonders? Conquering his fears, the troll puts you to death. ")
    ];

    private readonly Random _rand = new();

    public string Attack(IContext context)
    {
        if (context is not ZorkIContext zorkContext)
            throw new ArgumentException();

        if (Repository.GetItem<Troll>().IsStunned)
        {
            Repository.GetItem<Troll>().IsStunned = false;
            return "The troll slowly regains his feet. ";
        }

        var fatal = false;

        var possibleOutcomes = zorkContext.HasWeapon ? _outcomes.Union(_haveWeaponOutcomes).ToList() : _outcomes;
        var attack = possibleOutcomes[_rand.Next(possibleOutcomes.Count)];

        switch (attack.outcome)
        {
            case CombatOutcome.Miss:
                break;

            case CombatOutcome.DropWeapon:
            {
                attack = DropWeapon(context, zorkContext, attack);
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
            return new DeathProcessor()
                .Process(
                    $"{attack.text} It appears that that last blow was too much for you. I'm afraid you are dead. \n",
                    context).InteractionMessage;

        return attack.text;
    }

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
}
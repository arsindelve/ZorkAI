using GameEngine;
using Model.Interface;

namespace ZorkOne.ActorInteraction;

internal class TrollCombatEngine : CombatEngineBase<Troll>
{
    private readonly List<(CombatOutcome outcome, string text)> _haveWeaponOutcomes =
    [
        (CombatOutcome.Miss, "The troll's swing almost knocks you over as you barely parry in time."),
        (CombatOutcome.Disarm, "The axe hits your {weapon} and knocks it spinning."),
        (CombatOutcome.Disarm, "The troll swings, you parry, but the force of his blow knocks your {weapon} away.")
    ];

    private readonly List<(CombatOutcome outcome, string text)> _outcomes =
    [
        (CombatOutcome.Miss, "The axe crashes against the rock, throwing sparks! "),
        (CombatOutcome.Miss, "The axe sweeps past as you jump aside. "),
        (CombatOutcome.Miss, "You stagger back under a hail of axe strokes. "),
        (CombatOutcome.Miss, "The troll's axe barely misses your ear. "),
        (CombatOutcome.Miss, "The troll's swing almost knocks you over as you barely parry in time."),
        (CombatOutcome.Miss, "The troll swings his axe, but it misses."),
        (CombatOutcome.Miss, "The troll swings; the blade turns on your armor but crashes broadside into your head."),
        (CombatOutcome.Miss, "The troll swings his axe, but it misses."),

        (CombatOutcome.SmallWound, "The troll swings his axe, and it nicks your arm as you dodge. "),
        (CombatOutcome.SmallWound, "The axe gets you right in the side. Ouch!"),
        (CombatOutcome.SmallWound, "The flat of the troll's axe skins across your forearm. "),

        (CombatOutcome.Stun, "The troll's mighty blow drops you to your knees. "),
        (CombatOutcome.Stun, "The troll hits you with a glancing blow, and you are momentarily stunned."),
        
        (CombatOutcome.Fatal, "The troll's axe removes your head. "),
        (CombatOutcome.Fatal, "The troll's axe stroke cleaves you from the nave to the chops. "),
        (CombatOutcome.Fatal,
            "The flat of the troll's axe hits you delicately on the head, knocking you out. The troll scratches his " +
            "head ruminatively:  Might you be magically protected, he wonders? Conquering his fears, the troll puts you to death. ")
    ];

    public TrollCombatEngine() : base(new RandomChooser())
    {
    }

    /// <summary>
    ///     Constructor for unit testing
    /// </summary>
    /// <param name="chooser"></param>
    public TrollCombatEngine(IRandomChooser chooser) : base(chooser)
    {
    }

    internal string Attack(IContext context)
    {
        if (context is not ZorkIContext zorkContext)
            throw new ArgumentException();

        var possibleOutcomes = zorkContext.HasWeapon
            ? _outcomes.Union(_haveWeaponOutcomes).ToList()
            : _outcomes;

        return Go(possibleOutcomes, zorkContext);
    }
}
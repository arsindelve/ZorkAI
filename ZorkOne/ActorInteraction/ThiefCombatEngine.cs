using GameEngine;
using Model.Interface;

namespace ZorkOne.ActorInteraction;

internal class ThiefCombatEngine(IRandomChooser chooser) : CombatEngineBase<Thief>(chooser)
{
    private readonly List<(CombatOutcome outcome, string text)> _haveWeaponOutcomes =
    [
        (CombatOutcome.Disarm, "The thief neatly flips the {weapon} out of your hands, and it drops to the floor. ")
    ];

    private readonly List<(CombatOutcome outcome, string text)> _outcomes =
    [
        (CombatOutcome.Miss, "The thief stabs nonchalantly with his stiletto and misses. "),
        (CombatOutcome.Miss, "The thief tries to sneak past your guard, but you twist away. "),
        (CombatOutcome.Miss, "The thief stabs nonchalantly with his stiletto and misses. "),
        (CombatOutcome.Miss, "You dodge as the thief comes in low. "),
        (CombatOutcome.Miss, "You parry a lightning thrust, and the thief salutes you with a grim nod. "),

        (CombatOutcome.Stun, "The thief attacks, and you fall back desperately. "),
        (CombatOutcome.Stun, "The thief rams the haft of his blade into your stomach, leaving you out of breath. "),
        (CombatOutcome.Stun, "The butt of his stiletto cracks you on the skull, and you stagger back. "),

        (CombatOutcome.SmallWound, "The thief slowly approaches, strikes like a snake, and leaves you wounded. "),
        (CombatOutcome.SmallWound, "The stiletto touches your forehead, and the blood obscures your vision. "),
        (CombatOutcome.SmallWound, "A quick thrust pinks your left arm, and blood starts to trickle down. "),
        (CombatOutcome.SmallWound, "The thief strikes at your wrist, and suddenly your grip is slippery with blood. "),
        (CombatOutcome.SmallWound, "The thief draws blood, raking his stiletto across your arm. "),
        (CombatOutcome.SmallWound, "The stiletto flashes faster than you can follow, and blood wells from your leg. "),

        (CombatOutcome.Fatal, "Finishing you off, the thief inserts his blade into your heart. "),
        (CombatOutcome.Fatal, "The stiletto severs your jugular.  It looks like the end. "),
        (CombatOutcome.Fatal,
            "The thief bows formally, raises his stiletto, and with a wry grin, ends the battle and your life. "),
        (CombatOutcome.Fatal, "The thief comes in from the side, feints, and inserts the blade into your ribs. "),
        (CombatOutcome.Fatal,
            "The thief knocks you out. Forgetting his essentially genteel upbringing, he cuts your throat. "),
        (CombatOutcome.Fatal,
            "Shifting in the midst of a thrust, the thief knocks you unconscious with the haft of his stiletto. A pragmatist, he dispatches you as a threat to his livelihood. ")
    ];

    public ThiefCombatEngine() : this(new RandomChooser())
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
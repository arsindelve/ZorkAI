using ZorkOne.Command;

namespace ZorkOne.Item;

public class Troll : ItemBase, ICanBeExamined, ITurnBasedActor
{
    private readonly TrollCombatEngine _engine = new();

    public bool IsDead { get; set; }

    public override string[] NounsForMatching => ["troll"];

    public override string NeverPickedUpDescription => ExaminationDescription;

    public override string InInventoryDescription => ExaminationDescription;

    public override string CannotBeTakenDescription =>
        "The troll spits in your face, grunting \"Better luck next time\" in a rather barbarous accent.";

    public string ExaminationDescription =>
        "A nasty-looking troll, brandishing a bloody axe, blocks all passages out of the room. ";

    public string? Act(IContext context)
    {
        if (IsDead)
            return null;

        return _engine.Attack(context);
    }
}

internal enum CombatOutcome
{
    Miss,
    SmallWound,
    DropWeapon,
    Stun,
    Fatal
}


internal class TrollCombatEngine
{
    private readonly List<(CombatOutcome outcome, string text)> _haveWeaponOutcomes =
    [
        (CombatOutcome.Miss, "The troll's swing almost knocks you over as you barely parry in time."),
        (CombatOutcome.DropWeapon,
            "The axe hits your {weapon} and knocks it spinning."),
        (CombatOutcome.DropWeapon,
            "The troll swings, you parry, but the force of his blow knocks your {weapon} away.")
    ];

    // He's stunned
    //  (CombatOutcome.Miss, "The troll stirs, quickly resuming a fighting stance."),

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
        (CombatOutcome.Fatal,
            "The flat of the troll's axe hits you delicately on the head, knocking you out. The troll scratches his head ruminatively:  Might you be magically protected, he wonders? Conquering his fears, the troll puts you to death. "),

        (CombatOutcome.Miss, "The troll swings; the blade turns on your armor but crashes broadside into your head."),
        (CombatOutcome.Miss, "The troll swings his axe, but it misses."),
        (CombatOutcome.SmallWound, "The troll swings his axe, and it nicks your arm as you dodge. "),
        (CombatOutcome.SmallWound, "The flat of the troll's axe skins across your forearm. "),
        (CombatOutcome.Stun, "The troll's mighty blow drops you to your knees. "),
        (CombatOutcome.Miss, "You stagger back under a hail of axe strokes. ")
    ];

    private readonly Random _rand = new();

    public string Attack(IContext context)
    {
        if (context is not ZorkIContext zorkContext)
            throw new ArgumentException();

        var fatal = false;

        var possibleOutcomes = zorkContext.HasWeapon ? _outcomes.Union(_haveWeaponOutcomes).ToList() : _outcomes;
        var attack = possibleOutcomes[_rand.Next(possibleOutcomes.Count)];

        switch (attack.outcome)
        {
            case CombatOutcome.Miss:
                break;

            case CombatOutcome.DropWeapon:
            {
                var weapon = zorkContext.GetWeapon();
                if (weapon is null)
                    // this should not happen based on logic above.
                    break;
                context.Drop(weapon);
                attack.text = attack.text.Replace("{weapon}", weapon.Name);
                break;
            }
            case CombatOutcome.SmallWound:
                if (zorkContext.LightWoundCounter > 0)
                    fatal = true;
                else
                    zorkContext.LightWoundCounter = 30;
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
}

// https://github.com/PDP-10/zork/blob/134871aea9cab2cf91982efad038091d416dddf8/dung.mud#L904
// https://github.com/PDP-10/zork/blob/134871aea9cab2cf91982efad038091d416dddf8/dung.mud#L1119

// It appears that that last blow was too much for you. I'm afraid you are dead.
// Almost as soon as the troll breathes his last breath, a cloud of sinister black fog envelops him, and when the fog lifts, the carcass has disappeared. Your sword is no longer glowing.

// A quick stroke, but the troll is on guard. 
// The troll takes a fatal blow and slumps to the floor dead.
// The haft of your sword knocks out the troll. (KO)
// You charge, but the troll jumps nimbly aside .
// A good stroke, but it's too slow; the troll dodges.
// Your sword misses the troll by an inch.
// The troll is knocked out! (KO)
// The troll is confused and can't fight back. The troll slowly regains his feet.
// Clang! Crash! The troll parries
// You are still recovering from that last blow, so your attack is ineffective.
// A good slash, but it misses the troll by a mile.
// The troll is battered into unconsciousness. (KO)
// The fatal blow strikes the troll square in the heart:  He dies. (Fatal)
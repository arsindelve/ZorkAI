using ZorkOne.ActorInteraction;

namespace ZorkOne.Item;

public class Troll : ContainerBase, ICanBeExamined, ITurnBasedActor
{
    private readonly TrollCombatEngine _trollAttackEngine = new();

    public bool IsUnconscious { get; set; }

    public bool IsDead { get; set; }

    public override string[] NounsForMatching => ["troll"];

    public override string NeverPickedUpDescription => ExaminationDescription;

    public override string InInventoryDescription => ExaminationDescription;

    public override string CannotBeTakenDescription =>
        "The troll spits in your face, grunting \"Better luck next time\" in a rather barbarous accent. ";

    public string ExaminationDescription => IsUnconscious
        ? "An unconscious troll is sprawled on the floor. All passages out of the room are open."
        : !HasItem<BloodyAxe>()
            ? "The troll, disarmed, cowers in terror, pleading for his life in the guttural tongue of the trolls. "
            : "A nasty-looking troll, brandishing a bloody axe, blocks all passages out of the room. ";

    public string? Act(IContext context)
    {
        if (IsDead || IsUnconscious || !HasItem<BloodyAxe>())
            return null;

        return _trollAttackEngine.Attack(context);
    }

    // TODO: Give the axe back to the troll: The troll scratches his head in confusion, then takes the axe.
    // TODO: Give other stuff to the troll: The troll, who is not overly proud, graciously accepts the gift and, being for the moment sated, throws it back. Fortunately, the troll has poor control, and the sword falls to the floor. He does not look pleased.

    public override void Init()
    {
        StartWithItemInside<BloodyAxe>();
    }
}

// https://github.com/PDP-10/zork/blob/134871aea9cab2cf91982efad038091d416dddf8/dung.mud#L904
// https://github.com/PDP-10/zork/blob/134871aea9cab2cf91982efad038091d416dddf8/dung.mud#L1119

// It appears that that last blow was too much for you. I'm afraid you are dead.
// Almost as soon as the troll breathes his last breath, a cloud of sinister black fog envelops him, and when the fog lifts, the carcass has disappeared. Your sword is no longer glowing.

// A quick stroke, but the troll is on guard. 
// The troll takes a fatal blow and slumps to the floor dead. (Fatal)
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
// Your sword crashes down, knocking the troll into dreamland. (KO)
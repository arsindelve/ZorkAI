namespace ZorkOne.Item;

public class Troll : ItemBase, ICanBeExamined
{
    public override string[] NounsForMatching => ["troll"];
    
    public string ExaminationDescription => "A nasty-looking troll, brandishing a bloody axe, blocks all passages out of the room. ";
    
    public override string NeverPickedUpDescription => ExaminationDescription;

    public override string InInventoryDescription => ExaminationDescription;

    public override string? CannotBeTakenDescription =>
        "The troll spits in your face, grunting \"Better luck next time\" in a rather barbarous accent.";
}

// It appears that that last blow was too much for you. I'm afraid you are dead.
// Almost as soon as the troll breathes his last breath, a cloud of sinister black fog envelops him, and when the fog lifts, the carcass has disappeared. Your sword is no longer glowing.

// The axe crashes against the rock, throwing sparks!
// The troll's axe removes your head. (Fatal)
// The troll's axe barely misses your ear.
// The troll's swing almost knocks you over as you barely parry in time.
// The axe gets you right in the side. Ouch!
// The troll swings his axe, but it misses.
// The troll swings, you parry, but the force of his blow knocks your sword away. (Drop Sword)
// The troll hits you with a glancing blow, and you are momentarily stunned. (Stun)
// The flat of the troll's axe hits you delicately on the head, knocking you out. The troll scratches his head ruminatively:  Might you be magically protected, he wonders? Conquering his fears, the troll puts you to death. (Fatal)
// The troll swings; the blade turns on your armor but crashes broadside into your head.
// The troll swings his axe, but it misses.
// The troll swings his axe, and it nicks your arm as you dodge.
// The troll's mighty blow drops you to your knees. (Stuns)
// The troll stirs, quickly resuming a fighting stance.
// You stagger back under a hail of axe strokes. (Stuns)

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
namespace Model;

/// <summary>
/// This is your verb thesaurus. Enjoy responsibly. 
/// </summary>
public static class Verbs
{
    /// <summary>
    ///     The "pick it up" family. Kept here as the single source of truth so the take/drop processor,
    ///     the pronoun resolver, and the "them" antecedent tracking all agree on what counts as taking.
    /// </summary>
    public static readonly string[] TakeVerbs = ["take", "get", "grab", "pick up", "hold", "acquire", "snatch"];

    /// <summary>
    ///     The "put it down" family. A short list today (the engine only acts on "drop"), but centralized
    ///     so adding a synonym updates the processor and pronoun scoping together rather than drifting.
    /// </summary>
    public static readonly string[] DropVerbs = ["drop"];

    public static readonly string[] DrinkVerbs = ["swallow", "sip", "drink", "consume", "gulp", "guzzle", "slurp"];

    /// <summary>
    ///     The "go for a swim" family (ZIL: <c>SYNONYM SWIM BATHE WADE</c>). Used by the river-bank
    ///     locations (Shore, Sandy Beach) that expose the local-global river/water objects.
    /// </summary>
    public static readonly string[] SwimVerbs = ["swim", "bathe", "wade"];
    public static readonly string[] FixVerbs = ["fix", "repair", "seal", "patch", "stop", "block", "mend", "plug"];
    public static readonly string[] ApplyVerbs = ["use", "apply", "stick", "put", "place", "spread", "smear", "press"];
    public static readonly string[] PushVerbs = ["push", "press", "activate", "toggle", "click", "flip", "switch", "depress"];
    public static readonly string[] TypeVerbs = ["type", "punch", "key", "press"];

    public static readonly string[] MoveVerbs =
        ["move", "go", "walk", "run", "travel", "proceed", "head", "stroll", "escape", "march", "wander", "venture",
            "jog", "trek", "hike"];

    public static readonly string[] KillVerbs =
        ["kill", "attack", "defeat", "destroy", "murder", "stab", "punch", "fight", "slay", "strike", "hit", "beat",
            "assault"];
    public static readonly string[] BreakVerbs =
    [
        "break", "smash", "shatter", "mung", "vandalize", "destroy", "crack", "bust", "demolish", "wreck",
        "crush", "pulverize", "splinter", "ruin", "deface", "mangle", "trash", "obliterate", "annihilate"
    ];
    public static readonly string[] CloseVerbs = ["close", "shut", "bar", "lower", "cover", "latch", "fasten"];
    public static readonly string[] OpenVerbs = ["open", "lift", "pry", "unbar", "raise", "uncover", "unlatch", "unseal"];
    public static readonly string[] GiveVerbs =
        ["give", "offer", "transfer", "present", "provide", "hand", "donate", "deliver", "pass", "feed"];

    /// <summary>
    ///     The "show it to someone" family. Distinct from <see cref="GiveVerbs" /> — showing keeps the
    ///     object in your hand (the ZIL syntax is <c>SHOW OBJECT (HAVE) TO OBJECT</c>), whereas giving
    ///     transfers it. Deliberately excludes "present" (a give synonym) so the two verbs don't collide.
    /// </summary>
    public static readonly string[] ShowVerbs = ["show", "display"];
    public static readonly string[] ThrowVerbs =
        ["throw", "toss", "launch", "hurl", "fling", "heave", "chuck", "lob", "pitch", "cast", "sling"];

    public static readonly string[] LookVerbs = ["look", "peek", "peer", "check", "observe", "glance"];

    public static readonly string[] TouchVerbs =
        ["touch", "rub", "feel", "press", "stroke", "pat", "tap", "poke", "brush", "caress", "prod", "nudge"];

    /// <summary>
    ///     The "look at / examine" family. Distinct from <see cref="LookVerbs" /> (which is about
    ///     glancing/observing and is used for things like "look through the crack"). These are the
    ///     synonyms a player reaches for when they want to inspect a specific object. Always pair this
    ///     with a noun match — the bare "look" room-description command is handled earlier by the
    ///     global command factory and never reaches here.
    /// </summary>
    public static readonly string[] ExamineVerbs =
        ["examine", "look at", "look", "look into", "inspect", "x", "view", "study", "peek", "peer"];

    public static readonly string[] SayVerbs =
    [
        "say", "yell", "shout", "utter", "scream", "mumble", "whisper", "speak", "declare", "state", "announce", "tell",
        "holler", "exclaim", "proclaim", "chant", "recite"
    ];

    /// <summary>
    ///     The "jump / leap" family — a player flinging themselves into the air. Used both for fatal
    ///     leaps (e.g. Canyon View and the Dome Room in Zork, the rift in Planetfall) and survivable
    ///     ones (e.g. Up A Tree). Centralized here so every location recognizes the same synonyms.
    /// </summary>
    public static readonly string[] JumpVerbs = ["jump", "leap"];
}

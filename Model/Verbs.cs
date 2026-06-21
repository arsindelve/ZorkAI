namespace Model;

/// <summary>
/// This is your verb thesaurus. Enjoy responsibly. 
/// </summary>
public static class Verbs
{
    public static readonly string[] DrinkVerbs = ["swallow", "sip", "drink", "consume"];
    public static readonly string[] FixVerbs = ["fix", "repair", "seal", "patch", "stop", "block", "mend", "plug"];
    public static readonly string[] ApplyVerbs = ["use", "apply", "stick", "put", "place", "spread", "smear", "press"];
    public static readonly string[] PushVerbs = ["push", "press", "activate", "toggle", "click"];
    public static readonly string[] TypeVerbs = ["type", "punch", "key", "press"];

    public static readonly string[] MoveVerbs =
        ["move", "go", "walk", "run", "travel", "proceed", "head", "stroll", "escape"];

    public static readonly string[] KillVerbs = ["kill", "attack", "defeat", "destroy", "murder", "stab", "punch"];
    public static readonly string[] CloseVerbs = ["close", "shut", "bar", "lower"];
    public static readonly string[] OpenVerbs = ["open", "lift", "pry", "unbar", "raise"];
    public static readonly string[] GiveVerbs = ["give", "offer", "transfer", "present", "provide"];
    public static readonly string[] ThrowVerbs = ["throw", "toss", "launch", "hurl", "fling", "heave", "chuck", "lob"];

    public static readonly string[] LookVerbs = ["look", "peek", "peer", "check", "observe", "glance"];

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
        ["say", "yell", "shout", "utter", "scream", "mumble", "whisper", "speak", "declare", "state", "announce", "tell"];

    /// <summary>
    ///     The "jump / leap" family — a player flinging themselves into the air. Used both for fatal
    ///     leaps (e.g. Canyon View and the Dome Room in Zork, the rift in Planetfall) and survivable
    ///     ones (e.g. Up A Tree). Centralized here so every location recognizes the same synonyms.
    /// </summary>
    public static readonly string[] JumpVerbs = ["jump", "leap"];
}
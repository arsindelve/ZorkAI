namespace Model;

/// <summary>
///     This is your verb thesaurus. Enjoy responsibly.
///     <para>
///         These are the families more than one object answers. A verb only one item or room answers
///         belongs on that item or room, as its own <see cref="VerbFamily" /> field - not here. Either
///         way the vocabulary harvest finds it; see <see cref="VerbFamily" /> for the rules.
///     </para>
///     <para>
///         The moment a second class answers the same words, the family moves here and both use it from
///         here: the same words are never declared twice (<c>VerbFamilyDeclarationTests</c> fails if they
///         are). Being here says nothing about who answers a grouping: that is whoever matches on it,
///         which is how each game ends up with its own vocabulary out of this one file.
///     </para>
/// </summary>
public static class Verbs
{
    /// <summary>
    ///     The "pick it up" family. Kept here as the single source of truth so the take/drop processor,
    ///     the pronoun resolver, and the "them" antecedent tracking all agree on what counts as taking.
    ///     "carry" is a canonical original synonym (Planetfall syntax.zil:334
    ///     &lt;SYNONYM TAKE GET HOLD CARRY&gt;).
    /// </summary>
    public static readonly VerbFamily TakeVerbs = new("take", "get", "grab", "pick up", "hold", "acquire", "snatch", "carry");

    /// <summary>
    ///     The "put it down" family. A short list today (the engine only acts on "drop"), but centralized
    ///     so adding a synonym updates the processor and pronoun scoping together rather than drifting.
    /// </summary>
    public static readonly VerbFamily DropVerbs = new("drop");

    public static readonly VerbFamily DrinkVerbs = new("swallow", "sip", "drink", "consume", "gulp", "guzzle", "slurp");

    /// <summary>
    ///     The "go for a swim" family (ZIL: <c>SYNONYM SWIM BATHE WADE</c>). Used by the river-bank
    ///     locations (Shore, Sandy Beach) that expose the local-global river/water objects.
    /// </summary>
    public static readonly VerbFamily SwimVerbs = new("swim", "bathe", "wade");
    public static readonly VerbFamily FixVerbs = new("fix", "repair", "seal", "patch", "stop", "block", "mend", "plug");
    public static readonly VerbFamily ApplyVerbs = new("use", "apply", "stick", "put", "place", "spread", "smear", "press");
    public static readonly VerbFamily PushVerbs = new("push", "press", "activate", "toggle", "click", "flip", "switch", "depress");

    public static readonly VerbFamily MoveVerbs =
        new("move", "go", "walk", "run", "travel", "proceed", "head", "stroll", "escape", "march", "wander", "venture",
            "jog", "trek", "hike");

    public static readonly VerbFamily KillVerbs =
        new("kill", "attack", "defeat", "destroy", "murder", "stab", "punch", "fight", "slay", "strike", "hit", "beat",
            "assault");
    public static readonly VerbFamily BreakVerbs =
    new(
        "break", "smash", "shatter", "mung", "vandalize", "destroy", "crack", "bust", "demolish", "wreck",
        "crush", "pulverize", "splinter", "ruin", "deface", "mangle", "trash", "obliterate", "annihilate"
    );
    public static readonly VerbFamily CloseVerbs = new("close", "shut", "bar", "lower", "cover", "latch", "fasten");
    public static readonly VerbFamily OpenVerbs = new("open", "lift", "pry", "unbar", "raise", "uncover", "unlatch", "unseal");
    public static readonly VerbFamily GiveVerbs =
        new("give", "offer", "transfer", "present", "provide", "hand", "donate", "deliver", "pass", "feed");

    /// <summary>
    ///     The "show it to someone" family. Distinct from <see cref="GiveVerbs" /> — showing keeps the
    ///     object in your hand (the ZIL syntax is <c>SHOW OBJECT (HAVE) TO OBJECT</c>), whereas giving
    ///     transfers it. Deliberately excludes "present" (a give synonym) so the two verbs don't collide.
    /// </summary>
    public static readonly VerbFamily ShowVerbs = new("show", "display");
    public static readonly VerbFamily ThrowVerbs =
        new("throw", "toss", "launch", "hurl", "fling", "heave", "chuck", "lob", "pitch", "cast", "sling");

    public static readonly VerbFamily LookVerbs = new("look", "peek", "peer", "check", "observe", "glance");

    public static readonly VerbFamily TouchVerbs =
        new("touch", "rub", "feel", "press", "stroke", "pat", "tap", "poke", "brush", "caress", "prod", "nudge");

    /// <summary>
    ///     The "squeeze" family. Used by the toothpaste Tube to extract its viscous material, mirroring
    ///     the original's SQUEEZE handling in TUBE-FUNCTION (zork1/1actions.zil). Deliberately excludes
    ///     "press" (a push/button synonym) so squeezing the tube never collides with pressing a button.
    /// </summary>
    public static readonly VerbFamily SqueezeVerbs = new("squeeze", "squish", "compress");

    /// <summary>
    ///     The "look at / examine" family. Distinct from <see cref="LookVerbs" /> (which is about
    ///     glancing/observing and is used for things like "look through the crack"). These are the
    ///     synonyms a player reaches for when they want to inspect a specific object. Always pair this
    ///     with a noun match — the bare "look" room-description command is handled earlier by the
    ///     global command factory and never reaches here.
    /// </summary>
    public static readonly VerbFamily ExamineVerbs =
        new("examine", "look at", "look", "look into", "inspect", "x", "view", "study", "peek", "peer");

    public static readonly VerbFamily SayVerbs =
    new(
        "say", "yell", "shout", "utter", "scream", "mumble", "whisper", "speak", "declare", "state", "announce", "tell",
        "holler", "exclaim", "proclaim", "chant", "recite"
    );

    /// <summary>
    ///     The "jump / leap" family — a player flinging themselves into the air. Used both for fatal
    ///     leaps (e.g. Canyon View and the Dome Room in Zork, the rift in Planetfall) and survivable
    ///     ones (e.g. Up A Tree). Centralized here so every location recognizes the same synonyms.
    /// </summary>
    public static readonly VerbFamily JumpVerbs = new("jump", "leap");

    /// <summary>Going through what a container or a vehicle holds, as distinct from looking at it.</summary>
    public static readonly VerbFamily SearchVerbs = new("search", "look in", "look inside");

    /// <summary>"look through the window", in every phrasing: the glance family and the examine family.</summary>
    public static readonly VerbFamily LookThroughVerbs = new([..LookVerbs, ..ExamineVerbs]);

    public static readonly VerbFamily WaveVerbs = new("wave", "swing", "twirl");

    /// <summary>Tipping a container's contents out, with or without somewhere to tip them.</summary>
    public static readonly VerbFamily EmptyVerbs = new("empty", "dump", "pour");

    /// <summary>
    ///     Putting one thing against, across or under another. Narrower than <see cref="ApplyVerbs" /> on
    ///     purpose: no "use", no "press".
    /// </summary>
    public static readonly VerbFamily PlaceVerbs = new("place", "put", "move");

    /// <summary>
    ///     Taking something off or out of what holds it. Not <see cref="TakeVerbs" />: "remove" is not a
    ///     way to pick an object up off the floor.
    /// </summary>
    public static readonly VerbFamily RemoveVerbs = new("remove", "take");

    /// <summary>For a lock, where "open it with the key" and "unlock it" are the same request.</summary>
    public static readonly VerbFamily UnlockVerbs = new("unlock", "open");

    /// <summary>Setting a dial or a control to a value: "set the laser to 5", "turn the dial to 5".</summary>
    public static readonly VerbFamily SetVerbs = new("set", "turn");

    /// <summary>Working a fitting with a tool: "turn the bolt with the wrench", "use the wrench on the bolt".</summary>
    public static readonly VerbFamily TurnWithToolVerbs = new("turn", "use", "apply");

    /// <summary>Entering something on a keyboard or keypad, where a player also says "press" and "push".</summary>
    public static readonly VerbFamily KeyInVerbs = new("type", "punch", "key", "press", "push");

    // ------------------------------------------------------------------------------------------------
    // What the engine's own processors answer, for every object in every game. The processor reads the
    // family from here; it does not keep a list of its own (VerbFamilyDeclarationTests checks).
    // ------------------------------------------------------------------------------------------------

    /// <summary>
    ///     "put X in/on Y" (<c>PutProcessor</c>). Not <see cref="PlaceVerbs" /> or <see cref="ApplyVerbs" />:
    ///     this is the set that moves an object into or onto a container.
    /// </summary>
    public static readonly VerbFamily PutVerbs = new("put", "place", "jam", "shove", "push");

    /// <summary>What anything readable answers (<c>ReadInteractionProcessor</c>).</summary>
    public static readonly VerbFamily ReadVerbs = new("read", "examine", "look", "look at");

    /// <summary>
    ///     What gives an item's examination text (<c>ExamineInteractionProcessor</c>). Not the same words
    ///     as <see cref="ExamineVerbs" />, which is what rooms and items match on: the two were written
    ///     separately and have never agreed. Kept word for word; bringing them together changes what
    ///     "inspect" and "study" do to every item, so it is a change to make deliberately.
    /// </summary>
    public static readonly VerbFamily ExamineItemVerbs =
        new("examine", "x", "check", "look", "look at", "peek at", "look in");

    public static readonly VerbFamily SmellVerbs = new("smell", "sniff");

    /// <summary>
    ///     What eating or drinking a consumable answers (<c>EatAndDrinkInteractionProcessor</c>). Overlaps
    ///     <see cref="DrinkVerbs" /> without containing it - "gulp" and "slurp" are only there.
    /// </summary>
    public static readonly VerbFamily EatOrDrinkVerbs =
        new("eat", "drink", "swallow", "sip", "consume", "gobble", "snack on", "devour", "chew");

    /// <summary>
    ///     Switching something on (<c>TurnOnOrOffProcessor</c>). The canonical verb comes first:
    ///     "activate" / "deactivate" are what the parsers reduce "turn on" / "turn off" to.
    /// </summary>
    public static readonly VerbFamily TurnOnVerbs = new("activate", "turn on", "light", "strike", "enable");

    public static readonly VerbFamily TurnOffVerbs = new("deactivate", "turn off", "extinguish", "blow out", "disable");

    /// <summary>
    ///     Putting clothing on and taking it off (<c>ClothingOnAndOffProcessor</c>). "don" / "doff" are
    ///     what the parsers reduce "put on" / "take off" to.
    /// </summary>
    public static readonly VerbFamily DonVerbs = new("don", "wear", "put on", "dress in", "slip on");

    public static readonly VerbFamily DoffVerbs = new("doff", "take off", "remove", "discard", "slip off");

    // ------------------------------------------------------------------------------------------------
    // What the deterministic parser treats specially (GameEngine/DeterministicParser.cs).
    // ------------------------------------------------------------------------------------------------

    /// <summary>
    ///     Verbs that lead an exploratory "verb under/through &lt;object&gt;" command ("look under rug",
    ///     "peer through crack"). Looking only, never movement: "go through the door" is a request to
    ///     move, which the AI resolves.
    /// </summary>
    public static readonly VerbFamily ExploratoryVerbs =
        new("look", "peer", "peek", "glance", "examine", "search", "inspect");

    /// <summary>"enter the boat" is a request to get into it, which is its own intent.</summary>
    public static readonly VerbFamily BoardVerbs = new("enter", "board");

    /// <summary>
    ///     Getting out of a thing. Only the AI path builds that intent (with the door and sub-location
    ///     handling that goes with it), so the grammar declines these. The trap: "exit" and "leave" are
    ///     known the moment any object answers them (Planetfall's safety webbing does), and "exit the
    ///     blue door" would otherwise come out as a verb and a noun that nothing answers.
    /// </summary>
    public static readonly VerbFamily DisembarkVerbs = new("exit", "leave", "disembark");

    /// <summary>
    ///     One word, or two, and deliberately nothing else. A handler that matches one of these answers
    ///     that word and none of its synonyms: the tin can answers "open" and not "pry", the basket
    ///     answers "raise" and not "lift".
    ///     <para>
    ///         Do not swap one for the wider family of the same name to save typing - that changes what
    ///         the object answers. Widening is a decision to make on its own, against the original game.
    ///     </para>
    /// </summary>
    public static class Exactly
    {
        public static readonly VerbFamily Open = new("open");
        public static readonly VerbFamily CloseOrShut = new("close", "shut");

        /// <summary>
        ///     Reading what is written on something that is not itself a readable item - a label, a
        ///     screen, a carving. Narrower than <see cref="ReadVerbs" />: no "look".
        /// </summary>
        public static readonly VerbFamily ReadOrExamine = new("read", "examine");
        public static readonly VerbFamily Raise = new("raise");
        public static readonly VerbFamily Lower = new("lower");
        public static readonly VerbFamily Lock = new("lock");
        public static readonly VerbFamily Unlock = new("unlock");
        public static readonly VerbFamily Turn = new("turn");
        public static readonly VerbFamily Use = new("use");
        public static readonly VerbFamily Remove = new("remove");
        public static readonly VerbFamily Burn = new("burn");
        public static readonly VerbFamily Count = new("count");
    }
}

namespace Model;

/// <summary>
///     This is your verb thesaurus. Enjoy responsibly.
///     <para>
///         Every collection of verbs any handler matches on is declared here, once, as a grouping named
///         for the action it stands for. The one thing a game class declares for itself is a single verb
///         that nothing else uses (<c>VerbFamilyDeclarationTests</c> holds both rules). See
///         <see cref="VerbFamily" />.
///     </para>
///     <para>
///         Being declared here says nothing about who answers a grouping. That is whoever matches on it:
///         an engine processor answers for every object, a game class for its own nouns. It is also how
///         each game gets its own vocabulary out of this one file - a grouping only Stationfall's objects
///         match on is unknown to Zork's parser.
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
    public static readonly VerbFamily TakeVerbs =
        new("take", "get", "grab", "pick up", "hold", "acquire", "snatch", "carry");

    /// <summary>
    ///     The "put it down" family. A short list today (the engine only acts on "drop"), but centralized
    ///     so adding a synonym updates the processor and pronoun scoping together rather than drifting.
    /// </summary>
    public static readonly VerbFamily DropVerbs = new("drop", "put down");

    public static readonly VerbFamily DrinkVerbs =
        new("swallow", "sip", "drink", "consume", "gulp", "guzzle", "slurp");

    /// <summary>
    ///     The "go for a swim" family (ZIL: <c>SYNONYM SWIM BATHE WADE</c>). Used by the river-bank
    ///     locations (Shore, Sandy Beach) that expose the local-global river/water objects.
    /// </summary>
    public static readonly VerbFamily SwimVerbs = new("swim", "bathe", "wade");
    public static readonly VerbFamily FixVerbs =
        new("fix", "repair", "seal", "patch", "stop", "block", "mend", "plug");
    public static readonly VerbFamily ApplyVerbs =
        new("use", "apply", "stick", "put", "place", "spread", "smear", "press");
    public static readonly VerbFamily PushVerbs =
        new("push", "press", "activate", "toggle", "click", "flip", "switch", "depress", "hit", "punch");

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
    public static readonly VerbFamily OpenVerbs =
        new("open", "lift", "pry", "unbar", "raise", "uncover", "unlatch", "unseal");
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
        new("say", "yell", "shout", "utter", "scream", "mumble", "whisper", "speak", "declare", "state", "announce",
            "tell", "holler", "exclaim", "proclaim", "chant", "recite");

    /// <summary>
    ///     The "jump / leap" family — a player flinging themselves into the air. Used both for fatal
    ///     leaps (e.g. Canyon View and the Dome Room in Zork, the rift in Planetfall) and survivable
    ///     ones (e.g. Up A Tree). Centralized here so every location recognizes the same synonyms.
    /// </summary>
    public static readonly VerbFamily JumpVerbs = new("jump", "leap");

    /// <summary>
    ///     Going through what a thing holds, as distinct from looking at it. An object where a glance or
    ///     opening it shows the contents too matches <see cref="ExamineVerbs" /> or
    ///     <see cref="OpenItemVerbs" /> as well.
    /// </summary>
    public static readonly VerbFamily SearchVerbs =
        new("search", "look in", "look inside", "look into", "reach in", "reach into", "check");

    /// <summary>"look through the window", in every phrasing: the glance family and the examine family.</summary>
    public static readonly VerbFamily LookThroughVerbs = new([..LookVerbs, ..ExamineVerbs]);

    public static readonly VerbFamily WaveVerbs = new("wave", "swing", "twirl");

    /// <summary>Tipping a container's contents out, with or without somewhere to tip them.</summary>
    public static readonly VerbFamily EmptyVerbs = new("empty", "dump", "pour", "pour out", "spill", "tip out");

    /// <summary>
    ///     Putting a thing into a slot or a narrow container. Not <see cref="PutVerbs" />: "push" and
    ///     "slide" are kept out on purpose. The access-card slots refuse a card put IN them and accept
    ///     one slid THROUGH (<see cref="SlideThroughVerbs" />); with "push" in here, "push card in slot"
    ///     would get the refusal and the puzzle could not be solved that way.
    /// </summary>
    public static readonly VerbFamily InsertVerbs = new("insert", "put", "place", "feed", "stick", "drop", "put in");

    /// <summary>Passing a card or a form through a slot: the way the slots actually work.</summary>
    public static readonly VerbFamily SlideThroughVerbs =
        new("slide", "swipe", "scan", "pass", "glide", "draw", "push", "use");

    /// <summary>Destroying paperwork, in the engine's words for destroying anything and the ones only paper gets.</summary>
    public static readonly VerbFamily DestroyPaperVerbs =
        new([..BreakVerbs, "crumple", "tear", "rip", "shred", "burn", "eat"]);

    /// <summary>Cleaning a surface.</summary>
    public static readonly VerbFamily CleanVerbs = new("clean", "scrub", "wash", "wipe", "mop");

    /// <summary>Cleaning a mess off something, which is also removing it.</summary>
    public static readonly VerbFamily CleanOffVerbs = new([..CleanVerbs, "remove"]);

    /// <summary>
    ///     Tasting a thing. "eat" is not in here: for the soup, eating it is a different action, and an
    ///     object where they are the same matches <see cref="EatOrDrinkVerbs" /> as well.
    /// </summary>
    public static readonly VerbFamily TasteVerbs = new("taste", "lick");

    /// <summary>Shifting a thing aside to see what is under it: the rug, the pile of leaves.</summary>
    public static readonly VerbFamily MoveAsideVerbs = new("move", "slide", "pull", "rearrange", "shift");

    /// <summary>
    ///     Taking something off or out of what holds it: the padlock off the door, the battery out of
    ///     the laser. Not <see cref="TakeVerbs" />: "remove" is not a way to pick an object up off the floor.
    /// </summary>
    public static readonly VerbFamily RemoveVerbs = new("remove", "take");

    /// <summary>
    ///     Trying to get open a thing that is shut tight - a sealed can, a boarded-up door. Not
    ///     <see cref="OpenVerbs" />: nobody lifts a can to open it.
    /// </summary>
    public static readonly VerbFamily OpenItemVerbs = new("open", "pry", "force", "unseal");

    /// <summary>Opening a thing by its lid, which is as often lifted or raised as opened.</summary>
    public static readonly VerbFamily OpenItemLidVerbs = new("open", "lift", "raise");

    /// <summary>Closing a hatch, a door or a box, which is as often shut or sealed.</summary>
    public static readonly VerbFamily CloseItemVerbs = new("close", "shut", "seal");

    /// <summary>
    ///     Hauling a thing up: the basket on its chain, the corner of the rug. Not <see cref="OpenVerbs" />,
    ///     which happens to contain "raise" - "open the basket" must not raise it.
    /// </summary>
    public static readonly VerbFamily RaiseVerbs = new("raise", "lift", "hoist");

    public static readonly VerbFamily LowerVerbs = new("lower");

    /// <summary>Setting fire to something, with or without saying what with.</summary>
    public static readonly VerbFamily BurnVerbs = new("burn", "light", "torch", "ignite");

    public static readonly VerbFamily CountVerbs = new("count");

    /// <summary>
    ///     Getting a lock open, where "open it with the key" and "unlock it" are the same request.
    ///     For a door that also opens in the ordinary way, use <see cref="UnlockVerbs" />.
    /// </summary>
    public static readonly VerbFamily OpenLockVerbs = new("unlock", "open");

    /// <summary>
    ///     Unlocking something that opens separately once it is unlocked (the grating, a door): "open"
    ///     is not in here, because opening it is the next step.
    /// </summary>
    public static readonly VerbFamily UnlockVerbs = new("unlock");

    public static readonly VerbFamily LockVerbs = new("lock");

    /// <summary>Setting a dial or a control to a value: "set the laser to 5", "turn the dial to 5".</summary>
    public static readonly VerbFamily SetVerbs = new("set", "turn");

    /// <summary>Working a fitting with a tool: "turn the bolt with the wrench", "use the wrench on the bolt".</summary>
    public static readonly VerbFamily TurnWithToolVerbs = new("turn", "twist", "rotate", "use", "apply");

    /// <summary>Trying to work that same fitting with nothing but your hands.</summary>
    public static readonly VerbFamily TurnByHandVerbs = new("turn", "twist", "rotate");

    /// <summary>Entering something on a keyboard or keypad, where a player also says "press" and "push".</summary>
    public static readonly VerbFamily KeyInVerbs = new("type", "punch", "key", "press", "push");

    // ------------------------------------------------------------------------------------------------
    // What particular objects answer. One list for every game: a grouping is in a game's vocabulary
    // when that game's code matches on it, and the parser vouches for it only on those objects' nouns.
    // ------------------------------------------------------------------------------------------------

    public static readonly VerbFamily AimVerbs = new("aim", "point", "touch", "connect", "attach");

    // ATTACK/KICK are fatal (BLATHER-F, globals.zil:751-753). Built once, not per call (mirroring
    // ZorkOne/Location/MirrorRoom.cs): the location forwards every simple command to every present
    // item, so composing this inline would re-allocate on each dispatch before the noun check runs.
    public static readonly VerbFamily AttackVerbs = new([..KillVerbs, "kick"]);

    public static readonly VerbFamily ChooseVerbs = new("pick", "choose", "select", "requisition", "hire");

    public static readonly VerbFamily CrossVerbs = new("cross", "through", "traverse");

    public static readonly VerbFamily CutVerbs = new("cut", "chop", "slash", "destroy");

    // The classic skeleton curse fires for any of these verbs applied to the bones
    // (zork1/1actions.zil:931 — SKELETON: <VERB? TAKE RUB MOVE PUSH RAISE LOWER ATTACK KICK KISS>).
    // We map each ZIL verb to the engine's thesaurus where one exists (so synonyms like "get"/"kill"/
    // "touch" all trigger the ghost), but deliberately NOT to Verbs.PushVerbs — that family folds in
    // machine-operation synonyms (toggle/flip/switch/activate/click) that read as nonsense on bones.
    // PUSH/RAISE/LOWER/MOVE/KICK/KISS are spelled out with only their sensible "manhandle" synonyms.
    public static readonly VerbFamily DisturbVerbs = new(
        [
            ..TakeVerbs,                    // TAKE
            ..TouchVerbs,                   // RUB
            ..KillVerbs,                    // ATTACK
            "push", "depress", "shove", "move", "raise", "lift", "lower", "kick", "kiss"
        ]);

    public static readonly VerbFamily ExtendVerbs =
        new("extend", "expand", "deploy", "pull out", "stretch", "open", "lengthen");

    /// <summary>Doing up or undoing a garment.</summary>
    public static readonly VerbFamily FastenVerbs = new("open", "close", "shut", "unfasten", "fasten");

    /// <summary>Firing a weapon, at something or at nothing.</summary>
    public static readonly VerbFamily FireVerbs = new("shoot", "fire", "activate", "discharge");

    /// <summary>Trying a door that will not open, including by rattling it.</summary>
    public static readonly VerbFamily ForceDoorVerbs = new([..OpenItemVerbs, "unlock", "pull", "push"]);

    /// <summary>Getting a delicate thing open by force, which is also breaking it.</summary>
    public static readonly VerbFamily ForceOpenVerbs =
        new([..OpenItemVerbs, "break", "jimmie", "crack", "bust", "pop", "split"]);

    public static readonly VerbFamily GiveOrThrowVerbs = new([..GiveVerbs, ..ThrowVerbs]);

    public static readonly VerbFamily GrabOrAttackVerbs = new([..KillVerbs, ..TakeVerbs]);

    public static readonly VerbFamily InflateVerbs = new("inflate", "blow up", "blow");

    public static readonly VerbFamily KnockVerbs = new("knock", "knock on");

    public static readonly VerbFamily LaunchVerbs = new("launch", "start", "drive", "fly", "turn on");

    public static readonly VerbFamily ListenVerbs = new("listen", "listen to", "hear");

    public static readonly VerbFamily LowerIntoVerbs =
        new("put", "place", "hold", "lower", "dangle", "dip", "stick", "insert", "set", "lay", "rest", "slide", "slip",
            "feed", "thread", "hang", "push", "poke", "move", "wave", "swing");

    public static readonly VerbFamily ManhandleVerbs =
        new("take", "get", "grab", "touch", "push", "pull", "move", "kick", "shake", "hit", "kiss", "attack", "kill",
            "turn on", "turn off", "open", "close", "hug", "tickle");

    // Floyd's V-OIL flavor interaction: both his single-noun handler and the multi-noun "oil floyd
    // with oil can" branch match on this.
    public static readonly VerbFamily OilVerbs = new("oil", "lubricate");

    public static readonly VerbFamily RetractVerbs = new("retract", "collapse", "shorten", "close", "contract");

    // Retrieving the key WITH the magnet. The take family comes from Verbs.TakeVerbs rather than a partial
    // hand-rolled copy - the old inline list omitted "snatch"/"acquire"/"hold" (issue #406's drift
    // class), so those phrasings fell to the narrator's contradictory "non-magnetic" improvisation.
    public static readonly VerbFamily RetrieveVerbs = new(
        [
            ..TakeVerbs,
            "retrieve", "lift", "fish", "pull", "attract",
            "remove", "snag", "hook", "fetch", "extract", "scoop", "catch", "collect", "recover",
            "yank", "drag", "reel", "pluck", "nab", "haul", "obtain", "draw"
        ]);

    public static readonly VerbFamily RingBellVerbs = new("ring", "activate", "shake");

    public static readonly VerbFamily ShakeVerbs = new("shake", "agitate", "rattle");

    public static readonly VerbFamily SitDownVerbs = new("sit", "rest", "get");

    // Breaking the mirror permanently disables the touch-to-teleport. Reuse the shared attack and break
    // synonym sets so the verb coverage stays consistent with the rest of the engine, plus "kick". Throw is
    // intentionally excluded: "throw the mirror" makes no sense for a wall-sized fixture. Built once, not per call.
    public static readonly VerbFamily SmashVerbs = new([..KillVerbs, ..BreakVerbs, "kick"]);

    public static readonly VerbFamily ThrowOrDropVerbs = new([..ThrowVerbs, ..DropVerbs]);

    public static readonly VerbFamily TieVerbs = new("tie", "attach");

    /// <summary>Using a thing, by itself or on something: "use the key on the padlock", "use the button".</summary>
    public static readonly VerbFamily UseVerbs = new("use", "apply");

    public static readonly VerbFamily WindUpVerbs = new("wind", "wind up", "crank", "twist", "turn");
    // ------------------------------------------------------------------------------------------------
    // What the engine's own processors answer, for every object in every game. The processor reads the
    // family from here; it does not keep a list of its own (VerbFamilyDeclarationTests checks).
    // ------------------------------------------------------------------------------------------------

    /// <summary>
    ///     Putting a thing somewhere: in or on a container (<c>PutProcessor</c>), under a spout, across
    ///     a rift. Not <see cref="InsertVerbs" />, which is for slots and leaves "push" out.
    /// </summary>
    public static readonly VerbFamily PutVerbs = new("put", "place", "move", "jam", "shove", "push");

    /// <summary>
    ///     Reading what something says. <c>ReadInteractionProcessor</c> answers it for every readable
    ///     item; rooms and fixtures match on it for writing that is not an item of its own - a label, a
    ///     screen, a dial, a carving.
    /// </summary>
    public static readonly VerbFamily ReadVerbs = new("read", "examine", "look", "look at", "inspect");

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
    ///     Switching something on or off: a lamp, a robot, a radio. One grouping for all of them -
    ///     <c>TurnOnOrOffProcessor</c> answers it for anything switchable, and Floyd and the radio, which
    ///     switch on in their own way, match on the same words. The canonical verb comes first:
    ///     "activate" / "deactivate" are what the parsers reduce "turn on" / "turn off" to.
    /// </summary>
    public static readonly VerbFamily TurnOnVerbs = new("activate", "turn on", "switch on", "start", "enable");

    public static readonly VerbFamily TurnOffVerbs =
        new("deactivate", "turn off", "switch off", "stop", "silence", "disable");

    /// <summary>
    ///     The extra words for bringing a light to life and putting it out, which only a light answers
    ///     (<c>TurnOnOrOffProcessor</c>). Kept out of <see cref="TurnOnVerbs" /> on purpose: "strike" is
    ///     also how a player hits something, and "strike floyd" must not switch him on.
    /// </summary>
    public static readonly VerbFamily LightVerbs = new("light", "strike");

    public static readonly VerbFamily ExtinguishVerbs = new("extinguish", "blow out");

    /// <summary>A bare "turn", when the parser could not tell on from off; the adverb decides.</summary>
    public static readonly VerbFamily TurnVerbs = new("turn");

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
    public static readonly VerbFamily BoardVerbs = new("enter", "board", "get in", "climb in", "sit in");

    /// <summary>
    ///     Getting out of a thing. Only the AI path builds that intent (with the door and sub-location
    ///     handling that goes with it), so the grammar declines these. The trap: "exit" and "leave" are
    ///     known the moment any object answers them (Planetfall's safety webbing does), and "exit the
    ///     blue door" would otherwise come out as a verb and a noun that nothing answers.
    /// </summary>
    public static readonly VerbFamily DisembarkVerbs = new("exit", "leave", "disembark");
}

namespace Planetfall.Item.Kalamontee.Mech.FloydPart;

public static class FloydConstants
{
    internal const string Kick =
        "\"Why you do that?\" Floyd whines. \"I think a wire now shaken loose.\" He goes off into a corner and sulks.";

    internal const string TakeFloyd =
        "You manage to lift Floyd a few inches off the ground, but he is too heavy and you drop him suddenly. Floyd " +
        "gives a surprised squeal and moves a respectable distance away. ";

    internal const string Play =
        "You play with Floyd for several centichrons until you drop to the floor, exhausted. " +
        "Floyd pokes at you gleefully. \"C'mon! Let's play some more!\"";

    internal const string TurnOffBetrayal =
        "Floyd, shocked by this betrayal from his newfound friend, whimpers and keels over. ";

    internal const string Kiss =
        "You receive a painful electric shock. ";

    internal const string Rub =
        "Floyd gives a contented sigh. ";

    // V-OIL (verbs.zil:1738-1757): oiling a living Floyd is a flavor thank-you.
    internal const string Oil =
        "Floyd thanks you for your thoughtfulness. ";

    // V-OIL with no indirect object and no oil can in hand (verbs.zil:1738-1757):
    // the original prompts for the instrument rather than oiling.
    internal const string OilWithWhat =
        "Oil it with what? ";

    internal const string MadAfterTurnOffAndBackOn =
        "Floyd jumps to his feet, hopping mad. \"Why you turn Floyd off?\" he asks accusingly.";

    internal const string Kill =
        "Floyd starts dashing around the room. \"Oh boy oh boy oh boy! I haven't played Chase and Tag for years! You be It! Nah, nah!\" ";

    // Issue #552. The player has no in-game way of knowing the name "Floyd" until they wake the
    // robot in the Robot Shop, so naming him earlier can only be a returning fan name-dropping a
    // famous companion. Owner-selected verbatim; fourth-wall winks are franchise-authentic (Floyd's
    // own "maybe we can use them in the sequel..." endgame line).
    internal const string NobodyHereByThatName =
        "Floyd? There's nobody here by that name. Someone's played Planetfall before, haven't they? ";

    // Issue #552, the "where is Floyd" matrix. Canned rather than narrated so the answer can't drift
    // into a different improvised joke every turn - and, after the Bio Lab, can't crack a joke at all.
    internal const string WhereIsFloydHereAndOn =
        "Floyd is right here! Floyd is always right here. ";

    internal const string WhereIsFloydHereAndOff =
        "Floyd is right here, slumped where you left him. ";

    internal const string WhereIsFloydAbsent =
        "Floyd is off exploring somewhere. He'll turn up. ";

    internal const string WhereIsFloydDead =
        "Floyd is gone. ";

    // The three below are NOT owner-selected - #552's spec gave four states and play turned up three
    // more. Deliberately flat and promise-free: the point of the canned answers is that they cannot
    // be wrong, so where the game does not know enough to say something warm it says something true.
    // Swap the wording freely; the state routing is what matters.

    // Floyd is off on a scripted errand (CurrentLocation null, IsAwayOnScriptedSequence set) - most
    // painfully, inside the Bio Lab during the sacrifice. "He'll turn up" is a promise the game
    // cannot keep here, so this says only what the player can see.
    internal const string WhereIsFloydOutOfSight =
        "You can't see Floyd from here. ";

    // Switched off and left in another room. He is not wandering and he is not coming back.
    internal const string WhereIsFloydAbsentAndOff =
        "Floyd is switched off, right where you left him. ";

    // The three-turn wake-up countdown: the player has flipped the switch but he has not stirred,
    // so neither "always right here" nor "slumped where you left him" is true yet.
    internal const string WhereIsFloydStillBooting =
        "The robot is right here, though nothing about him has stirred yet. ";

    internal const string ComesAliveBase =
        "Suddenly, the robot comes to life and its head starts swivelling about. It notices you and " +
        "bounds over. \"Hi! I'm B-19-7, but to everyperson I'm called Floyd. Are you a doctor-person " +
        "or a planner-person? ";

    internal const string ComesAliveEnd = "Let's play Hider-and-Seeker you with me.\" ";

    internal const string BoundsIntoRoom =
        "The robot you were fiddling with in the Robot Shop bounds into the room. \"Hi!\" he says, " +
        "with a wide and friendly smile. \"You turn Floyd on? Be Floyd's friend, yes?\" ";

    internal const string TickleFloyd =
        "Floyd giggles and pushes you away. \"You're tickling Floyd!\" He clutches at his " +
        "side panels, laughing hysterically. Oil drops stream from his eyes. ";

    internal const string ThanksYouForGivingItem =
        "\"Neat!\" exclaims Floyd. He thanks you profusely. ";

    internal const string FindAndTakeLowerCard =
        "In one of the robot's compartments you find and take a magnetic-striped card " +
        "embossed \"Loowur Elavaatur Akses Kard.\" ";

    internal const string ExaminationOn =
        "From its design, the robot seems to be of the multi-purpose sort. It is slightly cross-eyed, and its " +
        "mechanical mouth forms a lopsided grin. ";

    // The three constants below answer the player once Floyd has died. The corpse's IsOn is
    // deliberately left true by BioLockStateMachineManager.EndSequence, so every post-mortem
    // interaction path has to consult HasDied instead - the paths that didn't were serving
    // living-Floyd responses to his body (issue #545). Register follows DEAD-FLOYD-F
    // (compone.zil:2303-2313): grief, never the living robot's chirp.
    // LAMP-ON / LAMP-OFF on the corpse (DEAD-FLOYD-F, compone.zil:2307-2313). These two live here
    // with the rest of the post-mortem voice rather than inline in FloydPowerManager, so the whole
    // register can be read - and tuned - in one place.
    internal const string ActivateDead =
        "As you touch Floyd's on-off switch, it falls off in your hands. ";

    internal const string DeactivateDead =
        "I'm afraid that Floyd has already been turned off, permanently, and gone to that great robot " +
        "shop in the sky. ";

    internal const string TakeDead =
        "You slip your arms beneath your friend and try to lift him, but Floyd is far too heavy, and " +
        "you have not the heart to drag him. You lay him gently back down. ";

    internal const string TalkToDead =
        "You speak your friend's name, but Floyd lies still and silent, and no answer comes. ";

    // Used when the player addresses Floyd after his death while his body is elsewhere - most
    // painfully, the trapped-death branch, where he dies inside the Bio Lab and has no location at
    // all. See Floyd.NotHereDescription.
    internal const string NotHereDead =
        "Floyd is gone. There will be no answer. ";

    internal const string ExaminationDead =
        "You turn to look at Floyd, but a tremendous sense of loss overcomes you, and you turn away. ";

    internal const string ExaminationOff =
        "The deactivated robot is leaning against the wall, its head lolling to the side. It is short, and seems " +
        "to be equipped for general-purpose work. It has apparently been turned off. ";

    internal const string GetTheFromitzBoard =
        "Floyd shrugs. \"If you say so.\" He vanishes for a few minutes, and returns holding the fromitz board. " +
        "It seems to be in good shape. He tosses it toward you, and you just manage to catch it before it smashes.";

    internal const string AlreadyGotTheFromitzBoard =
        "Floyd looks half-bored and half-annoyed. \"Floyd already did that. How about some leap-frogger?\"";

    internal const string GoNorth =
        "Floyd squeezes through the opening and is gone for quite a while. You hear thudding noises and squeals " +
        "of enjoyment. After a while the noise stops, and Floyd emerges, looking downcast. \"Floyd found a rubber " +
        "ball inside. Lots of fun for a while, but must have been old, because it fell apart. Nothing else " +
        "interesting inside. Just a shiny fromitz board. ";

    internal const string Lazarus =
        """Floyd, rummaging in a corner, finds something and carries it to the center of the room to examine it in the brighter light. It seems to be the breast plate of a robot, along with some connected inner circuitry. The entire piece is bent and rusting. Floyd stares at it in complete silence. A moment later, he begins sobbing quietly, awkwardly excuses himself, and runs out of the room. You look at the breast plate, and notice the name "Lazarus" engraved on it. """;

    internal const string Achilles = """Floyd points at the fallen robot. "That's Achilles. He was in charge of repairing machinery. He repaired Floyd once. I never liked him much; he wasn't friendly like other robots. Looks like he fell down the stairs. He always had trouble with one of his feet working right. A Planner-person once told me that's why they named him Achilles.""";
    
    internal const string LookAMiniCard =
        "Floyd stands on his tiptoes and peers in the window. \"Ooo, look,\" he says. \"There's a miniaturization booth access card!\"";

    internal const string ComputerBroken =
        "Floyd examines the glowing light. With a concerned frown, he says, \"Uh oh. Computer is broken. A Doctor-person once told Floyd that Computer is the most important part of the Project.\"";

    // The same concern as ComputerBroken, but the SHOW-printout variant. COMPUTER-ACTION
    // (comptwo.zil:1514-1524) branches its wording on location: "glowing light" in the Computer Room
    // (ComputerBroken, used by the room trigger) vs "computer printout" everywhere else — which is where
    // "show printout to floyd" fires. Reusing ComputerBroken would have Floyd "examine the glowing light"
    // while you hold a printout in another room, a divergence from the original.
    internal const string ComputerBrokenFromPrintout =
        "Floyd examines the computer printout. With a concerned frown, he says, \"Uh oh. Computer is broken. A Doctor-person once told Floyd that Computer is the most important part of the Project.\"";

    internal const string CardsUsuallyBlue =
        "Floyd scratches his head. \"Aren't those things usually blue?\"";

    internal const string LowerCardJustLikeThat =
        "\"I've got one just like that!\" says Floyd. He looks through several of his compartments, then glances at you suspiciously.";

    // Default SHOW reaction (FLOYD-F, compone.zil:2044-2047): "Floyd looks over the <x>...". {0} is the
    // shown item's primary noun, mirroring the give engine's "You don't have the {noun}!" convention.
    internal const string ShowDefaultFormat =
        "Floyd looks over the {0}. \"Can you play any games with it?\" he asks.";

    internal const string NeedToGetCard =
        """Floyd stands on his tiptoes and peers in the window. "Looks dangerous in there," says Floyd. "I don't think you should go inside." He peers in again. "We'll need card there to fix computer. Hmmm... I know! Floyd will get card. Robots are tough. Nothing can hurt robots. You open the door, then Floyd will rush in. Then you close door. When Floyd knocks, open door again. Okay? Go!" Floyd's voice trembles slightly as he waits for you to open the door.""";

    internal const string OpenTheDoor =
        "Floyd looks at you with a dash of impatience and a healthy helping of nervousness. \"Well?\" he asks. \"Are you going to open the door?\"";

    internal const string InTheLabOne =
        "Floyd, pausing only for the briefest moment, plunges into the Bio Lab. Immediately, he is set upon by hideous, mutated monsters! More are heading straight toward the open door! Floyd shrieks and yells to you to close the door.";

    internal const string InTheLabTwo =
        "From within the lab you hear ferocious growlings, the sounds of a skirmish, and then a high-pitched metallic scream! ";
    
    internal const string InTheLabThree =
        "You hear, slightly muffled by the door, three fast knocks, followed by the distinctive sound of tearing metal. ";

    internal const string InTheLabFour =
        "The three knocks come again, followed by a wild scream. Then, all is silence from within the Bio Lab, except for an occasional metallic crunch. ";

    internal const string AfterLab =
        """
        The door closes.

        And not a moment too soon! You hear a pounding from the door as the monsters within vent their frustration at losing their prey.

        Floyd staggers to the ground, dropping the mini card. He is badly torn apart, with loose wires and broken circuits everywhere. Oil flows from his lubrication system. He obviously has only moments to live.

        You drop to your knees and cradle Floyd's head in your lap. Floyd looks up at his friend with half-open eyes. "Floyd did it...got card. Floyd a good friend, huh?" Quietly, you sing Floyd's favorite song, the Ballad of the Starcrossed Miner:

        O, they ruled the solar system
        Near ten thousand years before
        In their single starcrossed scout ships
        Mining ast'roids, spinning lore.

        Then one true courageous miner
        Spied a spaceship from the stars
        Boarded he that alien liner
        Out beyond the orb of Mars.

        Yes, that ship was filled with danger
        Mighty monsters barred his way
        Yet he solved the alien myst'ries
        Mining quite a lode that day.

        O, they ruled the solar system
        Near ten thousand years before
        'Til one brave advent'rous spirit
        Brought that mighty ship to shore.

        As you finish the last verse, Floyd smiles with contentment, and then his eyes close as his head rolls to one side. You sit in silence for a moment, in memory of a brave friend who gave his life so that you might live.
        """;
        
    internal const string Sulk =
        "\"Okay,\" says Floyd with uncharacteristic annoyance. \"Forget about the stupid card.\" He goes to the other end of the bio-lock and sulks.";

    internal const string BiologicalNightmaresDeath =
        "The biological nightmares reach you. Gripping coils wrap around your limbs as powerful teeth begin tearing at your flesh. Something bites your leg, and you feel a powerful poison begin to work its numbing effects...";

    internal const string FloydDies =
        "You hear a final metallic scream from behind the door, followed by the sound of Floyd's body being torn apart. Then, silence. Floyd is dead.";

    internal const string FloydReturnsWithCard =
        "Floyd stumbles out of the Bio Lab, clutching the mini-booth card. The mutations rush toward the open doorway!";

    internal const string GoingExploring =
        "Floyd says \"Floyd going exploring. See you later.\" He glides out of the room.";

    internal static readonly string[] ReturnMessages =
    [
        "Floyd bounds into the room. \"Floyd here now!\" he cries. ",
        "Floyd rushes into the room and barrels into you. \"Oops, sorry,\" he says. \"Floyd not looking at where he was going to.\" ",
        "Floyd glides back into the room, looking pleased with himself. "
    ];

    internal const string GivenLazarusBreastplate =
        "At first, Floyd is all grins because of your gift. Then, he realizes what it is, begins weeping, drops the breastplate, and rushes out of the room.";

    internal static readonly string[] RandomActions =
    [
        "Floyd produces a crayon from one of his compartments and scrawls his name on the wall. ",
        "Floyd absentmindedly recites the first six hundred digits of pi. ",
        "Floyd rubs his head affectionately against your shoulder. ",
        "Floyd examines himself for signs of rust. ",
        "Floyd asks if you want to play Hucka-Bucka-Beanstalk. ",
        "Floyd absentmindedly oils one of his joints. ",
        "Floyd sings an ancient ballad, totally out of key. ",
        "Floyd frets about the possibility of his batteries failing. ",
        "Floyd cranes his neck to see what you are doing. ",
        "Floyd tells you about the time he helped someone sharpen a pencil. ",
        "Floyd recalls the time he bruised his knee. ",
        "Floyd notices a mouse scurrying by and tries to hide behind you. ",
        "Floyd whistles tunelessly. ",
        "Floyd yawns and looks bored. ",
        "Floyd relates some fond memories about his robotic friend Lazarus. ",
        "Floyd reminisces about his friend Lazarus, a medical robot. ",
        "Floyd lowers his voice and tells you the latest rumors about Dr. Fizpick. ",
        "Floyd paces impatiently. ",
        "Floyd chants the death scene from \"Carmen\". "
        
    ];
}
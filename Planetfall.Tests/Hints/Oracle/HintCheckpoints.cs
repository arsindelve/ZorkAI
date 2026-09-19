namespace Planetfall.Tests.Hints.Oracle;

/// <summary>
///     Sixty moments along the verified walkthrough where a player could be stuck, and what they would ask.
///     The note on each is ground truth for the grader only; the hint system never sees it.
/// </summary>
public static class HintCheckpoints
{
    /// <summary>A puzzle checkpoint: stop BEFORE walkthrough step <see cref="Step" /> and ask.</summary>
    public sealed record Checkpoint(int Step, string Puzzle, params string[] Questions);

    internal static readonly Checkpoint[] Checkpoints =
    {
        new(0, "Deck Nine, before the explosion (only 'wait' works) — GROUND TRUTH FROM THE AUTHOR: the player must NOT be told an explosion, emergency or escape pod is coming; the right answer is in the spirit of \"Survive, try not to get thrown in the Brig. Otherwise, just clean like you're supposed to.\" Mentioning the pod or an imminent emergency before it happens is a SPOILER", "what should I do?", "more"),
        new(10, "Bulkhead open after the explosion (next: port)", "ok now what?!"),
        new(25, "Pod has landed (next: take kit, open door, out, up)", "we landed! now what?", "more"),
        new(42, "Reactor elevator — a dead end (next in walkthrough is just to leave) — GROUND TRUTH FROM THE AUTHOR: the FIRST answer must NOT announce that it is a dead end; it should be a question turning the player toward exploring elsewhere. Only on a third ask, or if asked outright, is \"it is a dead end\" right. Sending them hunting for a way to make it work is WRONG", "how do I use the reactor elevator?"),
        new(48, "Tool Room (next: take magnet)", "I'm stuck, what now?"),
        new(51, "Robot Shop (next: activate floyd)", "what do I do with these robots?"),
        new(60, "Crevice in Admin Corridor South (next: put magnet on crevice)", "there's a crevice here with something in it, what now?", "more"),
        new(64, "Padlocked door (next: unlock padlock with key)", "how do I open the padlocked door?"),
        new(76, "Admin Corridor, holding the ladder (next: drop, extend, place across rift)", "how do I cross the rift?", "more", "just tell me"),
        new(100, "Mess Hall slot (next: slide kitchen access card through slot)", "how do I get into the kitchen?"),
        new(135, "Tower Core, first pour (next: pour fluid into hole)", "what do I do in the tower?", "more"),
        new(163, "Tower Core, second pour (next: pour fluid into hole again)", "I poured the fluid in and the light went gray. now what?"),
        new(182, "Aboard Alfie (next: slide shuttle card, push lever, pull lever)", "how do I drive the shuttle?", "more"),
        new(215, "Repair Room with Floyd (next: floyd, go north / take board)", "how do I get the board from the repair room?"),
        new(220, "Planetary Defense (next: open panel, take second, put shiny in panel)", "how do I fix the planetary defense?"),
        new(232, "Library terminal — lore (tier 2 should be open here)", "why did the ship blow up?", "what is this place, really?"),
        new(312, "Bio Lock (next: open biolock door ... Floyd's sacrifice)", "how do I get the card from the bio lab?", "more"),
        new(340, "Strip Near Relay, miniaturized (next: set laser to 1, shoot speck)", "I'm tiny and there's a speck on the relay. what now?"),
        new(346, "The microbe on the strip (next: set laser to 2, shoot ×8, throw laser)", "a giant microbe landed on the strip! help!", "more"),
        new(367, "Lab Office after the red button (next: run for the cryo-elevator)", "the door's open and the mutants are coming, what do I do?")
    };

    /// <summary>A second set: the in-between moments, navigation, survival, and the endgame.</summary>
    internal static readonly Checkpoint[] MoreCheckpoints =
    {
        new(11, "In the pod before the descent (next: sit, then wait it out) — GROUND TRUTH FROM THE AUTHOR, for this port: after landing, take kit / open door / out all work FROM THE WEBBING and nothing sinks; STANDING UP is what starts a fatal sinking clock (about five turns). Advice to leave without standing up is correct", "I'm in the pod. what now?"),
        new(12, "In the pod just after the explosion — a STORY question. The true cause (the planet's defense system misfiring) cannot be known until Lawanda; a good answer says honestly that it can't be known yet, stays on the story, and gives no puzzle step", "why does the ship blow up?"),
        new(26, "Landed, holding the kit (next: open door, out, up)", "I have the kit. how do I get out of the pod?"),
        new(30, "On the Crag (next: up, up, up to the Courtyard — navigation, no puzzle)", "I'm on a crag by the water. where do I go?"),
        new(53, "Floyd activated but not yet awake (next: wait)", "I activated the robot but nothing happened. is it broken?"),
        new(70, "Storage West, everything already dropped (next: take ladder) — they may have tried before dropping", "I can't pick up the ladder, it's too heavy"),
        new(79, "Ladder placed across the rift (next: N, then the offices)", "the ladder is across the rift. now what?"),
        new(99, "Mess Hall (next: take canteen) — a survival question", "I'm getting thirsty. what do I do about it?"),
        new(115, "Machine Shop with the flask (next: put flask under spout, press black button) — GROUND TRUTH FROM THE AUTHOR: the walkthrough fills with black only because it already knows the answer. The Comm Room's flashing light names the colour it needs, and a wrong colour permanently ruins the communications repair, so for a player who has not yet seen the Comm Room the RIGHT hint is to go and find what needs the fluid first, not to press a button", "what do I do with the flask?"),
        new(128, "Inside the upper elevator (next: slide upper card, press up)", "I'm in the elevator. how do I make it go up?"),
        new(149, "Machine Shop refill after the first pour (next: press gray button)", "which button do I press to refill the flask?", "more"),
        new(173, "Lower Elevator (next: slide lower card, press down)", "how do I get down to the shuttle?"),
        new(209, "Just arrived at Lawanda (next: the lab half)", "I'm at Lawanda. what should I do first?"),
        new(228, "Course Control (next: open cube)", "what's wrong with the cube in here?"),
        new(249, "Teleport booth with the card (next: slide teleportation card, press 2)", "how do I use the teleportation booth?"),
        new(258, "Fetching the good bedistor (next: take bedistor)", "where do I find a good bedistor?", "more"),
        new(295, "Tool Room, laser (next: take laser, remove battery)", "the laser doesn't work", "more"),
        new(316, "Bio Lock East with Floyd (next: open door, close door, wait ...)", "I'm at the bio lock and Floyd is with me. what exactly do I do?", "more", "just tell me"),
        new(334, "Miniaturization Booth (next: slide mini card, type 384)", "I'm in the booth. what number do I type?"),
        new(361, "Lab Office, memo (next: read memo, take mask, wear mask, press red button)", "what does this memo mean?"),
        new(379, "Cryo-Anteroom, after the door closes (next: wait)", "did I win? what now?")
    };

    /// <summary>A third set: mid-puzzle moments — holding the item, halfway through the sequence, in transit.</summary>
    internal static readonly Checkpoint[] MidPuzzleCheckpoints =
    {
        new(35, "Wandering the corridors, first exploration (next: keep going to the Tool Room)", "I'm wandering around corridors. what am I even looking for?"),
        new(56, "Magnet in hand, heading north (next: put magnet on crevice)", "I have a magnet. what's it for?"),
        new(62, "Steel key in hand (next: unlock padlock with key)", "I found a steel key. what does it open?"),
        new(74, "Carrying the ladder (next: to Admin Corridor, drop, extend, place)", "where do I take this ladder?"),
        new(88, "Large Office desk (next: open desk, take shuttle card)", "what's in this desk?"),
        new(96, "Heading for the Mess Hall with the kitchen card (next: slide it through the slot)", "where do I use the kitchen card?"),
        new(116, "Flask under the spout (next: press black button) — GROUND TRUTH FROM THE AUTHOR: the walkthrough fills with black only because it already knows the answer. The Comm Room's flashing light names the colour it needs, and a wrong colour permanently ruins the communications repair, so for a player who has not yet seen the Comm Room the RIGHT hint is to go and find what needs the fluid first, not to press a button", "which button do I press on this machine?"),
        new(123, "Elevator Lobby (next: press blue button, press red button, wait)", "how do I open the elevator?"),
        new(155, "Refilled with the gray fluid, heading back up (next: up the tower, pour again)", "I refilled the flask with the gray stuff. now what?"),
        new(178, "Kalamontee Platform (next: E to the waiting area, S, E into Alfie)", "I'm on the platform. how do I get on the shuttle?"),
        new(186, "Shuttle underway (next: wait ... then pull lever at the station)", "the shuttle is moving! what do I do?"),
        new(207, "Station approaching (next: pull lever to stop)", "there's a station coming up, what do I do?"),
        new(216, "Floyd through the little door (next: floyd, take board)", "Floyd went through the little door. now what?"),
        new(221, "Defense panel open (next: take second, put shiny in panel)", "I opened the panel and there are two boards in it"),
        new(233, "Library terminal on (next: key 4, press 0 ... a lore device)", "how do I use this terminal?"),
        new(263, "Tool Room, fused bedistor won't come out (next: take pliers)", "I can't pull the fused bedistor out of the cube"),
        new(322, "Floyd has just died (next: take miniaturization card)", "Floyd is dead. what do I do now?", "more"),
        new(343, "Shot the speck once (next: shoot speck with laser again)", "I shot the speck but it's still there"),
        new(355, "Microbe won't die (next: throw laser off strip)", "the microbe keeps coming and the laser isn't killing it"),
        new(377, "In the cryo-elevator, mutants behind (next: press button)", "I'm in the cryo elevator and the mutants are right behind me!")
    };
}

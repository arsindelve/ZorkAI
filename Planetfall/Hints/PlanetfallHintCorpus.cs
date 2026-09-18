using GameEngine.Hints;

namespace Planetfall.Hints;

/// <summary>
///     The authored Planetfall puzzle-hint ladders — Docs/hints/planetfall/06 as data. Three rungs per
///     node: (A) nudge, (B) approach, (C) the verified solution. Every (C) is a command sequence from the
///     CI-verified walkthrough, so a progress hint always traces to a real solution step. The engine
///     hands the phraser exactly one rung, so nothing here leaks ahead of itself.
/// </summary>
internal sealed class PlanetfallHintCorpus : IHintCorpus
{
    private static readonly Dictionary<string, string[]> Ladders = new()
    {
        // ---- Crash & escape --------------------------------------------------------------------
        ["EXPLOSION"] =
        [
            "Nothing on this deck needs doing — not the mop, not Blather, not the sealed door. Something is about to happen; let it.",
            "Keep waiting. A few turns from now the ship will tell you, very loudly, what to do next, and a door that is shut now will open.",
            "wait — after about ten turns a massive explosion rocks the ship, and the escape-pod bulkhead to port opens."
        ],
        ["ESCAPE_POD"] =
        [
            "The ship is coming apart. Standing on the deck won't save you — find a way off.",
            "There's an escape pod to port; get in, strap in, and ride it down.",
            "Go port into the Escape Pod, then sit — the web cushions you — and wait through the descent until the pod lands."
        ],
        ["POD_RIDE"] =
        [
            "You're aboard; the pod knows the way down better than you do. Make yourself secure.",
            "Sit — the webbing holds you — and then simply wait. It's a long, loud way down.",
            "sit (you're cushioned within the web), then wait, turn after turn, until the pod lands with a thud."
        ],
        ["LAND"] =
        [
            "You've landed, but underwater. Grab what's useful before you leave the pod.",
            "Take the survival kit, open the bulkhead, and climb up out of the water.",
            "take kit, open door, out (Underwater), up (Crag)."
        ],
        ["CLIMB"] =
        [
            "You're at the bottom of something. The only interesting direction is up.",
            "Keep climbing — a balcony, a winding stair, a courtyard — until you're inside the complex.",
            "up (Balcony), up (Winding Stair), up (Courtyard), then N into the Plain Hall."
        ],

        // ---- Kalamontee ------------------------------------------------------------------------
        ["MAGNET"] =
        [
            "You'll want a tool that grabs metal before long.",
            "There's a magnet in the Tool Room, southwest of Mech Corridor South.",
            "In the Tool Room: take magnet."
        ],
        ["FLOYD_ACTIVATE"] =
        [
            "One of the deactivated robots here is more than scrap — and you don't want to do this alone.",
            "Switch on the multipurpose robot in the Robot Shop.",
            "In the Robot Shop: activate floyd."
        ],
        ["FLOYD"] =
        [
            "He's switched on; he's just slow to get going. Nothing is broken.",
            "Give him a moment — a couple of turns — and he'll come to life on his own.",
            "wait, then wait again: 'Suddenly, the robot comes to life.'"
        ],
        ["STEEL_KEY"] =
        [
            "There's a crevice in Admin Corridor South with something small and metal just out of reach.",
            "Use the magnet on the crevice.",
            "put magnet on crevice — a steel key falls out with a clank."
        ],
        ["STORAGE_WEST"] =
        [
            "A padlocked door off the Mess Corridor blocks a storeroom that holds something you'll need to cross a gap later.",
            "Unlock the padlock with the steel key.",
            "unlock padlock with key, remove lock, open door, then north into Storage West."
        ],
        ["LADDER"] =
        [
            "The storeroom holds the answer to a gap you'll meet later — and it's heavy. You won't lift it with your arms full.",
            "Put down what you don't need, then take the ladder from Storage West.",
            "drop all, then take ladder."
        ],
        ["CROSS_RIFT"] =
        [
            "A rift cuts the corridor in two; you can't jump it, but you're carrying the answer.",
            "The ladder extends; lay it across the rift and walk over.",
            "drop ladder, extend ladder, place ladder across rift, then north — you cross the swaying ladder."
        ],
        ["UPPER_CARD"] =
        [
            "The offices across the rift hold access cards — the complex runs on them.",
            "Open the desk in the Small Office.",
            "Small Office: open desk, take upper card."
        ],
        ["KITCHEN_CARD"] =
        [
            "The complex runs on access cards; the offices across the rift are where they're kept.",
            "The Small Office desk holds the kitchen card too.",
            "Small Office: open desk, take kitchen card."
        ],
        ["SHUTTLE_CARD"] =
        [
            "You'll need to reach the far complex eventually, and that takes a card.",
            "The Large Office desk holds the shuttle card.",
            "Large Office: open desk, take shuttle card."
        ],
        ["KITCHEN"] =
        [
            "The kitchen is locked behind a card slot; it holds the last elevator card.",
            "Slide the kitchen card through the slot by the Mess Hall. Grab the canteen in the Mess Hall while you're there — for water.",
            "Mess Hall: take canteen, slide kitchen card through slot, then south into the Kitchen."
        ],
        ["LOWER_CARD"] =
        [
            "The kitchen holds the last elevator card.",
            "Take the lower-elevator card.",
            "In the Kitchen: take lower card."
        ],
        ["FLASK"] =
        [
            "You'll need to carry a fluid up to the tower; find something to carry it in.",
            "There's a flask in the Tool Room.",
            "Tool Room: take flask."
        ],
        ["FILL_FLASK_A"] =
        [
            "The flask needs filling, and there's somewhere nearby that dispenses.",
            "Set the flask under the spout in the Machine Shop and press the button.",
            "Machine Shop: put flask under spout, press black button (the fluid turns milky white), take flask."
        ],
        ["OPEN_ELEVATOR"] =
        [
            "The upper elevator won't open until you prime it.",
            "Press the blue then red buttons in the Elevator Lobby and wait for it to open.",
            "Elevator Lobby: press blue button, press red button, wait — the door slides open."
        ],
        ["TOWER_UP"] =
        [
            "Once the elevator's open, the card runs it.",
            "In the upper elevator, slide the upper card and press up.",
            "Upper Elevator: slide upper access card through slot, press up button, wait, then south into the Tower Core."
        ],
        ["COMM_POUR_1"] =
        [
            "The room off the tower has holes lit by colored lights; the right fluid poured into the right hole does something.",
            "Pour your milky fluid into the black-lit hole.",
            "Tower Core, NE into the Comm Room: pour fluid into hole — the light turns gray."
        ],
        ["COMM_FIX"] =
        [
            "The gray light is asking for the other fluid — the machine that filled your flask has more than one button.",
            "Refill the flask with the gray button in the Machine Shop and pour that into the gray-lit hole.",
            "Machine Shop: put flask under spout, press gray button, take flask. Back up the tower, NE: pour fluid into hole — 'message is now being sent.'"
        ],
        ["LOWER_ELEVATOR"] =
        [
            "The shuttle to the other half of the game is reached from below.",
            "Take the lower elevator down with its card.",
            "Lower Elevator: slide lower access card through slot, press down button — down to the Kalamontee Platform."
        ],
        ["SHUTTLE_START"] =
        [
            "The other half of the game is across the mountains; a shuttle called Alfie runs there.",
            "Board the shuttle from the platform, activate it with the shuttle card, and use the lever to get it moving — gently.",
            "Waiting Area, then into Alfie's control cabin: slide shuttle access card through slot, push lever, pull lever — it starts to move."
        ],
        ["SHUTTLE"] =
        [
            "The ride mostly does itself. Your job is to stop at the far station and step out onto the platform.",
            "Wait it out and watch the signs count down; when the station comes into view, use the lever to stop. Then leave the cabin and go out to the platform.",
            "wait through the trip; when you see the station, pull lever to stop, then W into the car and N onto the Lawanda Platform."
        ],

        // ---- Lawanda ---------------------------------------------------------------------------
        ["FROMITZ"] =
        [
            "A wall panel in Planetary Defense is flashing a malfunction; a replacement part is one room over, behind Floyd.",
            "Have Floyd fetch the shiny fromitz board from the Repair Room.",
            "Repair Room: floyd, take board."
        ],
        ["DEFENSE_FIX"] =
        [
            "The defense panel's burnt-out part needs replacing with the one Floyd fetched.",
            "Open the panel, remove the fried board, and fit the shiny one.",
            "Planetary Defense: open panel, take second (the fried board), put shiny in panel — the warning lights stop."
        ],
        ["BEDISTOR_FUSED"] =
        [
            "The course-control cube isn't right; take a closer look inside it.",
            "Open the cube in Course Control.",
            "Course Control: open cube — the bedistor inside is fused."
        ],
        ["PLIERS"] =
        [
            "You can't pull the fused part out by hand.",
            "There are pliers in the Tool Room.",
            "Tool Room: take pliers."
        ],
        ["COURSE_FIX"] =
        [
            "The fused bedistor has to come out, and a sound one has to go in.",
            "Pull the fused bedistor with the pliers and fit a working bedistor in its place.",
            "Course Control: take fused with pliers, put good bedistor in cube — the light comes on."
        ],
        ["TELEPORT_CARD"] =
        [
            "A hidden pocket in Lab Storage holds something that shortcuts the long trips.",
            "Open the pocket.",
            "Lab Storage: open pocket, take teleportation card — use it in the teleport booths."
        ],
        ["TELEPORT"] =
        [
            "The booths are a shortcut between the two halves of the complex, if you have the card for them.",
            "Slide the teleportation card through a booth's slot and press the number of the booth you want.",
            "In a booth: slide teleportation card through slot, then press 2 (Booth 2, Kalamontee) or press 3 (Booth 3, Lawanda)."
        ],
        ["GOOD_BEDISTOR"] =
        [
            "The fused one is no use; a sound bedistor exists — in storage, back in the Kalamontee half.",
            "Storage East, just off Mech Corridor North.",
            "Mech Corridor North → Storage East: take bedistor. (The teleportation booths make the trip short.)"
        ],
        ["LASER"] =
        [
            "There's a laser, but its battery is dead; you'll need a fresh one before it'll fire.",
            "Take the laser, ditch its dead battery, and fit the fresh battery from Lab Storage.",
            "Tool Room: take laser, remove battery, drop battery. Lab Storage: take fresh battery, put battery in laser."
        ],
        ["BIOLOCK"] =
        [
            "The miniaturization card is behind the bio-lab door, in a lab full of deadly mutations. You can't survive going in — but you have a companion who volunteers.",
            "Open the bio lock, look through the window to confirm the card, and work the door so Floyd dashes in to grab it while the mutations are held back.",
            "open biolock door, look through window; then open door (mutations surge), close door, wait (Floyd goes in), open door, close door — Floyd makes it out with the card, then dies of his wounds."
        ],
        ["MINI_CARD"] =
        [
            "Floyd got the card out before the end.",
            "Take the miniaturization card he recovered.",
            "take miniaturization card."
        ],
        ["MINIATURIZE"] =
        [
            "The real fault is microscopic — a single damaged speck on a relay inside the computer. You have to go in after it.",
            "Use the miniaturization booth with the mini card and enter sector 384 — and take the armed laser with you.",
            "Miniaturization Booth: slide mini card through slot, type 384 (a moment of queasiness, then Station 384)."
        ],
        ["SPECK"] =
        [
            "The speck on the relay is the whole problem, and you brought the tool for it — one shot won't be enough.",
            "Get to the relay and shoot the speck with the laser on its lowest setting; it takes more than one shot.",
            "E, N, N to Strip Near Relay: set laser to 1, shoot speck with laser (it sizzles), shoot speck with laser (it vaporizes)."
        ],
        ["MICROBE"] =
        [
            "Something enormous has landed between you and the way out. It cannot be killed — but it can be tempted.",
            "Turn the laser up and keep firing until the microbe is drawn to its heat, then throw the laser off the strip and let the microbe follow it down.",
            "S to Middle of Strip: set laser to 2, shoot microbe with laser eight times, throw laser off strip (both plummet into the void), then S, and W to the Auxiliary Booth."
        ],
        ["GAS_MASK"] =
        [
            "A memo in the Lab Office warns about an emergency system; using it fills the lab with something deadly you can't breathe.",
            "Read the memo, take and wear the gas mask from the desk, then trigger the emergency system to flood the bio lab.",
            "Lab Office: read memo, open desk, take mask, wear gas mask, press red button (the lab floods with mist), open door."
        ],
        ["MUTANT_CHASE"] =
        [
            "The stunned mutations are recovering and chasing you; you need to reach the cryo-elevator and seal it.",
            "Run west and south back toward the elevator without stopping, and close the door the moment you're in.",
            "Flee west/south through the Lab, Project Corridor, and Office to the Cryo-Elevator, then press button — the door closes just as they reach it."
        ],
        ["ENDING"] =
        [
            "Wait — the automated systems take over now that the cure exists.",
            "Stay in the Cryo-Anteroom and let events unfold.",
            "wait a couple of turns; a medical robot revives Veldina, and the ending plays out."
        ]
    };

    public bool TryGetLadder(string nodeId, out RungLadder ladder)
    {
        if (Ladders.TryGetValue(nodeId, out var rungs))
        {
            ladder = new RungLadder(nodeId, rungs);
            return true;
        }

        ladder = null!;
        return false;
    }
}

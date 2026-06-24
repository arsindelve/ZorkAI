# Zork I Puzzle & Solution Corpus (progress-hint grounding)

The grounding source for Zork **progress hints** — the laddered "what do I do?" content that the
[lore source](05-lore.md) does *not* cover. This is the Zork analog of the Planetfall invisiclues,
**authored from the verified walkthrough** (`ZorkOne.Tests/Walkthrough/WalkthroughTestOne.cs`, the
complete 350-point path) so every solution is correct-by-construction for this port. Built entirely
from in-repo test content — no external corpus, no third-party material.

## How to use this corpus

Each puzzle is pre-laddered into **three rungs** — the laddering layer reveals one rung per "I need
more help", and stops at the rung the player needs:

- **(A) nudge** — vaguest pointer; orients without giving anything away.
- **(B) approach** — names the tool/place/idea; still requires the player to execute.
- **(C) solution** — the exact verified command(s).

IDs match the [structure DAG](01-structure-dag.md) where applicable. `find`/`case` = points for first
taking the treasure / for depositing it in the trophy case (two-stage scoring). Localization decides
*which* puzzle is the player's active blocker; this corpus supplies the rungs for it.

> Authored from the verified path; the command rungs are confirmed against CI. The (A)/(B) framing is
> editorial scaffolding for gentle disclosure, not a second source of truth.

---

## Getting underground

### `HOUSE_ENTRY` — get inside the white house
- **Where:** West/South/Behind House. **Needs:** nothing.
- **A:** The house is boarded up at the front, but go all the way around it.
- **B:** The window at the back of the house can be opened.
- **C:** From Behind House: `open window` → `enter` (into the Kitchen) → `W` to the Living Room. Grab the `lamp` and `sword` here — you'll need both.

### `TRAP_DOOR` — reach the cellar / Great Underground Empire
- **Where:** Living Room. **Needs:** lamp (lit), and notice the rug.
- **A:** Something in the Living Room is covering more than the floor.
- **B:** The oriental rug is hiding something; move it before you look down.
- **C:** `move rug` → `open trap door` → `turn on lantern` → `down`. (The door crashes shut behind you — that's expected.)

### `TROLL` — get past the troll
- **Where:** Troll Room (N of Cellar). **Needs:** the sword from the Living Room.
- **A:** The troll blocks every passage out; you'll have to fight.
- **B:** You took a weapon from above the trophy case — use it.
- **C:** `kill troll with sword` (repeat if he isn't felled the first swing). Then pass through.

---

## Treasures & their puzzles

### `PAINTING` — the painting *(find 4 / case 6)*
- **Where:** Gallery (E from the chasm edge, S of Cellar).
- **A:** A famous work still hangs in the gallery.
- **B:** It's loose on the wall — just take it.
- **C:** From Cellar: `S` → `E` (Gallery) → `take painting`. Deposit later: `put painting in case`.

### `COFFIN` + `SCEPTRE` + `POT_OF_GOLD` — the rainbow chain *(coffin find 10/case 15; sceptre 4/6; gold 10/10)*
- **Where:** Egyptian Room → End of Rainbow (Aragain Falls).
- **A:** The gold coffin is a treasure *and* holds another; and a rainbow you can't yet cross is the key to a third.
- **B:** Carry the coffin to the end of the rainbow, open it for the sceptre, and wave the sceptre at the falls.
- **C:** Reach the Egyptian Room via the Dome Room (`tie rope to railing` → `down` → Torch Room → Temple → Egyptian Room), `take coffin`. Carry it to End of Rainbow, `open coffin`, `take sceptre`, `wave sceptre` → the rainbow solidifies and a pot of gold appears; `take gold`.

### `CRYSTAL_SKULL` — exorcise the spirits *(find 10/case 10)*
- **Where:** Entrance to Hades → Land of the Living Dead.
- **A:** Spirits bar the way to Hades and jeer at you. They must be driven off, ceremonially.
- **B:** You need the brass **bell**, the black **book**, and the **candles** (with a **match** to light them) — and they must be used in the right order.
- **C:** Carry bell (Temple), book + candles (Altar), matches (Dam Lobby). At Entrance to Hades: `ring bell` → `light match` → `light candles with match` → `read book` → the spirits are banished. `S`, `take skull`.

### `TORCH` — the ivory torch *(find 14/case 6)*
- **Where:** Torch Room (below the Dome Room). **Note:** the torch is also a permanent light source — **and an open flame** (see `GAS_ROOM`).
- **A:** A flaming torch sits on a pedestal — a treasure that also lights your way.
- **B:** Just take it; but remember it's *fire*, which matters in one place underground.
- **C:** `take torch`. Deposit it **last** — putting the torch in the case is the final scoring move (see `ENDGAME`).

### `JADE` — the jade figurine *(find 5/case 5)*
- **Where:** Bat Room (coal mine approach).
- **A:** A vampire bat guards this room and will fling you about — unless you smell bad enough.
- **B:** The clove of **garlic** (from the sack in the Kitchen) keeps the bat away. Carry it before entering.
- **C:** With garlic in hand, enter the Bat Room → `take jade`.

### `SAPPHIRE_BRACELET` — *(find 5/case 5)*
- **Where:** Gas Room. **A:** A bracelet lies in the gas room. **B:** Simply take it as you pass through — but mind the open-flame rule below. **C:** `take bracelet` (on your way through the Gas Room).

### `DIAMOND` — coal → diamond machine *(find 10/case 10)*
- **Where:** Machine Room (deep coal mine). **Needs:** coal, the screwdriver, and the torch+screwdriver routed past the gas room (see `GAS_ROOM`).
- **A:** There's a machine down here that transforms what you feed it — and coal to feed it.
- **B:** Put the coal in the machine, close the lid, and turn the switch with the screwdriver.
- **C:** `open lid` → `put coal in machine` → `close lid` → `turn switch with screwdriver` → `open lid` → `take diamond`.

### `GAS_ROOM` — getting the torch past the gas (the famous trap)
- **Where:** Gas Room / Shaft Room. **This is a soft-lock if done wrong** (see [03 soft-locks](03-softlock.md) #2).
- **A:** The gas room reeks of coal gas. Bringing fire in here would be the last thing you do.
- **B:** Don't carry an open flame (the torch, or lit candles/match) through the gas room. Use the **basket** in the Shaft Room and the **lamp** for light instead.
- **C:** In the Shaft Room: `put torch in basket`, `put screwdriver in basket`, `turn on lantern`, then `lower basket`. Retrieve them later from the **Drafty Room** at the bottom of the shaft (via the Timber Room — `drop` your flammables before the squeeze).

### `COINS` + `SKELETON_KEY` — the maze *(coins find 10/case 5; key is a tool)*
- **Where:** the Maze (W of Troll Room).
- **A:** West of the troll is a maze of twisty passages, all alike — with a dead adventurer's remains in it.
- **B:** Navigate to the skeleton; take the **coins** (treasure) and the **skeleton key** (opens the grating to the surface).
- **C:** Path from Troll Room: `W` → `S` → `E` → `up` (skeleton room) → `take coins`, `take key`.

### `CHALICE` (+ recover stolen treasures) — the thief *(chalice find 10/case 5)*
- **Where:** Cyclops Room → Treasure Room (the thief's lair).
- **A:** A cyclops blocks the way up to the thief's lair; the thief himself must be dealt with to get the chalice and anything he's stolen.
- **B:** Frighten the cyclops with a word, then fight the thief with the nasty **knife** — but first let him do you one favor (see `EGG`).
- **C:** In the Cyclops Room: `Ulysses` (he flees, opening the wall), `up` to Treasure Room. After giving him the egg (`EGG`): `kill thief with knife` → `take chalice` (and recover the egg, canary, and anything stolen).

### `EGG` + `CANARY` + `BAUBLE` — don't break the egg *(egg find 5/case 5; canary 6/4; bauble 1/1)*
- **Where:** Up A Tree (forest) → the thief → forest again.
- **A:** A jewel-encrusted egg is in a tree. It's delicate — and it has something inside it.
- **B:** **Do not force the egg open yourself** — you'll ruin it and the canary inside, losing points permanently (see [03](03-softlock.md) #1). Let the **thief** open it for you. Then the canary, wound in the forest, produces one more treasure.
- **C:** `Up` (the tree) → `take egg`. Give it to the thief: `give egg to thief`. After killing the thief, recover the egg + `canary`. Back at Up A Tree: `wind canary` → a brass `bauble` appears below; `down`, `take bauble`.

### `TRUNK_OF_JEWELS` + `TRIDENT` — drain the reservoir *(trunk find 15/case 5; trident 4/11)*
- **Where:** Reservoir / Atlantis Room. **Needs:** the dam drained first (see `DAM`).
- **A:** A trunk of jewels lies in the muddy reservoir bed — but only once the water is gone. Beyond it, Poseidon's trident.
- **B:** Drain the reservoir at the dam, then walk the bed north; the trident is heavy, so manage your load.
- **C:** After `DAM`: Reservoir `take trunk`; Reservoir North `take pump`; Atlantis Room `take trident` (drop something if "your load is too heavy").

### `DAM` — drain Flood Control Dam #3
- **Where:** Dam / Maintenance Room. **Needs:** the wrench (Maintenance), the yellow button pressed.
- **A:** The dam holds back the reservoir; its gates can be opened, but not by the bolt alone.
- **B:** In the Maintenance Room, press the **yellow** button (it activates the bolt), grab the **wrench**, then turn the bolt at the dam.
- **C:** Maintenance: `take wrench`, `take screwdriver`, `press yellow button`. At the Dam: `turn bolt with wrench` → the sluice gates open and the reservoir drains.

### `EMERALD` + `SCARAB` — the river & the buoy *(emerald find 5/case 10; scarab 5/5)*
- **Where:** Frigid River → Sandy Beach / Sandy Cave. **Needs:** the inflatable boat + pump; **no sharp objects** (see [03](03-softlock.md) #4).
- **A:** A folded pile of plastic by the dam base is a boat; the river will carry you to treasures downstream.
- **B:** Inflate it with the pump, but **empty your hands of anything sharp** (sword/knife/screwdriver) or it punctures. Float down, take the buoy, land, and dig the beach.
- **C:** Dam Base: `inflate plastic with pump`, `get in boat`, `launch`, `wait` (×3 down the river), `take buoy`, `E` to Sandy Beach, `leave boat`. `take shovel`, Sandy Cave: `dig in sand with shovel` (×4) → `take scarab`. `open buoy` → `take emerald`.

### `PLATINUM_BAR` — the Loud Room *(find 10/case 5)*
- **Where:** Loud Room (E of Round Room).
- **A:** This room is deafeningly loud; you can't act normally while it echoes.
- **B:** Silence the echo with a single word.
- **C:** In the Loud Room: `echo` → then `take bar`.

---

## Endgame

### `ENDGAME` — all 19 treasures → the Stone Barrow
- **Where:** Living Room (trophy case) → Stone Barrow.
- **A:** With every treasure in the case, the game's final secret reveals itself.
- **B:** Deposit the **torch last**; a map then appears. Follow it from the west.
- **C:** With all treasures deposited, `put torch in case` (score 350) → `take map`. Then West of House → `SW` to Stone Barrow → `W` Inside the Barrow (the bridge to ZORK II/III).

---

## Provenance & coverage

- **Source:** `ZorkOne.Tests/Walkthrough/WalkthroughTestOne.cs` — the complete, CI-verified 350-point
  path. `WalkthroughTestTwo/Three.cs` and `ExploringZorkViaAPI.cs` cover alternate routes/edge cases
  and can extend specific entries (e.g. maze variants) later.
- **Scope:** this is the *progress-hint* corpus (puzzles & solutions). World/lore questions
  ("who was Megaboz?") are the [lore source](05-lore.md)'s job — different intent, different corpus.
- **Relationship to the engine:** localization picks the active puzzle; this corpus supplies its rungs;
  the laddering layer (and/or LLM-2) phrases one rung at a time. Because the (C) rungs are verified
  commands, the grounding guarantee holds — a Zork progress hint can always trace to a real solution
  step, never a confabulation.

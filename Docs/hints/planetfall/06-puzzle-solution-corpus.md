# Planetfall Puzzle & Solution Corpus (progress-hint grounding)

The grounding source for Planetfall **progress hints** — the laddered "what do I do?" content. This is
an **original, in-repo corpus authored from the verified walkthrough**
(`Planetfall.Tests/Walkthrough/WalkthroughTestOne.cs`, the complete 80-point path), so every solution
is correct-by-construction for this port. It is the engine-side replacement for depending on the
external Planetfall invisiclues — same role as the [Zork puzzle corpus](../zorkone/06-puzzle-solution-corpus.md),
same format. (The external invisiclues can still be folded in later as *additional* phrasing/coverage,
but are no longer required to ship progress hints.)

## How to use this corpus

Each puzzle is pre-laddered into **three rungs** — the laddering layer reveals one per "I need more
help" and stops at the rung the player needs:

- **(A) nudge** — vaguest pointer; orients without giving anything away.
- **(B) approach** — names the tool/place/idea; still requires the player to execute.
- **(C) solution** — the exact verified command(s).

IDs match the [puzzle DAG](01-puzzle-dag.md). ★ = optional planetary-system repair · ◆ = mandatory
spine. Score = the walkthrough checkpoint at/after the step. Localization decides *which* puzzle is the
active blocker; this corpus supplies its rungs. **Survival-clock hints (sleep/eat/sickness)** are a
separate category at the end.

> Authored from the verified path; the (C) command rungs are confirmed against CI. The (A)/(B) framing
> is editorial scaffolding for gentle disclosure, not a second source of truth.

---

## Crash & escape (the opening)

### `ESCAPE_POD` — survive the explosion *(score 3)*
- **Where:** Deck Nine → Escape Pod. **Needs:** nothing (time-gated).
- **A:** The ship is coming apart. Standing on the deck won't save you — find a way off.
- **B:** There's an escape pod to port; get in, strap in, and ride it down.
- **C:** `port` (into the Escape Pod) → `sit` (the web cushions you) → `wait` through the descent until the pod lands.

### `LAND` — get out of the pod *(score 6)*
- **Where:** Escape Pod → Underwater → Crag. **Needs:** nothing.
- **A:** You've landed, but underwater. Grab what's useful before you leave the pod.
- **B:** Take the survival kit, open the bulkhead, and climb up out of the water.
- **C:** `take kit` → `open door` → `out` (Underwater) → `up` (Crag), then keep climbing up toward the complex (Balcony → Winding Stair → Courtyard).

---

## Kalamontee — getting through the complex

### `MAGNET` — pick up the magnet
- **Where:** Tool Room (SW of Mech Corridor South).
- **A:** You'll want a tool that grabs metal before long.
- **B:** There's a magnet in the Tool Room. **C:** `take magnet`.

### `FLOYD` — wake the robot *(score 8)*
- **Where:** Robot Shop (E of the Machine Shop). **Needs:** nothing.
- **A:** One of the deactivated robots here is more than scrap — and you don't want to do this alone.
- **B:** Activate the multipurpose robot and give him a moment to come to life.
- **C:** `activate floyd` → `wait` a couple of turns until "the robot comes to life." Floyd now follows you and is needed later (the bio lab).

### `STEEL_KEY` — the magnet & the crevice
- **Where:** Admin Corridor South. **Needs:** `MAGNET`.
- **A:** There's a crevice here with something small and metal just out of reach.
- **B:** Use the magnet on the crevice. **C:** `put magnet on crevice` → a steel key falls out (`clank`).

### `STORAGE_WEST` + `LADDER` — open the padlocked door *(score 12)*
- **Where:** Mess Corridor → Storage West. **Needs:** the steel key.
- **A:** A padlocked door blocks a storeroom that holds something you'll need to cross a gap later.
- **B:** Unlock the padlock with the steel key, then take the ladder inside.
- **C:** `unlock padlock with key` → `remove lock` → `open door` → `N` (Storage West) → `take ladder`.

### `CROSS_RIFT` — bridge the rift *(score 16)*
- **Where:** Admin (north end). **Needs:** `LADDER`.
- **A:** A rift cuts the corridor in two; you can't jump it, but you're carrying the answer.
- **B:** The ladder extends; lay it across the rift and walk over.
- **C:** `drop ladder` → `extend ladder` → `place ladder across rift` → `N` (you cross the swaying ladder).

### `UPPER_CARD` / `KITCHEN_CARD` / `SHUTTLE_CARD` — the office desks *(score 17 / 18 / 19)*
- **Where:** Small Office & Large Office (across the rift). **Needs:** `CROSS_RIFT`.
- **A:** The offices hold access cards — the complex runs on them.
- **B:** Open the desks. The small office desk holds the **upper-elevator** and **kitchen** cards; the large office desk holds the **shuttle** card.
- **C:** Small Office: `open desk` → `take upper card`, `take kitchen card`. Large Office: `open desk` → `take shuttle card`.

### `KITCHEN` + `LOWER_CARD` — into the kitchen *(score 23 → 24)*
- **Where:** Mess Hall → Kitchen. **Needs:** `KITCHEN_CARD`.
- **A:** The kitchen is locked behind a card slot; it holds the last elevator card.
- **B:** Slide the kitchen card through the slot, go in, and take the lower-elevator card. (Grab the **canteen** in the Mess Hall while you're here — for water.)
- **C:** Mess Hall `take canteen` → `slide kitchen card through slot` → `S` (Kitchen) → `take lower card`.

### `FLASK` + `FILL_FLASK_A` — fill the flask
- **Where:** Tool Room (flask) → Machine Shop (spout). **Needs:** the flask.
- **A:** You'll need to carry a fluid up to the tower; find something to carry it in and somewhere to fill it.
- **B:** Take the flask, set it under the spout in the Machine Shop, and press the button to dispense.
- **C:** Tool Room `take flask` → Machine Shop `put flask under spout` → `press black button` (fluid turns milky white) → `take flask`.

### `OPEN_ELEVATOR` + `TOWER_UP` — reach the tower *(score 28)*
- **Where:** Elevator Lobby → Upper Elevator → Tower Core. **Needs:** `UPPER_CARD`, the filled flask.
- **A:** The upper elevator won't open until you prime it; then the card runs it.
- **B:** Press the blue then red buttons in the Elevator Lobby and wait for it to open; in the elevator, slide the upper card and press up.
- **C:** Elevator Lobby `press blue button` → `press red button` → `wait` (the door slides open) → `N` Upper Elevator → `slide upper access card through slot` → `press up button` → `wait` → `S` Tower Core.

### `COMM_FIX` ★ — repair communications (the tower fluid puzzle) *(score 34)*
- **Where:** Tower Core (NE, the colored-light holes). **Needs:** `TOWER_UP` + filled flask. **Optional system.**
- **A:** The tower has holes lit by colored lights; pouring the right fluid in does something — but it takes two tries with two different fluids.
- **B:** Pour your milky fluid into the **black-lit** hole (the light turns gray). Go refill the flask using the **gray** button this time, come back, and pour into the now **gray-lit** hole.
- **C:** First trip (`FILL_FLASK_A`, black button): Tower Core `NE` → `pour fluid into hole` (light turns gray). Refill via Machine Shop `press gray button`. Back up: `NE` → `pour fluid into hole` → "message is now being sent."

### `LOWER_ELEVATOR` + `SHUTTLE` — ride to Lawanda *(score 38 → 42)*
- **Where:** Lower Elevator → Kalamontee Platform → Alfie shuttle → Lawanda. **Needs:** `LOWER_CARD`, `SHUTTLE_CARD`.
- **A:** The other half of the game is across the mountains; a shuttle ("Alfie") runs there.
- **B:** Take the lower elevator down to the platform, board the shuttle, activate it with the shuttle card, and drive it with the lever.
- **C:** Lower Elevator `slide lower access card through slot` → `press down button` → to Kalamontee Platform → board Alfie → `slide shuttle access card through slot` → `push lever` → `pull lever` (sets it moving) → `wait` through the trip → `pull lever` to stop → Lawanda.

---

## Lawanda — the lab half

### `FROMITZ` + `DEFENSE_FIX` ★ — repair the meteor defense *(score 48)*
- **Where:** Repair Room (board) → Planetary Defense (panel). **Needs:** `FLOYD`. **Optional system.**
- **A:** A wall panel here is flashing a malfunction; a replacement part is one room over, behind Floyd.
- **B:** Have Floyd fetch the shiny **fromitz board** from the Repair Room, remove the burnt-out part from the Planetary Defense panel, and fit the new one.
- **C:** Repair Room `floyd, take board` → Planetary Defense `open panel` → `take second` (the fried board, leaving an empty socket) → `put shiny in panel` (warning lights stop). `drop fried`.

### `BEDISTOR_FUSED` + `PLIERS` + `COURSE_FIX` ★ — repair course control *(score 54)*
- **Where:** Course Control (cube) + Tool Room (pliers). **Optional system.**
- **A:** The course-control cube has a fused component; you can't pull it out by hand, and you'll need a sound replacement.
- **B:** Get the **pliers**, pull the fused bedistor with them, and fit a working bedistor in its place.
- **C:** Course Control `open cube` (the bedistor is fused) → fetch `PLIERS` (Tool Room) and a good bedistor → `take fused with pliers` → `put good in cube` (the light goes on).

### `TELEPORT_CARD` — the teleportation card
- **Where:** Lab Storage. **A:** A hidden pocket here holds a card that shortcuts the long trips. **B:** Open the pocket. **C:** `open pocket` → `take teleportation` (use it in the teleport booths to skip backtracking).

### `LASER` — arm the laser
- **Where:** Tool Room (laser) → Lab Storage (fresh battery). **Needs:** for `COMPUTER_FIX`.
- **A:** There's a laser, but its battery is dead; you'll need a fresh one before it'll fire.
- **B:** Take the laser, ditch its dead battery, and fit the fresh battery from Lab Storage.
- **C:** Tool Room `take laser` → `remove battery` → `drop battery`. Later, Lab Storage `take fresh battery` → `put battery in laser`.

### `BIOLOCK` + `MINI_CARD` — Floyd and the bio lab *(score 56 → 57)*
- **Where:** Bio Lock (Main Lab → bio lab doors). **Needs:** `FLOYD` alive & present. **This is the game's turning point — Floyd is lost here.**
- **A:** The miniaturization card is behind the bio-lab door, in a lab full of deadly mutations. You can't survive going in — but you have a companion who volunteers.
- **B:** Open the bio lock, look through the window to confirm the card, and work the door so Floyd dashes in to grab it while the mutations are held back.
- **C:** `open biolock door` → reach Bio Lock East, `look through window` (the magnetic-striped card is inside) → `open door` (mutations surge) → `close door` → `wait` (Floyd goes in) → `open door` (mutations rush the doorway) → `close door` (Floyd makes it out — then dies of his wounds). `take miniaturization card`.

### `COMPUTER_FIX` ◆ — cure The Disease (miniaturize & destroy the microbe) *(score 61 → 75)*
- **Where:** Miniaturization Booth → the microbe strip. **Needs:** `MINI_CARD` + charged `LASER`. **Mandatory.**
- **A:** The real fault is microscopic — a single damaged microbe in the computer. You have to go in after it.
- **B:** Use the miniaturization booth with the mini card, enter the damaged sector, set the laser low, and shoot the speck until it's destroyed; then leave via the auxiliary booth.
- **C:** `put battery in laser` (if not done) → Miniaturization Booth `slide mini card through slot` → `type 384` (Station 384, score 61) → `set laser to 1` → `shoot speck with laser` (it sizzles, 63) → `shoot speck with laser` (it vaporizes, 71) → exit `W` to the Auxiliary Booth (75).

### `GAS_MASK` — clear the lab office *(part of the escape)*
- **Where:** Lab Office. **Needs:** `COMPUTER_FIX` done (exit via auxiliary booth).
- **A:** A memo here warns about an emergency system; using it fills the lab with something deadly you can't breathe.
- **B:** Read the memo, take and **wear the gas mask** from the desk, then trigger the emergency system to flood the bio lab.
- **C:** `read memo` → `open desk` → `take mask` → `wear gas mask` → `press red button` (the lab floods with mist) → `open door`.

### `MUTANT_CHASE` — escape to the cryo-elevator *(score 80)*
- **Where:** Lab → Cryo-Elevator. **Needs:** `GAS_MASK`. **Time/chase pressure.**
- **A:** The stunned mutations are recovering and chasing you; you need to reach the cryo-elevator and seal it.
- **B:** Run west and south back toward the elevator without stopping, and close the door the moment you're in.
- **C:** Flee `w`/`s` through the Lab → Project Corridor → Office → Cryo-Elevator (the monsters on your heels the whole way) → `press button` (the door closes just as they reach it).

---

## Endgame

### `ENDING` — revival
- **Where:** Cryo-Anteroom. **Needs:** `MUTANT_CHASE` (i.e. the cure). **The ending varies by which ★ systems you fixed** (Comm / Defense / Course Control) — see the endings table in `PlanetfallContext.cs`.
- **A:** Wait — the automated systems take over now that the cure exists.
- **C:** `wait` a couple of turns; a medical robot revives Veldina, leader of Resida, and the ending plays out (best ending if all three ★ systems were repaired).

---

## Survival-clock hints (a separate category — sleep / eat / sickness)

Not puzzles — these fire on the survival clocks (`Hunger` / `Tired` / `Day`), proactively or on
"why am I X?" (see [01 § survival hints](01-puzzle-dag.md) and [03 soft-locks](03-softlock.md)).

- **Tired / sleep:** *A:* You're flagging; you need rest. *B:* Find a safe place to sleep — a bunk. *C:* Head to the dorm/sleeping quarters and `sleep` there (sleeping in an unsafe spot is risky).
- **Hungry / thirsty:** *A:* You need food and water. *B:* The kitchen and the dispensers have rations; the canteen carries water. *C:* Eat the rations / drink from the filled canteen when the warning escalates.
- **Sick (The Disease):** *A:* You're getting sick and it's getting worse — time matters. *B:* There's experimental medicine that buys time, but the real fix is in the lab. *C:* Take the experimental medicine to slow it; prioritize the cure (`COMPUTER_FIX`). *(Lore — "why am I sick?" — is the [lore source](05-lore.md)'s job.)*

---

## Provenance & coverage

- **Source:** `Planetfall.Tests/Walkthrough/WalkthroughTestOne.cs` — the complete CI-verified 80-point
  path. `WalkthroughBioLock.cs` / `WalkthroughMutantChase.cs` / `WalkthroughDontFixAnything.cs` cover
  the bio-lock and chase sequences and the minimal (no-★) route; they can extend those entries later.
- **Scope:** the *progress-hint* corpus (puzzles & solutions) plus survival nudges. Lore/world
  questions are the [lore source](05-lore.md)'s job — different intent, different corpus.
- **Grounding guarantee:** every (C) rung is a verified command, so a Planetfall progress hint always
  traces to a real solution step — never a confabulation. With this corpus, Planetfall no longer
  depends on the external invisiclues to ship progress hints.

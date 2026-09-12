> # ⛔ STOP — SPOILERS ⛔
>
> **This file solves the game.** It exists only as the ZIL-verified oracle the port's tests are
> written against. If you intend to *play* Stationfall, close this file now and do not read the
> rest of this folder. See `Docs/Stationfall-Port-Plan.md` for a spoiler-free status of the port.

# Stationfall — Full Walkthrough (80 / 80 points)

Complete run to the maximum score and the **Intergalactic Mega-Hero** rank. Synthesized from public
solutions and **verified against the ZIL** in `stationfall-source/`; `file:line` cites mark the
authoritative mechanics. Legend:

- `command` — what the player types (parser nouns/verbs taken from the ZIL `SYNONYM`/`ADJECTIVE`).
- **[ROOM]** — room checkpoint (the ZIL `DESC`, i.e. the status-line name the oracle asserts on).
- **[+N → T]** — score award N, running total T (see the table in [`README.md`](README.md)).
- ⚠ — a trap/timing hazard; ✎ — a ZIL note for the port author.

Stationfall is non-linear and requires several crossings between the **station** and the **village**
(through the iris hatch) and one trip **In Space**. The phase order below is dependency-correct
(nothing is used before it is obtained). Exact *filler* — eating for hunger, sleeping to pass days —
is situational and flagged; the puzzle logic is the ZIL-exact part.

> ✎ **Ordering caveat.** This is a validated synthesis of a known 80-point route re-derived from the
> ZIL, not a byte-for-byte transcript. When the port can actually execute it, the walkthrough test is
> what certifies the precise sequence; treat the ZIL mechanics/citations as the fixed contract and the
> connective navigation as adjustable.

---

## Starting state

You begin in **[Deck Twelve]** of the S.P.S. *Duffy*, wearing the **chronometer** and **Patrol
uniform** (the **ID card** is in its pocket, `ID-RANK 6`), and already carrying all three **forms**
(they are physical feelies — `IN PROTAGONIST`, `ship.zil:36-61`). Nothing needs fetching from the
Forms Storage Room (a red herring). Your companion is chosen at the Robot Pool.

---

## Phase 0 — Depart the *Duffy* → dock at the station  [+5]

```
east                         [Cargo Bay Entrance]
north                        [Robot Pool]
put robot use authorization form in slot     → "Authorization approved. Type the bin number…"
type 3                       (bin 3 = Floyd; ⚠ 1=Rex later crushes you, 2=Helen shreds forms)
south                        [Cargo Bay Entrance]
east                         [Cargo Bay]
open hatch
enter truck                  (Floyd follows you in)
close hatch                  ⚠ must be closed at launch or the cabin vents — you die (ship.zil:1164)
read time                    ← note the number T (the chronometer reading = INTERNAL-MOVES)
enter pilot seat             (Floyd automatically takes the copilot seat — PILOT-SEAT-F ship.zil:1070)
put class three activation form in slot       → "Spacecraft activated. Type in the course heading."
type <COURSE>                ← COURSE = ((T ÷ 50 − 132)² ÷ 4) + 103, integer math (verbs.zil:2136)
wait                         (repeat through the launch/flight; the truck flies itself)
```
After a few turns the station resolves into view and the truck docks: *"Docking bay one is occupied.
Defaulting to bay two,"* gravity engages, a voice whispers **"Stationfall."** **[+5 → 5]**, now in
**[Docking Bay #2]** (`ship.zil:1197-1220`).

```
get up
take kit                     (the survival kit: thermos, gray/orange goo — you need the THERMOS later)
open hatch
exit                         [Docking Bay #2]
east                         [Level Five]  ← the central hub of the Command Module
```
⚠ A **wrong course** leaves you stranded in empty space to suffocate. ✎ The course is recomputed from
`INTERNAL-MOVES` at the instant you `type`, and the chronometer **stops after Day 2** — do this on Day
1. For the test, pin the chronometer start value and compute the course from it.

---

## Phase 1 — Recon: the diary, the note, the fromitz board

Level Five's exits: N Workshop · NE North Junction · E elevator · SE South Junction · W Docking Bay 2
· **Up** Level Four · **Down** Level Six. The **Scientific Sub-Module** is north, through the Tube.

```
ne                           [North Junction]
n                            [North Connection]
n                            [Tube]
n                            [Engineering Lab]
n                            [Engineering Office]
read diary                   ← Prof. Schmidt: the alien dots are read by NONVISUAL (taste) properties;
                               "my pal, the mayor" has the linguistic training (station.zil:3116)
s                            [Engineering Lab]
down                         [Bio Lab]
n                            [Bio Office]
read note                    ← "think of this station as a cell and the pyramid as a mechanized
                               bacteriophage…" (station.zil:3238)
s                            [Bio Lab]
up                           [Engineering Lab]
up                           [Astro Lab]
take twenty-prong board      (for the jammer, much later — station.zil:3157)
down                         [Engineering Lab]
s                            [Tube]
s                            [North Connection]
```
✎ Eat when hungry: `open kit`, `eat orange goo` / `eat gray goo` (the hunger daemon kills you at
level 5 — `globals.zil:1194`).

---

## Phase 2 — Tools + the log + the entry form (station errands before the village)

Collect the drill, detonator, log intel, validation stamp, and prepare the village entry form.

```
# Drill (Paper Recycling Plant, off Printing Plant on Level 7)
s                            [North Junction]
sw                           [Level Five]
down                         [Level Six]
down                         [Printing Plant]
open trash can
take crumpled form           (the Illegal Space Village Entry Form FW-83-Q — station.zil:2731)
nw                           [Paper Recycling Plant]
take drill                   (comes with the SMALL bit installed — too small for the safe)
se                           [Printing Plant]

# Detonator (Main Storage, off Mess Hall on Level 2) — via the ladder
up                           [Level Six]
up                           [Level Five]
up                           [Level Four]
up                           [Level Three]
up                           [Mess Hall]        ⚠ don't drink the COFFEE (poisoned) / touch the FPU (acid)
n                            [Main Storage]
take detonator               (a BLACKENED dud diode is inside it — a red herring, station.zil:1866)
s                            [Mess Hall]

# Iron the entry form (Laundry, off Level Three)
down                         [Level Three]
nw                           [Laundry]
put crumpled form in presser
turn on presser              → crumpled form becomes the ironed VILLAGE-FORM (station.zil:1399)
take form
e                            [Level Three]   ⚠ leave promptly — I-PRESSER slams shut and kills a loiterer

# Validation stamp + log tape (Commander's area, off South Junction on Level Five)
# down the ladder from Level Three to Level Five:
down                         [Level Four]
down                         [Level Five]
se                           [South Junction]
se                           [Commander's Office]
e                            [Commander's Quarters]
look under bed
take stamp                   (the VALIDATION-STAMP — globals.zil:876)
validate form                (stamp the ironed form → VILLAGE-FORM-VALIDATED, station.zil:2759)
take log tape
w                            [Commander's Office]
put log tape in reader
turn on reader
press red button             (repeat ~6-7×; each shows more entries — READ-LOG station.zil:792)
turn off reader              ⚠ the reader overheats and explodes after ~14 turns on (interrupts.zil:435)
```
The log reveals the crucial chain: **the storage-bin key is in the Commander's safe**, and **the
Dome storage bin hides the fuel cells** (and is the only way down to Levels 8–9). It also fingers
**Shady Dan** for the modified ID cards and the alien ship/pyramid in **Docking Bay #1**.

---

## Phase 3 — Cross into the Village  [+6]

The iris hatch is opened by feeding the validated form into the slot at **East Connection** (Military
connector) or **South Connection** (Diplomatic connector).

```
nw                           [South Junction]
n                            [Level Five]  — (routing note: East Connection is reachable via
                                              North Junction → NE → East Junction → E)
ne                           [North Junction]
se                           [East Junction]
e                            [East Connection]
put form in slot             → the iris hatch grinds half-open  **[+6 → 11]**  (globals.zil:742)
```
⚠ The **space suit is too bulky** to pass the half-dilated hatch (`VILLAGE-BOUNDARY-F`,
`village.zil:5`) — you carry it, never wear it, when crossing, and you can't cross *with* it at all;
plan the In-Space trip as its own out-and-back (Phase 5). Crossing drops you into zero-g (the whole
village is `WEIGHTLESSBIT`).

```
w                            [Makeshift Connector]  (through the hatch)
e                            [Broadway]             — the village hub
```

---

## Phase 4 — The Village sweep

Broadway's spokes: N Recruitment · NE Rec Shop · E Field Office · SE Barbershop · S Grocery ·
SW Pet Store · W Makeshift Connector · Down Fortune Teller. The southern half (Main Street, Alley,
Casino, airlock) is reached through the Trading Post / Grimy Passage.

### 4a — Headlamp

```
e                            [Field Office]
take headlamp                (needed for the dark "In Space" area — village.zil:210)
wear headlamp
w                            [Broadway]
```
✎ **Foil sub-puzzle** (do it once you hold the **platinum detector** from the Bank, 4c): in the
**Barbershop**, `turn on detector` (it beeps loudest at the mirror — `interrupts.zil:64`), then
`break mirror` → the mirror shatters and reveals the foil (`MIRROR-F`, `village.zil:277`),
`take foil` **[+4 → …]**. ⚠ The detector overheats; you're forced to drop it after ~100 turns
(`interrupts.zil:78`) — do this promptly and `turn off detector` when done.

### 4b — Pet Store: ostrich nip, the balloon, and cage

```
sw                           [Pet Store]   (from Broadway)
search ceiling               → reveals a ceiling PANEL (globals.zil:314)
open panel
take ostrich nip             **[+3 → …]**  (village.zil:540)
open cage                    → the Arcturian balloon creature + its leash float out (village.zil:385)
```
✎ Keep the **nip in hand** — the ostrich (4d) will follow whoever visibly holds it. Do **not**
`give` the nip to the ostrich (that knocks it out cold).

### 4c — Southern cluster: detector, spray can, coin-box, roulette, suit, boots, ID, the dot-key

```
# reach the southern half via Trading Post
s                            [Grocery]        (Broadway → S)
s                            [Trading Post]
sw                           [Greasy Straw]
sw                           [Main Street]
s                            [Bank]
take platinum detector       (village.zil:1036)   ← now go back and do the Barbershop foil (4a)
n                            [Main Street]
up                           [Mayor's Office]
open book                    → a PAPER flutters out: the taste→meaning key (village.zil:958)
read paper                   ← decodes with the alien dots to "element 78" = platinum (village.zil:1000)
take paper
down                         [Main Street]
se                           [Alley]
sw                           [Pawn Shop]
take spray can               (the "spore" can — lures the balloon; village.zil:1445)
ne                           [Alley]
se                           [Doc Schuster's]     ← the ostrich lives here (village.zil:1687)
                             (lead it out later, holding the nip)
```
**Reprogram the ID card** (needed for the Armory) at **Shady Dan's**:
```
nw                           [Alley]
ne                           [Warehouse]
up                           [Trading Post]
se                           [Shady Dan's]
put card in machine
turn on machine
type 8                       (any rank ≥7 opens security doors; 7=Commander/Captain — village.zil:1862)
take card
```
⚠ **Boots-vs-card:** taking the **magnetic boots** off while the ID card is on you *scrambles the
card permanently* (`SETG ID-SCRAMBLED`, `village.zil:1940`) and there is no un-scramble in the
village. Reprogram the card **before** the In-Space trip and keep the card out of your hands whenever
you doff the boots.

**Roulette → space suit; boots:**
```
nw                           [Trading Post]
e                            [Saloon]
e                            [Casino]
turn wheel                   → opens the Casino's N and Up exits  **[+4 → …]**  (village.zil:734)
up                           [Flophouse]
open locker
take space suit              (village.zil:815)   ⚠ never remove the suit in vacuum = lungs rupture
down                         [Casino]
w                            [Saloon]
w                            [Trading Post]
se                           [Shady Dan's]
down                         [Junk Yard]
take magnetic boots          (village.zil:1920)
w                            [Alley]
```

---

## Phase 5 — In Space (the explosive)  [+3]

The airlock is below the **Warehouse**. Bring **space suit + magnetic boots + lit headlamp**.

```
# to the Warehouse (from Alley)
ne                           [Warehouse]
open inner door
down                         [Airlock]
close inner door
wear suit                    (now safely sealed)
wear boots
turn on headlamp
open outer door              ⚠ this flushes any loose/undropped items (and the ostrich/balloon) into space
down                         [In Space]          **[+3 → …]**  (village.zil:1339; the room is DARK)
```
Sweep the headlamp to find the **explosive** (a pencil of frozen "Liquid Gorzium," FREZONE):
```
take explosive
read label                   (it sublimes over time — keep it COLD)
open bottle                  (the thermos from your kit)
empty bottle
put explosive in bottle
close bottle                 ✎ sealed in the closed thermos it melts 4× slower (interrupts.zil:361)
```
Return through the airlock, reversing carefully:
```
up                           [Airlock]
close outer door
open inner door
remove suit                  ✎ safe here (inner door shut); NEVER in vacuum
turn off headlamp
up                           [Warehouse]
```
⚠ Now remove the boots **without the ID card on you** if you still need the card readable — but the
card is already reprogrammed (Phase 4c), and you won't re-use the ID changer, so scrambling it now is
harmless *as long as you've already opened everything the card needs*. Simplest: keep the boots on
until after the Armory (Phase 7), then doff them. If you doff them now with the card, that's fine —
the reprogrammed rank was already written; you only lose the ability to *re-change* it.

---

## Phase 6 — The dot decode (element 78 = platinum)

You now hold the **paper** (taste-key) from the Mayor's book. Confirm the answer by tasting the alien
dots — the alien ship is on the station, in **Docking Bay #1** (Level 6). (Do this when you're back on
the station; grouped here for narrative.) The two pieces together spell out: reflect the pyramid's
"deadly emanations" with **element 78** — trans-molecular **platinum**, i.e. the **foil**
(`village.zil:975-1002`, `station.zil:2394`).

```
# on the station, Docking Bay #1:
in                           [Alien Ship]
taste dots                   ← the dots encode meaning by taste, not sight (station.zil:2394)
out                          [Docking Bay #1]
```
✎ Floyd is terrified of the **skeleton** in the alien ship (`station.zil:2345`).

---

## Phase 7 — Back on the station: Armory + zapgun  [+5], then the coin  [+5]

Cross back through the iris hatch (`e` from Makeshift Connector, or `n` from Grimy Passage). With the
ID card at rank ≥7:

```
# Armory is off End of Corridor (Level 6), behind a security door
… reach [End of Corridor]  (Level Six → SE)
put card in reader           → the security door opens (ID-RANK > 6 — station.zil:3073)
n                            [Armory]            **[+5 → …]**  (station.zil:1054)
take zapgun                  (7 shots — station.zil:1066)
s                            [End of Corridor]   (the door auto-recloses behind you)
```
Now free the **coin** — cross back to the village, **Loan Shark** (off Alley):
```
… reach [Loan Shark]
shoot box with gun           → the strong box vaporizes, leaving the coin  (village.zil:1619)
take coin                    **[+5 → …]**   (one galakmid — the PX currency, village.zil:1628)
```

---

## Phase 8 — The PX timer (ostrich trick)  [+6]

Back on the station, the **PX** is off North Junction / North Connection. Lead the **ostrich** here
from Doc Schuster's by holding the **nip**.

```
put coin in slot
type 6                       → the All-Purpose Timer klunks into the dispenser but is unreachable
                               by hand (station.zil:286).  (type 9 = Large Drill Bit, optional.)
# with the ostrich in the room (it followed the nip):
put nip in hole              → the ostrich jams its head in and knocks the item out  **[+6 → …]**
                               (OSTRICH-INTO-DISPENSER station.zil:319)
take timer
```

---

## Phase 9 — The M-series hyperdiode (Chapel star)  [+7]

The **seven-pointed star** hangs out of reach in the **Chapel** (Level Three). You reach it by riding
the hydrogen **balloon** up — but only after the **eternal flame** is out, and only in a gravity room.

```
# lead the balloon to the Chapel with the spray can:
#   in each room adjacent to the balloon, `spray can` and it drifts into your room; repeat toward the Chapel
… arrive [Chapel]  (balloon lured just outside)
examine pulpit               → it's openable (station.zil:1577)
open pulpit
turn off switch              → the eternal flame goes out (station.zil:1614)  ⚠ do this BEFORE the
                               balloon enters (a lit chapel expels the balloon)
spray can                    → the balloon enters the Chapel
take leash                   → you rise to the ceiling: HANGING-IN-AIR (village.zil:485)
                               ⚠ not while wearing the space suit or magnetic boots
take star                    **[+7 → …]**  (awards the diode's value — station.zil:1509)
open star
take m hyperdiode            (the genuine DIODE-M — station.zil:1533)
drop leash                   → you settle back to the deck
```

---

## Phase 10 — The medium drill bit (Floyd)  [+3]

The **medium bit** sits in the **heating chamber** in the **Robot Shop** (off South Connection) — too
hot / the opening too small for a human hand. **Floyd** fetches it.

```
… arrive [Robot Shop]   (Floyd with you)
floyd, get medium bit        → Floyd reaches into the chamber and hands it over  (ship.zil:370)
take medium bit              **[+3 → …]**  (station.zil:2530)
```
✎ Oliver, a sleeping newborn robot, is here; Floyd bonds with it (pays off in the ending).

---

## Phase 11 — Blow the safe → the key  [+3, +7]

In the **Commander's Quarters**. You need: DRILL + **medium bit**, DETONATOR with the genuine
**DIODE-M**, the **TIMER**, and the **EXPLOSIVE** (still cold in the thermos).

```
remove small bit from drill
put medium bit in drill
drill safe                   → a pencil-diameter hole (MAKE-HOLE-WITH-DRILL station.zil:996)
drop drill                   (it only drills once)
open detonator
take blackened diode         (remove the dud — it will NOT fire, interrupts.zil:398)
put m diode in detonator
connect timer to detonator
open bottle
take explosive
put explosive in hole        **[+3 → …]**  (fits only a medium/large hole — station.zil:983)
connect explosive to detonator
set timer to 10              → it starts ticking (I-TIMER interrupts.zil:385)
w                            [Commander's Office]   ⚠ get out of the room — an in-room blast kills you
wait                         (until the muffled explosion)
e                            [Commander's Quarters]
open safe
take key                     **[+7 → …]**  (station.zil:1024 — opens the Dome storage bin)
```
⚠ Blowing the safe also queues a station-wide **lights-out** on a later day; and the explosive's
`I-EXPLOSIVE-MELT` timer means you shouldn't dawdle after taking it out of the thermos.

---

## Phase 12 — Survive Plato's attack  [+7]

As the pyramid's influence grows (`ROBOT-EVILNESS` rises each ~1000 millichrons and on every
score/wake — `station.zil:3466`), **Plato** — the bookish library robot, secretly the pyramid's agent
— ambushes you with a stun ray (`I-PLATO-ATTACK`, `station.zil:3481`). When it triggers:

```
floyd, help me               → between the first and second stun shots, Floyd knocks the gun away
                               → you survive  **[+7 → …]**  (requires FLOYD-TOLD — station.zil:3588)
```
✎ This can fire at various points; handle it whenever it happens. It's gated on Floyd still being
"good" enough to intervene — do it before the pyramid fully corrupts him.

---

## Phase 13 — Sleep to advance the days  [+3]

Days advance **only by sleeping** (`WAKING-UP`, `globals.zil:1059`); the sleep daemon forces it on
you. Sleep in any safe bed (barracks / officer quarters / Sick Bay / village Flophouse):

```
lie down on bed
wait                         → you sleep; waking into Day 2 gives  **[+3 → …]**  (globals.zil:1072)
get out of bed
```
⚠ Never sleep in the space suit, or in the running spacetruck with a wrong course (fatal on waking,
`globals.zil:1008`). ⚠ From **Day 3** the elevator shaft has gravity: entering it when the car isn't
at your level is a fatal plunge (`station.zil:2945`) — `push button` to call the car first, or use the
ladder. By now you should have all of: **foil, zapgun, timer, DIODE-M-blown key, explosive spent,
paper**, and Floyd nearby.

---

## Phase 14 — Endgame (point of no return): the Factory  [+2, +2, +5]

Everything above must be done first. Opening the Dome bin starts the reactor-overload countdown.

```
# Dome (Level 1), holding the KEY, the ZAPGUN, and the FOIL:
… arrive [Dome]
unlock bin with key
open bin                     → the fuel cells detonate, knock you out (you drop everything in the Dome),
                               LOOSEN the air-shaft grating, and start the launch countdown
                               (I-ANNOUNCEMENT queued — station.zil:2121). Floyd is drawn to the Factory.
take all                     (recover your gear — you MUST have the zapgun and foil)
open grating                 → bend the now-loose grating aside (station.zil:2210)
enter grating                [Top of Air Shaft]   **[+2 → …]**  (station.zil:3693)
down                         [Air Shaft]
down
down
down
down                         [Bottom of Air Shaft]   (five handholds down — AIR-SHAFT-MOVEMENT-F)
open grating                 → you drop through the floor grating…
down                         [Computer Control]   (Level 9)
```
Clear the **exercise machine** the pyramid parked in your way (blocks Up to the Factory). Bring the
**jammer** (from Storage-5) with the **twenty-prong board** installed:
```
put twenty-prong board in jammer
set jammer to 710            (the machine's diagnostic frequency — the Gym sign, station.zil)
turn on jammer               → the exercise machine freezes; a forklift descends and clamps it
turn off jammer              → both grind to life, grapple, and explode — the path clears (station.zil:151)
up                           [Factory]   (Level 8)   **[+2 → …]**  (station.zil:3868)
```
The **pyramid** sits on a pedestal, exposed. **Floyd**, fully corrupted, blocks you with a stun ray.
```
examine floyd
shoot floyd                  ← the game's pivotal, tragic act: FLOYD-SHOT set, his stun gun drops
                               (only possible in the Factory — ship.zil:503)
put foil on pyramid          → the platinum reflects the pyramid's own emanations back into it; it
                               overloads and explodes, the launch aborts, the station is saved.
```
Floyd dies in your arms — *"One last game of Hider-and-Seeker… Ollie ollie… oxen… free"* — and
**Oliver** toddles in. **[+5 → 80]** — **You have won, with 80 of 80 points: Intergalactic
Mega-Hero.** (`PYRAMID-F`, `station.zil:3915-3954`.)

---

## Oracle checkpoint summary (for the walkthrough test)

Assert on these, in order (room `DESC` on entry, score after the awarding action):

1. **[Docking Bay #2]** + score 5
2. Wake Day 2 → score +3
3. iris hatch opens (`put form in slot`) → score +6
4. `turn wheel` [Casino] → +4
5. `take ostrich nip` → +3
6. `take foil` → +4
7. **[Armory]** → +5
8. `take coin` → +5
9. ostrich knocks out timer → +6
10. `take star` → +7
11. `take medium bit` → +3
12. `put explosive in hole` → +3
13. `take key` → +7
14. survive Plato (`floyd, help me`) → +7
15. **[In Space]** → +3
16. **[Top of Air Shaft]** → +2
17. **[Factory]** → +2
18. `put foil on pyramid` → +5 = **80**, win text + `Intergalactic Mega-Hero`.

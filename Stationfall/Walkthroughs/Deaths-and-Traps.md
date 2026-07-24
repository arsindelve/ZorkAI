# Stationfall — Deaths, Traps & Red Herrings (negative-path catalogue)

Failure cases worth encoding as **negative-path tests** and worth getting right in the port (the
original is famously unforgiving). Grouped by kind; each cites the ZIL. "Death" = `JIGS-UP`;
"softlock" = renders the game unwinnable without killing you (the port should reproduce these
faithfully unless a deliberate divergence is chosen).

## Instant-death timing traps

| Trap | Trigger | ZIL |
|---|---|---|
| **Vacuum launch** | `launch` / reach docking with the spacetruck **hatch open** or while **not seated** | `ship.zil:1158-1174` |
| **Wrong course** | `type <wrong course>` → truck halts in empty space, fuel → 0, `I-SUFFOCATE` queued | `ship.zil:1192-1226`; `verbs.zil:2136` |
| **Rex crushes you** | select robot **1** (Rex); he later follows you into the Cargo Bay Entrance and crushes you | `ship.zil:880-885` |
| **In-room blast** | be in the room with the explosive when the timer hits 0 | `interrupts.zil:409` |
| **Presser** | linger in the **Laundry** while the presser steams (`I-PRESSER`) | `station.zil:1426` |
| **Log reader explodes** | leave the **log reader** on ~14+ turns | `interrupts.zil:435` |
| **Drilling live machinery** | `drill` the dispenser / exercise machine / any active device | `station.zil:231` |
| **Poisoned coffee / FPU acid** | `drink coffee` or touch the Food Processing Unit in the Mess Hall | `station.zil:1662`+ |
| **Electrified beacon** | `push red button` in the Comm Center once `ROBOT-EVILNESS` is high | `station.zil:593`+ |
| **Elevator plunge** | from **Day 3** on, enter the elevator shaft when the car isn't at your level | `station.zil:2945` |
| **Hull welders** | linger in a **lit** room with a welder present (`WELDER-COUNTER` > 3) — flee to dark, leave, or `shoot welder` | `interrupts.zil:5`; `globals.zil:1281` |
| **Hunger / thirst** | ignore the hunger daemon to level 5 → "collapse from extreme thirst and hunger" | `globals.zil:1194` |
| **Unsafe sleep** | sleep in the **space suit**, or in the running wrong-course truck | `globals.zil:1008-1023` |
| **Station H-bomb** | fail to destroy the pyramid before the reactor-overload countdown (`I-ANNOUNCEMENT`→`I-LAUNCH`) fires | `station.zil:3661/3678` |

## Vacuum / airlock rules (In Space)

| Rule | Effect | ZIL |
|---|---|---|
| No suit in vacuum | Enter/remain in the airlock (outer door open) or In Space without the suit → lungs rupture | `village.zil:1201-1206` |
| **Remove suit in vacuum** | `remove suit` while in vacuum → `VACUUM-DEATH` | `village.zil:824-829` |
| Suit but no boots | Open the outer door with the suit on but boots off → swept out, `LOST-IN-SPACE` | `village.zil:1204-1206,1333` |
| Outer door flush | Opening the outer airlock door flushes **loose items** (and any ostrich/balloon) into space | `village.zil:1211-1235` |

## Softlocks (unwwinnable, not fatal)

| Softlock | Cause | ZIL |
|---|---|---|
| **Scrambled ID card** | `remove boots` while the **ID card** is on you → `ID-SCRAMBLED`; the changer then refuses it, and there's no un-scramble in the village. Reprogram to rank ≥7 first and keep the card off you when doffing boots. | `village.zil:1940-1944, 1837-1843` |
| **Knocked-out ostrich** | `give nip to ostrich` (instead of just *holding* the nip) → it "keels over into a grinning pile"; you can't use it on the dispenser hole | `village.zil:1731` |
| **Helen shreds forms** | select robot **2** (Helen); giving her any form → confetti | `ship.zil:832-836,889-893` |
| **Sublimated explosive** | let the FREZONE melt (`MELT-COUNTER` > 210) — carry it **sealed in the closed thermos** (4× slower); heat/flame/welder destroys it instantly | `interrupts.zil:358,361` |
| **Balloon lost** | grab the **leash** in a *weightless* room → you drop everything; bring the balloon into a **lit** Chapel → it's expelled; grab the leash in the **space suit** / **magnetic boots** → fails | `village.zil:475-505`; `station.zil:1628` |
| **Detonator won't fire** | leave the **blackened dud** diode in, or use **DIODE-J** → it fizzles; only the genuine **DIODE-M** (Chapel star) works | `interrupts.zil:398-399` |
| **Suit through the hatch** | try to cross the iris hatch wearing/carrying the space suit → blocked (too bulky) | `village.zil:5` |
| **Elevator to 8/9** | the elevator only serves Levels **1–7**; Levels 8–9 are reachable **only** via the Dome air shaft | `station.zil:2929` |

## Red herrings (present to mislead; harmless)

| Item / place | Why it's a decoy | ZIL |
|---|---|---|
| **Forms Storage Room** | sealed pallets; nothing needed there (all forms start in inventory) | `ship.zil:160-219` |
| **Assignment Completion Form + Deck-Twelve slot/west door** | the slot always rejects it and the west door is a fake that never opens | `globals.zil:730`; `ship.zil:20-32` |
| **Safe dial** (8000 settings, 12–20-number combo) | uncrackable by design — you must drill+blast, not `unlock` | `station.zil:924` |
| **Blackened "M" diode** | `clean` reveals an "M" but it is neither DIODE-M nor DIODE-J — it will not detonate | `station.zil:1880`; `interrupts.zil:398` |
| **Twelve-prong fromitz board** | wrong prong count for the jammer; you need the **twenty**-prong board | `station.zil:3195` |
| **Twenty-ohm bedistor** | robots give evasive answers; a spare-part decoy | `station.zil:63` |
| **Skeleton (Alien Ship)** | crumbles if touched; Floyd fears it — pure atmosphere | `station.zil:2345,2425` |
| **Wall dial (Commander's Quarters, "4473")** | decoy; not the safe | `station.zil:870`+ |
| Cut content | `CHURCH` and `INSURANCE-OFFICE` rooms, and a 7th dream (gallium/sponge-cat), are **commented out** — not in the game | `village.zil:132,1106`; `globals.zil:976` |

## The ending is not the commented-out text

The "S.P.S. *Flathead* / Captain Sterling / promotion to Lieutenant First Class" passage at
`misc.zil:166-185` is **dead commented-out code**. The shipped victory (`PYRAMID-F`,
`station.zil:3919-3954`) is the **Floyd death scene** ("One last game of Hider-and-Seeker… Ollie
ollie… oxen… free") followed by **Oliver's** arrival, then `+5`, `TELL-SCORE`, `QUIT`. The port must
render this scene, not the leftover promotion text.

> # ⛔ STOP — SPOILERS ⛔
>
> **This file solves the game.** It exists only as the ZIL-verified oracle the port's tests are
> written against. If you intend to *play* Stationfall, close this file now and do not read the
> rest of this folder. See `Docs/Stationfall-Port-Plan.md` for a spoiler-free status of the port.

# Stationfall — Critical Path (shortest route to win)

The minimum required to **destroy the pyramid** — score is irrelevant here (you still finish well
above zero, but this run skips lore-reading and optional pickups like the Large Drill Bit). Use it as
a fast end-to-end regression once the Full-80 run passes. See
[`Walkthrough-Full-80.md`](Walkthrough-Full-80.md) for the exact commands and ZIL citations of each
sub-goal; this file is the dependency skeleton.

## Why almost nothing is optional

Winning requires `PUT FOIL ON PYRAMID` in the **Factory**, which forces a long dependency chain — the
game is tightly gated:

```
WIN  ← put foil on pyramid (Factory)  ← shoot Floyd (zapgun, in Factory)
Factory (L8)  ← clear exercise machine (jammer + 20-prong board, set 710, on→off)
              ← reach Computer Control (L9) via the Dome air shaft
air shaft     ← open the Dome storage bin  ← KEY
KEY           ← blow the safe: DRILL + medium bit + DETONATOR(+DIODE-M) + TIMER + EXPLOSIVE
  medium bit  ← Floyd fetches from the Robot Shop heating chamber
  DIODE-M     ← seven-pointed star (Chapel) ← ride the BALLOON up ← eternal flame OFF
  TIMER       ← PX dispenser ← COIN + OSTRICH-in-hole trick
    COIN      ← shoot the strong box ← ZAPGUN
    OSTRICH   ← lead it with the NIP (Pet Store ceiling)
  EXPLOSIVE   ← In Space (airlock: SUIT + BOOTS + HEADLAMP)  ← SUIT via roulette wheel → Flophouse
ZAPGUN        ← Armory (security door) ← ID card reprogrammed to rank ≥7 (Shady Dan's)
FOIL          ← break the Barbershop mirror (platinum detector)
Village access ← validated entry form (crumpled → pressed → stamped) in the iris-hatch slot
Survive Plato ← "floyd, help me" during his ambush (or you die)
```

Because of this, the "critical path" is essentially the Full-80 route **minus**: reading the diary /
note / log / paper for their own sake (you still must *open the safe* and *have the foil*, but you can
skip narrating the lore), the Large Drill Bit (`type 9`), and any pure exploration.

## Ordered sub-goals (each expands to a block in the Full walkthrough)

1. **Depart** → dock at **[Docking Bay #2]** (select Floyd; computed course). *(Phase 0)*
2. Grab tools: **drill** (Paper Recycling), **detonator** (Main Storage), **20-prong board**
   (Astro Lab), **jammer** (Storage-5), **thermos** (from the kit). *(Phases 1–2)*
3. Get the **validation stamp** (under the Commander's bed) and blow through the **log** only enough
   to know the key is in the safe. Prepare + validate the **village entry form**. *(Phase 2)*
4. **Enter the village** (form in slot → iris hatch). *(Phase 3)*
5. Village grabs: **platinum detector** → **foil** (break mirror); **spray can**; **ostrich nip** +
   **balloon** (open cage); **roulette** → **space suit**; **boots**; **headlamp**; reprogram the
   **ID card** to ≥7 (Shady Dan's). *(Phase 4)*
6. **In Space**: suit+boots+headlamp through the airlock → **explosive** (sealed in the thermos).
   *(Phase 5)*
7. **Armory** (ID ≥7) → **zapgun**; then **shoot the strong box** → **coin**. *(Phase 7)*
8. **PX**: coin + `type 6` → ostrich knocks out the **timer**. *(Phase 8)*
9. **Chapel**: flame off, balloon in, ride up, **take star** → **DIODE-M**. *(Phase 9)*
10. **Floyd fetches the medium drill bit** (Robot Shop). *(Phase 10)*
11. **Blow the safe** (drill + medium bit + detonator w/ DIODE-M + timer + explosive) → **key**.
    *(Phase 11)*
12. **Survive Plato** whenever he attacks: `floyd, help me`. *(Phase 12)*
13. Sleep as needed (hunger/day clock); by **Day 3** avoid the elevator shaft unless the car is called.
14. **Endgame** (point of no return): Dome bin (key) → air shaft → **[Top of Air Shaft]** →
    **[Computer Control]** → jammer 710 on/off clears the exercise machine → **[Factory]** →
    `shoot floyd` → `put foil on pyramid` → **WIN**. *(Phase 14)*

## Test value

This route touches every hard gate exactly once, so as a regression it exercises: the computed course,
the iris-hatch form chain, the airlock/vacuum rules, the security-door/ID-rank check, the
ostrich+dispenser interaction, the balloon/flame/star chain, the detonator/diode/timer bomb, the
air-shaft descent, the jammer/exercise-machine puzzle, and the Floyd-shot win gate.

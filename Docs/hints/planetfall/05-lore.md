# Planetfall Lore Source (built from the C# source)

**Proof-of-concept lore corpus for the in-world-guide.** This is the grounding source for *mechanic*
and *lore* questions ("why is everything deserted?", "why am I getting sick?", "what is this place?")
— the category the walkthrough and invisiclues don't cover. Every fact here is **extracted from the
game's own descriptive text in the C# port** (room/item descriptions, readable items, the library
terminal), digested into plain English, with a source citation and a **spoiler tier**. Nothing is
sourced from the LLM's training memory or the original ZIL — this *is* the grounding that lets the
guide answer lore questions without confabulating.

> **Note on the phonetic text.** The library terminal and most in-world documents are written in a
> phonetic future-English (`"Xe Dizeez"` = *The Disease*, `kriioojeniks` = *cryogenics*). The facts
> below are decoded/digested, not transcribed.

## Spoiler tiers — gate answers by what the player has discovered

The guide reuses the localization read (what's been visited / examined / read) to bound how much to
reveal. A lore answer is filtered to the player's current tier:

| Tier | Available once the player has… | Example fact |
|---|---|---|
| **T0 — observable** | just arrived / can see it | "The complex is deserted and automated." |
| **T1 — environmental** | read the diary / examined nearby items | who you are; that you've fallen ill |
| **T2 — investigated** | reached & used the **Lawanda library terminal** | the cryogenic project; the origin of The Disease |
| **T3 — endgame** | completed the cure / reached the Cryo-Anteroom | Veldina, the revival, the full resolution |

Rule: never reveal a fact above the player's current tier. A blocked answer becomes a nudge
("you haven't learned that yet — the Lawanda library might have answers").

---

## Who you are (T1 — `Item/Feinstein/Diary.cs`)

You are an **Ensign Seventh Class** in the Stellar Patrol — the lowest rank, stuck there despite
your family serving **five generations** (your great-great-grandfather was a High Admiral and a
founding father of the Patrol). You grew up on **Gallium**. You serve aboard the **S.P.S. Feinstein**,
on the third of four tours, tormented by your petty martinet commander, **Ensign First Class Blather**,
who has you scrubbing **Deck Nine**, the filthiest deck on the ship. The Feinstein was sent to
investigate a remote planetary system that archaeologists think may once have been part of the
**Second Union**. (The diary is read progressively, entry by entry, via its "More" button — itself a
nice model for laddered disclosure.)

## How the game opens (T0)

A massive explosion tears the Feinstein apart while you're swabbing Deck Nine. You reach an **escape
pod** and ride it down to the planet's surface — **Resida** — landing underwater near an automated
complex. From there you're alone (save for a robot you can reactivate).

## Why everything is deserted (T0 observable → T2 explained)

- **T0:** The complex is fully automated and abandoned — machines run, doors open, but there are no
  people. (Visible immediately from room descriptions.)
- **T2 — the real reason** (`Item/Lawanda/Library/Computer/ProjectMenu.cs`): the planet's entire
  population was **cryogenically frozen** and is in stasis, waiting out a plague. The two complexes
  exist solely to monitor the sleepers and run automated research. There are no living inhabitants to
  meet — everyone is asleep in the cryogenic chambers below.

## The Disease & why you're getting sick (T1 onset → T2 explanation)

- **T1 — what's happening to you** (`SicknessLevel.cs`, `PlanetfallContext` Disease clock): you have
  contracted **The Disease** — the same plague that emptied the planet. It worsens over the days of
  your adventure: perfect health → feverish → "somewhat sick" → "very sick" → near death. This is the
  central time pressure (see [03 soft-locks](03-softlock.md) #1).
- **Experimental treatment** (`Item/Lawanda/MedicineBottle.cs`): a bottle labelled *"Disease
  suppression medicine — experimental"* exists — it buys time, it is not the cure.
- **T2 — origin** (`ProjectMenu.cs`, MenuOne): The Disease was accidentally **released from the Center
  for Advanced Cryogenic Research**, which had been researching how to extend the cryogenic period
  indefinitely. The research succeeded — but the plague escaped and spread.

## The Project — what this place is for (T2 — `ProjectMenu.cs`)

The grand plan the automated complexes were built to execute, in four phases:

1. **Phase One** — construct the Kalamontee and Lawanda complexes.
2. **Phase Two** — mass cryogenic freezing of Resida's population.
3. **Phase Three** — automated monitoring of the sleepers while sophisticated computers conduct a cure
   search.
4. **Phase Four** — revival and inoculation of the population once a cure is found.

The catch the player lives out: the automation has been running unattended for a long time and has
**broken down** — which is why a half-trained ensign who washes up here ends up completing Phase Four
by hand.

## The two complexes (T0/T2)

- **Kalamontee** — where you land and spend the first half. Built on a twin-peak plateau; the valley
  was once a tourist spot, the large building at the bend of the Gulmaan River a former provincial
  capital (`Item/Kalamontee/Plaque.cs`).
- **Lawanda** — the research/lab half, reached by shuttle; home to the library, the bio lab, and the
  miniaturization/cure machinery. Both were sited in the mountains so their heat eased transport and
  comms and so the vast **reactors and cryogenic chambers** could be built below (`ProjectMenu.cs`,
  MenuTwo).

## The planetary systems (T2 — `Item/Lawanda/Library/Computer/TechnologyMenu.cs` MenuFive)

Three automated systems keep Resida habitable — and they're failing, which is the source of several
puzzles and the multiple endings:

- **Course Control** — maintains an ideal climate / keeps the orbit stable.
- **Defense** — destroys dangerous incoming meteors.
- **Project Control** — monitors the Project's progress (the recently-added third system; it's the
  malfunctioning one flagged on the monitors).

(These map to the optional ★ repair puzzles and the ending matrix — see [01 DAG](01-puzzle-dag.md)
and the endings table in `PlanetfallContext.cs`.)

## The world of Resida (T2 — Library: History / Geography / Culture / Technology menus)

- **History** (`HistoryMenu.cs`): legends say Resida's people descend from the **Second Union**,
  ancient spacefarers. A high civilization existed millennia ago but collapsed into a centuries-long
  dark age — the **Great Hiatus** — before the **New Technocracy** of the last five centuries restored
  it to pre-Hiatus heights. The Disease struck at that peak.
- **Geography** (`GeographyMenu.cs`): ~48% land; two main continents **Andoor** and **Fruulik** plus
  six lesser; global capital **Pilandoor**; ~9% live in undersea cities; off-worlders live in space
  colonies at the planet's Trojan points and on moons of the gas giant Blustin.
- **Technology** (`TechnologyMenu.cs`): all major diseases have been curable for a century;
  **cryogenics** lets doctors freeze patients until a cure is found (life expectancy ~147 years);
  robots like the multipurpose **B-19 series** now do work that once took whole teams.

## Floyd (T1 — your companion)

A **multipurpose B-19-series robot** you reactivate in the Robot Shop. Childlike, enthusiastic,
loyal — the emotional center of the game. (His characterization lives in the Floyd item code, not
this doc; referenced here only as a world fact.)

## The fourth-wall gag (meta — `Item/Feinstein/Brochure.cs`)

A brochure aboard the Feinstein advertises *Planetfall* as "the leading export of Blow'k-bibben-Gordo,"
by S. Eric Meretzky. Pure meta-humor — the guide should treat it as a joke, not lore, and not let it
leak into serious answers.

---

## Worked example: the two questions that prompted this

**"Why is everything deserted?"**
- *T0 answer:* "This whole complex is automated and abandoned — the machines run themselves, but
  there's no one here."
- *T2 answer (after the library):* "Resida's entire population is in cryogenic stasis, frozen to wait
  out a plague. These complexes were built to watch over the sleepers and search for a cure — so the
  only 'inhabitants' are asleep in the chambers far below you."

**"Why am I getting sick?"**
- *T1 answer (grounded in live state):* "You've caught The Disease — the same plague that emptied this
  place. You've been getting worse since it set in, and it won't stop on its own. There's experimental
  medicine that buys time, but you'll want to find the real cure." *(Gently points at the stakes and
  the goal without spoiling the lab/miniaturization solution.)*
- *T2 addition:* "It was released from a cryogenic-research center — the same research that built this
  whole installation."

Both answers are **fully grounded in the cited source files**, tier-gated, and stop short of the
puzzle solution — which is exactly the behavior the in-world guide needs.

## Coverage / gaps

This proves the lore source is **buildable from the port's own text** with no external corpus — the
Planetfall lore content gap is effectively closeable from the codebase. Still to do before Stage
(lore): a complete pass over **all** room descriptions and readable items (this draft covers the
backstory spine, not every flavor detail), and a decision on whether to store the lore as a curated
doc like this or to retrieve live from the item/location descriptions at query time. **(This is
Planetfall only — the Zork I lore source is a separate problem, per [00 § open decisions](../00-master-plan.md#6-open-decisions-yours-to-make).)**

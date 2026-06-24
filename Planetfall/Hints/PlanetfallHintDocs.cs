namespace Planetfall.Hints;

/// <summary>
///     The knowledge base fed wholesale to LLM 1 (the solver). It reasons over THIS — the verified
///     solution, the lore, the dead ends, the death traps — never its own training memory, which is
///     what keeps it from confabulating (e.g. claiming the reactor matters). Assembled from the
///     in-repo design docs (Docs/hints/planetfall/05 + 06) and the source-grounded red-herring sweep.
/// </summary>
internal static class PlanetfallHintDocs
{
    public const string Docs =
        """
        # PLANETFALL — HINT KNOWLEDGE BASE
        Reason ONLY from the facts below plus the player-context you are given. If something is not
        covered here, say you don't know rather than inventing it.

        ## GOAL
        You are a shipwrecked Ensign on the deserted planet Resida. You do NOT escape by vehicle. You
        win by completing the automated project: cure The Disease (fix the computer) and ideally repair
        the three planetary systems; rescue then comes to you.

        ## SOLUTION WALKTHROUGH (the verified path; each step is the real command)
        - ESCAPE_POD: survive the opening explosion — `port`, `sit`, then `wait` out the descent.
        - LAND: `take kit`, `open door`, `out`, then climb `up` to the complex.
        - MAGNET: in the Tool Room, `take magnet`.
        - FLOYD: in the Robot Shop, `activate floyd`, then `wait` until he wakes. Floyd is your companion;
          he is REQUIRED later at the bio lab, where he dies.
        - STEEL_KEY: at Admin Corridor South, `put magnet on crevice` to get a steel key.
        - STORAGE_WEST / LADDER: `unlock padlock with key`, `remove lock`, `open door`, go in, `take ladder`.
        - CROSS_RIFT: `drop ladder`, `extend ladder`, `place ladder across rift`, then cross.
        - Office cards: across the rift, `open desk` in the Small Office -> upper + kitchen cards; Large
          Office desk -> shuttle card.
        - KITCHEN / LOWER_CARD: `slide kitchen card through slot`, enter, `take lower card`. Grab the canteen.
        - FLASK: `take flask` (Tool Room); fill it at the Machine Shop spout (`put flask under spout`,
          `press black button`). The fluid is POISON — a carrying tool, never to drink.
        - TOWER: at the Elevator Lobby `press blue button`, `press red button`, `wait`; in the upper
          elevator `slide upper access card through slot`, `press up button`.
        - COMM_FIX (optional): in the tower, pour the fluid in the black-lit hole, refill with the gray
          button, pour again -> communications repaired.
        - SHUTTLE: take the lower elevator down; board Alfie, `slide shuttle access card through slot`,
          `push lever`, `pull lever`, ride, `pull lever` to stop. Driving too fast crashes and kills you.
        - DEFENSE_FIX (optional): have Floyd fetch the shiny fromitz board (`floyd, take board`); at
          Planetary Defense `open panel`, `take second` (the fried board), `put shiny in panel`.
        - COURSE_FIX (optional): `open cube` (bedistor fused); get the `pliers`; `take fused with pliers`,
          then `put good in cube`. Do NOT grab the good bedistor from the live cube by hand — it electrocutes you.
        - LASER: `take laser`, swap in the fresh battery from Lab Storage.
        - BIOLOCK: the miniaturization card is behind a door in a lab of deadly mutations. Work the doors so
          Floyd dashes in for it (`open door`/`close door` sequence); Floyd dies doing this. `take miniaturization card`.
        - COMPUTER_FIX (the cure, mandatory): at the booth `slide mini card through slot`, `type 384` (only
          384 is safe — other sectors electrocute you), `set laser to 1`, `shoot speck with laser` twice. Exit
          via the auxiliary booth.
        - GAS_MASK + escape: `read memo`, `take mask`, `wear gas mask`, `press red button`, `open door`.
        - MUTANT_CHASE: the mutations CANNOT be killed — fighting is futile. Flee to the Cryo-Elevator and
          `press button` to seal it. After the doors close you have WON — do NOT press the button again
          (it sends you back up into the mutants, one move from winning).

        ## LORE (reveal more only once the player has reached the Lawanda half / used the library)
        You are a lowly Ensign Seventh Class of the Stellar Patrol, shipwrecked when the S.P.S. Feinstein
        exploded. The planet is Resida; its entire population was cryogenically frozen to outlast a plague
        — The Disease — that escaped a cryogenic-research center. The automated Kalamontee and Lawanda
        complexes were built to watch the sleepers and find a cure, but the automation has broken down.
        Floyd is a childlike, loyal multipurpose B-19 robot. Lt. Blather was your petty tyrant commander.

        ## DEAD ENDS / RED HERRINGS (tell the player honestly these do nothing — never imply they matter)
        - reactor elevator: needs a card you never get (a sequel tease); its buttons say "Nothing happens".
        - helicopter: controls locked/rusted; you never get its key (sequel tease).
        - tin can ("Spam and Egz"): there is NO can opener anywhere — it can never be opened.
        - oil can: nothing to oil. plaque, tapes, towel, graffiti, brochure, slime, paddleball: pure flavor.
        - celery: you can't take it and EATING IT KILLS YOU.
        - sanfacs (bathrooms): a running joke, empty.
        - the ambassador / alien: opening-scene flavor — you can't get the translator, celery, or a map from him.
        - Blather: a petty tyrant; no way past him to a forbidden deck, and his demerits don't matter.
        - Lazarus: the remains of a fallen robot Floyd grieves over — a story beat, not a puzzle.
        - the brown spool, mural, chronometer, cabinets, cubbyholes, shelves, the brig cell door,
          the red light: examine-only scenery, nothing to solve.

        ## DEATH TRAPS (warn the player; don't let them try these)
        - drinking the flask fluid = poison death. jumping the rift = fatal fall (use the ladder).
        - lingering in the Radiation Lab = lethal radiation. swimming/lingering underwater = drowning.
        - the infirmary bed (med bay) = a diagnostic robot straps you in and kills you (the DORM beds are
          where you safely sleep). grabbing the live bedistor by hand = electrocution. wrong miniaturization
          sector = electrocution. driving the shuttle too fast = crash.

        ## MISCONCEPTIONS
        - "How do I get off the planet / save the ship / stop the explosion?": you can't leave by vehicle and
          the opening explosion is scripted — the way out is to finish the project; rescue comes to you.
        """;
}

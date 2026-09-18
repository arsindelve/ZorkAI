# How the game plays, and what the player is up against

## How the game plays, and what the player is up against

### From shipwreck to rescue

The player begins as an Ensign Seventh Class aboard the **S.P.S. Feinstein**, scrubbing **Deck Nine** under Ensign First Class Blather. This is not a command position, and the opening is not a puzzle about saving the ship. Blather’s demerits are mostly humiliation and comedy; the explosion is unavoidable. The immediate task is to recognize an emergency, reach the **Escape Pod**, and survive its automated descent.

Afterward, the player climbs out of dangerous water onto **Crag**, then through **Balcony**, **Winding Stair**, and **Courtyard** into a deserted installation. The first impression is of abandonment: dusty dormitories, dry sanitary fixtures, stopped walkways, unfamiliar machines, and instructions written in a distorted form of the player’s language. The islands are actually the high ground of **Kalamontee** and **Lawanda**, on the planet **Resida**. The old plaque on Balcony describes a valley where the player now sees ocean.

The explanation should emerge through investigation rather than through an unsolicited opening lecture. Resida’s population is in cryogenic suspension. The Disease escaped during successful research into indefinite cryogenic preservation; the inhabitants subsequently froze themselves while automated facilities searched for a cure. That undertaking is **the Project**. Its research is almost complete, but essential machinery has failed, and the repair robot **Achilles** lies broken in **Repair Room**. A stranded deck-scrubber must finish work that the automation can no longer perform.

The player has also contracted the Disease. Finding shelter and food buys working time, but does not solve the long-term problem. Repairing the Project’s computer is necessary to reach the ending. **Course Control** saves the planet from its failing orbit; **Planetary Defense** prevents the meteor-defense system from destroying rescuers; the sending equipment in **Comm Room** makes contact possible. The defective defenses are the hint book’s probable explanation for the Feinstein’s destruction, not an explanation known at the moment of the explosion.

**Floyd**, the childlike B-19-series robot in **Robot Shop**, becomes both companion and indispensable helper. The verified sequence is `activate floyd`, then two `wait` commands: the initial “Nothing happens” is misleading because startup is already underway. His apparent silliness should not make the coach dismiss his observations about the broken computer or his ability to enter robot-sized spaces.

### Reading and commanding the world

Planetfall is a parser adventure. The player describes an action, and the game interprets its verb, objects, and relationship: `open desk`, `take canteen`, `unlock padlock with key`, or `put flask under spout`. Directions move between rooms: the playthrough uses `N`, `NE`, `SW`, `E`, `W`, `up`, and `out`. A description of stairs does not guarantee that `up` is the mapped command: the **Escalator**, for example, is traversed with `E` and `W`.

Use the room descriptions as operational information. Objects may be inside desks or pockets rather than listed on the floor. In **Small Office**, `open desk` exposes the **kitchen access card** and **upper elevator access card**. In **Lab Storage**, `open pocket` exposes the **teleportation access card** and **piece of paper**. `read output` in **Computer Room** supplies the damaged sector number, **384**. Some reading commands automatically take the document, as the transcript’s “(Taken)” shows.

Short object names work when they distinguish the intended object. The playthrough successfully uses `take upper card`, `take kitchen card`, `take teleportation`, `take second`, and `put good in cube`. These are not universal passwords: “second” identifies an installed fromitz board, while “good” distinguishes the replacement bedistor from the fused one. When several cards, doors, buttons, or liquids are present, use a specific name.

A particularly important verb is **slide**. Card readers are shallow swipe slots, not containers. The working commands include `slide kitchen access card through slot`, `slide upper access card through slot`, `slide lower access card through slot`, `slide shuttle access card through slot`, `slide teleportation card through slot`, and `slide mini card through slot`. Inserting a card into a shallow slot is refused and supplies the clue to slide it through instead. A successful swipe generally authorizes a device only temporarily; it is not necessarily a permanent unlock.

Other essential constructions are `press up button`, `press down button`, `push lever`, `pull lever`, `pour fluid into hole`, `take fused with pliers`, `set laser to 1`, `shoot speck with laser`, and `wear gas mask`. There is no general rule that `use` substitutes for understanding the mechanism.

Address Floyd with a comma: in **Repair Room**, the verified pair is `floyd, go north`, then `floyd, take board`. His first trip discovers the **shiny fromitz board**; asking for it before that discovery does not bypass the prerequisite. “Nothing happens” can also mean a genuinely useless action—the **Reactor Elevator** never becomes a working route—so distinguish delayed responses from dead ends by the surrounding evidence.

`wait`, abbreviated `z`, deliberately passes a turn. It is useful during an elevator ride or a robot’s startup, but dangerous during starvation, exhaustion, or pursuit. Saving is supported and sensible before hazardous experiments. Death is not a harmless undo: the game reports the score and offers another chance under the Treaty of Gishen IV, but the supplied rules do not establish that the current puzzle state survives intact.

The local writing is phonetic English. “X” commonly replaces “th,” “c” replaces “ch,” and doubled vowels indicate long sounds. Thus **“Hii Prooteen Likwid Dispensur”** is a food dispenser, not an inscrutable alien machine. Reading labels aloud often makes the clue intelligible.

Commands for eating, sleeping, and medicine are not exercised in the verified transcript. The forms given below for those actions come from the supplied interaction notes; they should not be confused with a claim that the transcript demonstrates a complete survival schedule.

### Points and progress

The maximum score is **80**. It measures discoveries and accomplishments, not valuables accumulated or demerits avoided. The score report also gives the day, Galactic Standard Time when the chronometer is worn, and a rank.

The ranks are **Beginner** through 12 points; **Space Cadet**, 13–24; **Ensign First Class**, 25–36; **Lieutenant**, 37–48; **Planetary Commodore**, 49–64; **System Captain**, 65–72; **Cluster Admiral**, 73–79; and **Galactic Overlord**, 80 or more. These score labels are distinct from the narrative promotion in the ending.

The full ordinary scoring account is: three points each for entering **Escape Pod** and **Crag**; two for first activating Floyd and two for first firing the laser; four each for entering **Storage West**, **Admin Corridor North**, **Kitchen**, **Tower Core**, **Kalamontee Platform**, and **Lawanda Platform**; one each for taking the kitchen, shuttle, upper elevator, lower elevator, and miniaturization access cards; two for completing Floyd’s fatal card-retrieval scene; six each for repairing communications, planetary defense, and course control; four each for entering **Strip Near Station** and **Auxiliary Booth**; eight for repairing the computer; and five for entering **Cryo-Elevator**.

Finishing with fewer than 80 points is possible. Do not interpret the three six-point system repairs as interchangeable optional extras. With course control repaired but rescue systems incomplete, Resida survives but the player can be stranded. Broken defense takes precedence in that explanation: it destroys the approaching Patrol ship; otherwise broken communications causes the ship to give up. Without course control, Resida is doomed even if a working communications-and-defense combination permits the player’s evacuation.

The complete victory includes the cure, rescue by the **S.P.S. Flathead**, and Floyd’s restoration. His earlier sacrifice cannot be reversed locally by operating his switch.

### What a turn costs

An ordinary time-consuming turn advances **Galactic Standard Time by 54 units**. Hunger and fatigue are scheduled against this clock. Other mechanisms use their own turn updates, so neither “one command” nor “one millichron” should be assumed to mean the same thing in every subsystem.

Survival checks occur at the beginning of an ordinary turn; the 54-unit advance comes at its end. Reaching a threshold at the end of an action therefore normally produces its warning at the next check. Each warning schedules the next deadline from the actual time it appeared, which rounds practical intervals upward to whole turns.

Sleep takes priority. If sleep is already due, the sleep-and-waking sequence replaces the action the player was trying to perform. A last-second movement command cannot outrun an exhaustion event that has already become due. Otherwise the checks run through sickness notices, fatigue, and hunger, so several warnings can arrive together.

Some commands are free with respect to ordinary time, but the sources do not provide a complete free-command list. Companion and device activity need not freeze with the chronometer. Never promise that repeated examinations or other supposedly free commands safely suspend an emergency.

Keep the **chronometer** carried and worn. `read chronometer` and `examine chronometer` report the time. Removing it does not stop any clock; it merely makes ordinary time reporting unavailable.

The initial reading is randomly selected from **4500–4699**. Days do not advance simply because the displayed time passes a particular number: they advance through sleeping and waking. The new-day heading is **“***** SEPTEM [day plus five], 11344 *****”**, so Day 2 is Septem 7.

Morning readings become progressively later: Day 2 begins at **1600–1679**, Day 3 at **1750–1829**, Day 4 at **1950–2029**, Day 5 at **2150–2229**, Day 6 at **2450–2529**, Day 7 at **2800–2879**, and Day 8 at **3200–3279**. The changing waking time is part of the worsening condition, not a watch-setting puzzle.

The verified transcript contains no meals or nights. It establishes working puzzle commands, not evidence that ordinary play can safely ignore survival.

### The opening emergency

The first explosion occurs on **move 10**. On Deck Nine the decisive cue is **“The door to port slides open.”** The verified opening waits ten times, then uses `port` to enter **Escape Pod**, followed immediately by `sit`. The reply **“You are now safely cushioned within the web”** confirms protection.

The emergency bulkheads close on move 11. A player caught away from Deck Nine and the pod dies on move 12; a player still on Deck Nine loses the boarding opportunity when the pod door closes, and dies aboard the ship on move 14. Exploring **Deck Eight** or **Reactor Lobby**, getting imprisoned in **Brig**, or trying to prevent the explosion wastes the only useful window. There is no escape from the Brig.

The pod flies itself. Remain seated and `wait` until landing reveals the **survival kit** and **towel**. Standing during flight risks being thrown into the bulkhead when the Feinstein explodes; standing at landing is fatal even if the earlier impact was survived.

After landing, the verified safe sequence is `take kit`, `open door`, `out`, then immediately `up` from **Underwater**. Continue `up` through Crag, Balcony, and Winding Stair to Courtyard. The towel is available but is not required by this demonstrated escape.

Do **not** follow the hint book’s claim that the player must stand to reach the door. The transcript opens it and leaves while still nominally seated. Explicitly standing after landing tips the pod into the water and begins an irreversible sinking sequence: it is fully submerged at the third update, may creak under pressure at the fourth, and kills anyone remaining inside at the fifth. Sitting down again does not reset it. Collect supplies first and leave promptly.

Underwater is independently lethal. The first visit permits only two initial danger updates before the next kills; after leaving once, a later entry is fatal at its next underwater update. Do not inspect the scenery, return for forgotten provisions, or test how long the player can hold their breath. `up` is the solution.

### Hunger and thirst are one clock

There is no separate hydration puzzle. Hunger and thirst form a single condition, relieved by either of the game’s useful foods. The player starts well-fed, with the first warning scheduled **4,000 time units** after the initial time—about 75 ordinary increments.

The first warning is **“A growl from your stomach warns that you're getting pretty hungry and thirsty.”** There are then **900 units**, about 17 turns, before **“You're now really ravenous and your lips are quite parched.”** That stage allows **300 units**, about six turns, before **“You're starting to feel faint from lack of food and liquid.”**

After another **200 units**, about four turns, comes **“If you don't eat or drink something in a few millichrons, you'll probably pass out.”** Only **100 units**, about two turns, remain before **“You collapse from extreme thirst and hunger.”** These are successive warnings, not a single generous allowance renewed by noticing them.

The useful starting provisions are the **red goo**, **brown goo**, and **green goo** inside the survival kit. Open the kit and use `eat red goo`, `eat brown goo`, or `eat green goo`. All three are nutritionally identical. Red tastes like cherry pie, brown like Nebulan fungus pudding, and green like lima beans; the flavors do not indicate medicine or different strengths.

Each eaten blob restores the well-fed condition and sets the next hunger warning **1,450 units** ahead, about 27 ordinary increments. The player must actually be hungry; **“Thanks, but you're not hungry”** preserves the food. The kit must be open and either directly carried or lying directly in the room. Nesting it inside another container does not satisfy the eating rule. The goo cannot be carried separately because it would ooze through the fingers.

Never `shake kit`. That scatters and destroys every remaining blob, even if the kit is closed. `empty kit` misleadingly suggests shaking, but the suggestion is not good survival advice.

### Securing a lasting food supply

The renewable food source is the **protein-rich liquid** in **Kitchen**, south of **Mess Hall**. The **canteen** starts on a Mess Hall bench. The kitchen access card is in the Small Office desk beyond the rift, so food access requires real exploration rather than finding a tap.

The verified preparation chain begins in **Tool Room** with `take magnet`. In **Admin Corridor South**, `put magnet on crevice` retrieves the steel **key**. In **Mess Corridor**, `unlock padlock with key`, `remove lock`, and `open door` permit entry north to **Storage West**. Unlocking alone is insufficient because the open padlock still physically obstructs the door.

Take the collapsed **ladder**, carry it to **Admin Corridor**, then use `drop ladder`, `extend ladder`, and `place ladder across rift`. Cross with `N`, not a jumping command. An unextended ladder is too short and falls irretrievably into the rift; explicitly jumping is fatal. In Small Office, `open desk` and `take kitchen card` provide authorization.

Back in Mess Hall, use the transcript’s `take canteen`, `slide kitchen access card through slot`, and `S`. Enter promptly: the kitchen door closes automatically, and swiping again while it is open does not reset its running timer. Leaving Kitchen north also closes it.

The documented filling sequence is `open canteen`, `put canteen in niche`, and `push button`; `put canteen under spout` is also supported. The matching octagonal shapes explain the mechanism. Only an **open, empty canteen**, positioned beneath the spout, catches the food. An absent or closed canteen causes a useless spill; filling an already full one merely makes a mess. Take the canteen afterward.

Use `drink protein liquid` when hungry. The reply explicitly says it has quenched thirst and satisfied hunger. This restores well-fed status and postpones hunger for **3,600 units**, about 67 ordinary increments—much longer than one survival-kit blob. Drinking while well-fed is refused without consuming it. No dispenser supply limit is established.

Do not confuse this brown food with **chemical fluid** carried in the **glass flask** from Machine Shop. Drinking the flask’s contents is fatal: **“Mmmmm....that tasted just like delicious poisonous chemicals!”** Distinguish the nouns instead of casually commanding `drink liquid` near both supplies.

The **tin can** labelled **“Spam and Egz”** in Storage West cannot be opened, and no can opener exists. The ambassador’s **celery** is poisonous to humans; tasting his **slime** is a joke, not nourishment. **Sanfac A**, **Sanfac B**, **Sanfac C**, **Sanfac D**, **SanFac E**, and **Sanfac F** have dry, dusty, nonfunctional fixtures, not drinking water. The ocean is not an established food or water solution.

### Morning hunger

Sleep resets hunger, but does not restore the opening allowance. If the player went to sleep well-fed, the next morning’s first hunger warning is only **800 units** away, about 15 ordinary turns. If the player went to sleep hungry at any stage, waking adds **“You are also incredibly famished. Better get some breakfast!”**, and the warning is only **200 units** away, about four turns.

Despite that message, the internal hunger stage begins well-fed in either case. The difference is how soon escalation resumes, not waking already one step from death.

This is why the kitchen and a prepared canteen matter. Sleeping can interrupt the hunger ladder, but advances the Disease and leaves a much shorter morning food deadline. It is not a sustainable substitute for eating.

### Fatigue and safe sleep

The first fatigue warning is scheduled **3,600 units** after the opening day begins, about 67 ordinary increments. Day 2 gives **5,800 units**, about 108; Day 3, **5,550**, about 103; Day 4, **5,200**, about 97; Day 5, **4,800**, about 89; Day 6, **4,300**, about 80; Day 7, **3,700**, about 69; and Day 8, **3,000**, about 56. Later days retain the 3,000-unit allowance.

The first cue is **“You begin to feel weary. It might be time to think about finding a nice safe place to sleep.”** After **400 units**, roughly eight turns, the warning becomes **“You're really tired now. You'd better find a place to sleep real soon.”** Another **135 units**, roughly three turns, produces **“If you don't get some sleep soon you'll probably drop.”** After **60 units**, roughly two turns, comes **“You can barely keep your eyes open.”** Another **50 units**, effectively one ordinary increment, makes sleep due.

From the first warning there are roughly fourteen ordinary increments before forced sleep, not an entire additional expedition. Route toward a dormitory early.

The reliable safe sleeping rooms are **Dorm A**, **Dorm B**, **Dorm C**, and **Dorm D**. Dorm A is south of **Rec Corridor**, Dorm B north; Dorm C is south of **Dorm Corridor**, Dorm D north. Use `enter bed`. While well-rested, entering merely places the player in a bunk. Once tired, it schedules sleep after **16 time units**, normally at the beginning of the next ordinary turn, and prevents getting out while that sleep is pending. Do not enter a comfortable bunk “just to inspect it” when tired and expect to leave immediately.

Collapsing anywhere inside one of these four dormitories automatically puts the player into a bunk. Elsewhere, an apparently sheltered room is not enough. Ground sleep has a **30 percent chance** of death by ferocious beasts, except where a specific drowning condition kills first. The beast risk is not restricted to outdoors or visibly dark rooms.

The **Infirmary Bed** is a trap, not an alternative to the dormitories. Entering it immediately triggers a malfunctioning diagnostic robot that straps the player down and administers all 347 of its serums and medicines before preparing surgery. There is no intervening escape turn. Do not confuse the useful medicine bottle on the shelf with treatment in that bed.

A request to sleep while rested can be refused with **“You're not tired!”**; a request in an unsuitable place can produce **“Civilized members of society usually sleep in beds.”** Sleep cannot simply be used to advance the calendar whenever convenient.

### What the night changes

A successful night advances the day, increases sickness by one level, restores the well-rested condition, and establishes the new morning clock and food deadline. Bed waking follows actual sickness: early on the player feels refreshed; at levels 3–6, weak and listless after restless sleep; at levels 7–8, weak and worn-out, with standing itself an effort.

The player remains in the occupied-bed position after waking. Use `get up` before ordinary navigation.

All directly carried items except those actually being worn are dropped onto the surrounding room’s floor. This is not theft. Recover important cards, tools, food, and medicine before leaving. A worn chronometer stays on the player.

Liquids make overnight handling more consequential. An **open canteen** containing protein liquid loses its contents when dropped by the waking sequence; closing it prevents that loss. A directly carried **flask** of chemical fluid loses its contents during the same process, with no closure exception supplied. These rules do not establish that every liquid container everywhere in the world spoils overnight, but they do make carrying a prepared chemical dose through sleep unreliable.

Dreams are narrative interludes, not rooms to solve. They can contain apparent deaths, impossible scenery, shipboard characters, and unavailable objects without changing the waking puzzle. Before Floyd’s activation there is a 60 percent chance of a normal dream. After he has ever been activated, sleep first has a 13 percent chance of his special dream; otherwise it has the ordinary 60 percent dream check. His dream remains possible after his death and is not evidence of resurrection.

A living, active Floyd rejoins the player on waking and comments on their bed or floor sleeping. That reunion does not recover the objects left on the floor for them.

### The Disease and the shrinking load

The long-term deadline is sickness, not starvation. The player starts at sickness level **1**, and each new day increases it. At **level 9**, the player dies: **“You finally succumb to the ravages of your illness and collapse.”** Without treatment, waking into Day 9 is fatal, however safely the player slept and however well supplied they are.

The general health descriptions are **“You are in perfect health”** at levels 1–2; **“You feel a bit sick and feverish”** at 3–4; **“You are somewhat sick and feverish”** at 5–6; and **“You are very sick and feverish”** at 7–8. Apparent perfect health at the beginning does not mean the Disease clock has not started.

There are also daily announcements tied to the calendar rather than actual sickness. Day 2 brings slight weakness and flushing; Day 3 unusual weakness and suspected fever; Day 4 a bad-flu comparison; Day 5 worsening fever and headache; Day 6 heat, weakness, and a throbbing head; Day 7 almost no strength; Day 8 burning fever, near immobility, and a reeling brain.

These notices have an unusual eligibility rule: they appear at the first ordinary check **at or below**, not after, their day’s threshold. The thresholds for Days 2–8 are respectively **1850, 2250, 2500, 2700, 3000, 3000, and 3000**. A notice can be missed; notably Day 8’s supplied morning time is already later than its notice threshold. Silence is therefore not recovery.

The **red spool** in Infirmary, read through the **spool reader** in **Library**, describes variable incubation, high fever, and sharply increased nightly sleep requirements, with death eight to ten days after symptoms. That is the medical background. The player’s actionable limit is the explicit sickness-level progression.

Carrying capacity is a size allowance, not a fixed number of objects. It begins at **100 units**, falling by **10** for every sickness level above the first: 90, 80, 70, 60, 50, 40, and finally 30. A newly taken object must fit within the current allowance.

The reduction is not described as forcing an immediate midday spill. Its cruelest effect is after waking: yesterday’s inventory is on the floor, the player is weaker, and the same load may no longer be collectable. Plan caches rather than carrying every souvenir. The verified route deliberately uses `drop all` in Storage West before carrying the ladder, and later leaves obsolete elevator and shuttle cards in Elevator Lobby.

Some containers have their own small limits. The Patrol uniform’s pocket holds one unit; the scrub brush occupies two and will not fit. The **cardboard box** has ten units of internal capacity and occupies three itself. The sources do not fully specify how nested contents contribute to the player’s overall size total, so do not promise unlimited carrying through containers. Floyd can hold one given item; additional gifts are dropped rather than stored.

### Medicine buys time, not a cure

The **medicine bottle** is on a shelf in **Infirmary**, northwest of **Systems Corridor West**. Its label reads **“Dizeez supreshun medisin -- eksperimentul”**—Disease suppression medicine, experimental. Use `read bottle` to read the label and `drink medicine` to consume the dose.

It tastes extremely bitter and rolls sickness back **two levels**, never below level 1. Where both levels can be recovered, it also restores **20 units of carrying capacity**. The whole dose is consumed at once. Taking it before two levels can be recovered wastes part of its possible benefit.

Medicine does not reverse the date, restore earlier waking times or fatigue allowances, drain the flooded coast, or refill itself. Calendar-based sickness announcements may still sound bad after actual health has improved. In the full-benefit case it postpones the fatal sickness transition by two days; it does not make indefinite survival possible.

The real cure requires the Project to finish. In Computer Room, `read output` shows research and production complete, testing at **99.985 percent**, and a projected revival time of **0 days, 0.8 chrons**, interrupted by **“Malfunkshun in Sekshun 384!”** That estimate is not a promise that the broken computer will heal itself after waiting. Repair is necessary.

### The rising ocean and other day changes

The ocean is a real calendar hazard. **Crag** is submerged from Day 2 onward. On Day 2, Balcony says the landing crag is underwater; going down now leads to **Underwater**, not a safe ledge. On Day 3, water laps at Balcony’s base. Balcony itself is flooded from Day 4 onward, when descending from Winding Stair also leads directly into Underwater.

On Day 4, Winding Stair reports water lapping below; from Day 5 it reports splashing against the steps. Courtyard also gains the sound of surf from the staircase from Day 5.

The specific fatal overnight thresholds are **Crag on Day 1**, **Balcony on Day 3**, and **Winding Stair on Day 5**. The warning becomes a wave washing over the sleeping player, followed by drowning before they can get their bearings. Do not camp on the lower cliff, and do not treat the lack of beasts there as safety.

Course Control explains the larger danger: Resida’s orbit has drifted, its climate is worsening, and the small ice cap and rising seas are symptoms. The verified repair is `open cube`, `take fused with pliers`, and `put good in cube`, using the **wide-nosed pliers** from Tool Room and **good ninety-ohm bedistor** from Storage East. The lights change to indicate that course divergence is minimizing. The supplied calendar flood rules do not establish that already scheduled local flooding reverses after repair; do not promise newly exposed land.

No supported day-by-day schedule makes reactors or life support newly fail on a particular morning. **Systems Monitors** initially shows **LIIBREREE**, **REEAKTURZ**, and **LIIF SUPORT** green, with communications, defense, course control, and Project Control malfunctioning. This is a status display, not a room in which the player can repair every warning by operating the equipment.

One smaller daily change concerns Floyd’s hidden **lower elevator access card**. On an eligible successful swipe of another card in his presence, his chance of showing it is 5 percent on Day 1, 10 percent on Day 2, 30 percent on Day 3, and certain from Day 4. The transcript gets the offer immediately and then uses `take lower card`. Do not promise that timing to every player: searching the switched-off robot is the deterministic alternative.

### Travel clocks and expiring authorization

The shuttle has a time-of-day restriction, not just a puzzle about direction. Activating it when the time is **greater than 6000** is refused because evening operation needs special authorization. Exactly 6000 is not excluded. This checks activation; it is not an established automatic shutdown of a journey already moving.

The verified outward departure is from **Alfie Control East**: `slide shuttle access card through slot`, `push lever`, then `pull lever`. This starts at speed **5** and returns the lever to neutral, maintaining that speed. Leaving the lever at “+” accelerates by five each turn; “−” decelerates by five each turn.

Higher speed does **not** shorten the journey: each moving turn advances the shuttle one stage regardless of speed. There are 24 moving stages before arrival resolution. Watch for the **“Limit 45”** sign, the halfway warning, the illuminated **15**, **10**, and **5** signs, and finally the announcement that the brightly lit station and concrete platforms are approaching. Immediately after that last cue, `pull lever` gives the verified clean stop.

Arriving at 5–20 crashes but does not kill; 25 or more is fatal. Stopping early strands the car between stations, where inactivity may let authorization expire. Reactivation then remains subject to the evening restriction. Racing gains nothing.

Ordinary elevators also require patience and renewed authorization. In Elevator Lobby, `press blue button` calls Upper Elevator and `press red button` calls Lower Elevator. Calls complete on the fourth summon update. After swiping inside a car, use the appropriate `press up button` or `press down button`; the verified rides then use two `wait` commands before exiting through the reopened door. Idle elevator authorization expires after six updates, and arrival leaves a shorter remaining enabled interval. Swipe again rather than assuming yesterday’s or the previous trip’s authorization persists.

Teleportation is the practical later shortcut. The card is in the lab uniform’s pocket. In Booth 3, `slide teleportation card through slot`, then `press 2`, reaches **Booth 2**, east of Elevator Lobby. The return uses the same swipe there and `press 3`. Each departure consumes authorization; otherwise it lasts nominally thirty countdown turns. **“The ready light goes dark”** warns of expiry when the player is present. Loose portable objects on the departure booth’s floor travel too.

These travel facilities do not provide safe substitutes for beds. Also remember that **Waiting Area** has no call button for a Lower Elevator left upstairs.

### Damage that keeps accumulating

The **curved metal bar** is a magnet with a per-turn consequence. While carried, it silently and permanently scrambles one intact carried access card per turn until none remain. Putting the magnet or cards inside carried containers does not shield them. A damaged card later reports **“Damejd kard...akses deeniid.”**

The verified route avoids this by dropping the magnet in Storage West before collecting access cards beyond the rift. Follow that principle. There is no advance warning, no descrambling operation, and no reason to keep the magnet once the key is recovered.

Radiation is another persistent clock, separate from the Disease. Entering **Radiation Lab** starts poisoning that does **not** stop when the player leaves. On the fifth counted turn the player feels sick and dizzy; on the sixth they vomit and lose their hair; on the seventh they die. There is no radiation suit, and the lab uniform is not protection. The **lamp** and **brown spool** are bait for an unproductive fatal detour, not essential equipment to retrieve quickly. Experimental Disease medicine is not an established radiation antidote.

Some failures need only one wrong action rather than a countdown. An incorrect chemical dose permanently disables the sending console. The verified repair is black, then gray: in Machine Shop use `put flask under spout`, `press black button`, `take flask`; in Comm Room use `pour fluid into hole`. Repeat with `press gray button` for the second dose. The liquid turns milky white in the flask but retains its original chemical identity. Adding another dose to an already full flask does not mix or replace it.

Likewise, removing the good bedistor after installation is fatal because it is now live. Solving a repair is not an invitation to dismantle it again.

### The short endgame deadlines

The last stages replace leisurely exploration with tightly constrained actions. Prepare food, rest, cards, and the laser beforehand.

For Floyd’s Bio Lab retrieval, first let him register the failure in **Computer Room**, then take him to **Bio Lock East** and `look through window`. Once he volunteers, the verified sequence is exactly `open door`, `close door`, `wait`, `open door`, `close door`. The knocks after the wait are the reopening cue. An extra turn with the door open lets the mutants kill the player; opening before the knocks is fatal; failing to reopen immediately after them kills Floyd without recovering the card. After the completed sacrifice, use `take miniaturization card`.

In **Miniaturization Booth**, `slide mini card through slot` gives about thirty turns of authorization; `type 384` is the correct destination. Another recognized integer transports the player into a live sector and kills them.

Inside the computer, use the fresh laser battery from Lab Storage. The transcript replaces the old one with `remove battery`, `drop battery`, `take fresh battery`, and `put battery in laser`. At **Strip Near Relay**, `set laser to 1` produces red light that passes through the red casing; `shoot speck with laser` must score two hits. The transcript hits twice consecutively, but the notes allow misses, so follow the hit messages rather than assuming exactly two shots always suffice. A non-red beam destroys the relay instead of safely repairing it.

The successful repair announces **“Sector 384 will activate in 200 millichrons. Proceed to exit station.”** The supplied mechanics implement a 200-turn activation countdown. Expiry kills anyone still in the computer interior with a surge of current. Leaving the computer, not merely admiring the repaired relay, is the goal.

On returning south to **Middle of Strip**, the microbe appears. Its arrival is a grace event; afterward two unrepelled advances are survivable and the next kills. Non-red shots hold it back for that turn but do not erase earlier advances or kill it permanently. Red shots pass through its membrane.

The verified response is `set laser to 2`, eight successive `shoot microbe with laser` commands, then `throw laser off strip`. Successful shots heat the laser by one; non-firing turns cool it by one. The microbe follows a laser thrown off the strip only when its warmth exceeds seven. Throwing it away too cool loses the weapon without clearing the route. Shooting indefinitely is also dangerous: sufficiently excessive heat provokes a fatal lunge. The eight-shot sequence is the transcript’s working response, not permission to ignore prior heating, cooling, or failed discharges in a different run.

After disposal, `S` to **Strip Near Station**, then `W`, automatically returns the player through Station 384 to **Auxiliary Booth**. The main booth’s failure is scripted; it is not evidence that the repair was unsuccessful.

### Fungicide and the final chase

From Auxiliary Booth, `N` reaches **Lab Office**, whose only escape is through Bio Lab. Prepare before starting the short protection window. The verified commands are `examine desk`, `read memo`, `open desk`, `take mask`, and `wear gas mask`. Carrying the mask without wearing it is insufficient.

The revival announcement says **“Cryo-chamber access from Project Control Office now open.”** It means the mural in **ProjCon Office** has moved and the final elevator is available. Use this announcement as the operational cue rather than relying on an assumed wait count.

`press red button` releases fungicide, then `open door`, `w`, `open lab door`, and `w` carry the masked player through **Bio Lab** to **Bio Lock East**. Opening the office door before activating protection is fatal.

The mist lasts only two ordinary advances after activation. Entering Bio Lab and opening its western lab door have special grace treatment for this clock; opening the office door does not. These exceptions explain why the verified passage works. Extra examinations do not share that protection. Expiry inside Bio Lab lets the mutants surround and eat the player.

When the mist disappears after reaching Bio Lock East, pursuit begins. The exact verified escape continues:

`w` to **Bio Lock West**; `open door`; `w` to **Main Lab**; `w` to **Project Corridor East**; `w` to **Project Corridor**; `s` to **ProjCon Office**; `s` to **Cryo-Elevator**; `press button`.

During the chase, an ordinary stationary turn kills, and immediate backtracking runs straight into the pursuers. Bio Lock West grants one stationary allowance to open its door, accompanied by **“The mutants are almost upon you now!”** Closing the inner lab door is not a reliable pursuit-stopper. Do not pause for Floyd’s body, dropped equipment, a room examination, or combat.

The Cryo-Elevator button closes the door just in time and ends pursuit. The verified playthrough then uses `z` twice before the arrival message. Once the door opens, go `N` to **Cryogenic Anteroom** for the ending. Do not press the button again: a second journey returns the player to the waiting mutants and is fatal.

### Coaching the clocks

The practical rhythm is preparation followed by commitment. Secure kitchen access before the survival kit runs out. Keep the watch worn, treat the first fatigue warning as a routing decision, close the canteen before sleep, and reclaim essential equipment after waking. Use the experimental medicine for its actual two-level reprieve, not as a substitute for completing the Project.

Distinguish warnings that allow planning from cues demanding the very next action. Hunger escalates in several stages; Floyd’s knocks, the shuttle’s station approach, the fading fungicide, and the final elevator entrance do not permit casual experimentation.

The supplied port also has explicit testing commands such as `god mode no hunger`, `god mode no sleep`, and `god mode no survival`, with corresponding `... on` forms to restore those clocks. They consume a turn and are not ordinary puzzle solutions. They do not establish immunity to Disease, radiation, mutants, or other deaths. Likewise, resetting displayed time is not resetting the calendar or every pending event. If testing conveniences are in use, keep them conceptually separate from the survival rules the friend is meant to understand.


# The story, and when the player can know each part of it

## The story, and when it becomes knowable

### The truth behind the wreck

Planetfall begins as a shipwreck story and becomes a rescue of an entire civilization. The player is an Ensign Seventh Class aboard the **S.P.S. Feinstein**, doing menial work under **Ensign First Class Blather**. The Feinstein has interrupted its voyage to investigate a remote system that archaeologists believe may once have belonged to the Second Union. It attempts to contact the planet, receives no answer, and is destroyed.

The planet is **Resida**. Its deserted buildings are not evidence that everyone has died or fled. A planetwide plague, **the Disease**, drove the population into cryogenic suspension while an automated undertaking called **the Project** searched for a cure. The **Kalamontee** and **Lawanda** complexes were built on high mountain plateaus, with enormous reactors and cryogenic chambers beneath them. Their machinery was supposed to maintain the sleepers, conduct research, and eventually revive and inoculate everyone.

The research came extraordinarily close to completion before the main computer failed. Its last report records completed research and drug production, with drug testing at **99.985 percent**. It summoned a repair robot, but **Achilles**, the machinery-repair robot, lies broken at the foot of the stairs in **Repair Room**. Meanwhile other essential systems have failed. Communications cannot transmit the distress message; planetary defenses cannot discriminate properly between targets; and course control is allowing Resida’s orbit to deteriorate. The player must restore the machinery that the absent population can no longer repair.

The strongest supported explanation for the Feinstein’s destruction is the failed discrimination circuit in the automatic meteor defenses. The official hints explicitly call this *probably* the cause. The supplied material does not show a recording of the weapon firing at the Feinstein or a final confession explaining its destruction. Present it as the intended, strongly supported inference, not as an eyewitness fact. A bad ending demonstrates that those same malfunctioning defenses can destroy a second Patrol ship.

The player also contracts the Disease. Thus finding food, sleeping safely, and keeping machinery working are not separate from the story: the stranded ensign is living through the same emergency that sent Resida into suspension. Repairing the computer lets the Project finish its cure; repairing course control saves the planet; repairing communications and defense makes rescue possible. **Floyd** makes the computer repair possible at the cost of his life, and only the complete-victory ending returns him restored.

That is the guide’s full understanding. None of it should be delivered wholesale to someone who has merely climbed out of the ocean.

### How to control spoilers

Revelation is not a single fixed sequence. The opening, Floyd’s sacrifice, the computer repair, and the ending have strong chronological constraints, but much of the explanation is optional reading. A player may repair **Planetary Defense** before visiting the library, or inspect the **computer output** before learning what the Project means.

Track what has actually been seen, read, and heard—not merely which region has been entered. Reaching **Library Lobby** does not mean the player knows the Project’s four phases. The verified playthrough, for example, activates the terminal but reads a Zork article rather than the historical and Project articles. It also visits **Comm Room** without reading its distress message or playing its recording.

The commands below preserve the verified playthrough’s wording where it demonstrates an interaction. Optional reading commands and terminal selections not exercised in that run are supported by the study notes and identified as such. The supplied playthrough stops when the **Cryo-Elevator** opens at its lower landing; the final northward step and ending revelations are established by the ending notes, not by a displayed final scene in that transcript.

Most Residan writing appears to the player in corrupted, phonetic English, described initially as a corrupt form of **Galalingua**. The official decoding rule is that **x** substitutes for **th**, **c** for **ch**, double vowels generally indicate long sounds, and single vowels short sounds. Thus **“Xe Dizeez”** is **“The Disease”**, and **“Xe Prajekt”** is **“The Project.”** The translations here are for the guide’s understanding; players encounter the corrupted spelling.

### Before the explosion

On **Deck Nine**, the opening already establishes the player’s low rank, deck-scrubbing assignment, and relationship with Blather. It does not establish why the ship is about to explode. Blather’s bullying and any visit by the ambassador are characterization, not evidence of sabotage.

The **diary** supplies the personal and mission background. The notes establish `read diary`, followed by `press more` to advance after each entry has been read. Repeated reading alone does not advance it. These commands are not used in the verified run, and reading the entire diary before the emergency is unsafe: the first explosion occurs on move ten. The diary is a starting possession and can be read later if retained.

Its dated entries reveal the background progressively:

The **July 22, 11344** entry explains the transfer from the **S.P.S. Trilobyte** to the Feinstein for the third of four tours. The previous commander, **Ensign First Class Lim**, was friendly; Lim came from **Ash-Down V**, while the player comes from **Gallium**. Even sharing a cabin with five other ensigns seems luxurious to the player.

The **July 23** and **July 25** entries establish Blather’s character: the initially doubtful first impression becomes certainty after he confiscates Double Fanucci sets under a regulation that another ensign cannot find. The later grotch entries explain how an application to feed animals became an assignment to clean their cages instead. The Feinstein is carrying grotches collected on **Crassus** to zoology laboratories on **Tremain**, where their astonishing production of trot is to be studied.

The **Bozbar 26–28** and **August 2** entries add the diary’s concealment from Blather and the diplomatic business of the voyage. **Accardi-3** is called something like **Blow’k-bibben-Gordo** by its inhabitants; it is not formally part of the Union, and its ambassador is being taken to Tremain for membership negotiations. These details explain the opening visitor, not the planetary catastrophe.

The **August 7** entry describes compulsory Patrol instructional films, including one about avoiding alien diseases. It is background irony, not a means of preventing infection on Resida. **August 24** explains the player’s frustration at being denied astrophysics training and assigned remedial scrubbing despite five generations of family service. The great-great-grandfather was a High Admiral and a founding father of the Patrol.

The decisive mission clue is **Septem 4**: the Feinstein left hyperspace at about 7600, roughly two weeks early, to investigate a remote system that archaeologists on **Varshon** believe may have belonged to the **Second Union**. This establishes why the ship is here. **Septem 5** explains the immediate assignment: missing two pellets of trot earned extra deck-scrubbing shifts, including filthy Deck Nine.

The ambassador’s **brochure** advertising Planetfall as his world’s leading export is a fourth-wall joke. It is not an explanation of the setting. Nor should the guide invent individual opening-scene fates: the ambassador disappears from reach, and Blather’s survival is not established until the complete ending.

### What the escape reveals

The verified opening uses ten `wait` commands on **Deck Nine**, then `port` to enter **Escape Pod**, followed by `sit`. Further `wait` commands let the automated evacuation unfold. This matters narratively because the player is escaping, not piloting, repairing, or destroying the ship.

Through the viewport, the player sees bursts of light along the Feinstein’s hull and then an enormous explosion that blows it into pieces. This confirms the ship’s destruction, but not its cause. Neither the inaccessible Hyperspatial Jump Machinery Room nor the ship’s reactor supplies a preventive solution.

The pod then reveals three important environmental facts before landing: the nearby planet appears almost entirely ocean; it has only a few visible islands and an unusually small polar ice cap; and the pod judges it **human-habitable**. Finally, it approaches the nearer of two islands, a sheer-sided plateau covered with buildings. At this point the player does not yet know the name Resida, the history of those buildings, or why the ocean is so extensive.

After landing, the verified sequence is `take kit`, `open door`, `out`, and `up` from **Underwater** to **Crag**. There is no need to add a standing command: despite the official hints’ contrary claim, the playthrough opens the door and leaves without one.

A later sleep may produce a dream in which the player accidentally activates the Feinstein’s self-destruct while scrubbing a control panel. **That is a dream, not the solution to the shipwreck mystery.** Dreams do not supply reliable historical evidence or playable scenes. Do not let a player’s understandable guilt become the guide’s explanation.

### The first evidence on shore

From **Crag**, `up` reaches **Balcony**. Here the player first sees the **weathered metal plaque** and is told that its language resembles corrupted Galalingua. The notes establish reading or examining the plaque. Its inscription begins **“Xis stuneeng vuu uf xee Kalamontee Valee…”** and means:

“This stunning view of the Kalamontee Valley covers over forty square miles of that famous tourist spot. The large building at the bend in the Gulmaan River is the former provincial capitol building.”

The present view is ocean. That discrepancy is the earliest strong evidence that the landscape has changed catastrophically. It also supplies **Kalamontee** as a place-name before the shuttle signs or library explain the complex. It does not yet explain whether the land sank, the sea rose, or some other disaster occurred.

The route `up`, `up` continues through **Winding Stair** to **Courtyard**. The ancient, ruined castle contrasts with the more modern but dusty construction of **Plain Hall**. Deserted dormitories, empty working spaces, stopped walkways, rust, and disassembled robots establish prolonged neglect. They do not prove a massacre. The right early answer to “Where is everyone?” is that the installation appears deserted and some of its machinery still operates automatically.

The rising sea becomes observable over successive days. **Crag** is submerged from day two. **Balcony** describes water below on day two and lapping at its base on day three; it is flooded from day four. **Winding Stair** later reports water immediately below. These are real changes, not merely a picturesque ocean description. They corroborate an ongoing environmental disaster, but the orbital explanation remains a later deduction.

The optional **Observation Deck**, reached southwest from **Tower Core**, gives the clearest overview: the tower is about half a kilometer high, the complex occupies an island, and a similar island lies about twenty kilometers east. **Plan Room**, east of **Admin Corridor North**, names the two complexes on its maps: **“Kalamontee Kompleks”** and **“Lawanda Kompleks.”** The Lawanda map includes a deeply buried installation. These clues can suggest unseen underground facilities before the player knows they contain sleepers.

### Floyd and the absent workforce

In **Robot Shop**, the verified command is `activate floyd`. Its immediate reply is “Nothing happens,” but two `wait` commands bring the robot to life. His greeting identifies him as **B-19-7**, called **Floyd**, and asks whether the player is a “doctor-person” or a “planner-person.” The notes also support activating him as “robot” before the player knows his name.

His language is the first personal hint of the people who worked here: doctors, planners, and robots formed the installation’s community. His childlike enthusiasm should not be mistaken for a complete account of the disaster. He is a companion and witness to individual memories, not an encyclopedic historian.

The clearest explanation of failed maintenance comes later in **Repair Room**, reached north from **Systems Corridor West**. On entering with Floyd, the verified playthrough shows him identifying the broken robot as **Achilles**, who repaired machinery and once repaired Floyd. Achilles had trouble with one foot; Floyd thinks he fell down the stairs. His position at their foot supports that account, but Floyd’s explanation is still phrased as an apparent accident.

The same room demonstrates why Floyd is indispensable. `floyd, go north` sends him through the robot-sized opening; after he reports finding a shiny fromitz board, `floyd, take board` retrieves it. The player cannot use that doorway. This is not merely companionship layered over independent repair puzzles: a robot’s access and assistance replace maintenance capabilities that the complex has lost.

The optional **Infirmary** scene reveals another personal casualty. On the second qualifying turn there with Floyd, he discovers the remains of **Lazarus**, his medical-robot friend, and the **medical robot breastplate** appears. The breastplate is bent and rusting, with connected circuitry. The sources do not explain exactly how Lazarus was destroyed. Neither Lazarus nor Achilles can be restored by the player through a demonstrated repair procedure.

### The warnings in Kalamontee

**Systems Monitors**, west of **Admin Corridor**, can be visited before crossing the rift. Its room description already distinguishes working and malfunctioning systems; the notes also support `read monitors`.

Initially **“LIIBREREE,” “REEAKTURZ,”** and **“LIIF SUPORT”**—Library, Reactors, and Life Support—are green. **“KUMUUNIKAASHUNZ,” “PLANATEREE DEFENS,” “PLANATEREE KORS KUNTROOL,”** and **“PRAJEKT KUNTROOL”**—Communications, Planetary Defense, Planetary Course Control, and Project Control—report faults.

This is the earliest explicit statement that the installation’s failures extend beyond ordinary decay. Nevertheless, the names alone do not explain the Project or establish which faults caused which disasters. In the verified run, the player checks this room only after repairing communications, defense, and course control, so only Project Control remains faulty.

The most important early historical testimony is in **Comm Room**, northeast of **Tower Core**. Access in the playthrough uses the **upper elevator access card** from **Small Office**: inside **Upper Elevator**, `slide upper access card through slot`, `press up button`, then `wait` twice, `S` to **Tower Core**, and `NE` to **Comm Room**.

The left console is **“Reeseev Staashun”**, Receive Station. **“Tranzmishun Reeseevd”** means Transmission Received; **“Mesij Plaabak”** means Message Playback. The optional, notes-supported `press playback button` plays the Feinstein’s last transmission:

“Stellar Patrol Ship Feinstein to planetside … Please respond on frequency 48.5 … SPS Feinstein to planetside … Please come in …”

The communications officer then begins telling the admiral that there is no response on the standard frequencies. An explosion interrupts him, followed by static and silence. This proves that the ship was attempting contact immediately before destruction. It is a recording, not an opportunity to save or converse with the Feinstein.

The right console, **“Send Staashun”**, is the Send Station. The optional `read screen` reveals:

“To any ship of the Second Galactic Union: A planetwide plague has struck the entire population. Time is critical. Emergency assistance requested. <repeat message>”

The player sees **“Tuu enee ship uv xe Sekund Galaktik Yuunyun…”**, not this translation. This is an important spoiler boundary: **the plague can be learned here, before Lawanda or the library**. The message does not yet say that the population is frozen, that cryogenic research released the Disease, or that a cure is almost finished. Its appeal to the Second Galactic Union also links the ruins to the diary’s archaeological clue, without settling Resida’s entire history.

The sending system is broken. In the verified repair, each dose is collected in **Machine Shop** with `put flask under spout`, the appropriate button, and `take flask`, then delivered in Comm Room with `pour fluid into hole`. The sequence is `press black button` for the first dose and `press gray button` for the second. The black and then gray warning lights specify the required chemicals. The second delivery explicitly announces that the help message is now being sent. That confirms restored communication, not an immediate rescue.

### The player’s illness

Illness is revealed by elapsed days rather than a mandatory room visit. On day two an eligible morning notice says the player feels weak and slightly flushed without knowing why. On day three the player suspects a fever. Later notices describe flu-like illness, increasing fever, headache, and severe weakness. Sleep requirements increase and useful waking time contracts; carrying capacity also diminishes.

These symptoms alone do not identify their cause to the player. Once the Comm Room’s plague message has been read, infection is a reasonable inference. More specific confirmation comes through the medical information in Lawanda. Do not tell someone experiencing the first flushed feeling about the cryogenic research accident unless they have read its account.

The **medicine bottle** in **Infirmary** is labelled **“Dizeez supreshun medisin -- eksperimentul”**: **“Disease suppression medicine—experimental.”** The notes support `read bottle` and `drink medicine`. The single dose suppresses the illness by two stages; it does not cure it or reverse the calendar. Without treatment, the disease becomes fatal on the transition to day nine.

The **red spool**, also in Infirmary, is labelled **“Simptumz uv Xe Dizeez”**, Symptoms of the Disease. Its information becomes available at the **spool reader** in **Library**, west or up from **Library Lobby**. The notes-supported commands are `put red spool in reader` and `read reader`; another spool must first be removed if one is loaded.

The article says incubation varies from one day to several rotations. Once symptoms appear, death always follows in eight to ten days. High fever is the primary symptom, sharply increased nightly sleep the secondary symptom. This is the document that connects the player’s declining condition to the named Disease. It supplies prognosis, not a remedy. The long-term hope is completion of the Project, not repeated sleep or further doses from the empty bottle.

### What the library can explain

The **computer terminal** in **Library Lobby** is the principal historical source. It is not the damaged Project computer in **Computer Room**. In the verified run, `activate terminal` turns it on; `key 4 on the terminal`, `press 0 on the terminal`, `type 6`, `type 1`, and `type 0` demonstrate its numeric navigation. `deactivate terminal` switches it off.

The initial headings are **“Histooree,” “Kulcur,” “Teknolojee,” “Jeeografee,” “Xe Prajekt,”** and **“Inturlajik Gaamz”**: History, Culture, Technology, Geography, The Project, and Interlogic Games. The article selections below come from the study notes. Each pair means two separate commands from the main menu—for example, `type 5`, then `type 1`. From an article, `type 0` returns to its subject menu; another `type 0` returns to the main menu.

**History, 1 then 1: Racial Origins.** Ancient legends say ships of the Second Union once filled Resida’s skies and would return. Scientists formerly favored native evolution but now believe the planet was settled by people of the Second Union. Preserve that evidential distinction: the article reports the modern conclusion, while the promised return belongs to legend. Reading it names Resida and connects its people with the wider human history suggested by the diary.

**History, 1 then 2: Great Hiatus.** Archaeologists are certain that an advanced technological and social civilization existed thousands of years earlier, then declined into a dark age lasting centuries. The cause is unknown. Do not identify the Disease as the cause of this much older collapse.

**History, 1 then 3: Rise of the New Technocracy.** During the last five centuries, civilization recovered to its pre-Hiatus level. The Disease struck when Resida had regained that sophistication. The plague is therefore the catastrophe of a recovered technological society, not simply another name for the ancient dark age.

**The Project, 5 then 1: Origins of the Disease.** The **Center for Advanced Cryogenic Research** was trying to extend the cryogenic period indefinitely. Its research succeeded, but somehow released the Disease, which spread. This is the first direct explanation of the plague’s origin. The sources do not identify a pathogen, precise laboratory mechanism, deliberate release, or guilty individual. The irony is supported; a more elaborate conspiracy is not.

**The Project, 5 then 2: The Installations.** Kalamontee and Lawanda were built on twin mountain-peak plateaus. Their elevation facilitated transportation and communications and allowed enormous reactors and cryogenic chambers beneath them. This explains why the apparent islands have such extensive installations and why so much of Lawanda remains unseen underground. It does not mean they were originally small ocean islands.

**The Project, 5 then 3: Project Control.** This is the decisive answer to “What happened to everyone?” Phase One built the two complexes; Phase Two cryogenically froze the Residan population; Phase Three monitored the sleepers while extraordinarily sophisticated computers conducted automated research; Phase Four would revive and inoculate everyone. The population is waiting in suspension, not simply missing. The deserted accommodation belongs to an undertaking whose human participants no longer need to occupy its surface rooms.

The **ProjCon Office** logo and the matching logo on the **lab uniform** reinforce this explanation: a flame burns above a sleep chamber, with the office inscription **“Prajekt Kuntrool”**, Project Control. Examining either logo can suggest suspended life before the terminal provides the full explanation. The office’s rippling **mural** and Lawanda’s disproportionately large **Physical Plant** similarly hint at hidden space. None alone reveals the complete cryogenic plan.

### Resida before the disaster

The remaining terminal articles describe the civilization being saved. They are optional revelations, available as soon as the terminal is reached and used, not facts a newly landed player automatically knows.

**Technology, 3 then 1: Medicine** says all major diseases had been curable for more than a century, with cryogenic stasis available while doctors sought a cure, and average life expectancy at **147 revolutions**. Read this as the medical background against which the new Disease was an exceptional crisis. It does not make the experimental bottle a cure.

**Technology, 3 then 2: Agriculture** describes near-obsolete dirt farming, with most food produced by hydroponics and underwater algae farms. **3 then 3: Transportation** describes private scooters, longer-distance airbuses, and nuclear-fueled engines revolutionizing space travel. **3 then 4: Robotics** explains that miniaturization lets a single multipurpose **B-19** perform functions once divided among whole teams of specialized robots. This contextualizes Floyd without changing his capabilities into unlimited technical expertise.

**Technology, 3 then 5: Planetary Systems** explains the major machines: Course Control maintains an ideal climate; Defense destroys dangerous meteors; the more recently added Project Control monitors the Project. This is the explanatory link between the ominous system names and the world’s survival.

**Geography, 4 then 1: Planet Landmasses** says that since stabilization of Resida’s orbit, **47.79 percent** of its surface is land. The two main landmasses are **Andoor** and **Fruulik**, with six smaller ones. The global capital, **Pilandoor**, is on Andoor’s eastern coast. This plainly does not match the nearly ocean-covered world seen from the pod. The article is an account of the stabilized world, not a trustworthy description of present sea level.

**Geography, 4 then 2: Undersea Regions** says the first underwater habitats opened in **2992** and, nearly two centuries later, about nine percent of the population lives in twenty undersea cities. **4 then 3: Space Colonies** places settlements on **Fristin** and several moons of the gas giant **Blustin**, while most off-world residents occupy colonies at Resida’s Trojan points. These are background settlements, not reachable rescue destinations. The sources do not separately explain the fate of each off-world colony or how its residents fit into the mass-freezing operation.

**Culture, 2 then 1–3**, respectively, describes preserved literature, some attributed to the mythical Second Union period; the Primitive, post-Hiatus Renaissance, and recent video-and-laser periods of art; and recordings of important music from the last five centuries. These establish continuity of culture through collapse and recovery. They do not turn the ProjCon mural into an art-history puzzle.

The **Interlogic Games** articles are jokes about Zork, Deadline and Witness, and Starcross and Suspended. Likewise, the **Ballad of the Starcrossed Miner** belongs to Floyd’s characterization and the game’s intertextual humor, not to a hidden explanation of Resida’s plague.

### Why the ship died and the sea is rising

On first entering **Planetary Defense**, north of **Systems Corridor**, the player sees:

**“Surkit Boord Faalyur. WORNEENG: xis boord kuntroolz xe diskriminaashun surkits.”**

This means **“Circuit board failure. WARNING: this board controls the discrimination circuits.”** Even before library research, it suggests that the defenses cannot distinguish appropriate targets. Combined with the terminal’s explanation that they destroy meteors, and the Comm Room recording of an unanswered ship being destroyed, it supports the intended explanation of the Feinstein’s loss.

The verified repair is `open panel`, `take second`, and `put shiny in panel`, using the **shiny fromitz board** Floyd retrieved. The warning lights stop flashing. The failed **second board** becomes the **fried fromitz board** once removed. The meaning of this repair is not merely that a local machine is green again: incoming rescue traffic must not be mistaken for a target.

In **Course Control**, north of **Systems Corridor East**, the warnings are **“Bedistur Faalyur!”** and **“Kritikul diivurjins frum pland kors”**: **“Bedistor failure!”** and **“Critical divergence from planned course.”** The library’s climate-control and orbital-stabilization articles make these planetary, not shuttle-navigation, warnings.

The official hints explain the intended deduction: Resida was moved into a more favorable but less stable orbit, and course control keeps it there. A drift toward the sun explains melting ice caps and rising seas. The final doomed-planet ending confirms the ultimate consequence: the orbit can decay beyond correction, and Resida will plunge into its sun. Before that ending, distinguish the observed facts and intended inference from a witnessed astronomical measurement.

The verified repair opens the **large metal cube** with `open cube`, removes its **fused ninety-ohm bedistor** with `take fused with pliers`, and installs the **good ninety-ohm bedistor** with `put good in cube`. The replacement comes from **Storage East**; the **wide-nosed pliers** come from **Tool Room**. The lights change to indicate correction. This repair saves the planet, independently of whether the player can later leave it.

### How close the Project came

In **Computer Room**, south of **Project Corridor East**, the **pile of computer output** supplies the missing operational history. With Floyd present, entry also produces his concern that the computer is broken and a Doctor-person told him it was the Project’s most important part.

The verified `read output` displays the final page. In plain English it reads:

> Daily Status Report:  
> Preliminary research: 100.000%.  
> Intermediate research: 100.000%.  
> Final research: 100.000%.  
> Drug production: 100.000%.  
> Drug testing: 99.985%.  
> Projected time to revival procedure: 0 days, 0.8 chrons.  
>   
> Alert! Alert! Malfunction in Section 384! Summoning repair robot.

The player sees **“Daalee Statis Reeport,” “Drug Testeeng,”** and the other corrupted forms. The report ends at the alarm.

This proves that the Project was almost finished, not that it still required the player to invent a drug. Once Achilles has been found, the guide can explain why summoning a repair robot did not save it. If the printout is read first, the absence of that repair response remains a question until Repair Room is investigated.

The report supplies **384**, the number required for internal repair. The much larger human catastrophe has been prolonged by a minute obstruction in a relay. That contrast is central to the story: vast automated capability is helpless without one small act of maintenance.

### Floyd’s sacrifice

Do not reveal the sacrifice merely because the player has befriended Floyd or reached Lawanda. Its immediate setup occurs only after he understands the computer failure and sees the card beyond **Bio Lock East**.

In the verified route, `open biolock door` in **Main Lab**, followed by `SE` and `E`, reaches the inner lock. `look through window` shows the dim **Bio Lab**, ominous moving shapes, a blue glow through the northern crack, and a magnetic-striped card just inside the door. Floyd identifies it as needed to fix the computer and volunteers to fetch it because robots are tough.

The working sequence is exactly `open door`, `close door`, `wait`, `open door`, `close door`. The wait is for the three knocks. Floyd returns with the **mini-booth card**, then collapses and dies after the player shuts out the mutants. `take miniaturization card` collects what he dropped.

The biological creatures are uncontrolled experiments, not evidence that the entire population became monsters. The **Radiation Lab** beyond the cracked wall contains split radioactive canisters and supplies the blue glow, but the sources do not establish a precise causal history for every mutation. Do not add one.

Floyd’s death is real within the playable adventure. His switch cannot bring him back, and the player cannot repair or carry him away. The player sings his favorite ballad while holding him. Promising his eventual restoration here would spoil both the loss and a reward that depends on achieving the complete ending.

### Completing the cure

The verified commands in **Miniaturization Booth** are `slide mini card through slot` and `type 384`. They miniaturize and teleport the player to **Station 384**. The route `E`, `N`, `N` reaches **Strip Near Relay**.

The notes-supported examination of the **microrelay** explains the actual fault: an impurity has wedged into its contact point, preventing closure. A microscopic blue speck appears boulder-sized to the reduced player. The red plastic casing requires a red beam. The verified repair uses `set laser to 1` and two successful `shoot speck with laser` commands. Other runs can miss and need further shots. Destroying the speck starts the announcement that Sector 384 will activate in 200 millichrons.

The creature encountered on returning to **Middle of Strip** is explicitly an ordinary microbe, enormous only at the player’s scale. It is not revealed as the Disease organism. In the verified run, `set laser to 2`, eight `shoot microbe with laser` commands, and `throw laser off strip` heat and sacrifice the weapon; the microbe follows it into the void.

Returning through **Strip Near Station** to **Station 384** automatically transports the player out. Because the main booth has malfunctioned, the destination is now **Auxiliary Booth**. This failure forces the final escape through the Bio Lab rather than providing a convenient return to Computer Room.

In **Lab Office**, `examine desk` and `read memo` reveal that an emergency system floods the Bio Lab with deadly fungicide and requires precautions. `open desk` reveals the **gas mask**. In the verified run, the revival announcement occurs after `take mask`:

**“Revival procedure beginning. Cryo-chamber access from Project Control Office now open.”**

This is the first explicit confirmation that the repaired Project has advanced to revival and that the concealed route in ProjCon Office is available. It does not yet reveal who will awaken or what the final rewards will be.

The escape uses `wear gas mask`, `press red button`, `open door`, `w`, `open lab door`, and `w` into **Bio Lock East**. The mist buys passage; it does not permanently kill the mutants. The verified chase continues `w`, `open door`, `w`, `w`, `w`, `s`, `s` to **Cryo-Elevator**, then `press button`. The doors shut against the pursuers and the elevator descends. Two `z` commands in the supplied run bring the arrival message. The notes establish the final `north` into **Cryogenic Anteroom**. Do not press the elevator button again after arrival.

### What the ending means

**Cryogenic Anteroom** presents the ending rather than another exploration puzzle. A medical robot opens a cryo-unit and injects its occupant. A red-haired, middle-aged woman rises and studies the controls: **Veldina**, leader of Resida. Her identity, the actual awakening of the population, and the outcome of the repairs belong to this final revelation.

With **communications, planetary defense, and course control all repaired**, other cryo-units open and Veldina thanks the player for completing the cure and restoring the systems. A landing party from the **S.P.S. Flathead** arrives. **Captain Sterling** promotes the player to **Lieutenant First Class**. Blather, rescued from an escape pod, is revealed to have survived; he is demoted to **Ensign Twelfth Class** and assigned as the player’s personal toilet attendant. The people offer the player leadership of Resida, and a medical robot administers the Disease antidote.

Robot technicians then return a restored Floyd. His **helicopter key**, **reactor elevator card**, and **paddleball set** are ending rewards and a sequel joke, not evidence that those inaccessible routes should have been solved earlier. His restoration is not something the player could have performed after the Bio Lab scene.

The lesser endings explain why the three other repairs matter separately.

If **course control is repaired but planetary defense is not**, Resida and the cure are saved, but the defenses destroy the second Patrol ship. The player may be stranded forever, receiving an unlimited bank account and a country house. This explanation takes precedence if communications is also broken.

If **course control and defense are repaired but communications is not**, the searching rescue ship cannot establish contact and gives up. Resida survives, but the player again faces being stranded, with the same compensation.

If **course control is not repaired**, completing the cure is too late to save the planet’s orbit: Resida will fall into the sun. If both communications and defense work, the Flathead can still rescue the player from the doomed world. If either is also broken, there is no rescue passage in the ending.

The ending therefore resolves three different questions: **Can the sleepers be cured? Can their world survive? Can the player be rescued?** Restoring the main computer answers the first; course control answers the second; communications and defense together answer the third. Only the complete success also delivers the full personal reversal—from bullied deck-scrubber to honored rescuer—and Floyd’s return.

For a guide, that is the final discipline: help players understand the evidence they have earned without giving them the conclusion before they reach it. Early on, they are survivors on an inexplicably empty world. Investigation reveals a civilization waiting for help. At the end, their repairs determine whether that waiting finally ends in recovery, isolation, or the loss of Resida itself.


# Geography: the places and how they connect

# Geography: the places and how they connect

## The shape of the world

Think of the playable world as a succession of connected regions: the doomed **S.P.S. Feinstein**; the landing cliff and ruined castle; **Kalamontee**, with its residential corridor, administrative and mechanical wings, tower, and underground shuttle station; **Lawanda**, with its systems corridor, library, project offices, and laboratories; the miniaturized computer interior; and the final cryogenic area.

The two indispensable navigation hubs are **Corridor Junction** in Kalamontee and **Library Lobby** in Lawanda. Corridor Junction connects the residential rooms to the administrative wing, mechanical wing, and elevators. Library Lobby joins Lawanda’s northern systems corridor to its southern project corridor and contains the entrance to **Booth 3**. Once the teleportation access card is acquired, travel between these hubs becomes much easier.

Do not assume passages are geometrically symmetrical. From **Main Lab**, for example, southeast enters **Bio Lock West**, but west—not northwest—returns from that lock to Main Lab. The stopped **Escalator** is traversed east and west, despite being a staircase. Two different rooms are both called **Physical Plant**; their exits identify which complex the player occupies.

The distant places described in documents are not additional destinations. The library describes Resida’s continents **Andoor** and **Fruulik**, its capital **Pilandoor**, undersea cities, and off-world settlements. None is reachable on the playable map. The library’s account of a planet with 47.79 percent land contrasts with the nearly ocean-covered world seen during the landing. The Balcony plaque likewise describes a valley where the player now sees ocean. These are geographical evidence of catastrophe, not invitations to look for an unlisted route.

## The Feinstein and the one-way escape

**Deck Nine** is the starting corridor. **Up** leads to **Gangway**, **east** to **Reactor Lobby**, and **west**, **in**, or the verified command `port` enters **Escape Pod** when its bulkhead is open. The pod door is initially closed. Blather and the alien ambassador can visit this corridor; the ambassador’s slime and brochure are incidental, not navigational tools.

**Gangway** is the metal stair between Deck Nine below and **Deck Eight** above: **down** returns to Deck Nine and **up** reaches Deck Eight. Deck Eight is another corridor; **down** is its only usable escape route. Blather blocks its north, east, and west routes. The **Hyperspatial Jump Machinery Room** mentioned ahead is not accessible.

**Reactor Lobby** lies east of Deck Nine, with **west** returning. Blather blocks **east** toward the **Ion Reactor** and **south** toward the **Auxiliary Control Room**. Neither is a playable extension of the map. The reactor is scenery, not a means of preventing the disaster.

**Brig** is reached through Blather’s punishment rather than ordinary successful movement. Its south door is locked, and there is no escape. The graffiti are its only notable diversion. A player imprisoned here cannot get back to the pod.

The first explosion occurs on move ten and opens the pod’s port-side door. On the next move the emergency bulkheads cut off the gangway and Reactor Lobby. The pod boarding window then closes. The verified opening is ten `wait` commands on Deck Nine, followed by `port`, then `sit`. Do not guide anyone away from Deck Nine during this emergency.

**Escape Pod** contains the safety webbing and entirely automated controls. Before departure its open exit leads back to Deck Nine; after landing, `out` leads to **Underwater**. There is no piloting puzzle. Use `sit`, remain seated through the flight, and use `wait` until the landing reveals the **survival kit** and **towel**. The verified departure from the landed pod is `take kit`, `open door`, `out`. Contrary to the hint book’s claim, this playthrough opens the door and leaves without first standing. That matters: explicitly standing starts the pod’s sinking countdown. Take supplies before leaving, and do not linger.

The ship and pod are not regions the player can revisit after reaching the island.

## The landing cliff and ruined castle

**Underwater** has one mapped exit: **up** to **Crag**. There is nothing to collect. Currents and sharp rocks make this a timed death area; use `up` immediately. Returning underwater later is especially dangerous because the original grace period does not reset.

**Crag** is the cleft where the pod damaged the cliff edge. **Up** reaches **Balcony**; **down** returns underwater. **Balcony** is the octagonal, shattered-windowed structure built into the cliff. **Up** leads to **Winding Stair** and, initially, **down** returns to Crag. Its **weathered metal plaque** describes the former Kalamontee Valley and the provincial capitol beside the Gulmaan River. The discrepancy between that description and the present ocean is the room’s important information.

**Winding Stair** connects Balcony below to **Courtyard** above. The verified landing ascent is four successive `up` commands: Underwater to Crag, Crag to Balcony, Balcony to Winding Stair, and Winding Stair to Courtyard.

The lower cliff changes as the days pass. Crag is submerged from day two, when Balcony’s downward route leads directly underwater. Balcony is submerged from day four, when Winding Stair’s downward route likewise leads underwater. Water is audibly or visibly approaching before those changes. Sleeping at Crag on day one, Balcony on day three, or Winding Stair on day five causes overnight drowning. These are not safe camping places.

**Courtyard** belongs to a ruined stone castle. **North** reaches **Plain Hall**, **west** reaches **West Wing**, and **south** or **down** returns to Winding Stair. **West Wing** is a rubble-blocked dead end with a view over the cliff; **east** is its only exit. There is no rubble-clearing passage.

**Plain Hall** marks the transition from the ancient ruin to the more modern complex. **South** returns to Courtyard, **north** enters **Rec Area**, and **northeast** enters **Rec Corridor**.

## The residential corridor

Kalamontee’s main east–west residential route is:

**Rec Area — Rec Corridor — Mess Corridor — Dorm Corridor — Corridor Junction.**

Movement east follows that sequence; west reverses it. The final stretch between Dorm Corridor and Corridor Junction is enormously long in the narration, but it is one mapped connection. The **motorized walkway** is stopped and cannot be repaired into a shortcut.

**Rec Area** has **south** to Plain Hall, **east** to Rec Corridor, and a locked **north** door to **Conference Room**. The scattered games and tapes are background entertainment. The north door uses a numbered dial, not the steel key.

**Conference Room** contains a large round table. **South** returns through the shared door to Rec Area when it is open; **north** enters **Booth 1**. The combination is randomized. Its actual value is written on the **piece of paper** in the **lab uniform** pocket in **Lab Storage**, in Lawanda. The supported unlocking form is `set dial to` followed by that number, used from Rec Area. There is no universal number to memorize. Closing the door locks it again, and the dial cannot be operated from inside Conference Room. A player arriving through Booth 1 should therefore not assume the south door is open.

**Rec Corridor** has **west** to Rec Area, **southwest** to Plain Hall, **east** to Mess Corridor, **north** to **Dorm B**, and **south** to **Dorm A**.

**Dorm A** connects **north** to Rec Corridor and **south** to **Sanfac A**. Sanfac A returns **north**. **Dorm B** connects **south** to Rec Corridor and **north** to **Sanfac B**; Sanfac B returns **south**.

**Dorm Corridor** similarly has **north** to **Dorm D** and **south** to **Dorm C**, besides its east–west main passage. **Dorm C** connects **north** to Dorm Corridor and **south** to **Sanfac C**, whose return is **north**. **Dorm D** connects **south** to Dorm Corridor and **north** to **Sanfac D**, whose return is **south**.

All four dormitories are deserted, very long rooms of multi-tiered bunks and flimsy partitions. They are the reliable sleeping places. The occupied-bunk position, **In Bed**, is not a separate geographical destination: leaving it returns the player to the dormitory where they lay down. The supported commands are `enter bed` and `get up`. After waking, get up before navigating and recover dropped possessions from the dormitory floor. All four associated Sanfac rooms contain dry, dusty, nonfunctional fixtures, not drinking water or useful equipment.

**Mess Corridor** connects **west** to Rec Corridor, **east** to Dorm Corridor, **south** to **Mess Hall**, and **north**, through a padlocked door, to **Storage West**.

**Storage West** has only **south** back to Mess Corridor. It contains the unopened **tin can** labelled “Spam and Egz” and the **heavy-duty extendable ladder**. The can cannot be opened; the ladder is essential. The verified door sequence, with the steel key carried, is `unlock padlock with key`, `remove lock`, `open door`, then `N`. Unlocking alone is insufficient because the padlock still physically obstructs the door.

**Mess Hall** contains tables, benches, and the **octagonally shaped canteen**. **North** returns to Mess Corridor; **south** enters **Kitchen** through its card-operated door. The working sequence is `take canteen`, `slide kitchen access card through slot`, `S`. The door closes automatically, so enter promptly. Returning north from Kitchen closes it behind the player.

**Kitchen** has only **north** to Mess Hall. Its **food machine** dispenses nourishing protein liquid into the matching octagonal canteen. The supported preparation is to open the canteen, put it in the machine’s niche, and push the button. This is the renewable food destination, unlike the inaccessible contents of the tin can. It is also where the verified playthrough takes Floyd’s revealed **lower elevator access card**, but that card belongs to Floyd; it is not a permanent kitchen fixture.

## Corridor Junction and the administrative wing

**Corridor Junction** is the central Kalamontee crossroads: **west** to Dorm Corridor, **east** to **Elevator Lobby**, **north** to **Admin Corridor South**, and **south** to **Mech Corridor North**.

**Admin Corridor South** has **south** to Corridor Junction, **north** to **Admin Corridor**, and **east** to **SanFac E**. SanFac E is another nonfunctional sanitary facility and returns **west**. The important feature of Admin Corridor South is the jagged **crevice**, which conceals the **steel key**. The occasional glint is a clue, but waiting for it is unnecessary. Bring the **curved metal bar**, or magnet, from Tool Room and use the verified `put magnet on crevice`. The playthrough then uses the recovered key successfully without a separate pickup command.

**Admin Corridor** has **south** to Admin Corridor South, **west** to **Systems Monitors**, and **north** across the rift to **Admin Corridor North**. Initially the northern route is blocked by an eight-meter gap above sharp rocks.

Bring the collapsed ladder from Storage West to Admin Corridor. The exact verified preparation is `drop ladder`, `extend ladder`, `place ladder across rift`, then `N`. Dropping it first is necessary because it cannot be extended while carried. Placing it while still collapsed loses it permanently into the rift. Once it spans the gap, ordinary `N` and `S` cross safely despite the alarming swaying description. Explicit jumping is fatal. Do not collapse the bridge: it falls into the rift.

**Systems Monitors** returns **east** to Admin Corridor. Its screens report the condition of Library, Reactors, Life Support, Communications, Planetary Defense, Planetary Course Control, and Project Control. This is a diagnostic destination, not a repair room. The equipment cannot be operated locally to fix distant systems.

**Admin Corridor North** connects **south** across the ladder, **west** to **Small Office**, **north** to **Transportation Supply**, and **east** to **Plan Room**.

**Small Office** has **east** back to Admin Corridor North and **west** to **Large Office**. Its **small desk** contains both the **kitchen access card** and **upper elevator access card**. The verified commands are `open desk`, `take upper card`, and `take kitchen card`.

**Large Office** has only **east** to Small Office. It is the plush office with the broad picture window overlooking the installation and ocean. Its **large desk** contains the **shuttle access card**: `open desk`, `take shuttle card`.

**Plan Room** returns **west** to Admin Corridor North. Its fixed maps show the Kalamontee and Lawanda complexes; the cubbyholes are empty. **Transportation Supply** returns **south** to Admin Corridor North and is dark. It contains no obtainable transport equipment in the supplied material. The helicopter manual’s suggestion that access equipment can be obtained from transportation storage does not lead to a working helicopter puzzle.

## The mechanical wing

From Corridor Junction, three southward moves follow **Mech Corridor North**, **Mech Corridor**, and **Mech Corridor South**.

**Mech Corridor North** has **north** to Corridor Junction, **south** to Mech Corridor, **east** to **Storage East**, and **west** to Kalamontee’s **Physical Plant**.

**Storage East** returns **west**. It contains the **small cardboard box** holding a **good ninety-ohm bedistor**, **K-series megafuse**, **B-series megafuse**, and **cracked seventeen-centimeter fromitz board**, plus a **small oil can** on a shelf. The good bedistor is the replacement needed in Lawanda’s Course Control. The cracked board is not the sound replacement needed for Planetary Defense.

Kalamontee’s **Physical Plant** is the huge, dim room of catwalks and largely inactive heating and ventilation machinery. **Northeast** returns to Mech Corridor North; **southeast** reaches Mech Corridor. These diagonal returns matter: both corridor entrances point west, but the plant does not return simply east. There is no local machinery repair.

**Mech Corridor** has **north** to Mech Corridor North, **south** to Mech Corridor South, **west** to Physical Plant, and **east** to **Reactor Control**.

**Reactor Control** has **west** to Mech Corridor, **down** to **Reactor Access Stairs**, and **east** or **in** through the open door to **Reactor Elevator**. Its gauges and diagram describe a reactor far below, but do not provide access to it. Reactor Access Stairs is dark, with **up** returning to Reactor Control and no supplied downward continuation.

**Reactor Elevator** returns **west** or **out** to Reactor Control. Its Up button, Down button, and slot look promising, but this is a nonworking dead end. The verified `press up button` does nothing. There is no obtainable reactor-elevator authorization during ordinary play, no working trip to the reactors, and no megafuse repair to perform there. The **reactor elevator card** Floyd offers at the best ending is a sequel joke, not a missed key to this region.

**Mech Corridor South** has **north** to Mech Corridor, **southwest** to **Tool Room**, **south** to **Machine Shop**, and **southeast** to **Robot Shop**. These three workshops also form an east–west chain.

**Tool Room** connects **northeast** to Mech Corridor South and **east** to Machine Shop. It starts with the **large glass flask**, **wide-nosed pliers**, **curved metal bar**, and portable laser labelled “Akmee Portabul Laazur.” The magnet retrieves the administrative key; the pliers remove the fused bedistor; the flask carries communications chemicals; the laser is needed inside the computer.

**Machine Shop** connects **north** to Mech Corridor South, **west** to Tool Room, and **east** to Robot Shop. Its **dispensing machine** has four colored coolant buttons, three catalyst buttons, and two white acid/base buttons. The verified communications deliveries begin here with `put flask under spout`, `press black button`, `take flask`; the second delivery substitutes `press gray button`. These are separate trips with an emptied flask, not a mixture.

**Robot Shop** connects **northwest** to Mech Corridor South and **west** to Machine Shop. Floyd is the only robot that looks nearly serviceable. The verified `activate floyd` initially says “Nothing happens”; two `wait` commands later he awakens. The other disassembled robots are not salvageable transport or repair equipment.

Drop the magnet before collecting access cards. While carried, it silently and permanently scrambles carried magnetic cards, including cards inside containers. In the verified route it is left in Storage West before the office cards are acquired.

## The working elevators and tower

**Elevator Lobby** lies east of Corridor Junction. Its **west** passage returns there; **east** enters **Booth 2**. The **blue door north** leads to **Upper Elevator**, and the **red door south** leads to **Lower Elevator**, but only when the appropriate car is at the lobby with its door open.

Calling a car is not the same as authorizing it. Use `press blue button` for Upper Elevator or `press red button` for Lower Elevator, then wait for the door to open. The verified playthrough calls both in succession and uses two `wait` commands before entering the upper car. Repeated pressing during a pending call merely asks for patience.

**Upper Elevator** always has its exit to the **south**. At the lower landing that exit leads to Elevator Lobby; at the upper landing it leads to **Tower Core**. The verified upward journey is `N` from the lobby, `slide upper access card through slot`, `press up button`, `wait`, `wait`, `S`. The downward journey from Tower Core begins `N`, followed by the same swipe, `press down button`, two waits, and `S` into the lobby.

**Lower Elevator** always has its exit to the **north**. At the top it leads to Elevator Lobby; at the bottom to **Waiting Area**. The verified downward journey is `S` from the lobby, `slide lower access card through slot`, `press down button`, `wait`, `wait`, `N`. To return from Waiting Area, enter **south**, authorize the car, press the Up button, wait for arrival, and leave **north**.

The cards enable the controls only temporarily. Reswipe before travel rather than assume an earlier authorization remains valid. Directional `up` and `down` do not operate either elevator; the buttons do. Doors shut during travel and cannot be forced manually.

Neither remote landing supplies a call button. In particular, returning by shuttle to Waiting Area does not allow the player to summon a lower elevator left at Elevator Lobby. Keep track of where the car was left when mixing shuttles and teleportation.

**Tower Core** has **north** into Upper Elevator when it is present, **up** to **Helipad**, **northeast** to **Comm Room**, and **southwest** to **Observation Deck**.

**Observation Deck** returns **northeast**. Its view places the tower about half a kilometer above the island complex and shows another similar island roughly twenty kilometers east.

**Comm Room** returns **southwest**. It contains the **Receive Station**, which plays the Feinstein’s final transmission, and the malfunctioning **Send Station**, with its warning light and funnel-shaped override hole. The verified repair is `pour fluid into hole` with the black dose from Machine Shop, followed by another trip for gray and the same pouring command. The first dose changes the requested light to gray; the second starts transmission. Thus the practical route to its chemical supply is southwest to Tower Core, north into Upper Elevator, down to Elevator Lobby, west to Corridor Junction, then south three times to Mech Corridor South and south once more to Machine Shop.

**Helipad** has **down** to Tower Core and **in** to **Helicopter**. The fence prevents approaching the edge. **Helicopter** has only **out** back to Helipad. Its rusty controls are covered and locked. There is no playable helicopter flight, no obtainable helicopter access card, and no destination-selection puzzle. The ending’s **helicopter key** does not change that.

## Cards and teleportation

The practical authorization chain is important enough to keep straight without consulting inventory guesses.

The **kitchen access card** and **upper elevator access card** come from the small desk in Small Office. The kitchen card opens the south door of Mess Hall; the upper card enables Upper Elevator. The **shuttle access card** comes from the large desk in Large Office and enables the appropriate shuttle control cabin.

The **lower elevator access card** begins concealed inside Floyd in Robot Shop. It can be obtained deterministically by searching him while switched off. In the verified route he reveals it after witnessing `slide kitchen access card through slot` in Mess Hall; the player then enters Kitchen and uses `take lower card`. Do not send a different player to search Kitchen for a card that their Floyd has not revealed.

The **teleportation access card** is in the pale blue lab uniform’s pocket in Lab Storage. The verified acquisition is `open pocket`, `take teleportation`. The same pocket holds the paper giving the randomized Conference Room combination.

The **miniaturization access card**, also called the **mini card** or **mini-booth card**, begins on the Bio Lab floor just inside its western door. Floyd must bring it out to Bio Lock East. It operates Miniaturization Booth, not the three ordinary teleport booths. The player’s personal **ID card** operates none of these systems.

Readers require a swipe, not depositing a card. The verified form is `slide ... card through slot`. Inserting a card into a shallow reader does not activate it, and a damaged card cannot be repaired by trying again.

The ordinary teleport booths are:

**Booth 1**, north of Conference Room, exits **south**. It has destination buttons **2** and **3**, colored beige and tan.

**Booth 2**, east of Elevator Lobby, exits **west**. It has destination buttons **1** and **3**, colored brown and tan.

**Booth 3**, east of Library Lobby, exits **west**. It has destination buttons **1** and **2**, colored brown and beige.

In each booth, swipe the teleportation card and press the desired destination number. The verified inter-complex journey is `slide teleportation card through slot`, `press 2` from Booth 3, then `W` into Elevator Lobby. The reverse is the same swipe in Booth 2, `press 3`, then `W` into Library Lobby. The supported `press 1` selects Booth 1 from either other booth.

Activation lasts only about thirty turns and is consumed by a successful departure. The destination booth is not automatically enabled for a return. Floyd travels with the player if present, as do portable objects on the departure booth’s floor. Fixed booth fittings do not move.

Booth 1 is useful for the residential end of Kalamontee, but only if the Conference Room’s south door can be passed. Booth 2 is the convenient administrative and mechanical hub; Booth 3 is the convenient Lawanda hub. Once the teleportation card is available, the shuttle and lower elevator are no longer required for ordinary travel between the complexes.

## The shuttle stations and cars

**Waiting Area**, below Lower Elevator, is a bench-lined concrete platform. **South** re-enters the elevator if it is here and open; **east** reaches **Kalamontee Platform**. The benches are bolted down.

At both **Kalamontee Platform** and **Lawanda Platform**, **Alfie is south** and **Betty is north**, whenever that car is present. Initially Alfie is at Kalamontee and Betty at Lawanda. An absent car cannot be boarded. Kalamontee Platform has **west** to Waiting Area; Lawanda Platform has **east** to Escalator.

**Shuttle Car Alfie** connects **east** to **Alfie Control East**, **west** to **Alfie Control West**, and **north** to the platform at its current station. **Shuttle Car Betty** similarly connects east and west to **Betty Control East** and **Betty Control West**, but its platform exit is **south**. Both passenger cabins have seating and freight space, not useful loose cargo.

From either East control cabin, **west** returns to the passenger cabin; from either West control cabin, **east** returns. These doors are closed while the shuttle is between stations.

Use the cabin facing the tracks, not the terminal wall. From Kalamontee the east control cabin is appropriate; from Lawanda the west cabin is appropriate. Initially that means Alfie Control East for the outward trip and Betty Control West if taking the initially available return car. After crossing in Alfie, use Alfie Control West to drive it back.

The verified outward boarding route is **east** from Waiting Area, **south** into Shuttle Car Alfie, and **east** into Alfie Control East. Use `slide shuttle access card through slot`, then `push lever`, then `pull lever`. The first command starts the car at speed 5; the second centers the lever and holds that speed.

The lever controls acceleration, not a selected cruising speed. Left at “+,” it adds 5 every turn; at center it holds speed; at “−,” it subtracts 5 each turn until stopping. Faster travel does **not** shorten the trip, so remaining at 5 is the safe method.

Continue with `wait` while passing “Limit 45,” the halfway deceleration sign, and the flashing 15, 10, and 5 signs. When the game announces the brightly lit station and concrete platforms, use `pull lever` on the very next command. This reduces 5 to zero at the correct arrival moment. The verified exit is `W` to Shuttle Car Alfie, then `N` to Lawanda Platform.

Stopping earlier strands the car between stations; it does not complete the journey. Arriving at speeds 5 through 20 causes a survivable crash; 25 or more is fatal. Do not try to evade a crash by leaving the cabin at the last instant.

Shuttle activation is refused when the current time is greater than 6000. A car stopped in the tunnel can therefore become a serious trap if its controls expire after the cutoff. The ordinary successful low-speed journey avoids that problem.

## Lawanda’s corridor loop

From **Lawanda Platform**, go **east** to **Escalator**, then **east** again to **Fork**. Escalator is stopped but walkable; **west** reverses each connection. There is no repair task.

**Fork** has **west** to Escalator, **northeast** to **Systems Corridor West**, and **southeast** to **Project Corridor West**. These northern and southern branches meet again through Library Lobby.

**Systems Corridor West** connects **southwest** to Fork, **east** to **Systems Corridor**, **north** down the stairs to **Repair Room**, and **northwest** to **Infirmary**.

**Systems Corridor** has **west** to Systems Corridor West, **east** to **Systems Corridor East**, and **north** to **Planetary Defense**.

**Systems Corridor East** has **west** to Systems Corridor, **north** to **Course Control**, **east** to Lawanda’s **Physical Plant**, and **south** to **Library Lobby**.

The southern branch begins at **Project Corridor West**, which has **northwest** to Fork, **east** to **Project Corridor**, and **west** to **Sanfac F**. Sanfac F returns **east** and is another dry sanitary dead end.

**Project Corridor** has **west** to Project Corridor West, **east** to **Project Corridor East**, and **south** to **ProjCon Office**.

**Project Corridor East** has **west** to Project Corridor, **north** to Library Lobby, **east** to **Main Lab**, and **south** to **Computer Room**.

**Library Lobby** completes the loop: **north** reaches Systems Corridor East, **south** reaches Project Corridor East, **west** or **up** reaches **Library**, and **east** reaches Booth 3. Its **computer terminal** is an information source, not the damaged computer the player enters. The verified `activate terminal` displays its menus; `type` followed by a menu number selects an entry, and `type 0` backs up.

This loop gives a lost player two routes back to Fork. From Library Lobby, go north, west, west, southwest along the systems branch, or south, west, west, northwest along the project branch.

## Lawanda’s service rooms and library

**Repair Room** returns **south** or **up** to Systems Corridor West. The northern doorway is robot-sized and cannot be used by the player. Its locked cabinets and strange machinery offer no bypass. The fallen robot is **Achilles**, beyond repair.

The useful northern excursion is Floyd’s. The verified commands are `floyd, go north`, then `floyd, take board`. The first establishes that he has found the **shiny fromitz board**; the second retrieves it. Asking for the board before his exploratory visit does not work. There is no separately navigable player room beyond the opening.

**Infirmary** returns **southeast** to Systems Corridor West. It contains a **red spool**, **medicine bottle**, and **Infirmary Bed**. The medicine temporarily suppresses the Disease. Floyd can discover Lazarus’s **medical robot breastplate** here after spending time in the room. Do not confuse its beds with the safe dormitory bunks: entering the Infirmary Bed immediately triggers fatal treatment by a malfunctioning diagnostic robot.

**Planetary Defense** returns **south** to Systems Corridor. Its warning concerns the defense system’s failed discrimination circuits. The **small access panel** contains four fromitz boards. With the shiny replacement from Floyd, the verified repair is `open panel`, `take second`, `put shiny in panel`. This is why Repair Room and Planetary Defense belong together in route planning.

**Course Control** returns **south** to Systems Corridor East. Its **large metal cube** contains the **fused ninety-ohm bedistor**. Bring the good bedistor from Storage East and pliers from Tool Room. The verified commands are `open cube`, `take fused with pliers`, `put good in cube`. The repaired system corrects the planet’s course; do not remove the now-active good component.

Lawanda’s **Physical Plant** has only **west** to Systems Corridor East. It is notably larger than Kalamontee’s despite serving the smaller visible complex, suggesting the extensive installations below. It contains environmental-control equipment, not a usable route into those underground chambers.

**Library** returns **east** or **down** to Library Lobby. It contains the **green spool** and fixed **spool reader**. The green spool is the helicopter manual; the red spool from Infirmary describes Disease symptoms. The reader accepts one spool at a time. These rooms explain the world and the Project, but neither contains a secret corridor or an obtainable librarian.

## Project offices and laboratory approaches

**ProjCon Office** connects **north** to Project Corridor and **east** to Computer Room. Its logo shows a flame above a sleep chamber. Initially the south wall is covered by the garish **mural**, and there is no southern exit. After the revival announcement, the mural slides away and **south** enters **Cryo-Elevator**. There is no manual mural-opening solution.

**Computer Room** has **north** to Project Corridor East, **west** to ProjCon Office, **northeast** to Main Lab, and **south** to **Miniaturization Booth**. Its **glowing red light** signals the failure. The **computer output**, read with the verified `read output`, identifies damaged **Section 384** and shows that the cure project was nearly complete.

Bring Floyd here before the Bio Lock expedition. His reaction to the broken computer gives him the reason to retrieve the miniaturization card. Merely showing him the lab window without establishing that concern is insufficient.

**Main Lab** connects **west** to Project Corridor East, **southwest** to Computer Room, and **south** to **Lab Storage**. Its two guarded branches are **southeast** through the bio-lock door to **Bio Lock West**, and **northeast** through the radiation-lock door to **Radiation Lock West**.

**Lab Storage** has only **north** to Main Lab. It holds the **fresh laser battery** and **pale blue lab uniform**. The uniform pocket contains the teleportation card and Conference Room combination paper. Before entering the computer, replace the laser’s old battery: the verified commands include `remove battery`, then, with the fresh battery acquired, `put battery in laser`.

**Bio Lock West** connects **east** to **Bio Lock East** and **west**, through the outer door, to Main Lab. From Main Lab, the verified entrance is `open biolock door`, `SE`, `E`. The outer door automatically closes after a short interval; expect to reopen it when returning.

**Bio Lock East** connects **west** to Bio Lock West and **east**, through the inner **lab door**, to **Bio Lab**. Its window reveals the mutants and the magnetic-striped miniaturization card just inside the lab. The verified `look through window`, after Floyd has reacted to the computer, prompts his offer.

This is not a player-accessible shortcut to the card. Follow the exact verified sequence after his offer: `open door`, `close door`, `wait`, `open door`, `close door`. The wait produces his three knocks; reopening then admits him with the card, and closing immediately excludes the mutants. Floyd dies in Bio Lock East, leaving the card beside him. Use `take miniaturization card`. Return `W` to Bio Lock West, `open door`, then `W` to Main Lab.

## The radiation dead end

**Radiation Lock West**, northeast of Main Lab through its outer door, connects **east** to **Radiation Lock East** and **west** back to Main Lab when that door is open.

**Radiation Lock East** connects **west** to Radiation Lock West and **east**, through the inner door, to **Radiation Lab**. Its warning says radiation suits must be worn beyond this point. The inner door cannot be opened while the outer door remains open.

**Radiation Lab** returns **west** to Radiation Lock East. It contains damaged radioactive canisters, incomprehensible equipment, a **lamp**, and a **brown spool** labelled as instructions for repairing repair robots. The south-wall crack looks into Bio Lab but is too small to traverse.

This entire branch is a fatal temptation rather than an essential equipment route. Entering Radiation Lab starts radiation poisoning that continues after departure and kills on the seventh counted turn. No radiation suit or surviving retrieval method is supplied. The lab uniform does not solve it. Consequently, the lamp is not a practical way to explore Transportation Supply or Reactor Access Stairs, and the brown spool does not provide a usable Achilles repair.

## Miniaturization and the computer interior

**Miniaturization Booth**, south of Computer Room, exits **north**. It has a card slot and numeric keyboard. With the recovered card, the verified commands are `slide mini card through slot`, `type 384`. This both miniaturizes the player and teleports them to **Station 384**. Activation is temporary; other sector numbers send the player into live circuitry and are fatal.

**Station 384** is a metal plate with another overhead. **East** reaches **Strip Near Station**. A westward departure can return to the main booth before repair, but the normal outward journey is automatic when the player returns here from the strip.

**Strip Near Station** connects **west** to Station 384 and **north** to **Middle of Strip**. South is a sheer downward bend into the void, not an exit.

**Middle of Strip** connects **north** to **Strip Near Relay** and **south** to Strip Near Station. East and west are bottomless voids.

**Strip Near Relay** has only **south** as a usable walking exit. A component wall blocks north. The **vacuu-sealed microrelay** lies east, but cannot be entered; its red plastic enclosure contains the obstructing **speck**.

The verified inward route is `E`, `N`, `N`. Use `set laser to 1`, then `shoot speck with laser` until two hits destroy it. The verified run hits with both shots, but misses are possible. Red light passes through the red enclosure; another setting destroys the relay. Repair starts the 200-millichron countdown to sector activation, so the player must leave the computer.

Going `S` to Middle of Strip after repair summons the microbe and blocks further southward travel. The verified escape uses `set laser to 2`, eight consecutive `shoot microbe with laser` commands, then `throw laser off strip`. Shooting holds the creature off and heats the weapon; throwing the sufficiently hot laser makes the microbe follow it into the void. Shooting forever is not a solution, and throwing away a cold weapon leaves the obstruction alive.

With the microbe gone, use `S`, then `W`. Returning to Station 384 automatically transports the player out. Before repair, that destination is Miniaturization Booth; after repair, the malfunction announcement diverts the player to **Auxiliary Booth**. Do not promise a return to Computer Room after the successful repair.

## Auxiliary Booth, Bio Lab, and the final escape

**Auxiliary Booth** is a receiving station with no slot or keyboard. Its only exit is **north** to **Lab Office**. Returning to it does not provide an outgoing teleport.

**Lab Office** has **south** to Auxiliary Booth and **west**, through the office door, to Bio Lab. There is no safe alternative corridor. Its **desk** contains the **gas mask**, and examining the closed desk reveals a **memo** explaining the emergency fungicide system. The wall’s white and black buttons control lab lights; the red button releases fungicide.

The verified preparation is `examine desk`, `read memo`, `open desk`, `take mask`, `wear gas mask`. During this preparation the revival announcement opens the hidden Cryo-Elevator entrance in ProjCon Office. Make sure that announcement has occurred before starting the escape.

**Bio Lab** connects **east** through the office door to Lab Office and **west** through the lab door to Bio Lock East. Its northern crack is not a passage. The room contains the **triffid**, **mutant grue**, **mutant troll**, and **rat-ant**. Entering without active fungicide is fatal; entering the mist without wearing the mask is also fatal.

The verified departure from Lab Office is `press red button`, `open door`, `w`, `open lab door`, `w`. This reaches Bio Lock East just as protection expires and pursuit begins. Opening the office door without first releasing fungicide is fatal. Prepare the mask and route before pressing the button; the mist lasts only a very short time.

From Bio Lock East, the verified chase route is:

`w` to **Bio Lock West**; `open door`; `w` to **Main Lab**; `w` to **Project Corridor East**; `w` to **Project Corridor**; `s` to **ProjCon Office**; `s` to **Cryo-Elevator**; `press button`.

That single stop to open the outer door in Bio Lock West is specifically allowed. Ordinary pauses, failed moves, examinations, or immediate backtracking during pursuit can kill the player. Closing doors behind oneself is not a reliable way to stop the mutants.

**Cryo-Elevator** has one button and a northern door. Pressing the button immediately closes the door against the monsters and starts the descent. The verified playthrough then uses `z`, `z`, after which the door opens north. Follow the arrival message rather than adding unnecessary button presses.

**North** enters **Cryogenic Anteroom**, the ending destination. It has no onward exits or exploration puzzle. Do not press the elevator button again after arrival: that sends the player back to the waiting mutants.

The geographical endpoint is the same, but the condition of the world is not. Course Control determines whether Resida survives; Communications and Planetary Defense determine whether rescue succeeds. The best ending requires all three repairs in addition to the computer repair. Thus a guide should settle unfinished travel and repair errands before entering Miniaturization Booth for the successful final excursion: that return sends the player into the Lab Office escape sequence, not back to the ordinary corridor network.


# Act one: the Feinstein, the escape pod, the landing, and reaching the complex

## Act one: the Feinstein, the escape pod, the landing, and reaching the complex

### What this opening is teaching

The opening is an evacuation, not a ship-repair puzzle. The player begins on **Deck Nine** of the **S.P.S. Feinstein**, an **Ensign Seventh Class** assigned to scrub the filthy metal deck. **Ensign First Class Blather** makes this seem like a problem about military discipline, but neither satisfying him nor escaping his supervision prevents the disaster. The ship will explode. The meaningful decisions are where the player is when the emergency begins, whether they board promptly, and whether they use the **safety webbing**.

The verified route is deliberately uneventful: `wait` ten times on Deck Nine, `port` into the **Escape Pod**, then `sit`. Continue using `wait` until landing. Take the **survival kit** with `take kit`, then use `open door`, `out`, and immediately `up`. Four consecutive `up` commands after leaving the pod take the player through **Crag**, **Balcony**, **Winding Stair**, and **Courtyard**. From Courtyard, `N` reaches **Plain Hall**, the modern construction adjoining the ruined castle and the entrance route into Kalamontee.

Keep that simple route in mind while the players experiment. Almost everything aboard ship is characterization or a distraction; the webbing and the escape window are not.

### Deck Nine and the first ten turns

Deck Nine is a featureless corridor curving to starboard, with a gangway above and a primary escape-pod entrance to port. Its description reports whether the pod bulkhead is open. Initially it is closed. Trying `open door` before the emergency is refused: “Why open the door to the emergency escape pod if there’s no emergency?” There is no key, authorization, or concealed opening procedure to discover.

The player starts with the **scrub brush**, **diary**, **chronometer**, and **Patrol uniform**. The uniform and chronometer begin worn; the uniform’s pocket contains the **ID card**. The brush is introduced as a “Patrol-issue self-contained multi-purpose all-weather scrub brush.” These possessions establish the player’s identity and circumstances, but none is needed to open the pod.

If players take their assignment seriously, `clean floor`, `scrub floor`, or `wash floor` makes the floor “a bit shinier.” Naming the deck instead also works. These actions do not require the brush in the supplied behavior, do not prevent the explosion, and do not earn release from duty. The joke is that conscientious work cannot solve the actual emergency.

The verified playthrough spends all ten opening turns on `wait`. On the tenth, a massive explosion rocks the ship, echoes thunder down the halls, and “The door to port slides open.” Other ordinary actions can occupy those turns, but players should not be encouraged to fill the interval with an expedition away from Deck Nine.

Ship directions matter here. The playthrough uses **`port`**, not north or south, to enter the newly opened pod. The notes also establish `west`, `in`, and `enter pod` as working alternatives. A closed entrance answers, “The escape pod bulkhead is closed.”

### Why exploring the ship goes wrong

The accessible ship is small, and its apparent onward destinations are deliberately misleading.

From **Deck Nine**, `up` reaches **Gangway**, a steep metal stairway between Deck Nine below and **Deck Eight** above. Another `up` reaches Deck Eight; `down` retraces these connections while the emergency bulkhead remains open. Deck Eight describes a corridor running port and starboard, with the **Hyperspatial Jump Machinery Room** forward. Blather blocks west, north, and east. That machinery room is not an accessible destination, and the hint book’s suggestion that the explosion might involve it is not a repair lead.

Alternatively, `east` from Deck Nine reaches **Reactor Lobby**. Here the **Ion Reactor** is to starboard, the **Auxiliary Control Room** aft, and the return corridor to port. `west` returns to Deck Nine before the emergency closure. Blather blocks `east` toward the reactor and `south` toward Auxiliary Control Room. Examining the Ion Reactor describes it humming away and advises against meddling; trying to take it gets the joke that it is “mercifully” not something the player can pick up. There is no useful reactor manipulation.

On the first visit to either Deck Eight or Reactor Lobby, Blather catches the player away from their post, awards twenty demerits, and threatens forty unless they return “in five seconds.” That is an active discipline countdown, not five free rooms of exploration. Its next three stages repeat the order to return, with Blather becoming progressively redder; at the fifth he drags the player to the **Brig**. Leaving the offending room stops its active countdown but does not erase accumulated discipline time. A third visit to the same offending room brings immediate imprisonment. Repeated attempts at his blocked exits merely produce different humiliating refusals.

The Brig has graffiti and a locked **cell door** to the south. `south` fails because the door is locked; `open cell door` gets “No way, Jose.” There is no escape mechanism. `read graffiti` or `examine graffiti` displays insults about Blather, including a deliberately bad limerick about demerits and the tar pools of Krather. It is not a cipher or password. Imprisonment leaves the player unable to reach the pod before the ship dies.

### Blather on the assigned deck

Blather can also visit Deck Nine without the player doing anything wrong. He is tall and beefy, with a tremendous misshapen nose, an immaculate uniform, and an oversized clipboard on which he writes demerits. The clipboard is descriptive, not a separately obtainable tool.

During opening moves two through six, an eligible visitor check has a one-in-six chance of bringing Blather, a one-in-six chance of bringing the ambassador, and a four-in-six chance of bringing neither. Neither visitor is therefore guaranteed. Do not mistake the verified playthrough’s visitor-free opening for a requirement.

Blather’s arrival includes criticism of the polishing, thirty demerits, and a threat of demotion to Ensign Ninth Class in the toilet-scrubbing division. His ordinary short visit includes variable petty disciplinary business and usually ends with fifty more demerits before he goes elsewhere. This routine does not restart identically on every possible reappearance.

Demerits themselves are not the survival problem. Conversation is variable and may merely earn thirty more for wasting his time. `salute blather` is safe: he calls it the first right thing the player has done today and awards “Only five demerits.” `take blather` fails.

Violence is different. `attack blather`, `kill blather`, or `kick blather` causes immediate death: he removes several of the player’s appendages and internal organs. There is no combat to win. `throw brush at blather`, while carrying the brush, is a special nonfatal joke: it bounces off his nose, falls from the player’s possession, and earns five hundred push-ups, ten thousand demerits, and five years of extra galley duty. It does not remove his blockade.

Leaving Deck Nine removes a visiting Blather from the scene. If he is there when the explosion begins, he orders the player to keep scrubbing and runs away. Neither response creates an alternate escape plan.

### The ambassador and his distractions

The alien ambassador from **Blow’k-bibben-Gordo** is there to enliven the opening. He has roughly twenty eyes, six legs, scaly skin that oozes green slime, and a mechanical translator around his neck. He arrives eating something like an enormous **celery** stalk, leaves a trail and pool of **slime**, and hands the player a **brochure**.

His conversation and behavior vary. He may discuss Admiral Smithers, Bocci, interspecies harmony, human appearances, or whether deck-cleaning is a religious ceremony. He sometimes introduces himself as **Br’gun-te’elkner-ipg’nun**. None of this is a required clue. On an ordinary active turn he may do nothing, leave, or make a remark; his usual departure takes him up the gangway and leaves the slime behind. The explosion instead sends him rushing away in panic after depositing still more slime.

The translator cannot be obtained. The ambassador has no map. `take celery` offends protocol and fails; **`eat celery` kills the player** because the alien metabolism is incompatible with a human’s. It is not food to save for the planet.

The slime can be called “slime,” “goo,” or “mess.” Examining, touching, feeling, or trying to take it yields the joke that it feels like slime and asks whether the player is glad not to have stepped in it. Smelling or tasting substitutes the corresponding sense. Unlike eating the celery, tasting the slime is not lethal in the supplied behavior, but it is not nourishment.

`clean slime`, `scrub slime`, and `remove slime` clean perhaps one ten-thousandth of it. `scrub slime with brush` also works. These actions do not actually clear the mess or complete an assignment; repeated scrubbing is wasted evacuation time.

The brochure can be read, examined, carried, or dropped. It advertises **PLANETFALL**, by **S. Eric Meretzky**, as Blow’k-bibben-Gordo’s leading export and urges the reader to buy a thousand copies. Treat it as a fourth-wall advertisement, not an in-world escape clue.

### Personal possessions and background reading

The **Patrol uniform** is a standard-issue one-pocket garment whose examination boasts about comfort, temperature regulation, insect repulsion, and protection against mild radiation. These claims do not establish a protective puzzle solution in this act. Putting something “in the uniform” means using its pocket.

The pocket holds one small space unit and initially contains the ID card. `examine pocket` lists its contents. It cannot be opened or closed: the game explicitly says there is no way to do either. The scrub brush occupies two units and will not fit. The uniform itself occupies three units when relevant to carrying.

Reading or examining the **ID card** gives:

> STELLAR PATROL  
> Special Assignment Task Force  
> ID Number: 6172-531-541

It is not an escape-pod authorization card and has no useful identity-check function here.

The **chronometer** is a digital wristwatch engraved “Good luck in the Patrol! Love, Mom and Dad.” Its starting reading varies from 4500 through 4699. `read chronometer` and `examine chronometer` report the time. Keep it worn: removing it does not stop time, only the convenient time report.

The **diary** is a battered recording machine with a screen, microphone/speaker, and little button. `examine diary` describes it, but **`read diary`** is what displays the current entry and enables advancement. Thereafter `press more` or `press button` advances to the next entry. Repeating `read diary` only rereads the same entry; pressing the button before the first reading does nothing.

Its entries explain the player’s transfer from the **S.P.S. Trilobyte**, fond memories of the approachable Ensign First Class Lim, life on Gallium, and growing disillusionment with Blather. They cover the confiscated Double Fanucci sets, the application to feed grotches that becomes an assignment to clean their cages, a diary inspection and punishment, the ambassador’s arrival from Accardi-3, compulsory instructional films, rejected astrophysics training, and five generations of family Patrol service. The latest entries say the Feinstein left hyperspace early to investigate a remote system possibly associated with the Second Union, and that missing two pellets of trot earned the extra shifts on Deck Nine. The player ends by contemplating transfer—or abandoning ship.

This is biography, not a set of locations to search. The diary’s remembered hiding place in an air duct and its Deck Four Supply Closet entry do not make those places accessible. Reading every entry aboard the ship consumes time needed for survival. At the end, “END OF DIARY — REWINDING” appears and the button goes dark; pressing resets it, after which a fresh `read diary` is needed to resume.

### The emergency window

The explosion is fixed at **move ten**, regardless of the player’s shipboard location. The successful response is immediate boarding:

**`port`**, then **`sit`**.

The distinction between the three bulkheads is important. The escape-pod bulkhead opens for boarding; the narrow emergency bulkhead at the gangway and the wide one along the starboard corridor close to seal the failing ship.

On **move eleven**, those latter two bulkheads shut. Deck Nine’s upward and eastward routes are blocked, as are Gangway’s downward route and Reactor Lobby’s return west. “The emergency bulkhead is closed” is a real barrier, not scenery. A player lingering on Deck Eight, Gangway, or Reactor Lobby can no longer count on returning.

On **move twelve**, a player outside both Deck Nine and Escape Pod dies amid explosions, darkness, and decompression. Someone still on Deck Nine instead sees the pod bulkhead clang shut. The boarding opportunity has ended. On **move fourteen**, anyone left aboard dies when the ship is torn apart.

Entering the pod does not reset the emergency. In the verified sequence, the boarding command is accompanied by the sound of emergency bulkheads closing; `sit` is accompanied by the pod door closing. Stepping back out onto Deck Nine is therefore particularly dangerous.

Do not promise that apparently free commands freeze every sequence. The notes distinguish the ship’s move checks from pod and character activity. When the emergency starts, the reliable coaching instruction is to act, not to test the clock.

### Inside the Escape Pod

The **Escape Pod** contains a mass of **safety webbing** filling half its space, an open bulkhead at boarding, and entirely automated controls. The general description says the webbing could hold several dozen people; examining it gives a smaller estimate. This has no mechanical significance.

First entry awards **three points**, but boarding alone does not guarantee survival. The exact verified command **`sit`** gives:

> You are now safely cushioned within the web.

Commands such as `enter webbing` and `sit in the webbing` also work. Sitting again merely reports that the player is already in the safety web.

There is no launch button to find, no autopilot programming puzzle, and no manual destination choice. The controls, lights, gyroscopes, thrusters, climate control, and viewport narrate a journey that proceeds automatically. Trying to open the door during flight is refused as a “phenomenally stupid idea.” The player cannot close the pod door manually either.

After `sit`, the verified playthrough uses **twelve `wait` commands** to reach landing. Coach by the landing message rather than making players trust a count after they have inserted other actions.

The first wait starts the slide down the ejection tube. The next shows the Feinstein shrinking through the viewport, bursts of light dotting its hull, and its final destruction. The pod tumbles; its gyroscopes then stabilize it while the autopilot searches for a destination.

An unseated player has a twenty-percent chance of dying head-first against the bulkhead at this explosion. Surviving with bruises is not proof that the web is optional. At landing, an unseated player dies without that chance of reprieve, thrown against a sharp corner of the control panel. `stand` or `leave webbing` is therefore exactly the wrong experiment during flight.

The planet appears almost entirely ocean, with a few islands and an unusually small polar ice cap. The viewport polarizes black when the sun enters view. The control panel announces “Approaching planet...human-habitable.” After several quiet waits come atmospheric buffeting, rising temperature, and a laboring climate-control system. The viewport clears, showing endless ocean while the computer searches for land and the thrusters slow the descent.

Finally the pod approaches the nearer of two islands: sheer cliffs rise to a broad plateau covered with buildings. These observations are early environmental clues. They do not give the player a new navigation task.

### Landing and the dangerous instinct to stand

A successful landing is announced by a thud. Through the viewport the player sees a rocky cleft and water below; the pod rocks as though precariously balanced. A previously unseen panel opens, revealing a **survival kit** and **towel**.

Take supplies **before** getting out of the web. The verified playthrough uses:

**`take kit` → `open door` → `out`**

It does **not** use `stand`. This is an important correction to the official hint book, which says the door cannot be reached from the web. In this supplied version, the verified commands open the door and leave while the player is still nominally seated. There is no need to insert a standing action.

The towel is also available and can be collected with `take towel`, although the verified run leaves it behind. It is an ordinary towel with writing in one corner. `read towel` gives:

> S.P.S. FEINSTEIN  
> Escape Pod #42  
> Don’t Panic!

No special drying, swimming, or repair function is established.

If the player does stand after landing, the pod shifts, falls, strikes something, and water rises past the viewport. This starts an irreversible sinking countdown. Sitting down again does not stop it, and standing repeatedly does not restart it. At the third sinking stage the pod is submerged, strikes underwater rocks, and continues downward. At the fourth, a closed pod creaks under pressure. At the fifth, remaining inside is fatal: either it splits open or the swirling water and pressure kill the occupant of an open pod.

Those are successive updates, not five guaranteed spare commands after standing. The initial update can accompany the standing action itself. Once this has started, opening the door and leaving are urgent.

Even on the safer verified route, `open door` produces the alarming but necessary response:

> The bulkhead opens and cold ocean water rushes in!

That is not evidence that the player chose the wrong exit. `out` is the proven next command. Do not add attempts to close the door or rescue the pod.

### What the survival kit buys

The survival kit contains one **red goo**, one **brown goo**, and one **green goo**. These are three meals with identical nutritional value, not medicines or color-coded puzzle ingredients.

`open kit` reveals the remaining blobs. When hungry, use `eat red goo`, `eat brown goo`, or `eat green goo`. The kit must be open and either directly carried or directly in the current room. Food in a kit nested inside another container does not satisfy the supplied eating rule.

The blobs cannot be taken separately: they would ooze through the player’s fingers and must be eaten from the kit. When already well-fed, the game refuses eating without consuming the portion. A successful meal restores well-fed status and gives 1450 time units before hunger returns. Red tastes like cherry pie, brown like Nebulan fungus pudding, and green like lima beans.

The destructive trap is **`shake kit`**. It scatters and permanently destroys all remaining food, even with the kit closed. `empty kit` misleadingly suggests shaking because the goo sticks to the inside; do not follow that suggestion. Preserve the meals rather than experimenting with their physical properties.

The normal opening does not require eating immediately. The player begins well-fed and well-rested. Ordinary turns advance the clock by 54 units; the first fatigue warning is scheduled 3600 units after the start, and the first hunger warning after 4000. Prolonged wandering can make both relevant, but neither is a reason to pause in the water.

### Underwater: leave immediately

`out` from the landed pod reaches **Underwater**. The player is momentarily disoriented, battered by currents against sharp rocks at an underwater cliff, and sees dim light filtering from above.

The entire solution is **`up`**.

There are no useful objects and no other mapped exits. This is not an underwater exploration area. On the first visit, two underwater danger updates precede the fatal next update, when an undertow drags the player across underwater obstructions. The exact relation of those updates to entry is not a license for two leisurely examinations. Go up immediately.

Leaving records that the player has already been underwater. On a later entry, the next underwater update is fatal without the initial grace period. Returning to salvage the pod or inspect the rocks is therefore especially unsafe.

### Crag and the climb

The first `up` reaches **Crag** and awards **three points**. This is a cleft in the island’s cliff wall, with freshly exposed stone where the edge collapsed beneath the pod’s weight. Turbulent water and sharp rocks are about two meters below; a small structure clings to the cliff about eight meters above.

The description explicitly says even an out-of-shape Ensign Seventh Class could climb to it. Players do not need a rope, ladder, climbing tool, or separate strength puzzle. Use **`up`** again to reach **Balcony**. `down` returns to the deadly water and is not a useful retreat.

Crag is not an overnight refuge. Sleeping there on the first night brings a wave that washes over and drowns the player; from day two onward the crag is flooded. The direction of safety is upward.

### Balcony and the vanished valley

**Balcony** is an octagonal room half carved into and half built out from the cliff. Shattered windows show ocean to the horizon. Beneath them rests a weathered metal **plaque**, barely readable in what appears to be corrupt **Galalingua**. A steep stairway leads upward; on the first day the crag remains visible eight meters below.

The verified route continues with **`up`**, but the plaque is the one worthwhile investigation on the climb. `read plaque` or `examine plaque` displays:

> Xis stuneeng vuu uf xee Kalamontee Valee kuvurz oovur fortee skwaar miilz uf xat faamus tuurist spot. Xee larj bildeeng at xee bend in xee Gulmaan Rivur iz xee formur pravincul kapitul bildeeng.

It says that this stunning view of the **Kalamontee Valley** covers more than forty square miles of a famous tourist spot, and identifies the large building at the bend in the **Gulmaan River** as the former provincial capitol.

The contrast is the clue: the plaque describes a valley, river, and buildings where the player now sees ocean. It is historical evidence of a radically changed landscape, not a map to a presently reachable river. The plaque cannot be taken.

The language is phonetic English. The hint book explains that “x” substitutes for “th,” “c” for “ch,” and doubled vowels generally indicate long vowel sounds. Encourage reading aloud rather than searching for a translator.

The water also continues rising during play. On day two, Balcony explicitly reports the landing crag underwater, and `down` now leads directly to Underwater. On day three water laps at Balcony’s base; sleeping there that night drowns the player, and from day four the balcony itself is flooded. Generic ocean-scenery descriptions are less reliable warnings than these changing room descriptions.

### Winding Stair and the ruined castle

From Balcony, **`up`** reaches **Winding Stair**, the middle of a long, steep stairway carved into the cliff. There is no local puzzle and nothing to collect. Another **`up`** reaches **Courtyard**.

The lower stairs also become unsafe as days pass. From day four, going down from Winding Stair leads directly into Underwater rather than Balcony; water can be heard below, and by day five it splashes against the steps. Sleeping on Winding Stair on day five is the corresponding fatal overnight flood. Do not let the apparent solidity of the stone steps become an argument for camping there.

Courtyard belongs to an ancient stone edifice reminiscent of castles seen during leave on **Ramos Two**. It has decayed into a ruin. Openings lead north and west, and the stairway is to the south. Both `south` and `down` lead back to Winding Stair.

The optional western detour is **West Wing**, a ruined section whose collapsed walls expose the cliff and ocean. Rubble blocks every route except `east`, back to Courtyard. There is no treasure or established rubble-clearing puzzle. Examining the castle merely confirms crumbling stonework; cliff and ocean are out-of-reach scenery.

The route onward is the verified **`N`** from Courtyard to **Plain Hall**.

### Arriving in Kalamontee

**Plain Hall** is old, dusty, and featureless, but notably more modern than the ruined castle to the south. It runs north–south, with a similar hall branching northeast. `south` returns to Courtyard; `north` leads to **Rec Area**. The verified playthrough chooses **`NE`**, entering **Rec Corridor**, the wide east–west hall with portals north and south and a branch southwest.

That is the handoff from evacuation to exploration. The player has reached the Kalamontee complex without needing any of its later tools or access cards. The immediately verified approach is therefore:

**Courtyard — `N` → Plain Hall — `NE` → Rec Corridor.**

Do not confuse arrival in this complex with the much later **Kalamontee Platform**, the shuttle station reached through the lower elevator.

At this point the appropriate explanation is still observational: an apparently abandoned, automated installation occupies the plateau above a drowned landscape. The reason for its desertion, the full meaning of the planetary systems, and the explanation of the ship’s destruction belong to later discoveries. The opening has established the questions, not answered them.

The essential habits to carry forward are already clear: distinguish a joke from a mechanism, read descriptions for physical clues, and respond promptly when the game identifies real danger. Here, the decisive words were the door opening to port, **safety** webbing, light from **above**, and a climb the player could plainly manage.


# Act two: Kalamontee

# Act two: Kalamontee

## What this act is doing

Kalamontee turns the shipwreck into an exploration-and-repair game. Its main dependency chain is simple once understood: the **curved metal bar** retrieves the **key**; the key opens **Storage West**; the **ladder** bridges the administrative rift; the offices beyond supply access cards; those cards open the kitchen, tower, and shuttle route. **Floyd** supplies the remaining elevator card and becomes indispensable later. The **glass flask** connects the Machine Shop’s chemical dispenser with the communications failure upstairs.

Do not present every locked door or broken machine as another necessary repair. Kalamontee contains deliberate dead ends: the Reactor Elevator, helicopter, dark supply room, unopened tin can, and unusable spare components all invite more attention than they deserve. The productive work is securing food, preserving the cards, activating Floyd, repairing communications, and reaching the shuttle with the equipment needed in Lawanda.

Communications is optional only in the narrow sense that the player can finish the adventure without repairing it. It is required for the complete victory and successful rescue. The same distinction will later apply to planetary defense and course control.

The native writing is phonetic English, not a substitution cipher requiring an inventory item. In particular, “x” often stands for “th,” “c” for “ch,” and doubled vowels indicate long sounds. Read labels aloud when players get stuck. The deserted buildings and failing automation are immediately observable; the fuller explanation—an entire population frozen while automated research seeks a plague cure—is information the Lawanda library supplies later.

## Getting oriented

From **Crag**, the verified ascent is `up` to **Balcony**, `up` to **Winding Stair**, and `up` to **Courtyard**. From Courtyard, `N` reaches **Plain Hall**, `NE` reaches **Rec Corridor**, and successive `E` commands reach **Mess Corridor**, **Dorm Corridor**, and **Corridor Junction**. The last crossing is narrated as a long walk beside a stopped motorized walkway. There is no walkway-repair puzzle or separate intermediate room.

Corridor Junction is the useful mental center of the complex. North is the administrative wing; south is the mechanical wing; east is **Elevator Lobby**; west returns toward the dormitories and kitchen.

South from Corridor Junction comes **Mech Corridor North**, then **Mech Corridor**, then **Mech Corridor South**. From that southern end, southwest is **Tool Room**, south is **Machine Shop**, and southeast is **Robot Shop**. The three workshops also connect laterally: Tool Room east to Machine Shop, then east to Robot Shop. Robot Shop northwest returns to Mech Corridor South.

North from Corridor Junction comes **Admin Corridor South**, containing the narrow crevice, then **Admin Corridor**, containing the enormous rift. These are different obstacles. The crevice conceals a key; the rift needs a bridge.

The coastal approach is not somewhere to settle indefinitely. Crag is submerged from day 2, Balcony from day 4, and the staircase later shows water reaching its steps. Going back **Underwater** is especially dangerous: the first visit has only a short grace period, and a later visit is fatal on the next underwater update. The working escape is immediate `up`, not investigation.

## The tools and their proper jobs

**Tool Room** initially displays four useful objects: a **large glass flask** below the lowest shelf, **wide-nosed pliers**, a U-shaped **curved metal bar** on an upper shelf, and a device labelled **“Akmee Portabul Laazur.”** These are practical tools with distinct jobs, not interchangeable ways of attacking every obstruction.

The verified collection commands are `take magnet`, `take flask`, `take pliers`, and `take laser`. “Magnet” identifies the curved metal bar even before the player has explicitly named its function. The U shape is the visual clue.

The magnet is the first tool needed, but it is also the most dangerous thing to keep. **While carried, it silently and permanently scrambles one intact carried access card per turn.** Putting cards or the magnet in a carried container does not shield them; neither does the uniform pocket. A scrambled card later produces **“Damejd kard...akses deeniid”** at its reader. Dropping the magnet prevents further damage, but does not repair cards already ruined. Finish the key puzzle and leave the magnet behind before collecting office cards.

The flask carries industrial chemicals for communications. The pliers and laser can wait until later visits, as they do in the verified playthrough, but they are not red herrings. The pliers remove the fused component in Lawanda’s Course Control. The laser is needed inside the damaged computer.

The laser begins on setting 5 with an **old battery** holding only three to five charges. It is fairly heavy and must be directly held to fire. Its dial takes numbers 1–6, producing red, orange, yellow, green, blue, and violet beams respectively; color words are not valid dial settings. Most casual shooting merely warms the target and wastes a charge. Shooting Floyd hurts and frightens him without solving anything.

The verified preparation on returning here later is `take laser`, `remove battery`, `drop battery`. In **Lab Storage** in Lawanda, the replacement sequence is `take fresh battery`, `put battery in laser`. The old battery must come out before the fresh one fits. There is no battery-making puzzle involving the Machine Shop’s acid and base.

For the coach’s understanding of why this tool matters: later, `set laser to 1` and repeated `shoot speck with laser` remove an impurity through a red plastic relay enclosure without destroying it. Afterward the player uses a non-red setting against a microbe and sacrifices the heated laser with `throw laser off strip`. Those are later puzzles, not reasons to expend the old battery experimenting in Kalamontee.

## Bringing Floyd to life

In **Robot Shop**, the player sees robot-like devices in various stages of disassembly. Only one robot, about four feet high, looks remotely serviceable. Examination before activation describes him switched off, leaning against a wall with his head to one side. The other robot remains cannot be reassembled or salvaged into a useful repair.

The verified command is `activate floyd`. Its immediate answer, **“Nothing happens,”** is misleading: activation has begun. In the playthrough, `wait`, `wait` brings him to life. He swivels his head, bounds over, introduces himself as **B-19-7**, called Floyd by “everyperson,” and proposes Hider-and-Seeker.

For a new player who does not yet know his name, the notes also support `activate robot` or `turn on robot`. Naming Floyd prematurely can produce a fourth-wall joke in other circumstances, but the supplied playthrough demonstrates that `activate floyd` works here. Do not interpret the initial nonresponse as a missing battery or another repair requirement, and do not keep toggling him.

First activation gives two points. The startup is delayed, but the player need not remain in Robot Shop: if elsewhere when he awakens, Floyd comes to find them. Once awake he normally follows, including across the ladder and into elevators.

Floyd sometimes wanders. He can fail to follow on a movement or spontaneously leave an otherwise ordinary room, then return after one to five turns. His absence does not establish that he cannot cross the route just taken. If a puzzle requires him, wait for him rather than abandoning the solution. A switched-off Floyd is different: he stays where left.

The notes support `deactivate Floyd` and `activate Floyd` for later switching. Turning him off upsets him; restarting him after his first awakening is immediate. He is not a machine that needs periodic oiling or replacement batteries. His remarks about batteries, games, and small remembered jobs are characterization, not supply requests.

## Floyd’s card and companionship

Floyd initially conceals the **lower elevator access card** inside a compartment. There are two ways to obtain it.

The verified playthrough uses his spontaneous reveal. After witnessing a successful kitchen-card swipe, Floyd claps and says he has a card of his own, retrieves it, and waves it. The player goes into **Kitchen**, where Floyd follows, and uses `take lower card`. That succeeds because he has already exposed the card; it is not a card initially lying in Kitchen.

This reveal is not guaranteed at the first swipe. The notes give chances of 5 percent on day 1, 10 percent on day 2, 30 percent on day 3, and certainty from day 4 onward, provided Floyd is alive, present, still has the concealed card, and has not already revealed it. Do not tell a player that kitchen authorization must always produce it.

The deterministic alternative, documented in the notes, is `search robot` or `open robot` while he is switched off. Before first activation, this obtains the card without depending on chance. Searching an active Floyd only tickles him: he laughs and pushes the player away. If he is already awake, switching him off permits the search, although he will complain when restarted. Once the hidden card is gone, searching finds only a crayon which the player leaves alone.

Floyd can hold one item given with `give [object] to Floyd`; a second gift is dropped rather than added to an unlimited inventory. `show [object] to Floyd` is different from giving it. This distinction becomes important in Lawanda, where showing the computer output can establish his concern about the broken computer. For now, his social responses, play, oiling, and remarks are optional. `oil Floyd`, while carrying the oil can, earns thanks but does not improve or repair him.

Do not substitute Floyd for the tools in the local chain. He does not retrieve this key, bridge this rift, or authorize an elevator without the player obtaining and using its card.

## The key in the crevice

**Admin Corridor South** has cracked walls and a jagged crevice across the floor. Before discovery, occasional turns produce a glint from the floor. `examine floor` identifies the crevice; the notes’ `examine crevice` reveals a shiny steel key partly covered with dust. Ordinary attempts to take it fail because the opening is too narrow for the player’s fingers.

The solution is magnetic attraction, not reaching farther. Carry the curved metal bar from Tool Room and use the verified command:

`put magnet on crevice`

A spray of dust and a loud clank accompany a piece of metal leaping onto the magnet. It is identified as a **steel key**, which the player tugs free. There is no requirement to wait for the random glint or examine the crack first.

The playthrough establishes that the key is acquired by this action: it is immediately usable at the padlock, and later appears among the possessions dropped by `drop all`. Do not add a compulsory `take key` step based on the room notes’ more cautious account.

From Admin Corridor South, the verified return to the locked storage door is `S` to Corridor Junction, `W` to Dorm Corridor, and `W` to Mess Corridor. The magnet has now done its useful work. Leave it behind before taking any access cards.

## The padlocked door

In **Mess Corridor**, the small northern door leads to Storage West. The obstacle is an attached padlock. Unlocking it and removing it are separate physical actions.

The exact verified sequence is:

`unlock padlock with key`  
`remove lock`  
`open door`  
`N`

Unlocking makes the padlock spring open, but does not detach it. Trying to open the door while the unlocked padlock still hangs on it fails with the explanation that the padlock must be removed. Trying to remove it before unlocking fails because it is locked to the door. The key must be carried, not merely somewhere on the map.

**Storage West** contains a large unopened **tin can** labelled **“Spam and Egz”**, and a **heavy-duty extendable ladder** leaning against the rear wall. First entry gives four points.

The tin can is a deliberate false food solution. `open tin can` produces a complaint about needing a can opener, but no can opener exists and the can cannot be opened. Do not send hungry players searching for one or trying the laser.

In the verified route, `drop all` here leaves the **survival kit**, **curved metal bar**, **key**, and **lock**, then `take ladder` collects the bridge. This is particularly valuable because it separates the magnet from the cards soon to be acquired. It is not necessary to abandon the survival kit permanently: remember where food was left and recover it if needed. The key and removed lock have no further established job in this act.

## Bridging the rift

Carry the collapsed ladder from Storage West through **Mess Corridor**, **Dorm Corridor**, **Corridor Junction**, and **Admin Corridor South** to **Admin Corridor**. The verified directions from Storage West are `S`, `E`, `E`, `N`, `N`.

Here the whole building has been torn apart. The player can see sky through the severed roof, rubble underfoot, and a rift at least eight meters wide and thirty meters deep. This is not the small key crevice. A westward doorway labelled **“Sistumz Moniturz”** leads to Systems Monitors; the desired offices are across the gap to the north.

The ladder begins only about two and a half meters long. Extended, it reaches about eight meters, matching the gap. It cannot be extended while held, and it cannot be carried normally while extended. Bring it here collapsed, then perform the exact verified sequence:

`drop ladder`  
`extend ladder`  
`place ladder across rift`  
`N`

The ladder swings across and rests on the far edge. Ordinary northward movement takes the player carefully across the swaying bridge to **Admin Corridor North**. Floyd follows. Returning with `S` is likewise safe.

The order is vital. Placing the collapsed ladder across the rift loses it forever because it is too short. Extending it back in Storage West leaves it too large to carry until collapsed again. Collapsing it after it spans the rift makes it plunge into the gap. Leave the completed bridge alone.

Explicit jumping is fatal—even with the ladder present. Use `N` or `S`, not “jump across.” The frightening description of a normal crossing is atmosphere, not a random chance of failure. Throwing objects into the rift simply loses them permanently.

## The offices and access cards

**Admin Corridor North** has three signed portals: west, **“Administraativ Awfisiz”**; north, **“Tranzportaashun Suplii”**; east, **“Plan Ruum.”** First arrival gives four points.

Go `W` into **Small Office**. A small desk faces the eastern doorway, and another exit leads west. The verified commands are `open desk`, `take upper card`, `take kitchen card`. The desk is not locked. Opening it reveals the **kitchen access card** and **upper elevator access card**.

Go `W` again into **Large Office**, a plush office with a broad scratched but unbroken picture window and a wide wooden desk. Here `open desk`, `take shuttle card` obtains the **shuttle access card**. Return `E`, `E` to Admin Corridor North.

The cards are identified by their embossed labels, not secret numbers. Their inscriptions include **“kitcin akses kard,” “upur elivaatur akses kard,”** and **“shutul akses kard.”** The lower card is labelled **“loowur elivaatur akses kard.”** Each of these four cards gives one point on first acquisition.

Keep their functions distinct. The kitchen card opens the kitchen; the upper card authorizes the tower elevator; Floyd’s lower card authorizes the shuttle elevator; the shuttle card authorizes the train controls. The personal **ID card** does none of these jobs.

Their slots are shallow swipe readers. Trying to insert or leave a card in one is refused, with a hint that something might be slid through. The reliable construction is `slide [specific card] through slot`. A carried card in an accessible container can be used, but the wrong card does not become correct through repeated attempts. Use distinguishing names rather than a bare “card” or “elevator card.”

## Food and the kitchen

From Admin Corridor North, return `S`, `S`, `S` to Corridor Junction, then `W`, `W` to Mess Corridor and `S` into **Mess Hall**. This is a large dining room lined with tables and benches. Its southern kitchen door has a small slot beside it. An **octagonally shaped canteen** sits on a bench.

The verified sequence is `take canteen`, `slide kitchen access card through slot`, `S`. The kitchen door quietly slides open, and south enters **Kitchen**. Its description identifies the food-production and dispensary area and specifically invites closer examination of the machine near the door.

The kitchen door cannot be opened by hand. Authorization starts a short automatic-closing interval, so go through promptly. Reswiping an already open door does not restart its closing counter. Once inside, the player can work at the machine; returning `N` to Mess Hall makes the door slide closed. Another visit requires the card again.

The verified playthrough enters Kitchen and obtains Floyd’s revealed lower card, but does not operate the food machine. The following working food commands are supplied by the object notes:

`open canteen`  
`put canteen in niche`  
`push button`  
`take canteen`

The machine is labelled **“Hii Prooteen Likwid Dispensur.”** Its octagonal niche matches the octagonal canteen: that shape is the clue to the receptacle, not merely decoration. An open, empty canteen in the niche catches the thick brown **protein-rich liquid**. `put canteen under spout` is also supported.

Without the canteen, liquid splashes onto the floor and evaporates. With its mouth closed, liquid runs over it without filling it. With a full canteen, another press merely overflows, potentially staining the worn Patrol uniform. Other objects do not fit the niche. There is no dispenser-supply limit established here.

`drink protein liquid`, when hungry, restores the player to well-fed condition and postpones hunger for 3,600 time units. If already well fed, the game refuses without consuming it. The drink satisfies hunger and thirst together. This is safe food, unlike the milky chemical fluid carried in the glass flask.

Close the canteen before sleeping: an open, directly carried canteen loses its contents when the waking routine drops the player’s possessions. Kitchen access and reusable food are consequently practical survival preparation, not merely optional scoring.

Do not believe the Mess Hall’s joke about looking under the table. Its alleged keys, food, and reactor-elevator pass are immediately retracted; nothing is there.

## Sleeping and pacing the exploration

The four dormitories are the safe local sleeping places. **Dorm A** lies south of Rec Corridor and **Dorm B** north; **Dorm C** lies south of Dorm Corridor and **Dorm D** north. All are long, deserted rooms lined with multi-tiered bunks and flimsy partitions. Their far ends lead to the corresponding **Sanfac A**, **Sanfac B**, **Sanfac C**, and **Sanfac D**.

The notes support `enter bed`, then `wait` as necessary, and `get up` after waking. Entering a bunk while tired commits the player to falling asleep; attempts to climb out before the pending sleep are refused. Entering while well rested does not force a new day. The occupied position is described as **“In Bed”**, with the player lying in a bunk.

If exhaustion catches the player standing in any of these four dormitories, they automatically climb into a bunk. Elsewhere, ground sleep risks death by beasts, and the lower coastal rooms have additional drowning dangers. Do not treat a sheltered-looking workshop or office as equivalent to a dormitory.

On waking, unworn directly carried objects are on the dormitory floor. Get out of bed and recover needed cards and tools. A carried flask’s chemical contents are lost during this drop, so avoid scheduling a night between dispensing a communications dose and delivering it. Illness worsens with successive days and reduces carrying capacity; sleep is necessary, but not an unlimited way to postpone the game.

The verified route does not show hunger or sleep interruptions. A coach should nevertheless respond to the actual warnings in the players’ session rather than insist on reproducing its uninterrupted itinerary. Keep the chronometer worn, use the survival kit’s food when necessary, and establish the kitchen before the limited emergency provisions are exhausted.

The sanitary facilities are bone-dry, dusty, nonfunctional scenery. They do not supply water, washing equipment, or another survival solution.

## The flask and chemical dispenser

**Machine Shop** has unusual machinery and, against the rear wall, a large dispenser with a spout and nine buttons. The first four, labelled **“KUULINTS 1 - 4,”** are red, blue, green, and yellow. The next three, **“KATALISTS 1 - 3,”** are gray, brown, and black. The last two are white: square **“BAAS”** and round **“ASID.”**

Take the glass flask from Tool Room. The verified first dose is:

`put flask under spout`  
`press black button`  
`take flask`

The flask fills with black chemical fluid, which gradually turns milky white. **The whitening does not change which chemical it is.** The game still remembers the button selected. A player who forgets the dose cannot identify it by its final white appearance.

Positioning matters. Merely holding a flask while pressing a button does not catch anything; the fluid spills and dries. Taking the flask removes it from beneath the spout. A full flask cannot receive a second useful dose: another press overflows without replacing or mixing its original contents. Thus pressing black and then gray into the same full flask is not a shortcut to two deliveries.

The notes support `empty flask` to discard an unwanted dose. The two white buttons can be distinguished by `press square button` and `press round button`, or by acid and base labels; “white button” alone is ambiguous. Neither acid nor base has a useful puzzle role. They do not manufacture a new laser battery.

Drinking the flask’s chemicals is fatal. Keep **protein-rich brown liquid in the canteen** and **milky chemical fluid in the glass flask** conceptually separate.

## Calling and riding the elevators

From Corridor Junction, `E` enters **Elevator Lobby**, a brightly lit room with a blue northern door and larger red southern door. Each has a matching call button. The little room east is **Booth 2**, not another elevator.

Blue calls **Upper Elevator**; red calls **Lower Elevator**. In the verified route, the player uses `press blue button`, `press red button`, `wait`, `wait`. The northern door then opens. Calling both early is useful: the lower car can finish arriving while the player attends to the tower.

A call takes time. The whirring behind blue and vibration behind red are responses to successful calls, not failures. Repeated pressing during an outstanding call earns **“Patience, patience...”** An open elevator at its far landing does not mean its lobby entrance is open.

Enter the upper car with `N`. It is a tiny room with a southern sliding door, Up and Down buttons, and a narrow slot. The exact verified ascent is:

`slide upper access card through slot`  
`press up button`  
`wait`  
`wait`  
`S`

The voice says **“Elevator enabled.”** Departure closes the door, movement begins, Hawaiian music plays, and the door opens at the other landing. South now exits into **Tower Core**.

Calling and authorizing are separate. An open car can still have disabled controls; pressing a travel button without authorization does nothing. Directional `up` and `down` are not substitutes for pressing the controls. The doors cannot be opened or closed manually.

Authorization expires after a short interval. The verified route reswipes the upper card before every trip; follow that reliable practice rather than assume that a previous ride leaves the car permanently enabled. From Tower Core, enter with `N`, use `slide upper access card through slot`, `press down button`, `wait`, `wait`, then `S` to return to Elevator Lobby.

## The tower and its distractions

**Tower Core** is a small circular room: north is the upper elevator, northeast the **Comm Room**, southwest **Observation Deck**, and up **Helipad**. First arrival gives four points.

Observation Deck encircles the tower and gives the useful geographical view. The tower stands about half a kilometer high; dormitories lie across the island, the rest of Kalamontee sprawls below, and another similar island is visible roughly twenty kilometers east. Northeast returns to Tower Core. There is no item to collect or viewing puzzle to solve.

On Helipad, a high fence limits the view and a weathered rotor vehicle sits on rusted, sagging struts. `enter helicopter`, supported by the notes, enters **Helicopter**; `out` returns, and `down` from Helipad reaches Tower Core. The vehicle’s control panel is covered and locked, and everything is coated in rust.

This is not an alternative route to Lawanda. The green spool found later describes a Helicopter Access Card and Control Panel Key supposedly available from Transportation Supply, but the hint book explicitly exposes the flight as a false lead. There is no playable helicopter trip. The helicopter key and reactor elevator card which Floyd presents in the best ending are a sequel joke, not missing objects the player should have found now.

## Repairing communications

From Tower Core, the verified `NE` enters **Comm Room**. Two broad consoles fill the ends of this small windowless room.

The left console, **“Reeseev Staashun,”** has a blinking red **“Tranzmishun Reeseevd”** light and a **“Mesij Plaabak”** button. The notes’ `press playback button` plays the Feinstein’s final call: its communications officer asks planetside to respond on frequency 48.5, reports no response, and is interrupted by an explosion and static. This is a recording, not a live conversation requiring the player to tune a transmitter.

The right console, **“Send Staashun,”** displays a repeating plea to ships of the Second Galactic Union: a planetwide plague has struck the population and emergency assistance is needed. `read screen`, from the notes, reads it. Initially it is not being transmitted.

The relevant warning is **“Malfunkshun in Sendeeng Kuulint Sistum.”** Beside the console are an enunciator panel and a funnel-shaped hole labelled **“Kuulint Sistum Manyuuwul Oovuriid.”** Initially the panel flashes a **black colored light**. That light identifies the required dispenser button. Although the failure concerns cooling, the successful chemicals are from the catalyst group.

Bring the black dose prepared in Machine Shop and use the exact verified command `pour fluid into hole`. The fluid disappears, the lights blink, and only a **gray light** remains.

This is progress, not completion. Return southwest to Tower Core, ride the upper elevator down, and return to Machine Shop. From Elevator Lobby the verified route is `W`, `S`, `S`, `S`, `S`: Corridor Junction, Mech Corridor North, Mech Corridor, Mech Corridor South, Machine Shop.

Now use `put flask under spout`, `press gray button`, `take flask`. Return `N`, `N`, `N`, `N`, `E` to Elevator Lobby, ride up again, and go `NE` from Tower Core to Comm Room. A second `pour fluid into hole` completes the repair. The enunciator goes dark, the coolant warning disappears, and a new signal shows the help message being sent: **“Tranzmishun in pragres.”** This earns six points and turns **“KUMUUNIKAASHUNZ”** green in Systems Monitors.

The verified solution is specifically **black, then gray, in two separate trips**. Do not replace it with the hint book’s generic “two or three trips.” Read the changing light, remember the button pressed, and do not be misled by the fluid turning white.

A wrong dose before completion permanently shuts down the send console with **“Kuulint Sistum Imbalins Kritikul -- Shuteeng Down Awl Sistumz.”** Gray first is wrong; anything but gray after black is wrong. Subsequent correct chemicals cannot undo the failure. Conversely, extra fluid after a successful repair does not break it, though it accomplishes nothing.

## Storage East and supplies for Lawanda

East of **Mech Corridor North** is **Storage East**, a small room with a **cardboard box** beneath the shelves and a **small oil can** on an otherwise bare dusty shelf. West returns to the corridor.

The box openly contains a **good ninety-ohm bedistor**, a **K-series megafuse**, a **B-series megafuse**, and a **cracked seventeen-centimeter fromitz board**. No opening step is needed to see or take these contents. The verified useful command is `take bedistor`.

The good bedistor and Tool Room’s pliers belong together. Later in **Course Control**, the verified sequence is `open cube`, `take fused with pliers`, `put good in cube`. The existing component is physically fused into its socket; fingers alone cannot pull it out, and the replacement cannot fit until that socket is empty. Once installed, the good bedistor is live: trying to remove it is fatal. There is no reason to test that after a successful repair.

The cracked board is not the sound replacement required for Planetary Defense. Its visible damage is real. Floyd later retrieves the **shiny fromitz board** from beyond the small doorway in Lawanda’s Repair Room. Do not treat any object called a fromitz board as equivalent.

The two megafuses suggest reactor maintenance, but no playable reactor-repair route exists here. They have no demonstrated useful installation. The cardboard box can serve as a portable container, but it does not protect access cards from the magnet.

The oil can’s established use is the optional kindness of lubricating Floyd. It does not repair the walkway, helicopter, or other machinery.

These useful objects need not all be taken on the first departure. The verified playthrough returns after obtaining the teleportation card to collect the bedistor, pliers, and later the laser. Planning ahead saves backtracking; leaving them here is not itself an irreversible failure.

## Information rooms and dead ends

**Systems Monitors**, west of Admin Corridor, is the complex’s status display. Its tables hold incomprehensible equipment, but the far wall tells the player which systems need attention. Initially **“LIIBREREE,” “REEAKTURZ,”** and **“LIIF SUPORT”** are green; **“KUMUUNIKAASHUNZ,” “PLANATEREE DEFENS,” “PLANATEREE KORS KUNTROOL,”** and **“PRAJEKT KUNTROOL”** indicate malfunctions. The notes support `read monitors` or `examine monitors`; the room description itself reports the state.

Repairs elsewhere update these indicators. In the verified later visit, communications, defense, and course control are green, leaving only Project Control faulty. The monitoring equipment cannot be operated as a substitute for doing those repairs.

**Plan Room**, east of Admin Corridor North, has empty cubbyholes and wall maps labelled **“Kalamontee Kompleks”** and **“Lawanda Kompleks.”** Kalamontee’s map shows two installations joined by the long hall and a **“Yuu ar heer”** arrow; Lawanda’s shows two installations, one apparently deep underground. The maps are fixed, and the cubbyholes are empty. This is orientation, not a hidden-blueprint puzzle.

**Transportation Supply**, north of Admin Corridor North, is dark. South returns. **Reactor Access Stairs**, down from **Reactor Control**, is also dark; up returns. Neither supplies a productive route. The hint book’s apparent invitation to find a lamp is another circular lead: the lamp is in the lethal Radiation Lab, and there is no usable radiation suit.

**Physical Plant** is accessible west of both Mech Corridor North and Mech Corridor. Its heating and ventilation machinery is too intricate to operate and bolted in place. Northeast returns to Mech Corridor North; southeast to Mech Corridor. Nothing needs fixing here.

**Reactor Control**, east of Mech Corridor, shows gauges and a diagram of a massive reactor buried below. The eastern metal door starts open, permitting entry to **Reactor Elevator**. In the verified exploratory detour, `press up button` produces **“Nothing happens,”** and `examine slot` only describes a small slot. The down button is equally unproductive. Return `W` to Reactor Control and `W` to Mech Corridor. Neither the steel key, ordinary elevator cards, megafuses, nor the Mess Hall table joke provides a solution.

## Recreation and the later shortcut

West of Rec Corridor is **Rec Area**, also reachable north from Plain Hall. It contains scattered games and tapes and a locked northern door with a numbered dial. The games include Chess, Cribbage, Galactic Overlord, and Double Fannucci; the tapes are entertainment. Neither collection supplies necessary equipment.

The northern door opens into **Conference Room**, nearly filled by a large round table. North from there is **Booth 1**. This route is optional and need not delay the first trip to Lawanda.

The combination is randomized, not a number a coach can memorize. Its clue is the **piece of paper** in the **lab uniform** pocket in Lawanda’s **Lab Storage**. The verified `open pocket` there reveals both the paper and **teleportation access card**; `take teleportation` collects the card. The notes’ `read paper` supplies the current game’s combination. Back at the Rec Area dial, `set dial to [that number]` opens the door and resets the dial to zero.

Valid dial settings run from 0 through 999. Wrong guesses merely set the dial, but brute force wastes time. Closing the door relocks it, and the dial cannot be operated from inside Conference Room. Do not close it behind the player casually; Booth 1 is the other route out only when properly authorized.

**Booth 2** lies east of Elevator Lobby. It is a tiny room marked with a large 2, with buttons for destinations 1 and 3. **Booth 1**, beyond Conference Room, has buttons for 2 and 3. **Booth 3** is east of **Library Lobby** in Lawanda.

Once the teleportation card has been found, the verified return shortcut is `slide teleportation card through slot`, `press 2` in Booth 3, then `W` from Booth 2 into Elevator Lobby. Going back uses `E` into Booth 2, `slide teleportation card through slot`, `press 3`, then `W` into Library Lobby. Floyd travels with the player, squealing in alarm.

Each teleport requires authorization; using a booth consumes its ready state, and unused authorization also expires. Objects lying on the departure booth’s floor travel too. This is the convenient later link between the complexes, not an initial alternative to obtaining the shuttle card.

The remaining western scenery is equally straightforward. **West Wing**, west of Courtyard, is a ruined dead end with rubble and ocean views; east returns. The Balcony’s plaque, read with `read plaque`, describes the old Kalamontee Valley tourist panorama and the former provincial capital beside the Gulmaan River. Its contrast with the present ocean is informative, but it is not a portable tool or a coded lock clue.

## Down to the shuttle

After the tower work, descend to Elevator Lobby. If necessary, call the lower car with `press red button` and wait for its southern door to open. Enter with `S`.

**Lower Elevator** is larger than the upper car and has its sliding door to the north. The exact verified descent is:

`slide lower access card through slot`  
`press down button`  
`wait`  
`wait`  
`N`

North now exits into **Waiting Area**, a concrete platform with sparse benches. Go `E` to **Kalamontee Platform**, whose faded sign reads **“Shutul Platform -- Kalamontee Staashun.”** First arrival gives four points.

Initially **Shuttle Car Alfie** stands south of this platform. Use `S` to board, then `E` into **Alfie Control East**. The passenger cabin has seating for about twenty people and freight space; its western and eastern exits lead to control cabins, and north returns to the platform. The east-facing controls are correct for departure because their window shows rails running into the tunnel. The opposite end faces the terminal wall.

The essentials at this point are an intact shuttle card, access to the lower elevator, and Floyd activated. Communications should be repaired for the best ending; food and a sensible sleep plan should be established. The bedistor, pliers, and laser can come now or be collected on a later return.

One return-travel trap deserves advance warning: **Waiting Area has no elevator call button.** If later travel leaves the lower car upstairs and brings the player back here by shuttle, it cannot be summoned from this landing. The ordinary first descent leaves it downstairs, but mixing shuttle trips and teleportation can change that situation.

## Starting the crossing safely

The shuttle controls teach a different idea from the elevators. The lever controls acceleration, not a destination or a fixed chosen speed. Its upper **“+”** position adds 5 every turn; its central position maintains current speed; its lower **“-”** position subtracts 5 every turn until the car stops.

The verified departure in Alfie Control East is:

`slide shuttle access card through slot`  
`push lever`  
`pull lever`

The first command produces **“Shuttle controls activated.”** Pushing starts the car at 5 and closes the control-cabin door. Pulling once returns the lever to center, leaving the speed at 5.

That is the safest cruising speed, and it is not slower in journey turns than racing. Every moving turn advances the journey one stage regardless of displayed speed. Leaving the lever up merely makes the eventual arrival dangerous.

Use `wait` until the approach announcement. The sequence of landmarks includes **“Limit 45,” “Hafwaa Mark -- Beegin Deeseluraashun,”** then red-lit **15**, **10**, and **5** signs. At the verified low speed, do not stop at those signs. Wait until the game says the shuttle is approaching a brightly lit area and the station’s concrete platforms can be made out. On the very next command, `pull lever`.

The speed falls from 5 to zero, the lever returns to center, and Alfie glides safely into the station. `W` returns to Shuttle Car Alfie; `N` exits to **Lawanda Platform**.

Stopping earlier leaves the train between stations with the control-cabin door closed. Failing to stop at the approach causes a crash: arrival at 5–20 is survivable but damaging, while 25 or more is fatal. Trying to leave the cabin at the last instant does not evade the crash.

Shuttle activation is refused when the current time is greater than 6000 because evening use requires special authorization. This is a time-of-day restriction, not a missing card the player can find in Kalamontee. It does not automatically halt a journey already in progress. If the cutoff intervenes, use a safe dormitory and resume the following morning rather than search the reactor or helicopter for another pass.

That completes Kalamontee’s first-act work: the player has a dependable food source, a living companion, an intact card network, a working distress transmission, and a route to the machinery that can save the planet.


# Act three: Lawanda, the repairs, the computer, and the endgame

# Act three: Lawanda, the repairs, the computer, and the endgame

## What this act is really about

Lawanda turns the deserted installation into an understandable system. Resida’s population is not gone: it is in cryogenic suspension beneath the complexes. The Project’s automated laboratories have almost finished producing and testing a cure for the Disease, but the computer has failed just short of completion. The machinery repairer, Achilles, is also broken. The player must finish the work that the automation can no longer do.

Keep two objectives distinct when coaching. **Repairing the computer is necessary to reach the ending. Repairing communications, planetary defense, and course control determines what that ending saves.** Course control saves Resida from falling into its sun. Communications and planetary defense together make rescue possible. All three, in addition to the computer repair, produce the complete victory and Floyd’s restoration.

The sensible order is to explore Lawanda, obtain the teleportation access card, finish the planetary repairs, and prepare the laser before committing to the computer sequence. Above all, **have Floyd retrieve the shiny fromitz board before the Bio Lab expedition**. His death there ends his ordinary usefulness as a companion; there is no local repair or switch-on command that brings him back.

Do not mistake the leisurely verified itinerary for immunity to the survival rules. Hunger, fatigue, and disease remain concerns in ordinary play. Lawanda’s Infirmary is not a safe substitute for the Kalamontee dormitories. Finish eating, sleeping, collecting tools, and optional reading before beginning the tightly timed final sequences.

## Arrival and the shape of Lawanda

After a successful Alfie journey, the verified route is `W` from **Alfie Control East** into **Shuttle Car Alfie**, then `N` to **Lawanda Platform**. Alfie is south of the platform; Betty, when present, is north. The faded sign reads “Shutul Platform -- Lawanda Staashun.” First arrival earns four points.

From **Lawanda Platform**, `E` enters **Escalator**, and another `E` reaches **Fork**. Despite the description of ascending a mechanical stairway, these are eastward moves, not `UP`. The escalator is stopped and in disrepair; it is not another machine to fix.

Lawanda has two roughly parallel corridors. From **Fork**, northeast leads to **Systems Corridor West**, while southeast leads to **Project Corridor West**. Their eastern ends are joined by **Library Lobby**.

The northern route is **Systems Corridor West**, **Systems Corridor**, **Systems Corridor East**, moving east between them. From **Systems Corridor West**, north descends to **Repair Room**, northwest enters **Infirmary**, and southwest returns to **Fork**. From **Systems Corridor**, north enters **Planetary Defense**. From **Systems Corridor East**, north enters **Course Control**, east enters Lawanda’s **Physical Plant**, and south enters **Library Lobby**.

The southern route is **Project Corridor West**, **Project Corridor**, **Project Corridor East**, again joined east–west. **Project Corridor West** has northwest back to **Fork** and west to **Sanfac F**. **Project Corridor** has south to **ProjCon Office**. **Project Corridor East** has north to **Library Lobby**, east to **Main Lab**, and south to **Computer Room**.

This layout matters most at the end. The chase is not a maze puzzle: the player must run west along the southern corridor, turn south into **ProjCon Office**, then south again into the newly revealed **Cryo-Elevator**.

## Repair Room and the shiny fromitz board

The **Repair Room** contains strange silent machines, locked storage cabinets, a small northern doorway, and a motionless robot lying face down at the foot of the stairs. Floyd identifies the robot as **Achilles**, the machinery repairer who once repaired him. Achilles had a troublesome foot; Floyd thinks he fell downstairs. Examination after that conversation identifies the damaged robot by name and connects its twisted foot with Floyd’s story.

The cabinets and Achilles are deliberate distractions from the useful feature. There is no supported way to repair Achilles, salvage him, or open the cabinets. The machines are bolted down and beyond the player’s understanding. The northern doorway is explicitly robot-sized: the player cannot squeeze through it.

Floyd can. The verified commands are `floyd, go north`, followed by `floyd, take board`. The first instruction is indispensable. Floyd explores, plays with an old rubber ball until it falls apart, and returns reporting nothing interesting except a shiny fromitz board. Only then does he understand the retrieval request. Asking for the board before that discovery gets “What fromitz board?”

The second command sends him back and returns the **shiny fromitz board** directly to the player: he tosses it, and the player barely catches it before it could smash. The narrated minutes do not require additional waiting commands. Repeating either errand produces a refusal, not another board. The rubber ball is not obtainable.

This is the good replacement board. The **cracked seventeen-centimeter fromitz board** in **Storage East** is not an alternative merely because its dimensions match.

## Planetary Defense

From **Repair Room**, the verified route is `S`, `E`, `N`, through **Systems Corridor West** and **Systems Corridor** to **Planetary Defense**.

The dazzling controls include a rapidly blinking warning:

“Surkit Boord Faalyur. WORNEENG: xis boord kuntroolz xe diskriminaashun surkits.”

The failed board controls the meteor-defense system’s discrimination circuits. The official hints identify this failure as the probable reason the Feinstein was destroyed. More immediately, leaving it unfixed can cause the defense system to destroy the arriving rescue ship.

The repair is exactly:

`open panel` → `take second` → `put shiny in panel` → `drop fried`

The **access panel** contains four **seventeen-centimeter fromitz boards**, identified by position as first, second, third, and fourth. Only the second original board is safely removable. Removing it leaves one empty socket and changes its name to the **fried fromitz board**; examination outside the panel reveals blackened edges. The shiny replacement clicks into that socket, and the warning lights stop flashing.

The first, third, and fourth boards shock the player’s hand away when removal is attempted. Once the shiny board is installed, it becomes the second board and is similarly protected. Do not interpret that positional renaming as another defective component.

The panel must have an empty socket before a replacement fits. Putting the fried board back, or substituting the cracked board, does not perform the repair. Closing the panel is not required by the working sequence. Dropping the fried board is merely inventory housekeeping.

Successful repair earns six points and turns **PLANATEREE DEFENS** green in **Systems Monitors**.

## Course Control and the bedistor

From **Planetary Defense**, `S`, `E`, `N` reaches **Course Control** through **Systems Corridor** and **Systems Corridor East**.

Two warnings identify the problem: “Bedistur Faalyur!” and “Kritikul diivurjins frum pland kors.” A **large metal cube** stands in one corner. Use `open cube`; it reveals a **fused ninety-ohm bedistor**. The verified attempt `take bedistor` fails because the component is fused to its socket.

The replacement is the **good ninety-ohm bedistor** in the **cardboard box** in Kalamontee’s **Storage East**, east of **Mech Corridor North**. The necessary tool is the **pair of wide-nosed pliers** in **Tool Room**. The verified acquisition commands are `take bedistor` in **Storage East** and `take pliers` in **Tool Room**.

With both brought to **Course Control**, use `take fused with pliers`, then `put good in cube`. The first command pulls the damaged part free; the second extinguishes the warnings and lights the indicator “Kors diivurjins minimiizeeng”—course divergence minimizing. There is only one socket, so trying to install the good part before extracting the fused one fails.

The fused bedistor can be discarded afterward. The installed good bedistor must be left alone: taking it from the live cube is fatal, with “Kerzap!! You should know better than to touch an active bedistor!”

Course Control maintains Resida’s orbit and hence its climate. The hints connect the failing orbit with the melting ice caps and rising sea level. Repair earns six points and turns **PLANATEREE KORS KUNTROOL** green. This is not an optional convenience if the intention is to save the planet.

## The side rooms

Lawanda’s **Physical Plant**, east of **Systems Corridor East**, contains environmental-control machinery and has only a westward return. Its unusual size is the clue: it is larger than Kalamontee’s plant even though the visible Lawanda complex is smaller. The buried cryogenic chambers explain the discrepancy. There is no repair or portable equipment to obtain here.

**Sanfac F**, west of **Project Corridor West**, is a dusty sanitary facility without the bathing fixtures found near the dormitories. East returns to the corridor. Its fixed, dry toilets are scenery, not a water supply or puzzle.

The **Infirmary**, northwest of **Systems Corridor West**, is more consequential. It contains a **red spool**, a **medicine bottle**, and the **Infirmary Bed**, among mostly empty shelves and incomprehensible medical equipment. Southeast returns to the corridor.

The bottle label, read with `READ BOTTLE`, says “Dizeez supreshun medisin -- eksperimentul.” The supported command `DRINK MEDICINE` consumes its one bitter dose and rolls disease severity back two levels, never below the first level. It can restore carrying capacity lost to illness and postpone death, but it does not cure the Disease or reverse the calendar’s worsening fatigue schedule. Taking it while already nearly healthy wastes much of its benefit.

Never recommend the bed. `ENTER BED` immediately triggers a lethal treatment sequence: a rusty diagnostic robot straps the player down, administers all 347 of its serums and medicines, and prepares to amputate the player’s legs. There is no intervening opportunity to get up.

If Floyd remains here for the qualifying turns, he discovers the remains of **Lazarus**, his medical-robot friend. A **medical robot breastplate** appears, and Floyd becomes upset and wanders away. The dented breastplate is portable, but is not a repair component. Giving it to Floyd upsets him again; it does not equip or improve him.

## Library Lobby and its terminal

**Library Lobby** is the carpeted, dusty junction between the two corridor systems. North goes to **Systems Corridor East**, south to **Project Corridor East**, east to **Booth 3**, and west or up to **Library**. Its **computer terminal** is an information terminal, not the broken Project computer.

The verified command `activate terminal` produces a green flash and the main menu:

“1. Histooree; 2. Kulcur; 3. Teknolojee; 4. Jeeografee; 5. Xe Prajekt; 6. Inturlajik Gaamz.”

The playthrough demonstrates several equally usable input forms: `key 4 on the terminal`, `press 0 on the terminal`, and `type 6`. The simplest continuing convention is `type` followed by the desired menu number. `type 0` goes up one level, not necessarily all the way to the main menu. In the verified games visit, `type 6`, `type 1` displays the Zork article, and two successive `type 0` commands return through the games submenu to the main menu. `deactivate terminal` darkens the screen.

Turning the terminal off retains its selection rather than establishing a fresh main-menu state. A dark terminal cannot accept useful input. Invalid choices produce a feep, and zero has no parent menu to select at the main menu. Final articles direct the reader to consult the librarian for detailed spools, but no librarian can be summoned.

The native writing is phonetic English, not a cipher demanding a separate translation puzzle: “x” commonly stands for “th,” “c” for “ch,” and doubled vowels indicate long sounds.

## What the terminal explains

Each article below is reached by two separate numeric commands from the main menu—for example, `type 5`, then `type 3`. Reading is optional mechanically, but it supplies the reasons for nearly everything the player is doing.

Under **Histooree**, choices 1, 2, and 3 cover racial origins, the Great Hiatus, and the New Technocracy. The first explains that legends of settlement by the Second Union, once dismissed in favor of local evolution, are now taken seriously. The second describes an advanced ancient civilization followed by centuries of decline. The third says the last five centuries restored civilization to its pre-Hiatus sophistication, just before the Disease struck.

Under **Kulcur**, choices 1, 2, and 3 cover literature, art, and music. The library claims copies of great writings reaching back to the mythical Second Union, studies and reproductions from the Primitive, Renaissance, and recent video-and-laser-art periods, and recordings of important compositions from the last five hundred years. These are background entries, not clues to manipulating the mural or laser.

Under **Teknolojee**, choice 1 describes medicine and cryogenics: major diseases had been curable for a century, patients could wait in stasis for future cures, and average life expectancy had reached 147 revolutions. Choice 2 describes hydroponics and undersea algae farms replacing dirt farming. Choice 3 describes scooters, airbuses, and the recent transformation of space travel by nuclear-fuelled engines. Choice 4 explains how miniaturization allowed multipurpose B-19 robots to replace whole teams of older specialized robots. Choice 5 identifies Course Control as maintaining ideal climate, Defense as destroying dangerous meteors, and the recently added Project Control as monitoring the Project.

Under **Jeeografee**, choice 1 describes a stabilized Resida with precisely 47.79 percent land, the major landmasses **Andoor** and **Fruulik**, six lesser ones, and the capital **Pilandoor** on Andoor’s eastern coast. This historical account contrasts with the ocean-dominated world the player saw from the escape pod. Floyd’s Hucka-Bucka-Beanstalk interruption is incidental. Choice 2 says undersea habitats began in 2992 and, nearly two centuries later, housed about nine percent of the population in twenty cities. Choice 3 describes settlements on **Fristin**, moons of **Blustin**, and especially Resida’s Trojan-point colonies. None is a playable travel destination here.

Under **Xe Prajekt**, choice 1 explains the Disease’s origin: research at the Center for Advanced Cryogenic Research succeeded in extending suspension indefinitely, but somehow released the plague. Floyd’s accompanying crayon business does not yield a collectible crayon. Choice 2 explains why the complexes occupy the twin peak plateaus of Kalamontee and Lawanda: elevation aided transport and communications, while the mountains accommodated vast reactors and cryogenic chambers. Choice 3 describes the four phases—build the complexes, freeze the population, monitor the sleepers while automated research searches for a cure, then revive and inoculate everyone.

Under **Inturlajik Gaamz**, choices 1, 2, and 3 describe **Zoork**, **Dedliin and Witnis**, and **Starkros and Suspendid**. These are affectionate references to adventure, mystery, and science-fiction games, not games the player can launch inside Planetfall.

## Library and the spools

The **Library**, west or up from **Library Lobby**, contains a desk, small tables, a **green spool**, and a fixed **spool reader**. East or down returns to the lobby. The reader is a microfilm machine with a screen and circular opening.

The object notes supply `PUT GREEN SPOOL IN READER`, `READ READER`, and `TAKE GREEN SPOOL FROM READER`; the equivalent commands work for the red spool. Loading a spool displays its information. Only one fits at a time, and removing it blanks the screen. Examining a loose spool reads its label, not its stored article. Use “reader” rather than “screen” when necessary to distinguish it from the terminal.

The **green spool**, labelled “Helikoptur Opuraateeng Manyuuwul,” says trained pilots need a Helicopter Access Card and Control Panel Key from Transportation Storage; the rest is technical. This is not a hidden playable helicopter solution. The official hints explicitly establish that the required working equipment cannot be obtained during the adventure.

The **red spool** from **Infirmary**, labelled “Simptumz uv Xe Dizeez,” explains variable incubation, death eight to ten days after symptoms begin, high fever, and sharply increasing nightly sleep requirements. It diagnoses the player’s situation; it does not cure it.

The **brown spool** in **Radiation Lab** is labelled “Instrukshunz foor Reepaareeng Reepaar Roobots.” Its promise is a trap. There is no supported usable repair text, no way to repair Achilles, and no survivable ordinary expedition that brings it back to the reader.

## Lab Storage and the teleport booths

From **Library Lobby**, the verified route `S`, `E`, `S` leads through **Project Corridor East** and **Main Lab** to **Lab Storage**. North is the storage room’s only exit.

Here are the **fresh laser battery** and a **pale blue lab uniform**, whose pocket bears the Project’s flame-above-a-sleep-chamber logo. The verified `open pocket` reveals a **teleportation access card** and a **piece of paper**; `take teleportation` takes the card. It is not necessary to wear or even take the lab uniform to get these contents.

The paper supplies the current game’s Conference Room combination. `READ PAPER` gives “Week uv 14-Juun--2882. Kombinaashun tuu Konfurins Ruum: [number].” This number is not universal. It can be used at the dial in **Rec Area** with the supported `SET DIAL TO [number]`, opening the ordinary route into **Conference Room** and hence **Booth 1**. That detour is optional.

The important immediate shortcut is between **Booth 3**, east of **Library Lobby**, and **Booth 2**, east of Kalamontee’s **Elevator Lobby**. In **Booth 3**, the verified commands are `slide teleportation card through slot`, then `press 2`. The light flashes “Redee”; the player experiences a stomach-wrenching sensation and arrives in **Booth 2**. `W` exits to **Elevator Lobby**.

For the return, go `E` from **Elevator Lobby** into **Booth 2**, use `slide teleportation card through slot`, then `press 3`, and `W` out of **Booth 3** into **Library Lobby**. This removes the need to repeat the shuttle journey for the bedistor, pliers, laser, food, or sleep.

The numbers designate destinations, not settings to discover. **Booth 1** has buttons 2 and 3 and exits south to **Conference Room**. **Booth 2** has buttons 1 and 3; **Booth 3** has buttons 1 and 2. Their colors are consistent destination cues: brown for 1, beige for 2, tan for 3. Numeric commands avoid color ambiguity.

Each successful teleport consumes the departure booth’s authorization. Swipe again for a return journey; arriving does not leave the destination booth enabled. An unused authorization expires after roughly thirty turns. Floyd travels with the player if present, squealing and clutching his guidance mechanism. Portable objects on the departure booth’s floor travel too; fixed equipment does not.

Keep the magnet away from all these cards. Carrying the **curved metal bar** silently and permanently scrambles carried access cards, even inside containers. The verified player has already left it behind. Dropping the upper, lower, and shuttle cards in **Elevator Lobby**, as the playthrough does, is optional inventory management after teleportation becomes available, not a requirement of the system.

## Preparing the laser

The laser is the **“Akmee Portabul Laazur”** from Kalamontee’s **Tool Room**. The verified collection and stripping sequence is `take laser`, `remove battery`, `drop battery`. Back in **Lab Storage**, use `take fresh battery`, then `put battery in laser`. The response confirms that the battery rests in the depression on top of the weapon.

The original **old battery** has only three to five charges. The **fresh laser battery** has twenty charges plus a ten-sided roll. There is no battery-manufacturing puzzle involving the Machine Shop’s acid and base. Remove the old battery before fitting the fresh one: the depression holds only one.

The laser starts on setting 5. Its six numeric settings produce red, orange, yellow, green, blue, and violet light, respectively. `set laser to 1` and `set laser to 2` are verified commands. Color words are not valid substitutes for numbers. The laser must be held directly to fire; having it on the floor or inside another carried container is insufficient.

`shoot speck with laser` and `shoot microbe with laser` are the working targeted forms. A successful discharge spends one charge and heats the laser by one; a turn without a successful discharge cools it by one. Misses still consume ammunition and generate heat. A missing or exhausted battery produces only “Click.”

Warmth is not merely flavor. The notices at three, six, nine, and twelve heat units describe it as slightly warm, somewhat warm, very warm, and quite hot. That heat becomes the solution to the microbe, but excessive heat while continuing to fire at the creature is dangerous. Do not waste the fresh battery on ordinary objects or Floyd. Most such targets merely warm up.

## Computer Room and the reason Floyd helps

**Computer Room** is south of **Project Corridor East**, southwest of **Main Lab**, and east of **ProjCon Office**. Its south exit enters **Miniaturization Booth**. A glowing red light and a **pile of computer output** announce the fault.

Bring Floyd here before the Bio Lock scene. In the verified visit he examines the light and says the computer is broken and is the most important part of the Project. That expression of concern is a prerequisite for his later volunteering. The supported alternative is to show him the printout; merely giving it to him is not equivalent.

Use the verified `read output`. The game takes the output and displays its interesting last page: preliminary, intermediate, and final research are complete; drug production is complete; drug testing is at **99.985 percent**; projected revival was only **0.8 chrons** away. Then comes the alert:

“Malfunkshun in Sekshun 384! Sumuneeng reepaar roobot.”

The failure is in **Section 384**. The repair robot never came because Achilles is lying broken in **Repair Room**. This is why the player must enter the computer rather than find a console command that finishes the research.

Do not use the red light’s examination text as a dependable post-repair status report; its supplied response continues to describe a malfunction. The actual relay repair, subsequent teleportation, and revival announcement establish progress.

## The Bio Lock and Floyd’s sacrifice

**Main Lab** has two heavy doors: northeast to the radiation lock and southeast to the bio-lock. Use the verified `open biolock door`, then `SE` to **Bio Lock West**, then `E` to **Bio Lock East**.

The outer door automatically closes behind the player. **Bio Lock East** has a windowed eastern door into **Bio Lab**. `look through window` reveals a dim laboratory, blue light from a crack in the northern wall, ominous moving shapes, and a magnetic-striped card just inside the door.

If Floyd is alive, present, and already concerned about the computer, he identifies the needed card and volunteers. He warns the player not to enter, then explains the plan: open the door, let him rush in, close it, and reopen when he knocks. His claim that nothing can hurt robots is bravery, not a reliable statement about the danger.

Wait for that offer before opening the inner door. The verified sequence is exact:

`open door` → `close door` → `wait` → `open door` → `close door`

The first opening sends Floyd into the lab and starts the monsters toward the doorway. Close immediately. The closure is accompanied by growling, fighting, and a metallic scream. One `wait` produces the three fast knocks and tearing metal. Open immediately at those knocks. Floyd stumbles out carrying the **mini-booth card**, with the mutations following. Close immediately again.

Opening before Floyd is ready, reopening before the knocks, or inserting an extra action at either required closure can kill the player. Failing to reopen promptly after the knocks leaves Floyd trapped and dead without delivering the card. His ordinary wandering is not what is happening here; do not wait for him to return on his normal wandering schedule.

The successful final closure completes the sacrifice. Floyd collapses in a pool of oil, drops the **mini card**, and asks whether he has been a good friend. The player automatically cradles him and sings the Ballad of the Starcrossed Miner. No singing command is required, and no tool can prevent his death in this successful retrieval scene.

Use `take miniaturization card`: it is on the floor, not automatically in inventory. The scene earns two points, and first taking the card earns one. The verified return is `W` to **Bio Lock West**, `open door`, then `W` to **Main Lab**.

Floyd’s body remains in **Bio Lock East**. He is too heavy to carry, and the player will not drag him. Switching him on does not revive him; the switch falls off. Oil, spare boards, and the medical breastplate are not resurrection tools. His restoration belongs to the complete ending.

## The radiation wing is not an alternative

The northeastern door from **Main Lab** leads through **Radiation Lock West** and **Radiation Lock East** to **Radiation Lab**. The inner radiation-lock door cannot be opened while the outer one remains open. The warning requires radiation suits beyond that point, but no radiation suit exists.

**Radiation Lab** contains split radioactive canisters, incomprehensible machinery, a **lamp**, and the **brown spool**. A crack in its southern wall looks into **Bio Lab**, but is too small to pass through. It is not a way to obtain the miniaturization card without Floyd.

Entering starts radiation poisoning that continues after leaving: sickness and dizziness appear on the fifth counted turn, vomiting and hair loss on the sixth, and death on the seventh. The lab uniform does not make the expedition safe, and the experimental disease medicine is not an established cure for this radiation sequence.

The lamp can produce light, but retrieving it is not a survivable route to exploring the dark rooms. Neither the promising spool label nor the radiation-suit sign should persuade the coach to send players in search of a missing protection puzzle.

## Miniaturization and Section 384

Before entering **Miniaturization Booth**, have the freshly powered laser in hand and the miniaturization card available. Complete unfinished planetary repairs and optional exploration first. Successful computer repair eventually deposits the player on the far side of the Bio Lab rather than safely back in **Computer Room**.

From **Computer Room**, go `S`. The small booth has a card slot and numeric keyboard. The verified commands are `slide mini card through slot`, then `type 384`. The voice requests the damaged sector number; the walls expand away and the player reaches **Station 384**.

The miniaturization card is distinct from the teleportation access card. Authorization lasts thirty turns and is consumed by use. Any other recognized integer sends the player into an active computer sector and kills them with electric current. This is not a keypad to probe by trial and error.

**Station 384** consists of a metal plate underfoot and another overhead. The verified inward route is `E` to **Strip Near Station**, `N` to **Middle of Strip**, and `N` to **Strip Near Relay**. The silicon filament is a broad highway only because the player has become microscopic.

There is no useful route off its sides. South from **Strip Near Station** would plunge into the void. East and west from **Middle of Strip** are bottomless drops. At **Strip Near Relay**, a huge microcomponent blocks the north, and the sealed relay to the east can be inspected but not entered.

Returning west from **Strip Near Station** enters **Station 384** and automatically teleports the player out; no additional westward command is required. Before repair, that returns to the main booth. After repair, it invokes the auxiliary destination.

## The relay and the blue speck

At **Strip Near Relay**, the **vacuu-sealed microrelay** is enclosed in transparent red plastic. Examination explains that an impurity is wedged between its contacts. To the miniaturized player, the microscopic **speck** looks like a blue boulder.

The working sequence is `set laser to 1`, then `shoot speck with laser` until two shots have hit. In the verified playthrough both shots hit: the first makes the speck sizzle, and the second vaporizes it.

The color choice is essential. Red light passes through the red enclosure without breaking its vacuum seal. Settings 2 through 6 destroy the casing and ruin the relay, making the game unwinnable. The laser’s initial setting 5 is therefore disastrous here.

Two *hits*, not necessarily two commands, are required. In ordinary play red shots can miss. Aim improves by twelve percentage points after each miss, starting from a twenty-percent chance; misses still spend charges and heat the laser. The fresh battery is preparation for both imperfect aim and the next obstacle.

The second hit repairs the computer, earns eight points, and produces:

“Sector 384 will activate in 200 millichrons. Proceed to exit station.”

The relay begins closing. Leave rather than experiment further. A non-red shot can still destroy the repaired relay, and remaining anywhere inside the computer when the activation countdown expires is fatal.

## The microbe and the hot-laser solution

Go `S` from **Strip Near Relay**. On entering **Middle of Strip** after the repair, an elephant-sized red **microbe** lands ahead and blocks the way south. It is an ordinary microorganism at the player’s extraordinary scale.

Do not try to walk through it. Southward travel is blocked; the sides are voids. Retreating north makes it follow to **Strip Near Relay** and resets its closing distance, but does not provide a route around it.

The verified solution is `set laser to 2`, followed by eight consecutive `shoot microbe with laser` commands, then `throw laser off strip`.

That sequence works because non-red shots hold the creature off while heating the weapon. Setting 1 passes harmlessly through its red membrane. Other colors strike it, but it regenerates: repeated firing is not a way to kill it. Without effective shots, two closing advances are survivable and the next is fatal. Firing prevents that turn’s advance without undoing earlier advances. The initial color change spends one of those opportunities, so do not add inspections or experiments.

The hot weapon is bait. **Throwing it off the strip succeeds when its warmth is above seven and the active microbe is present.** In the verified route, eight uninterrupted non-red shots provide that warmth. The microbe lunges after the laser and both fall into the void. The weapon is permanently sacrificed, which is intended.

Heat falls by one on non-firing turns, so do not insert a pause between warming and throwing. Throwing a cold laser merely loses it. The alternative supported by the object notes—giving or throwing the laser directly to the microbe—requires at least eleven heat units; at eight through ten it eats the weapon and survives. That is why “throw it at the monster” is not interchangeable with the verified `throw laser off strip`.

Do not keep firing indefinitely after sufficient heat has accumulated. If the laser is already above thirteen heat units before another shot, the microbe’s frenzy causes a fatal fall. The warmth notices are clues to using and discarding the weapon, not invitations to reach maximum heat.

Once the microbe is gone, the verified escape is `S` to **Strip Near Station**, then `W`. **Station 384** automatically returns the player to full size, but announces that the main booth has malfunctioned and switches to **Auxiliary Booth**. That arrival earns four points.

## Auxiliary Booth and Lab Office

**Auxiliary Booth** has no slot or keyboard. It is a receiving station, not an available teleport escape. `N` enters **Lab Office**.

The office has locked files, a messy **desk**, three buttons, and a closed western door labelled “Biioo Lab.” The description states the predicament plainly: the only way out is through the mutant-infested **Bio Lab**.

Follow the verified preparation in order: `examine desk`, `read memo`, `open desk`, `take mask`, `wear gas mask`.

Examining the closed desk discovers the **memo** among its papers. Reading it explains that the emergency system floods the Bio Lab with deadly fungicide and that proper precautions are necessary. Opening the unlocked drawer reveals the **gas mask**. Wearing it, rather than merely carrying it, is the necessary precaution.

The white button is labelled “Lab Liits On,” the black “Lab Liits Of,” and the red “Eemurjensee Sistum.” The black and white buttons produce a relay click but no useful alternative to the emergency system. The red button releases the fungicide.

Do not open the office door first. Without active fungicide, monsters pour into the office and devour the player. Do not regard the gas mask alone as protection from them: it protects against the mist, not teeth and tentacles.

After arrival through the auxiliary booth, the revival announcement is scheduled. In the verified playthrough it occurs while taking the mask:

“Revival procedure beginning. Cryo-chamber access from Project Control Office now open.”

This means the mural in **ProjCon Office** has moved. It is the signal identifying the destination of the imminent escape, not an instruction to search the office for another exit.

## Crossing Bio Lab

With the mask already worn, the verified sequence is:

`press red button` → `open door` → `W` → `open lab door` → `W`

The red button produces hissing beyond the western door. Opening it reveals mist and choking biological nightmares. West enters **Bio Lab**; `open lab door` opens the western door into **Bio Lock East**; the next westward move escapes through it.

The four visible monsters are a **triffid**, a mobile plant with poisonous tentacles; a **mutant grue**, fanged and uncomfortable in the light; a **mutant troll**, charging with axe-like laboratory equipment; and a **rat-ant**, hairy, shelled, and equipped with enormous mandibles and a whip-like tail. No direct combat solution is established. The northern crack looks into **Radiation Lab**, not an escape passage.

The fungicide window is deliberately narrow. After activation, only two ordinary advances remain. Opening the office door spends one. Entering **Bio Lab** and opening its western **lab door** have special grace allowances that preserve the window. Leaving west spends the final advance outside the lab. This is why the apparently busy sequence works, but stopping to examine a creature does not.

Each red-button press renews the short protection interval, but all preparations should be finished before leaving **Lab Office**. Entering active mist without the mask both carried and worn is fatal. Letting the mist expire while still in **Bio Lab** is also fatal.

On the verified westward escape into **Bio Lock East**, the mist vanishes and the mutants recover. Floyd’s body is still there. This is not the time to stop beside it: the chase has begun.

## The run to Cryo-Elevator

Memorize the route rather than making the players solve it while being pursued. From **Bio Lock East**, the verified commands are:

`W` → `open door` → `W` → `W` → `W` → `S` → `S` → `press button`

They lead through **Bio Lock West**, **Main Lab**, **Project Corridor East**, **Project Corridor**, **ProjCon Office**, and into **Cryo-Elevator**.

The one door-opening pause is necessary because the outer bio-lock door closes automatically. In **Bio Lock West**, `open door` receives the warning that the mutants are almost upon the player, but the chase specifically allows that stationary turn. Move west immediately afterward.

Elsewhere, the rule is continuous forward movement. A stationary action—waiting, examining, fumbling with equipment, or failing to move through a blocked exit—ordinarily allows the mutants to catch up. Immediately returning to the room just left runs into their jaws. Closing the inner lab door after the chase starts is too late to stop them.

**ProjCon Office** originally displayed a garish orange-and-purple mural, sometimes seeming to ripple. It was not a wall to attack or pull down. The revival announcement moves it automatically and opens the southward route. Its western-wall logo is the same flame above a sleep chamber seen on the lab uniform.

Entering **Cryo-Elevator** earns five points. Although its description calls the northern door closed, the monsters are explicitly storming toward it: the player is not safe until using `press button`. That closes it just in time, ends the chase, and starts the descent.

The verified playthrough then uses `z` twice, after which the elevator door opens onto a room to the north. Follow the opening message rather than imposing a longer wait count from the notes.

The continuation established by the room and ending notes is `N` into **Cryogenic Anteroom**. **Do not press the button again after arrival.** A second journey takes the player back to the waiting mutants and is deliberately fatal.

## The endings

**Cryogenic Anteroom** is the ending, not another explorable chamber. A medical robot opens a cryo-unit and injects its occupant. **Veldina**, Resida’s red-haired leader, wakes and examines the controls. The cure has been completed because the player repaired the computer; the remaining outcome depends on communications, planetary defense, and course control.

With **all three repaired**, other cryo-units open and Veldina thanks the player for completing the cure and restoring the planetary systems. A landing party from the **S.P.S. Flathead** arrives. **Captain Sterling** promotes the player to **Lieutenant First Class**. Resida offers the player leadership of the world. Blather, himself rescued from an escape pod, is demoted to **Ensign Twelfth Class** and assigned as the player’s personal toilet attendant. A medical robot administers the Disease antidote.

Robot technicians then return a restored Floyd. He brings a **helicopter key**, a **reactor elevator card**, and a **paddleball set**, proposing their use in the sequel. These are ending jokes and rewards, not objects the player should have found earlier to unlock the helicopter or reactor elevator.

With **course control repaired but planetary defense unrepaired**, Resida survives, but the malfunctioning meteor defenses have destroyed the second Patrol ship. The player may remain stranded, receiving an unlimited bank account and a country house. This explanation takes precedence if communications is also broken.

With **course control and planetary defense repaired but communications unrepaired**, Resida survives, but the searching ship could not make contact and has departed. The stranded-player compensation is again the bank account and country house.

With **course control unrepaired but communications and planetary defense repaired**, the computer has produced a cure, but the orbit has decayed beyond correction and Resida will fall into its sun. The Flathead can nevertheless evacuate the player.

With **course control unrepaired and either rescue-related system also unrepaired**, the planet is doomed and there is no rescue passage. Completing the cure alone does not save the world or guarantee personal escape.

Floyd’s restoration belongs specifically to the all-three-repairs ending. Reaching some other conclusion does not unlock a separate repair opportunity afterward; the game is over.

## What must be finished, and what can be left

For the best ending, finish **communications, planetary defense, and course control before repairing the computer and leaving its interior**. Each planetary-system repair is worth six points, but their importance is not merely numerical. The optimum route totals eighty points through all the listed accomplishments.

If communications was postponed, the verified repair remains two separate chemical deliveries: in **Machine Shop**, `put flask under spout`, `press black button`, `take flask`; in **Comm Room**, `pour fluid into hole`. Return for a fresh dose with `put flask under spout`, `press gray button`, `take flask`, then again `pour fluid into hole` in **Comm Room**. Black changes the requested light to gray; gray completes the repair. A wrong dose while it is broken permanently shuts down the send console. The milky appearance of the carried fluid does not change which chemical was dispensed.

The verified visit to **Systems Monitors** after these three repairs shows **LIIBREREE**, **REEAKTURZ**, **LIIF SUPORT**, **KUMUUNIKAASHUNZ**, **PLANATEREE DEFENS**, and **PLANATEREE KORS KUNTROOL** green, with only **PRAJEKT KUNTROOL** malfunctioning. That last fault is the computer task, not a fifth unrelated repair.

The library articles and spools, Conference Room combination, medicine, Lazarus scene, and side-room exploration are optional. Teleportation is an extremely useful shortcut, not one of the ending’s repair checks. The megafuses, cracked board, inaccessible repair cabinets, brown spool, lamp, helicopter machinery, and radiation-suit search do not supply missing mandatory steps.

The essential late sequence is therefore clear: let Floyd obtain the shiny board while he still can; repair the planetary systems; let him understand the computer failure; follow his Bio Lock instructions exactly and collect the miniaturization card; enter sector 384 with a fresh laser battery; clear the speck with red light; lure the microbe off the strip with the heated laser; wear the gas mask before releasing fungicide; run to the opened cryogenic access; press the elevator button once, and go north when it arrives.


# Floyd

## Floyd

### The companion, not just the tool

Floyd is the apparently salvageable robot in **Robot Shop**, among machines otherwise dismantled beyond repair. He is about four feet tall, built for general-purpose work, and initially stands against a wall with his head lolling sideways. His designation is **B-19-7**; his introduction says that “to everyperson” he is called Floyd. Once awake, he is slightly cross-eyed, with a lopsided mechanical grin.

He is childlike, exuberant, distractible, affectionate, and capable of grief. He wants to play rather than work, regards a rubber ball as more interesting than an essential electrical component, and remembers the people and robots who once occupied the installation. His loyalty matters both mechanically and emotionally: he supplies access and retrieves things the player cannot reach, but the game first establishes him as a friend.

The library’s Robotics article puts his model in context: miniaturization has made it possible for a multipurpose B-19 to perform functions that once required whole teams of specialized robots. This is background, not evidence that Floyd can undertake every repair. In particular, do not promise that he can rebuild the other robots, repair Achilles, operate arbitrary machinery, or solve puzzles merely because he is called “multiple-purpose.”

### Waking him

From **Mech Corridor South**, **Robot Shop** is southeast; it is also east of **Machine Shop**. The verified startup is:

`activate floyd`  
`wait`  
`wait`

The activation command replies **“Nothing happens.”** That is misleading feedback, not failure: after the second `wait`, Floyd comes to life and introduces himself. The notes describe a three-step startup countdown; the playthrough establishes the practical timing as the activation turn followed by two waits. Repeating activation does not hasten it.

For players who have not learned his name, the supported introductory commands are `turn on robot`, `activate robot`, or `start robot`. The notes describe a possible joke about using Floyd’s name before discovering it, but **`activate floyd` works in the verified playthrough**, so do not insist that it must fail.

The first activation awards two points. The player need not stand in Robot Shop throughout startup: if they leave, Floyd can arrive in their current room and ask whether they turned him on and will be his friend. Trying to switch him off before he has awakened does not cancel startup.

An already active Floyd answers that he has already been activated. `turn off Floyd`, `deactivate Floyd`, and `stop Floyd` switch him off: he whimpers, feels betrayed, and keels over. He then neither follows nor chatters. After his first full awakening, switching him on again is immediate, without another startup delay or score award, and he angrily asks why the player turned him off.

This is ordinary deactivation, not death. The distinction becomes crucial later.

### Following, wandering, and company

An awake Floyd normally accompanies movement with **“Floyd follows you.”** The verified route shows him crossing the ladder over the administrative rift, riding elevators and the shuttle, and accompanying teleportation. His size does not prevent those journeys.

His presence is not guaranteed on every ordinary turn. When catching up, he has a one-in-five chance of going exploring instead, sometimes without a departure message. While already present, an otherwise eligible turn has a one-in-twenty chance of a spontaneous departure. Ordinary wandering lasts a random one to five subsequent turns, after which he returns to the player’s current location. A missing Floyd is therefore not automatically trapped, switched off, or unable to cross the last passage. Wait for him before a puzzle that requires his presence.

Scripted errands are different: Floyd does not abandon the Bio Lab expedition to follow the player as though it were an ordinary walk. Some locations also suppress routine chatter and spontaneous departures.

Questions about his whereabouts distinguish these states. He may be right here, switched off where he was left, still starting up, temporarily exploring, or out of sight on an errand. After death the answer is **“Floyd is gone.”** Do not interpret that last response as another wandering message.

Routine antics occur with a one-in-twelve chance on an eligible turn when no more important event intervenes. They include writing his name with a crayon, reciting six hundred digits of pi, rubbing his head against the player’s shoulder, checking for rust, oiling a joint, singing out of key, whistling, yawning, pacing, hiding from a mouse, worrying about batteries, proposing **Hucka-Bucka-Beanstalk**, and chanting the death scene from *Carmen*. He remembers pencil-sharpening, a bruised knee, Lazarus, and rumors about Dr. Fizpick. These remarks do not create collectible crayons, mice, batteries, or repair supplies, and battery anxiety is not a maintenance puzzle.

His one-time reactions to particular actions take precedence over ordinary activity; at most one such reaction is queued per turn. Even commands that do not advance the ordinary survival clock can allow companion activity. Do not promise that “free” commands freeze Floyd.

After successful sleep, a living, active Floyd is brought to the player for his waking greeting, even if he was elsewhere beforehand. At a bunk he bounces impatiently and calls the player “lazy bones”; after ground sleep he nudges the player and giggles at how silly they look. Once Floyd has ever been activated, sleep also has a 13 percent chance of a special dream in which he delivers papers and coffee in a crowded office and asks for a story. That dream can occur after his death. It is neither a resurrection nor a playable encounter.

Saving with Floyd alive and nearby can produce an excited fourth-wall remark about impending adventure. It is not a reliable warning that a hazard has just begun.

### Speaking, showing, and giving

Address orders directly, with the name and comma: **`floyd, ...`**. His ordinary conversation is variable, not a dependable question-and-answer encyclopedia. Sometimes he merely tilts his head, whirs, and fails to understand. A switched-off robot does not answer.

The two essential direct orders, both verified in **Repair Room**, are:

`floyd, go north`  
`floyd, take board`

Their discovery requirement matters; they are not interchangeable with a general request to fix the defenses.

Showing and giving are separate actions. `show [object] to Floyd` or `show Floyd the [object]` invites a reaction without transferring ownership. The object must be carried, although an accessible carried container counts. Most objects receive variable comments or **“Can you play any games with it?”**

The important exception is `show computer output to Floyd`. On its first useful showing, he recognizes the computer failure and recalls that a Doctor-person called the computer the Project’s most important part. This establishes the concern required for his later card-retrieval offer. **Giving him the output is not a substitute for showing it.** The verified route establishes the same concern simply by bringing him into **Computer Room**.

Showing the personal identification card, **shuttle access card**, **kitchen access card**, or **upper elevator access card** makes him ask whether those things are usually blue. This is characterization, not a color-changing instruction. Showing the **lower elevator access card** before his card reveal makes him say he has one just like it, search his compartments, and look suspiciously at the player. That uses up the same one-time recognition opportunity as his spontaneous reveal; it does not create another card.

`give [object] to Floyd` lets him carry one item in his hand. He thanks the player enthusiastically. Take it back to free that hand. If he already holds something, another gift is inspected and dropped on the floor, not stored in a hidden inventory. His concealed card compartment should not be mistaken for unlimited carrying capacity.

The **medical robot breastplate** is a special gift. Recognizing Lazarus’s remains makes him weep, drop the plate, and run off wandering. It is not a useful porter assignment.

### His lower elevator card

Floyd begins with the **lower elevator access card** hidden in a compartment. There are two supported ways to obtain it.

The deterministic method is `search robot` or `open robot` while he is switched off. The search finds and takes the magnetic-striped card. `open Floyd` is likewise a search, not opening him as an ordinary container. Once the hidden card is gone, another search finds only a crayon, which is left in place. Searching an active, living Floyd instead tickles him: he giggles, pushes the player away, and clutches his panels. If necessary, deactivate him, search him, then reactivate him, accepting his resentment.

Alternatively, he may reveal the card after witnessing a successful access-card swipe. He must be alive, present, still have the concealed card, and not already have revealed it. The chance is 5 percent on day 1, 10 percent on day 2, 30 percent on day 3, and certain from day 4 onward. Do not present the first swipe as a guaranteed trigger in every game.

In the verified playthrough, the reveal follows **`slide kitchen access card through slot`** in **Mess Hall**. Floyd claps, announces that he has a card himself, retrieves it from a panel, and waves it. The player then goes south into **Kitchen** and uses **`take lower card`**, which succeeds. The card is not a Kitchen furnishing; Floyd has brought it there.

Its practical use is demonstrated in **Lower Elevator** by **`slide lower access card through slot`**, followed by **`press down button`**. First taking the card awards one point.

Keep this and every other functional access card away from the **curved metal bar**, the magnet from **Tool Room**. Carrying the magnet silently and permanently scrambles one intact carried access card per turn; a pocket or carried container is not shielding. The verified route drops the magnet before collecting access cards. Floyd cannot repair a scrambled stripe.

### Small kindnesses and unkindnesses

The supported social commands have authored reactions but do not improve Floyd’s puzzle-solving ability. `play Floyd` produces a narrated bout of play that exhausts the player while Floyd wants more; it is not a separate timed minigame. `rub Floyd` earns a contented sigh. `kiss Floyd` gives a painful but nonfatal electric shock. `kick Floyd` makes him complain about a loose wire and sulk. Trying to kill him is interpreted as Chase and Tag, sending him running around excitedly.

With the **oil can** from **Storage East** carried, `oil Floyd`, `lubricate Floyd`, or `oil Floyd with oil can` earns thanks. Without the can he asks what to oil him with. Lubrication is kindness, not healing, an upgrade, or a way to avert his later injuries.

Shooting him with the laser makes him shout **“Yow!”**, jump away, and regard the player warily. It wastes a shot rather than solving anything. The hint book also suggests removing the player’s uniform in his presence as an amusement, but supplies no exact reaction; do not invent one.

### The Repair Room and Achilles

In **Repair Room**, north of **Systems Corridor West**, Floyd identifies the fallen **broken robot** as **Achilles**, the machinery repairer who once repaired him. Achilles was not friendly and had a troublesome foot; Floyd thinks he fell down the stairs. A Planner-person had explained that his name referred to that foot. After this account, examination of Achilles explicitly recalls his identity and old defect. Calling him **Achilles** also avoids confusing him with Floyd when both are present.

Neither Achilles nor the dismantled robots in Robot Shop can be repaired by the player. The promising **brown spool**, labelled as instructions for repairing repair robots, does not supply a rescue route: it is in the lethal **Radiation Lab**, and the supplied material establishes no usable repair instructions. Do not send players there to save a robot.

The little north doorway in Repair Room is explicitly too small for the player but suitable for Floyd. Use **`floyd, go north`** first. He squeezes through, plays with an old rubber ball until it falls apart, then returns saying that nothing interesting remains except a **shiny fromitz board**. The ball is not brought back.

Now use **`floyd, take board`**. He returns with the sound board and tosses it to the player, who narrowly catches it. Both errands resolve within their command descriptions: the narrated minutes are not instructions to issue extra waits.

Asking for the board before exploration gets **“What fromitz board?”** Asking him to explore again gets **“Not again.”** Asking for another retrieval after success does not produce a duplicate or recover a board installed elsewhere.

This board repairs **Planetary Defense**. The verified commands there are **`open panel`**, **`take second`**, and **`put shiny in panel`**; **`drop fried`** discards the removed damaged board. The good replacement stops the warning lights. Its significance is larger than points: the defense system’s faulty discrimination threatens incoming rescue ships. Obtain Floyd’s board before proceeding to his final expedition; afterward he will no longer be available for this errand.

### Other special places

In **Infirmary**, Floyd’s first qualifying turn passes without the special scene. On the next qualifying turn he discovers a bent, rusting breastplate and attached circuitry: the remains of **Lazarus**, his medical-robot friend. The **medical robot breastplate** appears, Floyd sobs, excuses himself, and runs off. Leaving before the scene resets its local waiting count; once completed, it does not repeat. Let players witness his grief without prematurely explaining its relation to later events. Do not confuse this scene with the Infirmary’s bed, which is an immediate lethal trap rather than a safe place to wait by lying down.

In **Computer Room**, his inspection of the glowing red light produces the concern needed for the Bio Lab expedition. In **Bio Lock East**, the window lets him identify the card and, with that concern established, offer to fetch it.

Teleportation has a distinct reaction. In **Booth 3**, **`slide teleportation card through slot`**, then **`press 2`**, carries him to **Booth 2** with a terrified squeal as he clutches his guidance mechanism. The reverse verified journey uses the same swipe in Booth 2 followed by **`press 3`**. His fright does not mean teleportation has injured him.

The **Library Lobby** terminal has a first-numeric-use reaction whose words are not supplied. Two article displays include specific interludes: Geography’s Planet Landmasses article includes his request to play Hucka-Bucka-Beanstalk, and The Project’s Origins of the Disease article includes his crayon name-writing. These are not new puzzle objects.

Other documented special-comment locations are **Physical Plant** in Kalamontee, the window in **Large Office**, **Observation Deck**, **Helicopter**, the elevators, **Kalamontee Platform**, **Lawanda Platform**, and the mural in **ProjCon Office**. First shuttle-control use also has a reaction. The sources identify these occasions without supplying dependable dialogue or additional mechanical effects. Likewise, recovering the key in **Admin Corridor South**, unlocking the padlock in **Mess Corridor**, opening the conference-room door from **Rec Area**, and taking the laser can prompt reactions. Do not make hearing one a prerequisite for the underlying puzzle: several actions succeed without accompanying remarks in the verified playthrough.

Ordinary chatter is suppressed in **Repair Room**, **Infirmary**, and **Bio Lock East**, and Floyd does not talk in **Comm Room**. Silence there is not evidence that he needs oil or has malfunctioned.

### Preparing the Bio Lab offer

Floyd’s sacrifice is not triggered merely by owning the right equipment or knowing that a card exists. He must be alive and present, have recognized the computer failure, and have volunteered before the inner door is opened.

The verified preparation is to enter **Computer Room** with him. He examines the light and says the computer is broken and central to the Project. **`read output`** then supplies the player’s own explanation: research and drug production are complete, drug testing is at 99.985 percent, and a malfunction in **Section 384** interrupted the process while summoning a repair robot.

From **Main Lab**, use **`open biolock door`**, **`SE`** into **Bio Lock West**, and **`E`** into **Bio Lock East**. The western outer door closes automatically. Then **`look through window`** reveals the dim **Bio Lab**, ominous moving shapes, and a magnetic-striped card just inside the door.

In the verified sequence, Floyd peers through after this examination. He warns the player not to enter, recognizes that the card is needed to fix the computer, and volunteers: robots are tough, he says, and nothing can hurt them. His trembling voice contradicts that reassurance. His proposal is his own; no additional order to “sacrifice yourself” is required.

Without his prior computer concern, he may identify the card without offering the expedition. Bring him to the warning light or show him the output rather than repeatedly opening the door. If he has volunteered but the player hesitates, he asks for the door on four subsequent waiting turns and then sulks on the fifth. Sulking alone does not erase his readiness.

### The door sequence and his death

Once Floyd has volunteered, the exact verified sequence in **Bio Lock East** is:

`open door`  
`close door`  
`wait`  
`open door`  
`close door`

The first opening sends him into **Bio Lab**. Mutations attack him and rush toward the open doorway. Close immediately. Behind the closed door the player hears growling, fighting, and a metallic scream.

The single `wait` then produces **three fast knocks**, followed by tearing metal. Those knocks are the cue: open immediately. Floyd stumbles out holding the **mini-booth card**, with the monsters pursuing. Close immediately again.

That final closure saves the player but not Floyd. He falls, drops the **mini card**, and lies torn apart, leaking oil, with exposed wires and broken circuits. The player automatically cradles his head and sings his favorite song, **the Ballad of the Starcrossed Miner**. There is no command needed to sing or comfort him. His last question is: **“Floyd did it...got card. Floyd a good friend, huh?”** The scene ends with his death and memorializes the friend who gave his life so the player might live.

This is the successful outcome of the retrieval, not proof that the player closed a door incorrectly. It awards two points. The required follow-up is **`take miniaturization card`**, which awards another point; the card has been dropped on the floor, not automatically put into inventory.

Timing errors are distinct from this unavoidable successful-sequence death. Opening before he is ready kills the player. Leaving the door open for an extra turn after either his entry or return kills the player. Reopening before the knocks kills the player. Failing to open on the next turn after the knocks leaves Floyd trapped and kills him without delivering the card or awarding the successful-scene points.

Leaving Bio Lock East during an unfinished attempt cancels the current choreography and closes the inner door; Floyd’s volunteered state is retained, and the attempt can restart later. That is not a method for completing the retrieval without injury, nor can it restart a genuinely completed death scene.

### What the player must do afterward

For the remainder of playable exploration, Floyd is gone. He no longer follows, wanders, offers cards, greets the player after sleep, or responds to saving. His body remains in **Bio Lock East**, described as the former companion lying in a pool of oil.

Examining him expresses loss. Trying to take him fails because he is too heavy and the player cannot bear to drag him. Speaking brings no answer. `turn on Floyd` does not revive him: his switch falls off. `turn off Floyd` says he has already been turned off permanently. Ordinary reactivation, oil, robot scraps, Achilles, and the brown spool provide no repair solution.

His card enables the continuation he made possible. In **Miniaturization Booth**, the verified commands are **`slide mini card through slot`** and **`type 384`**. The player must repair the computer personally; Floyd will not accompany that journey.

Afterward, the return through **Auxiliary Booth** and **Lab Office** forces an escape through Bio Lab and past his body. In the verified chase, the player passes through Bio Lock East without stopping. This is particularly important coaching: do not encourage a farewell examination or an attempt to collect Floyd while the mutants are pursuing. During that chase, pausing there is fatal.

### His return at the ending

Floyd’s loss is permanent during ordinary play, **not in every ending**. His restoration belongs specifically to complete victory, with **communications**, **Planetary Defense**, and **Course Control** all repaired, in addition to the mandatory computer repair.

At **Cryogenic Anteroom**, Veldina awakens, the cure and revival succeed, and the successful rescue brings the **S.P.S. Flathead** and Captain Sterling. The player receives an antidote and promotion, while Blather is demoted and assigned as the player’s toilet attendant. Robot technicians then return a restored Floyd.

He enthusiastically produces a **helicopter key**, a **reactor elevator card**, and a **paddleball set**, suggesting that perhaps they can be used in the sequel. These are an ending joke, not previously missed equipment or invitations to resume exploring Helicopter and Reactor Elevator. The ending does not continue as ordinary play.

The restored-Floyd scene is absent from the lesser ending branches. Those branches depend on the three planetary repairs, not a separate check of how lovingly the player treated him. Course Control saves Resida from its fatal orbital decay; communications and defense together permit rescue. Do not promise his return merely because the computer has been fixed.

### What to reveal, and when

Initially, present Floyd as the interesting robot the player can wake, not as a bundle of future solutions. If “Nothing happens” discourages them, reassure them that startup takes a moment. Let his greeting, games, memories, and spontaneous affection establish the relationship.

Give help in the order the game supplies its clues. For the lower elevator, suggest reading and using cards or examining the switched-off robot before announcing his hidden possession. At Repair Room, emphasize the robot-sized doorway; then let his report reveal the shiny board before supplying **`floyd, take board`**. At the computer and bio-lock, establish why the card matters before guiding the door sequence.

Do not forecast his death, call the Bio Lab scene a sacrifice in front of first-time players, or reveal the technicians’ eventual repair as advance reassurance. Both revelations undermine what the player is meant to experience. During the actual door sequence, however, be precise if assistance is needed: protect the player from a parser or timing failure without narrating the outcome ahead of the game.

After the successful death scene, it is fair to say that they followed the instructions correctly and that there is no local revival puzzle. Encourage continuing with the card rather than fruitlessly oiling him or risking Radiation Lab. Do not falsely claim that nothing anywhere can ever restore him; simply keep the ending undisclosed.

Finally, match explanations of the deserted installation and the Project to what the player has learned. Floyd’s memories can be discussed when heard; the full cryogenic and disease history belongs after library investigation; Veldina, the revival’s resolution, and Floyd’s restoration belong at the ending. The guide should know all of this in advance, but the players should discover their friend in the game’s own order.


# Dead ends, red herrings, ways to die, and ways to make the game unwinnable

## Dead ends, red herrings, and irreversible mistakes

### What counts as a lost game

Planetfall distinguishes three kinds of failure: immediate death, a living but unwinnable position, and an ending that is attainable but no longer the best one. A guide must distinguish them too. “You are still alive” does not mean that the essential equipment survives, and “the computer is repaired” does not mean that Resida or the rescue ship will survive.

The computer repair is compulsory. Communications, Planetary Defense, and Course Control determine the ending’s quality. Losing a tool needed for the computer can prevent any ending; permanently ruining communications still permits an ending, but rules out complete victory. Conversely, a dropped card, an expired authorization, an empty flask, or an absent wandering Floyd is usually recoverable.

When an irreversible failure is established, say so plainly and recommend returning to a position before it happened. Do not encourage fruitless attempts to repair a scrambled card, retrieve something from the rift, or revive Floyd with his switch. Death offers “another chance,” but the supplied rules do not establish precisely what that chance preserves. Do not promise harmless resurrection.

The verified playthrough supplies the working routes and commands below. Where it does not demonstrate an interaction—food, sleep, and many fatal experiments—the study notes supply the additional commands and consequences. In particular, the playthrough overrides the hint book’s claim that the pod door cannot be reached from the safety web.

### The ship is not a repair puzzle

The Feinstein cannot be saved. Deck Eight and Reactor Lobby are disciplinary cul-de-sacs, not alternative routes to a solution. Blather blocks access beyond them; the Hyperspatial Jump Machinery Room, Ion Reactor, and Auxiliary Control Room do not offer an accessible means of averting the explosion. Gangway is merely the connection between Deck Nine and Deck Eight. Once its emergency bulkhead closes, it becomes a trap.

The explosion occurs on move 10. The bulkheads close on move 11; on move 12, anyone outside Deck Nine and Escape Pod dies from the ship’s destruction and decompression. A player still on Deck Nine loses the boarding opportunity when the pod door shuts and dies in the enormous explosion on move 14. Entering the pod and stepping back out does not suspend this schedule.

The verified safe opening is ten repetitions of `wait` on Deck Nine, then `port`, immediately followed by `sit` in Escape Pod. “You are now safely cushioned within the web” is the confirmation that matters. Do not spend the emergency window inspecting the controls or answering Blather.

Blather’s demerits do not constitute a score puzzle. Scrubbing the floor makes it a little shinier but neither satisfies an escape requirement nor prevents the disaster. Saluting him is harmless; throwing the scrub brush at him produces punishment and comic outrage, not a route past him. Attacking, killing, or kicking him is different: he removes the player’s appendages and internal organs, killing them immediately.

Away from Deck Nine, his disciplinary countdown accumulates rather than resetting on each visit. Its fifth active stage sends the player to Brig; a third visit to the same offending room also causes immediate imprisonment. Brig has no escape. Its locked cell door and anti-Blather graffiti are not a key puzzle, password, or coded clue. Once jailed, the approaching shipwreck is effectively unavoidable.

The ambassador is another opening diversion. His mechanical translator cannot be taken, his brochure is an advertisement for Planetfall, and his conversation has no required answer. His slime cannot be usefully cleaned away: each scrubbing removes only “maybe one ten-thousandth.” Touching, smelling, or tasting the slime is a joke, not nourishment. Eating his celery is fatal because his metabolism is incompatible with the player’s. The celery also cannot be taken as emergency food.

The scrub brush, diary, brochure, towel, personal ID card, and decorative claims made for the Patrol uniform are not hidden solutions to the shipwreck. The diary supplies background, but reading it all aboard ship wastes the escape window. The ID card is useless as authorization for the installation’s readers. The uniform’s pocket can hold a small item, but it is not magnetic shielding. Keep the chronometer: unlike these diversions, it is useful for recognizing survival and shuttle deadlines.

### The pod and the water

Escape Pod is entirely automated. Its control panel, viewport, rockets, gyroscopes, autopilot, climate-control system, and provision panel do not conceal a piloting or repair puzzle.

Standing during the flight is dangerous twice. When the Feinstein explodes and the pod tumbles, an unseated player has a 20 percent chance of being thrown headfirst into the bulkhead and killed. Surviving that impact with bruises does not establish safety: landing while unseated is always fatal. Remain in the web until the landing message reveals the survival kit and towel.

The verified departure is `take kit`, `open door`, `out`, then `up` from Underwater to Crag. It works without `stand`. The towel is optional; its inscription identifies Escape Pod #42 and says “Don’t Panic!” It has no demonstrated drying, swimming, burning, or repair use.

An explicit post-landing departure from the web destabilizes the pod and starts an irreversible sinking countdown. Sitting down again does not cancel it. Submersion, underwater impacts, and ominous pressure noises are advancing warnings, not opportunities to search. At the fifth sinking stage, remaining inside kills the player whether the door is open or closed. Take provisions before standing, or simply follow the seated departure demonstrated in the playthrough.

Underwater has no useful object and only one mapped escape: `up`. The initial visit provides only a short grace period before the undertow kills the player. Leaving permanently records that first immersion; returning later is fatal at the next underwater update, without a renewed grace period. Never send someone back down to retrieve the pod or explore the rocks.

The ocean rises over subsequent days. Crag is submerged from day 2; Balcony’s downward route then leads straight to Underwater. Balcony is submerged from day 4, after which descending from Winding Stair likewise leads into the water. Ground sleep at Crag on day 1, Balcony on day 3, or Winding Stair on day 5 causes drowning during the night. The plaque’s picturesque account of the former Kalamontee Valley is evidence of environmental change, not a safe swimming route.

### Harmless places that invite too much attention

Courtyard’s ruined castle and West Wing are scenery. West Wing’s rubble does not conceal a supported excavation puzzle or another exit. Balcony’s weathered metal plaque describes a former tourist view and provincial capital; it cannot be taken. Winding Stair is a route and a flood-warning location, not a machinery puzzle.

Rec Area’s games and tapes are entertainment: Chess, Cribbage, Galactic Overlord, Double Fannucci, music, novels, and a champion’s biography. Conference Room’s large round table contains no established hidden object. The room and Booth 1 are optional because Booth 2 and Booth 3 already provide the useful cross-complex teleportation route.

The Conference Room combination is nevertheless real. The piece of paper in the lab uniform’s pocket gives the number for the current game; there is no universal combination. `SET DIAL TO` followed by that number opens the door from Rec Area. The legal range is 0–999, not an invitation to exhaust the hint book’s suggested thousand guesses. Closing the door locks it again, and the dial cannot be operated from inside. A player trapped on that side still has the northern route to Booth 1, but needs usable teleportation authorization to leave that way. Do not call this an unconditional permanent trap without checking the teleportation access card and the booth’s state.

Dorm A, Dorm B, Dorm C, and Dorm D are useful safe sleeping rooms. Their partitions are fixed scenery. Sanfac A, Sanfac B, Sanfac C, Sanfac D, SanFac E, and Sanfac F are not useful: their dusty, dry fixtures supply neither water nor equipment. Sanfac F’s smaller size does not make it special.

The stopped walkway between Dorm Corridor and Corridor Junction has no repair puzzle. Neither does the stopped Escalator between Lawanda Platform and Fork. Use them as ordinary routes. Shuttle seats, freight space, and Waiting Area benches are bolted-down scenery, not cargo or tools.

The Mess Hall tables conceal nothing. The answer to looking underneath them briefly claims three keys, food, and a reactor elevator pass, then explicitly retracts the claim. Tell the player that the whole discovery is a joke.

Plan Room’s maps give geographical context; its cubbyholes are empty, and the maps cannot be taken. Systems Monitors is genuinely useful, but only as a diagnostic display. Its complicated equipment cannot be operated to perform the repairs locally. Initially Library, Reactors, and Life Support are already green: there is no need to invent work for them.

Both rooms called Physical Plant are non-solutions. Their intricate heating and ventilation equipment cannot be operated by the player. Lawanda’s unusually large plant hints at the underground cryogenic installation; it does not supply a ventilation-control escape.

### The reactor and helicopter false trails

Reactor Control, Reactor Access Stairs, and Reactor Elevator lead nowhere productive. The reactor is buried below the complex, but there is no playable reactor-repair expedition. In the verified playthrough, `press up button` in Reactor Elevator gives “Nothing happens,” and `examine slot` merely describes the small slot. The Down button is equally ineffective. The elevator’s initially open door is not a puzzle to unlock.

The K-series megafuse and B-series megafuse in Storage East are plausible reactor components with no required installation. Do not let their presence turn the hint book’s fictitious reactor repair panel into a real destination.

Helipad, Helicopter, and Observation Deck are optional exploration. Observation Deck gives a view of the other island. Helipad’s high fence, fixed staircase, drooping rotor blades, and rusty helicopter provide no repair materials. Helicopter’s locked control-panel cover cannot be opened with any obtainable pre-ending key, and the aircraft cannot be flown.

The green spool in Library is labelled “Helikoptur Opuraateeng Manyuuwul.” Its directions to obtain a Helicopter Access Card and Control Panel Key from Transportation Storage are a deliberate false trail. Transportation Supply is a dark dead end, not a cache that can be successfully looted for those items. The official hints explicitly confirm that the necessary helicopter equipment does not exist during ordinary play.

Reactor Access Stairs and Transportation Supply advertise darkness, but the supplied rules do not establish a separate, specific death for moving around in those rooms. Do not invent one. They are unproductive regardless. The apparent solution—fetching the portable lamp from Radiation Lab—is itself a fatal detour.

At the best ending Floyd presents a helicopter key, reactor elevator card, and paddleball set. These are a sequel joke, not evidence that the player overlooked an earlier flight, reactor trip, or game.

### Spare parts, false tools, and useful leftovers

Storage East’s cardboard box is useful as a container and holds one important replacement: the good ninety-ohm bedistor. Its cracked seventeen-centimeter fromitz board is not a substitute for the shiny board. Installing a damaged board does not repair Planetary Defense. The fried board removed from Planetary Defense and fused bedistor removed from Course Control likewise have no subsequent repair value.

The oil can merely lets the player oil Floyd and receive thanks. It does not repair Achilles, restore Floyd after his sacrifice, fix the walkway, or make the helicopter airworthy. The disassembled robots in Robot Shop are beyond repair and cannot be assembled into a replacement companion.

Repair Room’s machines and locked cabinets are another false repair chain. Achilles cannot be repaired or salvaged. The robot-sized northern doorway is important, but only Floyd can use it. The old rubber ball he finds there falls apart; it is not an obtainable toy or a missed reward. The actual useful result is the shiny fromitz board.

The large tin can in Storage West, labelled “Spam and Egz,” cannot be opened. There is no can opener. Its apparent food is not an alternative to the survival kit or Kitchen.

Machine Shop’s dispenser has nine buttons, but the verified communications repair needs only black and gray. The red, blue, green, yellow, and brown doses have no necessary role in that sequence. The white square “BAAS” and white round “ASID” buttons produce useless base and acid. They cannot make a new laser battery or provide a demonstrated weapon against the mutants. All these chemicals are poisonous if drunk; the flask’s milky appearance does not make its contents food.

The old laser battery is not completely dead: it starts with three to five charges. It is inadequate insurance for the computer expedition, especially because shots can miss. The fresh laser battery in Lab Storage has substantially more charge. The verified replacement is `take laser`, `remove battery`, `drop battery` in Tool Room, then later `take fresh battery` and `put battery in laser` in Lab Storage.

Ordinary shooting is not a universal destruction tool. Most targets merely become warm; shooting Floyd hurts and alarms him without solving anything. The laser does not explode because it feels hot. Its heat is part of the microbe solution, though an excessively hot weapon can provoke the microbe into a fatal attack.

### Books, screens, uniforms, and other convincing clues

Library Lobby’s computer terminal is an information terminal, not the broken Project computer. The verified commands include `activate terminal`, `key 4 on the terminal`, `press 0 on the terminal`, `type 6`, `type 1`, and `deactivate terminal`. Its history, culture, geography, technology, and Project articles explain the world; the Interlogic Games articles advertise other adventures rather than launching playable games. The terminal’s advice to consult the librarian cannot be acted upon: no librarian can be summoned.

The spool reader in Library displays a spool placed in it and holds only one at a time. The green spool is the helicopter false trail. The red spool in Infirmary describes the Disease’s fever and increasing sleep requirements; it is useful explanation, not treatment. The brown spool in Radiation Lab promises “Instructions for Repairing Repair Robots,” but there is no successful Achilles-repair route, and no supported way to bring it safely to the reader.

The pale blue lab uniform is not radiation protection. Its flame-over-sleep-chamber logo, also present in ProjCon Office, explains the Project. Its useful contents are the teleportation access card and the optional Conference Room combination paper. The verified extraction is `open pocket`, then `take teleportation`.

The ProjCon Office mural is a genuine hint to a concealed passage, but not something to attack, take, or manually force aside. It opens after the revival announcement. Burstini Bonz and the mural’s garish style are flavour, not a password.

Computer Room’s pile of computer output is not a red herring. `read output` gives the indispensable damaged sector number, 384, and explains how close the Project was to completing drug testing. Floyd’s response to the glowing red light is also important. Merely examining that light later is not a dependable repair-status test: its examination text continues to describe a malfunction.

Lab Office’s memo explains the emergency fungicide. Its locked files do not supply another escape. The white “Lab Liits On” and black “Lab Liits Of” buttons only produce relay clicks in the supplied behaviour; they are not alternatives to the red “Eemurjensee Sistum” button. Auxiliary Booth is a receiving station with no outgoing controls.

Floyd’s crayon, battery worries, games, songs, and incidental stories are characterisation, not requests to collect a mouse, replace his batteries, or fetch toys. Lazarus’s medical robot breastplate is a memorial, not a repair component; giving it to Floyd makes him grieve and wander off. Dreams are similarly noninteractive. Their spiders, drowning, exploding ships, and apparent objects do not create additional playable deaths or tools.

### Hunger, exhaustion, and the Disease

Hunger and thirst are one condition. The opening grace period is 4,000 clock units. Thereafter the warnings progress through hungry, ravenous, faint, and almost passing out, with successive intervals of 900, 300, 200, and 100 units. An ordinary turn advances the clock by 54, so the last warning leaves only about two ordinary turns. “You collapse from extreme thirst and hunger” is the fatal result.

The three survival-kit blobs are identical in nourishment despite different flavours. Open the kit and use `EAT RED GOO`, `EAT BROWN GOO`, or `EAT GREEN GOO` when hungry; a meal resets the food allowance to 1,450 units. The kit must be carried directly or lying in the current room, not nested in another container. The goo cannot be carried separately.

Never obey the misleading suggestion to shake the kit. `SHAKE KIT` destroys every remaining blob, even if the kit is closed. This is not automatically an unwinnable game if Kitchen remains reachable, but it can turn the remaining journey to food into a fatal deadline. Leaving the kit in the pod is likewise a loss of the portable emergency supply, not proof that Kitchen food has ceased to exist.

The sustainable food source is Kitchen’s “Hii Prooteen Likwid Dispensur.” Take the octagonal canteen from Mess Hall, open it, put it in the matching niche, and press the dispenser button. The study-note sequence is `TAKE CANTEEN`, `OPEN CANTEEN`, `PUT CANTEEN IN NICHE`, `PUSH BUTTON`; take it away afterward. `DRINK PROTEIN LIQUID` when hungry restores nourishment for 3,600 units. An empty niche, a closed canteen, or an already full canteen merely causes spills. Neither the spills nor the uniform stain establishes a permanent machine failure.

Do not confuse that brown protein liquid with the chemical fluid. `DRINK FLASK`, `DRINK LIQUID`, or `DRINK FLUID` referring to a filled chemical flask kills the player with “delicious poisonous chemicals.”

Exhaustion is a second clock. The first tiredness warning normally leaves roughly fourteen ordinary turn increments before forced sleep, but “You can barely keep your eyes open” is an immediate deadline. Sleep is checked before the intended action: a last-minute movement command does not rescue someone already due to collapse.

Use the beds in Dorm A, Dorm B, Dorm C, or Dorm D. `ENTER BED` and, after waking, `GET UP` are supported commands. Forced sleep in any of those four dormitory rooms automatically puts the player in a bunk. Elsewhere, ground sleep has a 30 percent chance of death by ferocious beasts, except for the specific cliff-side nights that cause drowning. A sheltered-looking room is not automatically safe.

The Infirmary Bed is emphatically not an alternative. Entering it immediately summons a malfunctioning diagnostic robot that straps the player down, injects all 347 medicines, and prepares to amputate their legs. There is no intervening escape turn.

Sleep advances the day and the Disease. At sickness level 9 the player dies; without treatment this is the transition to day 9. Increasing fever, later waking, shorter waking allowances, and diminished carrying capacity are the warning signs. The medicine bottle in Infirmary is useful and need not be obtained by entering its bed. `DRINK MEDICINE` consumes its single dose and reduces sickness by two levels, never below level 1. It buys time, not a cure, and does not reverse the calendar, flooding, or calendar-based fatigue.

After waking, unworn directly carried objects lie on the surrounding room’s floor. They have not been stolen. Recover essential cards and tools before leaving, and remember that worsening illness may prevent taking the previous day’s entire load. A directly carried open canteen loses its protein liquid during this drop; close it before sleeping. A directly carried flask loses its chemical contents regardless. These are refillable losses, not permanent destruction of their containers.

### The magnet and the rift

The curved metal bar is indispensable briefly and dangerous afterward. While carried, it silently scrambles one previously intact carried access card per turn. Containers do not protect cards: neither the cardboard box nor a uniform pocket shields them. Damage can begin on the pickup turn. The personal ID card is exempt, but it is also useless for authorization.

The verified safe ordering is `take magnet` in Tool Room, `put magnet on crevice` in Admin Corridor South, then `unlock padlock with key`, `remove lock`, and `open door` in Mess Corridor. The playthrough then uses `drop all` in Storage West before taking the ladder and acquiring any access cards. That command is safe in context because there are no access cards in the load yet; do not prescribe it indiscriminately later.

A correct reader reporting “Damejd kard...akses deeniid” identifies permanent magnetic damage, not an expired activation. Drop the magnet immediately to protect any surviving cards. There is no descrambling remedy. By contrast, “Incorrect authorization” means the wrong card, and “not activated” usually means the correct swipe must be repeated.

Judge the consequences card by card. A ruined miniaturization access card before its essential use blocks the computer repair. A ruined upper elevator access card before communications is repaired may foreclose the best ending. Kitchen access governs continued food access; lower elevator and shuttle access govern the original route to Lawanda. Teleportation can replace some later transport needs, but only if already available. Do not declare every damaged card equally fatal, and do not reassure a player without checking what remains accessible.

The ladder is another irreversible trap. The verified bridge sequence in Admin Corridor is `drop ladder`, `extend ladder`, `place ladder across rift`, then `N`. The extension must happen before placement. An unextended ladder is too short and falls forever into the rift. `COLLAPSE LADDER` while it spans the gap also drops it into the chasm.

If the ladder is lost before the office cards are collected, the ordinary route to those unique cards is gone. If it is collapsed from Admin Corridor North, the player is stranded on the northern side. If it is lost after the cards have been brought south, however, the loss need not prevent completion. Check the actual location of the kitchen access card, upper elevator access card, and shuttle access card rather than treating every ladder loss identically.

Ordinary `N` and `S` crossings over the extended ladder are safe despite the swaying narration. Explicit jumping is fatal on either bank, even with the bridge present. `JUMP RIFT`, `JUMP ACROSS RIFT`, and equivalent commands send the player onto the sharp rocks.

Throwing objects into the rift permanently destroys access to them. This is especially serious for unique cards, the flask before communications repair, the good bedistor or pliers before Course Control repair, the shiny board before Planetary Defense repair, and the laser or usable battery before the computer expedition. An already-used key or discarded fried board is a different matter. Essential status depends on which task remains unfinished.

### Communications can be ruined without killing you

The send console in Comm Room requires black chemical first, gray chemical second. The current annunciator light gives the required colour, even though both doses turn milky white in the flask and the fault is called a coolant-system malfunction.

The verified first delivery is `put flask under spout`, `press black button`, `take flask` in Machine Shop, followed by `pour fluid into hole` in Comm Room. The light changes from black to gray. Repeat the dispenser sequence with `press gray button`, then `pour fluid into hole` again. The warning disappears and the message begins transmitting.

A wrong dose at either stage permanently shuts the send console down. “Kuulint Sistum Imbalins Kritikul -- Shuteeng Down Awl Sistumz” is the unmistakable failure message. Later correct doses disappear without effect. This is a best-ending lockout, not an immediate death or proof that every other planetary system has stopped.

Pressing another dispenser button over a full flask does not mix or replace its contents. If the player has filled the wrong dose but has not poured it into the console, the situation is still recoverable: `EMPTY FLASK` discards it, allowing a fresh correct fill. Once communications is successfully repaired, extra fluid cannot break it.

The receive console is separate. Its playback is the Feinstein’s final transmission, not a live channel that can summon rescue by itself.

### Transport traps that are not always permanent

The working Upper Elevator and Lower Elevator differ from Reactor Elevator. Calling a car and authorizing its controls are separate actions. The verified swipes are `slide upper access card through slot` and `slide lower access card through slot`, followed promptly by the appropriate `press up button` or `press down button`. A timeout is recoverable with another valid swipe; trying to insert a card into the shallow slot is simply the wrong operation.

In Alfie Control East, the verified outward shuttle sequence is `slide shuttle access card through slot`, `push lever`, then `pull lever`. That starts at 5 and returns the lever to its central, constant-speed position. Use `wait` until the message identifies the brightly lit station and concrete platforms, then immediately `pull lever` again to stop.

Speed does not shorten the journey: every moving turn advances one travel stage. Leaving the lever at “+” only increases the eventual collision. Arriving at 5–20 produces a nonfatal crash; at 25 or above it kills the player. Trying to leave the cabin at the last instant does not evade the crash.

Stopping early is not an arrival. The cabin remains shut between stations. Restart while authorization is available; if it has expired, swipe again. New activation is refused when time is greater than 6000 because evening travel requires special authorization that is not supplied. Exactly 6000 is not excluded. A stranded evening operator faces hunger and unsafe sleep rather than an established magical release from the cabin.

Use the cabin facing the tracks, not the terminal wall. Alfie initially departs Kalamontee from Alfie Control East; Betty initially departs Lawanda from Betty Control West. The opposite cabin becomes the return-driving position.

There is no call button at Waiting Area. If Lower Elevator has been left upstairs, arriving at Kalamontee by shuttle does not let the player summon it from below. This is a route-planning trap, particularly after mixing teleportation and both cars. Check whether a usable shuttle can return to Lawanda and whether teleportation is available; the closed lower landing alone does not prove an irreversible loss.

Teleportation itself requires a fresh authorization for each departure. The verified route is `slide teleportation card through slot`, then `press 2` from Booth 3 or `press 3` from Booth 2. Objects on the departure booth’s floor travel too. An apparently missing dropped object may therefore be at the destination, not destroyed.

### The two repairs that decide Resida’s future

Planetary Defense needs the shiny board, not the cracked spare. Before Floyd’s sacrifice, visit Repair Room and use the verified `floyd, go north`, then `floyd, take board`. Exploration must precede retrieval; asking for the board before Floyd discovers it fails. These errands resolve in their command descriptions and do not require waiting through narrated “minutes.”

In Planetary Defense, use `open panel`, `take second`, and `put shiny in panel`. The warning lights stop. `drop fried` disposes of the useless removed board. Attempts to pull the first, third, or fourth board produce an electrical shock and refusal, not a documented death. Once the shiny board is installed, attempting to remove it also produces the protective shock refusal.

Letting Floyd die before he has retrieved the shiny board forfeits the supported route to that replacement. The game may still reach an ending, but the best ending is lost because planetary defenses cannot be repaired. This is why the guide must check for the board or completed repair before initiating the Bio Lab sacrifice.

For Course Control, bring the good ninety-ohm bedistor from Storage East and wide-nosed pliers from Tool Room. The verified sequence is `open cube`, `take fused with pliers`, `put good in cube`. The warnings go out. Taking the fused part without pliers merely fails; taking the good bedistor back out after installation is fatal: “Kerzap!! You should know better than to touch an active bedistor!” Leave the successful repair alone.

Systems Monitors provides the useful pre-endgame audit. In the verified route, LIIBREREE, REEAKTURZ, LIIF SUPORT, KUMUUNIKAASHUNZ, PLANATEREE DEFENS, and PLANATEREE KORS KUNTROOL are green, with only PRAJEKT KUNTROOL still malfunctioning. That is the right state before the final computer repair.

### Floyd’s sacrifice and the unrecoverable card failure

Floyd’s normal wandering is not a soft lock. He ordinarily returns after one to five turns. A switched-off Floyd remains where he was left and can be reactivated; the first activation’s “Nothing happens” is a startup delay, not evidence of a broken robot. In the verified playthrough, `activate floyd`, `wait`, `wait` brings him to life.

His concealed lower elevator access card can be obtained by searching him while switched off. Alternatively, after he visibly waves it following a successful swipe, take it. The verified playthrough uses `take lower card` in Kitchen after his reveal. Do not assume every swipe guarantees the offer, especially early in the game.

Before Bio Lock East, let Floyd notice the broken computer in Computer Room, as he does in the playthrough. Showing him the computer output is the alternative; merely giving it to him is not the same prerequisite. Without that concern, seeing the card through the window is not enough to make him volunteer.

The exact successful sequence is `look through window`, followed by Floyd’s offer, then `open door`, `close door`, `wait`, `open door`, `close door`. The single wait advances to the three-knock cue. After the final closure, use `take miniaturization card`.

Each timing error has a distinct consequence. Opening before Floyd is ready lets the mutants devour the player. Leaving the door open after he enters kills the player. Reopening before the knocks also kills the player. Failing to reopen immediately after the knocks leaves Floyd dead inside without delivering the card, preventing the supported computer-repair route. Letting him return but failing to close the door immediately kills the player.

Walking out during an unfinished attempt can cancel and reset the choreography; it is not the same as waiting fatally in place. A genuinely completed failure, with Floyd dead inside, cannot be restarted.

His death after successful delivery is intended, not evidence that the player mistimed the rescue. The miniaturization access card must be on the floor of Bio Lock East; that distinguishes successful sacrifice from failed retrieval. His body cannot be carried, and his switch cannot revive him. Only the best ending supplies his restoration.

### Radiation is a death sentence, not a clothing puzzle

Radiation Lock East explicitly warns that radiation suits are required beyond it. There is no radiation suit anywhere. Neither the Patrol uniform nor the pale blue lab uniform supplies an exemption.

Entering Radiation Lab starts a poisoning countdown that continues after leaving. Sickness and dizziness appear on the fifth counted turn; vomiting and loss of hair on the sixth; death follows on the seventh. The supplied rules provide no cure for this countdown, and the experimental Disease medicine is not established as one.

Thus the portable lamp and brown spool are not prizes that can safely be grabbed during a quick visit. Once the player has entered Radiation Lab, advise returning to a position before entry rather than trying to race the symptoms. Its split canisters, incomprehensible equipment, and southern crack offer no solution. The crack looks into Bio Lab but is too small to pass through.

### The computer’s irreversible failures

In Miniaturization Booth the verified commands are `slide mini card through slot`, then `type 384`. The authorization is temporary but renewable. Any other recognized integer while the booth is active teleports the player into live circuitry and kills them. This is not a combination to discover experimentally.

Before entering, have a usable laser with the fresh battery installed. A player who reaches the interior unprepared can return before repairing the speck; entering Station 384 from the strip then returns them to Miniaturization Booth. After successful repair, the return instead goes to Auxiliary Booth and the endgame.

The verified inward route is `E`, `N`, `N` to Strip Near Relay. Use `set laser to 1`, then `shoot speck with laser` until two hits have occurred. The playthrough achieves this with two shots, but misses are possible and consume charges. The first hit only makes the speck sizzle; the second vaporizes it and announces that Sector 384 will activate in 200 millichrons.

Settings 2–6 destroy the red plastic enclosure and the vacuum-sealed relay. A heap of melted or shattered plastic is an unwinnable computer, not an alternative successful repair. The laser begins on setting 5, so assuming its initial setting is correct is particularly dangerous. Even after safely destroying the speck, a later non-red shot can still wreck the relay.

Once repaired, leave the computer interior before the 200-turn activation expires. Remaining anywhere on its four-room circuit when it powers up is fatal. The blank plates at Station 384 and featureless wall beyond Strip Near Relay have no further puzzle. Ordinary attempts to walk into the void or shattered relay are refused in the supplied rules; do not invent a fatal result for those refused directions.

### The microbe needs bait, not killing

On the return to Middle of Strip after repair, the microbe appears and blocks the southern route. Its arrival is a grace event. Thereafter two turns of unopposed approach are survivable; the next kills the player by digestion. Effective shots prevent an advance for that turn but do not erase earlier advances. Red light passes harmlessly through its red membrane.

The verified solution is `set laser to 2`, eight repetitions of `shoot microbe with laser`, then `throw laser off strip`. Both weapon and microbe fall into the void. Follow with `S`, `W` to return through Station 384 automatically.

That eight-shot sequence is the demonstrated case, not a universal instruction to ignore the weapon’s existing heat. A successful shot adds heat; a turn without one cools it. The throw-off-strip solution requires warmth above seven. If the laser is too cool, or the active microbe is not present with it, throwing it away simply loses the weapon. The successful message explicitly says that the hungry microbe lunges after it. Without that message, the obstacle has not been solved.

Feeding the laser directly has different thresholds. At warmth seven or less the microbe rejects it; at eight through ten it eats the weapon and survives; at eleven or more it eats the weapon, suffers from its heat, and rolls off the strip. The verified over-edge throw works earlier and avoids this misleading middle range.

Do not keep shooting indefinitely. The microbe regenerates, charges are finite, the sector is approaching activation, and a shot made when the weapon’s pre-shot warmth exceeds thirteen provokes a fatal heat-driven lunge that sends both player and microbe into the void. Laser warmth itself is not lethal; the creature’s response is.

A `Click` from an exhausted or missing battery while the microbe is advancing is a serious emergency. Setting changes, ineffective red shots, examinations, failed movement, and premature bait attempts can consume the remaining approach allowance. Retreating north once to Strip Near Relay resets the closing count there, but the microbe follows and there is no northern escape. This buys a short reprieve, not a replacement solution.

### The office, mist, and chase

After the repaired computer sends the player to Auxiliary Booth, there is no outbound teleportation control. The route is north to Lab Office and west through Bio Lab. Opening the office door before releasing fungicide, or after its protection expires, immediately admits the mutants and kills the player.

Prepare before pressing the emergency button. The verified office sequence is `examine desk`, `read memo`, `open desk`, `take mask`, `wear gas mask`, `press red button`, `open door`, `w`, `open lab door`, `w`.

The gas mask must be worn, not merely carried. Entering active fungicide without it is fatal; coming back into Lab Office from the treated Bio Lab without it is also fatal. Entering Bio Lab without active fungicide, before a chase is underway, is immediately fatal by mutant attack.

Fungicide provides a very short window. After its activation, two ordinary advances exhaust it. Entering Bio Lab and opening its western lab door receive special grace treatment, which is why the verified sequence works. Inspecting mutants or trying the light buttons instead can leave the player inside when the mist clears, causing immediate death. The red button can renew protection while still safely preparing in the office, but it is not permission to delay in the lab.

The four creatures—the triffid, mutant grue, mutant troll, and rat-ant—have no demonstrated combat solution. Acid, the office light buttons, and the wall crack do not bypass them.

On emerging west into Bio Lock East, the mist expires and the chase begins. The verified continuation is `w` to Bio Lock West, `open door`, `w` to Main Lab, `w` to Project Corridor East, `w` to Project Corridor, `s` to ProjCon Office, and `s` to Cryo-Elevator.

Continuous forward movement is the rule. A stationary turn is ordinarily fatal, including an examination or failed exit. Immediate backtracking runs directly into the pursuers and is fatal. There is one stationary allowance in Bio Lock West for opening the outer door, and one in Bio Lab; these are not renewable opportunities to explore. Closing the inner lab door during an active chase is “not soon enough” and does not provide a reliable barrier.

The outer bio-lock door closes automatically, so expect to reopen it during escape. Do not try to collect Floyd’s body or linger over his death at this stage. Ensure the revival announcement has opened ProjCon Office’s southern passage before committing to the chase; a still-blocked mural is not something that can be forced during pursuit.

### The last button can kill you twice

On entering Cryo-Elevator, immediately use the verified `press button`. It closes the door just before the mutants arrive and ends the chase. Waiting, looking around, or pressing unrelated scenery instead lets the mutants seize and kill the player.

The playthrough then uses `z`, `z` until the door opens onto the room to the north. Follow the arrival message rather than a conflicting turn count.

Once the door has opened at the bottom, go `N` into Cryogenic Anteroom. Do not press the button again. A second journey returns the elevator to the waiting mutants, who kill the player one move short of the ending.

### Recognizing a lesser ending before it happens

Finish the three planetary repairs before the computer expedition. After the relay repair, the forced return through Auxiliary Booth and the mutant escape are not a sensible opportunity to resume unfinished errands.

If communications, Planetary Defense, and Course Control are all repaired, complete victory includes the cure, Resida’s survival, rescue by the S.P.S. Flathead, promotion, Blather’s humiliation, and Floyd’s restoration.

If Course Control is repaired but Planetary Defense is not, the faulty defenses destroy the rescue ship. This outcome takes precedence if communications is also broken. If Course Control and Planetary Defense are repaired but communications is not, the searching ship gives up and leaves. Both branches save Resida but leave the player potentially stranded, compensated with a country house and unlimited bank account; neither supplies the complete-victory Floyd reunion.

If Course Control remains broken, Resida’s orbit is beyond correction and the planet will fall into the sun. Working communications and defenses still permit the player’s evacuation. Without both, the ending provides no rescue.

An omitted repair is not inherently irreversible while its equipment and routes remain available. A shut-down send console, a lost unique replacement, Floyd dead before retrieving the shiny board, or ruined authorization that blocks the remaining route is different. Explain the specific loss rather than saying merely that the score is low. Conversely, a missing optional view, spool, diary entry, oiling, or helicopter visit is not the reason for a bad ending.

### Do not turn the hint book’s jokes into objectives

The official hints deliberately invent Lieutenant Measle, an ambassador’s map, a Galley and stew, an autopilot handbook, a hose, birds and insects, reactor repair panels, helicopter flights, a radiation suit, and other tempting chains. Their questions are not evidence that those things exist. Waldo belongs to Suspended; the communicative giant spider belongs to Starcross. The towel cannot be turned into a demonstrated lamp, and acid and base cannot manufacture the laser battery.

For a player asking about one of these, the useful answer is direct: it is a joke or false trail, not missing progress. For a genuine object with only contextual value, explain that value. For a timed hazard or permanent failure, intervene before another exploratory command spends the remaining opportunity. Planetfall rewards curiosity, but the guide’s job is to know when curiosity is harmless, when it costs food and daylight, and when one more experiment destroys the route home.

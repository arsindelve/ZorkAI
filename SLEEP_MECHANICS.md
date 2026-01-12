# Planetfall Sleep and Fatigue Mechanics

This document provides a detailed specification of the sleep and fatigue system in Planetfall, suitable for reimplementation in any programming language.

## Overview

Planetfall uses a timer-based fatigue system that forces the player to find safe sleeping locations. Unlike the hunger system, sleep has complex interactions with:
- **Location safety** (sleeping in wrong places can kill)
- **Day/night cycle** (each sleep advances the day counter)
- **Sickness system** (wake-up messages vary by health)
- **Inventory management** (items drop when sleeping)

The sleep system creates time pressure that increases each day, with the game becoming unwinnable after day 9.

---

## State Variables

### Primary Variable

| Variable | Type | Initial Value | Description |
|----------|------|---------------|-------------|
| `SLEEPY_LEVEL` | Integer | 0 | Current fatigue state (0-5) |

### Related Variables

| Variable | Type | Initial Value | Description |
|----------|------|---------------|-------------|
| `DAY` | Integer | 1 | Current game day (death at day 9) |
| `SICKNESS_LEVEL` | Integer | 0 | Disease state (affects wake messages) |
| `HUNGER_LEVEL` | Integer | 0 | Hunger state (adjusted on wake) |
| `INTERNAL_MOVES` | Integer | Variable | Game turn counter, reset each day |

---

## Timer System

### Initial Setup

At game start:

```
Queue sleep_warning_interrupt at 3600 ticks
```

### Daily Reset

Each day after waking, the sleep timer is reset with progressively shorter intervals:

| Day | Ticks Until First Warning | Approximate Real Time |
|-----|---------------------------|----------------------|
| 2 | 5800 | ~1.5 hours |
| 3 | 5550 | ~1.4 hours |
| 4 | 5200 | ~1.3 hours |
| 5 | 4800 | ~1.2 hours |
| 6 | 4300 | ~1 hour |
| 7 | 3700 | ~55 minutes |
| 8 | 3000 | ~45 minutes |
| 9 | **DEATH** | N/A |

---

## Fatigue Level Progression

### State Machine

```
Level 0: Well-rested (starting state)
    |
    v (after 3600 ticks initially, varies by day)
Level 1: Beginning to feel weary
    |
    v (after 400 ticks)
Level 2: Really tired
    |
    v (after 135 ticks)
Level 3: About to drop
    |
    v (after 60 ticks)
Level 4: Can barely keep eyes open
    |
    v (after 50 ticks)
Level 5: FORCED SLEEP (location-dependent outcome)
```

### Warning Messages and Intervals

| Level | Ticks Until Next Level | Message |
|-------|------------------------|---------|
| 0 → 1 | 3600 (initial) / varies by day | "You begin to feel weary. It might be time to think about finding a nice safe place to sleep." |
| 1 → 2 | 400 | "You're really tired now. You'd better find a place to sleep real soon." |
| 2 → 3 | 135 | "If you don't get some sleep soon you'll probably drop." |
| 3 → 4 | 60 | "You can barely keep your eyes open." |
| 4 → 5 | 50 | (forced sleep occurs) |

---

## Sleep Warning Routine (Pseudocode)

```python
def sleep_warning_interrupt():
    global SLEEPY_LEVEL

    SLEEPY_LEVEL += 1

    # Special case: already in bed
    if player_location() == BED:
        print("\nYou suddenly realize how tired you were and how "
              "comfortable the bed is. You should be asleep in no time.")
        disable_interrupt(sleep_warning_interrupt)
        queue_interrupt(fall_asleep_interrupt, delay=16)
        return

    if SLEEPY_LEVEL == 1:
        queue_interrupt(sleep_warning_interrupt, delay=400)
        print("\nYou begin to feel weary. It might be time to think "
              "about finding a nice safe place to sleep.")

    elif SLEEPY_LEVEL == 2:
        queue_interrupt(sleep_warning_interrupt, delay=135)
        print("\nYou're really tired now. You'd better find a place "
              "to sleep real soon.")

    elif SLEEPY_LEVEL == 3:
        queue_interrupt(sleep_warning_interrupt, delay=60)
        print("\nIf you don't get some sleep soon you'll probably drop.")

    elif SLEEPY_LEVEL == 4:
        queue_interrupt(sleep_warning_interrupt, delay=50)
        print("\nYou can barely keep your eyes open.")

    elif SLEEPY_LEVEL == 5:
        handle_forced_sleep()
```

---

## Forced Sleep (Level 5)

When `SLEEPY_LEVEL` reaches 5, the outcome depends on location:

### Safe Locations

```python
def handle_forced_sleep():
    location = player_location()

    # Already in bed
    if location == BED:
        print("\nYou slowly sink into a deep and blissful sleep.")
        dreaming()
        return

    # In a dormitory room
    if location in [DORM_A, DORM_B, DORM_C, DORM_D]:
        print("\nYou climb into one of the bunk beds and "
              "immediately fall asleep.")
        move_player_to(BED)
        dreaming()
        return

    # Dangerous location - forced ground sleep
    handle_dangerous_sleep()
```

### Dangerous Ground Sleep

```python
def handle_dangerous_sleep():
    global DAY

    print("\nYou can't stay awake a moment longer. You drop to the "
          "ground and fall into a deep but fitful sleep.")

    # Day-specific drowning deaths
    if (DAY == 1 and HERE == CRAG) or \
       (DAY == 3 and HERE == BALCONY) or \
       (DAY == 5 and HERE == WINDING_STAIR):
        player_death("""

Suddenly, in the middle of the night, a wave of water washes over you.
Before you can quite get your bearings, you drown.""")
        return

    # 30% chance of beast attack
    if random_percent(30):
        player_death("""

Suddenly, in the middle of the night, you awake as several ferocious
beasts (could they be grues?) surround and attack you. Perhaps you
should have found a slightly safer place to sleep.""")
        return

    # 70% chance of survival
    dreaming()
```

### Death Locations by Day

| Day | Dangerous Location | Death Type |
|-----|-------------------|------------|
| 1 | CRAG | Drowning (wave of water) |
| 3 | BALCONY | Drowning (wave of water) |
| 5 | WINDING_STAIR | Drowning (wave of water) |
| Any | Any unsafe location | 30% beast attack |

---

## Voluntary Sleep (BED Interaction)

### Entering a Bed

```python
def bed_action_handler(verb):
    if verb in ["ENTER", "BOARD", "CLIMB"]:
        # Death trap in Infirmary
        if HERE == INFIRMARY:
            player_death("""You climb into the bed. It is soft and comfortable.
After a few moments, a previously unseen panel opens, and a diagnostic
robot comes wheeling out. It is very rusty and sways unsteadily, bumping
into several pieces of infirmary equipment as it crosses the room. As
the robot straps you to the bed, you notice some smoke curling from its
cracks. Beeping happily, the robot injects you with all 347 serums and
medicines it carries. The last thing you notice before you pass out is
the robot preparing to saw your legs off.""")
            return

        # Normal bed entry when tired
        if SLEEPY_LEVEL > 0:
            move_player_to(BED)
            queue_interrupt(fall_asleep_interrupt, delay=16)
            disable_interrupt(sleep_warning_interrupt)
            print("Ahhh...the bed is soft and comfortable. You should "
                  "be asleep in short order.")
            return

        # Not tired
        move_player_to(BED)
        print("You are now in bed.")
```

### Leaving a Bed

```python
def bed_leave_handler():
    # Cannot leave if falling asleep
    if is_interrupt_queued(fall_asleep_interrupt):
        print("How could you suggest such a thing when you're so tired "
              "and this bed is so comfy?")
        return False

    move_player_out_of(BED)
    return True
```

### Key Mechanics

- **Infirmary Death Trap**: Entering bed in INFIRMARY always kills player
- **Sleep Queue**: When tired and entering bed, `fall_asleep_interrupt` queues for 16 ticks
- **Forced Stay**: Cannot leave bed once sleep is queued
- **Objects Bounce**: Items placed on bed fall to floor

---

## Fall Asleep Routine

```python
def fall_asleep_interrupt():
    print("\nYou slowly sink into a deep and restful sleep.")
    disable_interrupt(fall_asleep_interrupt)
    dreaming()
```

This fires 16 ticks after entering bed while tired.

---

## V-SLEEP Verb Handler

```python
def verb_sleep():
    if SLEEPY_LEVEL == 0:
        print("You're not tired!")
        return

    if is_interrupt_queued(fall_asleep_interrupt):
        print("You'll probably be asleep before you know it.")
        return

    print("Civilized members of society usually sleep in beds.")
```

**Key Points:**
- Cannot sleep if not tired
- If already falling asleep, acknowledge it
- Otherwise, hint to find a bed

---

## Dreaming Routine

```python
DREAMS = [
    """...You find yourself on the bridge of the Feinstein. Ensign Blather
is here, as well as Admiral Smithers. You are diligently scrubbing the
control panel. Blather keeps yelling at you to scrub harder. Suddenly
you hit the ship's self-destruct switch! Smithers and Blather howl at
you as the ship begins exploding! You try to run, but your feet seem
to be fused to the deck...""",

    """...You gulp down the last of your Ramosian Fire Nectar and ask the
andro-waiter for another pitcher. This pub makes the finest Nectar on
all of Ramos Two, and you and your shipmates are having a pretty rowdy
time. Through the windows of the pub you can see a mighty, ancient
castle, shining in the light of the three Ramosian moons. The Fire
Nectar spreads through your blood and you begin to feel drowsy...""",

    """...Strangely, you wake to find yourself back home on Gallium. Even
more strangely, you are only eight years old again. You are playing
with your pet sponge-cat, Swanzo, on the edge of the pond in your
backyard. Mom is hanging orange towels on the clothesline. Suddenly
the school bully jumps out from behind a bush, grabs you, and pushes
your head under the water. You try to scream, but cannot. You feel
your life draining away...""",

    """...Your vision slowly returns. You are on a wooded cliff overlooking
a waterfall. A rainbow spans the falls. Blather stands above you,
bellowing that the ground is filthy -- scrub harder! You throw your
brush at Blather, but it passes thru him as though he were a ghost,
and sails over the cliff. Blather leaps after the valuable piece of
Patrol property, and both plummet into the void...""",

    """...At last, the Feinstein has arrived at the historic Nebulon system.
It's been five months since the last shore leave, and you're anxious
for Planetfall. You and some other Ensigns Seventh Class enter the
shuttle for surfaceside. Suddenly, you're alone on the shuttle, and
it's tumbling out of control! It lands in the ocean and begins sinking!
You try to clamber out, but you are stuck in a giant spider web. A
giant spider crawls closer and closer..."""
]

FLOYD_DREAM = """You are in a busy office crowded with people. The only one
you recognize is Floyd. He rushes back and forth between the desks,
carrying papers and delivering coffee. He notices you, and asks how
your project is coming, and whether you have time to tell him a story.
You look into his deep, trusting eyes..."""

def dreaming():
    # Special Floyd dream (13% chance if Floyd has been touched)
    if has_flag(FORK, TOUCHBIT) and random_percent(13):
        print(FLOYD_DREAM)
    # Normal dream (60% chance)
    elif random_percent(60):
        print("\n")
        print(random_choice(DREAMS))

    waking_up()
```

**Dream Selection:**
- 13% chance of Floyd dream (if FORK has TOUCHBIT)
- 60% chance of random dream from pool
- 27% chance of no dream (goes directly to waking)

---

## Waking Up Routine

```python
def waking_up():
    global DAY, SLEEPY_LEVEL, HUNGER_LEVEL, SICKNESS_WARNING_FLAG

    # Advance day
    DAY += 1

    # Enable next sickness check
    SICKNESS_WARNING_FLAG = True

    # Reset fatigue
    SLEEPY_LEVEL = 0

    # Reset daily timers
    reset_time()

    # Drop non-worn inventory items
    for item in player_inventory():
        if not has_flag(item, WORNBIT):
            drop_item_to_floor(item)

        # Spoil food in open canteen
        if item == CANTEEN and HIGH_PROTEIN in CANTEEN and is_open(CANTEEN):
            remove_from_game(HIGH_PROTEIN)

        # Spoil chemical fluid in flask
        if item == FLASK and CHEMICAL_FLUID in FLASK:
            remove_from_game(CHEMICAL_FLUID)

    # Display new date
    print(f"\n***** SEPTEM {DAY + 5}, 11344 *****\n")

    # Wake-up message based on location and health
    if player_location() != BED:
        print("You wake and slowly stand up, feeling stiff from your "
              "night on the floor.")
    elif SICKNESS_LEVEL < 3:
        print("You wake up feeling refreshed and ready to face the "
              "challenges of this mysterious world.")
    elif SICKNESS_LEVEL < 6:
        print("You wake after sleeping restlessly. You feel weak and listless.")
    else:
        print("You wake feeling weak and worn-out. It will be an effort "
              "just to stand up.")

    # Hunger adjustment
    if HUNGER_LEVEL > 0:
        HUNGER_LEVEL = 4
        queue_interrupt(hunger_warning_interrupt, delay=100)
        print(" You are also incredibly famished. Better get some breakfast!")
    else:
        queue_interrupt(hunger_warning_interrupt, delay=400)

    print()

    # Floyd greeting (if present and introduced)
    if has_flag(FLOYD, RLANDBIT) and FLOYD_INTRODUCED:
        move_object(FLOYD, HERE)
        FLOYD_SPOKE = True
        if player_location() == BED:
            print('Floyd bounces impatiently at the foot of the bed. '
                  '"About time you woke up, you lazy bones! '
                  'Let\'s explore around some more!"')
        else:
            print('Floyd gives you a nudge with his foot and giggles. '
                  '"You sure look silly sleeping on the floor," he says.')
```

### Wake-Up Effects Summary

| Effect | Details |
|--------|---------|
| Day advances | `DAY += 1` |
| Fatigue resets | `SLEEPY_LEVEL = 0` |
| Sickness enabled | Next sickness warning will fire |
| Items dropped | All non-worn items fall to floor |
| Food spoils | Open canteen loses protein liquid |
| Chemical spoils | Flask loses chemical fluid |
| Hunger check | If hungry, set to level 4 (urgent) |
| Floyd moves | Follows player to current location |

---

## Reset Time Routine

```python
def reset_time():
    global DAY, INTERNAL_MOVES

    if DAY == 2:
        clear_flag(BALCONY, TOUCHBIT)
        INTERNAL_MOVES = 1600 + random(80)
        queue_interrupt(sleep_warning_interrupt, delay=5800)

    elif DAY == 3:
        clear_flag(BALCONY, TOUCHBIT)
        INTERNAL_MOVES = 1750 + random(80)
        queue_interrupt(sleep_warning_interrupt, delay=5550)

    elif DAY == 4:
        clear_flag(WINDING_STAIR, TOUCHBIT)
        INTERNAL_MOVES = 1950 + random(80)
        queue_interrupt(sleep_warning_interrupt, delay=5200)

    elif DAY == 5:
        clear_flag(WINDING_STAIR, TOUCHBIT)
        INTERNAL_MOVES = 2150 + random(80)
        queue_interrupt(sleep_warning_interrupt, delay=4800)

    elif DAY == 6:
        clear_flag(COURTYARD, TOUCHBIT)
        INTERNAL_MOVES = 2450 + random(80)
        queue_interrupt(sleep_warning_interrupt, delay=4300)

    elif DAY == 7:
        clear_flag(COURTYARD, TOUCHBIT)
        INTERNAL_MOVES = 2800 + random(80)
        queue_interrupt(sleep_warning_interrupt, delay=3700)

    elif DAY == 8:
        INTERNAL_MOVES = 3200 + random(80)
        queue_interrupt(sleep_warning_interrupt, delay=3000)

    elif DAY == 9:
        player_death("Unfortunately, you don't seem to have "
                     "survived the night.")
```

### Day 9 Death

Reaching day 9 causes automatic death during the night. The player has exactly 8 complete days to finish the game.

---

## Tiredness Status Display

When player examines themselves:

```python
def display_tiredness_status():
    if SLEEPY_LEVEL == 0:
        print("You feel well-rested.")
    else:
        if SLEEPY_LEVEL > 2:
            intensity = "phenomenally"
        elif SLEEPY_LEVEL > 1:
            intensity = "quite"
        else:
            intensity = "sort of"
        print(f"You feel {intensity} tired.")
```

| Level | Status Message |
|-------|---------------|
| 0 | "You feel well-rested." |
| 1 | "You feel sort of tired." |
| 2 | "You feel quite tired." |
| 3+ | "You feel phenomenally tired." |

---

## Safe Sleep Locations

### Dormitory Rooms

Four dormitory rooms provide safe sleeping:

- **DORM_A** - Contains global BED object
- **DORM_B** - Contains global BED object
- **DORM_C** - Contains global BED object
- **DORM_D** - Contains global BED object

When player reaches `SLEEPY_LEVEL` 5 in any dorm, they automatically climb into a bunk bed.

### BED Object

```python
BED = {
    "description": "bed",
    "synonyms": ["BUNK", "BED"],
    "adjectives": ["MULTI", "TIERED", "BUNK"],
    "flags": [NDESCBIT, CLIMBBIT, VEHBIT],
    "is_global": True  # Available in multiple rooms
}
```

The BED is a global object present in all dormitory rooms.

---

## Dangerous Locations

### Infirmary (Always Fatal)

Entering bed in INFIRMARY triggers death by malfunctioning diagnostic robot. This is unconditional.

### Flood Zones (Day-Specific)

| Location | Fatal Day | Death Cause |
|----------|-----------|-------------|
| CRAG | Day 1 | Wave of water (drowning) |
| BALCONY | Day 3 | Wave of water (drowning) |
| WINDING_STAIR | Day 5 | Wave of water (drowning) |

### All Other Locations

Sleeping on the ground anywhere else has:
- 30% chance of beast/grue attack (death)
- 70% chance of survival (but items still drop)

---

## Special Interactions

### Chase Sequence

During chase sequences, sleep warnings are disabled:

```python
# In chase sequence handler
disable_interrupt(sleep_warning_interrupt)
```

This prevents the player from falling asleep mid-chase.

### Sleep + Hunger Interaction

If player goes to sleep while hungry (any `HUNGER_LEVEL > 0`):
- Wake up with `HUNGER_LEVEL = 4`
- Only 100 ticks until death from starvation
- Urgent message: "You are also incredibly famished. Better get some breakfast!"

---

## Complete Example Timeline

### Day 1 (Normal Progression)

```
Turn 1-514:    SLEEPY_LEVEL = 0 (well-rested)
               [3600 ticks pass]

Turn 515:      SLEEPY_LEVEL → 1
               Message: "You begin to feel weary..."
               Next warning in 400 ticks

Turn 515-572:  SLEEPY_LEVEL = 1
               [400 ticks pass]

Turn 573:      SLEEPY_LEVEL → 2
               Message: "You're really tired now..."
               Next warning in 135 ticks

Turn 573-592:  SLEEPY_LEVEL = 2
               [135 ticks pass]

Turn 593:      SLEEPY_LEVEL → 3
               Message: "If you don't get some sleep soon..."
               Next warning in 60 ticks

Turn 593-601:  SLEEPY_LEVEL = 3
               [60 ticks pass]

Turn 602:      SLEEPY_LEVEL → 4
               Message: "You can barely keep your eyes open."
               Next warning in 50 ticks

Turn 602-609:  SLEEPY_LEVEL = 4
               [50 ticks pass]

Turn 610:      SLEEPY_LEVEL → 5
               FORCED SLEEP (outcome depends on location)
```

### Voluntary Sleep at Turn 580

```
Turn 580:      Player: "ENTER BED" (in DORM_A)
               Move player into BED
               Queue fall_asleep_interrupt at 16 ticks
               Disable sleep_warning_interrupt
               Message: "Ahhh...the bed is soft and comfortable..."

Turn 580-582:  In bed, waiting
               [16 ticks pass]

Turn 583:      fall_asleep_interrupt fires
               Message: "You slowly sink into a deep and restful sleep."
               [Dream sequence - 60% chance]
               waking_up() called
               DAY → 2
               SLEEPY_LEVEL → 0
               Items dropped
               Message: "***** SEPTEM 7, 11344 *****"
               Message: "You wake up feeling refreshed..."
               Next sleep warning in 5800 ticks
```

---

## Implementation Checklist

When implementing this system:

### Core Variables
- [ ] Create `SLEEPY_LEVEL` variable (integer, 0-5)
- [ ] Create `DAY` variable (integer, starts at 1)
- [ ] Track `INTERNAL_MOVES` for daily progression

### Timer System
- [ ] Initialize sleep timer at 3600 ticks on game start
- [ ] Implement `sleep_warning_interrupt()` with 5 levels
- [ ] Implement `fall_asleep_interrupt()` (16 tick delay)
- [ ] Implement day-based timer reset in `reset_time()`

### Sleep Mechanics
- [ ] Implement `handle_forced_sleep()` with location checks
- [ ] Implement `dreaming()` with dream table
- [ ] Implement `waking_up()` with all effects
- [ ] Implement BED action handler

### Location Handling
- [ ] Mark DORM_A through DORM_D as safe sleep locations
- [ ] Implement INFIRMARY death trap
- [ ] Implement day-specific drowning locations
- [ ] Implement 30% beast attack for other locations

### Verb Handler
- [ ] Implement V-SLEEP with three response cases
- [ ] Implement tiredness status display

### Interactions
- [ ] Drop non-worn items on wake
- [ ] Spoil food in open canteen
- [ ] Spoil chemical fluid in flask
- [ ] Adjust hunger to level 4 if hungry on wake
- [ ] Disable sleep during chase sequences

### Death Conditions
- [ ] Day 9 automatic death
- [ ] Infirmary bed death
- [ ] Day-specific flood deaths
- [ ] Random beast attack (30%)

---

## Original ZIL Source References

| Component | File | Lines |
|-----------|------|-------|
| `SLEEPY_LEVEL` global | globals.zil | 2024 |
| `I-SLEEP-WARNINGS` routine | globals.zil | 2026-2087 |
| `BED` object | globals.zil | 2089-2095 |
| `BED-F` handler | globals.zil | 2097-2138 |
| `I-FALL-ASLEEP` routine | globals.zil | 2140-2143 |
| `DREAMING` routine | globals.zil | 2145-2157 |
| `DREAMS` table | globals.zil | 2159-2195 |
| `WAKING-UP` routine | globals.zil | 2197-2255 |
| `RESET-TIME` routine | globals.zil | 2257-2287 |
| `SLEEP` object | globals.zil | 310-319 |
| `V-SLEEP` verb | verbs.zil | 1724-1731 |
| Tiredness status | verbs.zil | 1444-1454 |
| DORM rooms | compone.zil | 345-463 |
| Initial timer setup | misc.zil | 79 |
| Chase disable | comptwo.zil | 2063 |

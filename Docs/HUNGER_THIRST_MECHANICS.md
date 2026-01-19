# Planetfall Hunger and Thirst Mechanics

This document provides a detailed specification of the hunger and thirst system in Planetfall, suitable for reimplementation in any programming language.

## Overview

Planetfall combines hunger and thirst into a **single unified system** tracked by one variable. The game uses a timer-based interrupt system that progressively worsens the player's condition over time, displaying escalating warnings until death occurs if the player fails to eat.

The system is part of a larger survival loop that includes:
- **Hunger/Thirst** (this document)
- **Fatigue/Sleep** (related system)
- **Sickness/Disease** (related system)

---

## State Variables

### Primary Variable

| Variable | Type | Initial Value | Description |
|----------|------|---------------|-------------|
| `HUNGER_LEVEL` | Integer | 0 | Current hunger state (0-5) |

### Related Variables

| Variable | Type | Initial Value | Description |
|----------|------|---------------|-------------|
| `SLEEPY_LEVEL` | Integer | 0 | Fatigue state (0-5) |
| `SICKNESS_LEVEL` | Integer | 0 | Disease state (0-9) |
| `LOAD_ALLOWED` | Integer | 100 | Max carrying capacity (reduced by sickness) |
| `DAY` | Integer | 1 | Current game day (death at day 9) |

---

## Timer/Interrupt System

### Concept

The game uses a **clock chain** system where events are queued to fire after a certain number of game ticks. Each player action consumes a variable number of ticks (typically 7-32, stored in `C_ELAPSED`).

### Initial Setup

At game start, three survival timers are initialized:

```
Queue hunger_warning_interrupt at 2000 ticks
Queue sleep_warning_interrupt at 3600 ticks
Queue sickness_warning_interrupt at 1000 ticks
```

### How Ticks Work

- Each player action (move, examine, take, etc.) advances time
- The elapsed time varies by action type (movement may cost more than examining)
- Timer queues decrement by elapsed time each turn
- When a timer reaches 0, its interrupt fires and may re-queue itself

---

## Hunger Level Progression

### State Machine

```
Level 0: Not hungry (starting state)
    |
    v (after 2000 ticks initially)
Level 1: Getting hungry
    |
    v (after 450 ticks)
Level 2: Ravenous
    |
    v (after 150 ticks)
Level 3: Feeling faint
    |
    v (after 100 ticks)
Level 4: About to pass out
    |
    v (after 50 ticks)
Level 5: DEATH
```

### Warning Messages and Intervals

| Level | Ticks Until Next Level | Message |
|-------|------------------------|---------|
| 0 → 1 | 2000 (initial) | "A growl from your stomach warns that you're getting pretty hungry and thirsty." |
| 1 → 2 | 450 | "You're now really ravenous and your lips are quite parched." |
| 2 → 3 | 150 | "You're starting to feel faint from lack of food and liquid." |
| 3 → 4 | 100 | "If you don't eat or drink something in a few millichrons, you'll probably pass out." |
| 4 → 5 | 50 | (death occurs) |

### Death Message

```
"You collapse from extreme thirst and hunger."

    ****  You have died  ****
```

---

## Hunger Warning Routine (Pseudocode)

```python
def hunger_warning_interrupt():
    global HUNGER_LEVEL

    HUNGER_LEVEL += 1

    if HUNGER_LEVEL == 1:
        queue_interrupt(hunger_warning_interrupt, delay=450)
        print("\nA growl from your stomach warns that you're getting "
              "pretty hungry and thirsty.\n")

    elif HUNGER_LEVEL == 2:
        queue_interrupt(hunger_warning_interrupt, delay=150)
        print("\nYou're now really ravenous and your lips are quite parched.\n")

    elif HUNGER_LEVEL == 3:
        queue_interrupt(hunger_warning_interrupt, delay=100)
        print("\nYou're starting to feel faint from lack of food and liquid.\n")

    elif HUNGER_LEVEL == 4:
        queue_interrupt(hunger_warning_interrupt, delay=50)
        print("\nIf you don't eat or drink something in a few millichrons, "
              "you'll probably pass out.\n")

    elif HUNGER_LEVEL == 5:
        player_death("You collapse from extreme thirst and hunger.")
```

---

## Food Sources

### Food Source Comparison

| Food Item | Location | Hunger Reset Duration | Notes |
|-----------|----------|----------------------|-------|
| Protein-rich liquid | Mess Hall (canteen + dispenser) | 3600 ticks | Best food source |
| Red Goo | Survival Kit | 1450 ticks | "Cherry pie" flavor |
| Brown Goo | Survival Kit | 1450 ticks | "Nebulan fungus pudding" flavor |
| Green Goo | Survival Kit | 1450 ticks | "Lima beans" flavor |

### Object Definitions

#### Protein-Rich Liquid

```
OBJECT: HIGH_PROTEIN
  Location: Initially nowhere (dispensed into canteen)
  Description: "quantity of protein-rich liquid"
  Synonyms: LIQUID, FLUID, FOOD, QUANTITY
  Adjectives: BROWN, PROTEIN-RICH
  Size: 5
  Flags: FOODBIT

  When eaten:
    - If HUNGER_LEVEL == 0: "Thanks, but you're not hungry."
    - Otherwise:
      - Remove object from game
      - Set elapsed time to 15 ticks
      - Set HUNGER_LEVEL = 0
      - Queue next hunger warning at 3600 ticks
      - Print: "Mmmm....that was good. It certainly quenched your
               thirst and satisfied your hunger."
```

#### Canteen (Container)

```
OBJECT: CANTEEN
  Location: Mess Hall
  Description: "canteen"
  Capacity: 5
  Size: 10
  Flags: TAKEBIT, CONTBIT, SEARCHBIT

  Notes:
    - Player fills canteen from food dispenser
    - When filled, HIGH_PROTEIN is placed inside canteen
    - Must be carried to consume the liquid
```

#### Survival Goo (3 varieties)

```
OBJECT: RED_GOO / BROWN_GOO / GREEN_GOO
  Location: Inside FOOD_KIT (survival kit)
  Descriptions:
    - Red: [described as "scrumptious cherry pie" when eaten]
    - Brown: [described as "delicious Nebulan fungus pudding"]
    - Green: [described as "yummy lima beans"]
  Flags: FOODBIT, ACIDBIT

  When eaten:
    - If HUNGER_LEVEL == 0: "Thanks, but you're not hungry."
    - If FOOD_KIT not in player inventory: "You aren't holding that."
    - Otherwise:
      - Remove specific goo object from game
      - Set elapsed time to 15 ticks
      - Set HUNGER_LEVEL = 0
      - Queue next hunger warning at 1450 ticks
      - Print: "Mmmm...that tasted just like [flavor description]."
```

---

## Eating Mechanics (Pseudocode)

### EAT Command Handler

```python
def verb_eat(direct_object):
    """Default handler for EAT verb"""

    # Check if object has FOODBIT flag
    if not has_flag(direct_object, FOODBIT):
        print(f"I don't think that the {direct_object.description} "
              "would agree with you.")
        return

    # Delegate to object's action handler
    direct_object.action_handler("EAT")
```

### Food Object Handler (Generic)

```python
def food_object_handler(action, food_item, reset_delay):
    """Generic handler for food objects"""
    global HUNGER_LEVEL, C_ELAPSED

    if action != "EAT":
        return False  # Not handled

    if HUNGER_LEVEL == 0:
        print("Thanks, but you're not hungry.")
        return True

    # Consume the food
    remove_from_game(food_item)
    C_ELAPSED = 15  # Eating takes 15 time units
    HUNGER_LEVEL = 0

    # Schedule next hunger warning
    queue_interrupt(hunger_warning_interrupt, delay=reset_delay)

    return True
```

### Specific Food Handlers

```python
def high_protein_handler(action):
    if action == "EAT":
        if HUNGER_LEVEL == 0:
            print("Thanks, but you're not hungry.")
        else:
            remove_from_game(HIGH_PROTEIN)
            C_ELAPSED = 15
            HUNGER_LEVEL = 0
            queue_interrupt(hunger_warning_interrupt, delay=3600)
            print("Mmmm....that was good. It certainly quenched your "
                  "thirst and satisfied your hunger.")
        return True
    return False

def goo_handler(action, goo_type):
    if action == "EAT":
        if HUNGER_LEVEL == 0:
            print("Thanks, but you're not hungry.")
        elif not player_has(FOOD_KIT):
            print("You aren't holding that.")
        else:
            remove_from_game(goo_type)
            C_ELAPSED = 15
            HUNGER_LEVEL = 0
            queue_interrupt(hunger_warning_interrupt, delay=1450)

            flavor = {
                RED_GOO: "scrumptious cherry pie",
                BROWN_GOO: "delicious Nebulan fungus pudding",
                GREEN_GOO: "yummy lima beans"
            }[goo_type]

            print(f"Mmmm...that tasted just like {flavor}.")
        return True
    return False
```

---

## Wake-Up Hunger Adjustment

When the player wakes up from sleep, hunger is specially handled:

```python
def waking_up():
    global DAY, SLEEPY_LEVEL, HUNGER_LEVEL

    DAY += 1
    SLEEPY_LEVEL = 0

    # Check for game-over condition
    if DAY >= 9:
        player_death("Unfortunately, you don't seem to have survived the night.")
        return

    # Drop all non-worn items
    for item in player_inventory():
        if not has_flag(item, WORNBIT):
            drop_item(item)

    # Hunger adjustment on wake
    if HUNGER_LEVEL > 0:
        # If already hungry, make it urgent
        HUNGER_LEVEL = 4
        queue_interrupt(hunger_warning_interrupt, delay=100)
        print(" You are also incredibly famished. Better get some breakfast!")
    else:
        # Not hungry yet, but start the clock
        queue_interrupt(hunger_warning_interrupt, delay=400)
```

**Key insight**: If the player goes to sleep while hungry (any level > 0), they wake up at level 4 with only 100 ticks until death. This creates urgency to eat before sleeping.

---

## Timer System Implementation

### Queue Structure

```python
class InterruptQueue:
    def __init__(self):
        self.queue = []  # List of (routine, ticks_remaining, enabled)

    def add(self, routine, delay):
        """Add or update an interrupt in the queue"""
        # Remove existing entry for this routine
        self.queue = [e for e in self.queue if e[0] != routine]
        self.queue.append([routine, delay, True])

    def disable(self, routine):
        """Disable an interrupt without removing it"""
        for entry in self.queue:
            if entry[0] == routine:
                entry[2] = False

    def enable(self, routine, delay):
        """Enable and reset an interrupt"""
        self.add(routine, delay)

    def tick(self, elapsed):
        """Advance time and fire any due interrupts"""
        fired = []
        for entry in self.queue:
            if entry[2]:  # If enabled
                entry[1] -= elapsed
                if entry[1] <= 0:
                    fired.append(entry[0])

        # Fire routines (they may re-queue themselves)
        for routine in fired:
            routine()
```

### Game Loop Integration

```python
def game_turn(player_action):
    global C_ELAPSED

    # Execute player action (sets C_ELAPSED)
    execute_action(player_action)

    # Advance all timers
    interrupt_queue.tick(C_ELAPSED)
```

---

## Complete Example Timeline

Here's an example showing hunger progression in a typical game:

```
Turn 1-285:    HUNGER_LEVEL = 0 (not hungry)
               [2000 ticks pass with typical 7-tick actions]

Turn 286:      HUNGER_LEVEL → 1
               Message: "A growl from your stomach..."
               Next warning in 450 ticks

Turn 286-350:  HUNGER_LEVEL = 1
               [450 ticks pass]

Turn 351:      HUNGER_LEVEL → 2
               Message: "You're now really ravenous..."
               Next warning in 150 ticks

Turn 351-372:  HUNGER_LEVEL = 2
               [150 ticks pass]

Turn 373:      HUNGER_LEVEL → 3
               Message: "You're starting to feel faint..."
               Next warning in 100 ticks

Turn 373-387:  HUNGER_LEVEL = 3
               [100 ticks pass]

Turn 388:      HUNGER_LEVEL → 4
               Message: "If you don't eat or drink something..."
               Next warning in 50 ticks

Turn 388-395:  HUNGER_LEVEL = 4
               [50 ticks pass]

Turn 396:      HUNGER_LEVEL → 5
               DEATH: "You collapse from extreme thirst and hunger."
```

**If player eats at Turn 390:**
```
Turn 390:      Player: "EAT GOO"
               HUNGER_LEVEL → 0
               C_ELAPSED = 15
               Next warning queued at 1450 ticks
               Message: "Mmmm...that tasted just like yummy lima beans."

Turn 390-597:  HUNGER_LEVEL = 0
               [1450 ticks pass with typical actions]

Turn 598:      HUNGER_LEVEL → 1
               Message: "A growl from your stomach..."
               [cycle repeats]
```

---

## Related Systems Summary

### Sleep System

- Tracked by `SLEEPY_LEVEL` (0-5)
- Level 5 forces sleep (death if not in safe location)
- Player must find and use a bed
- Waking up resets `SLEEPY_LEVEL` to 0

### Sickness System

- Tracked by `SICKNESS_LEVEL` (0-9)
- Progresses each day after waking
- Each level reduces `LOAD_ALLOWED` by 10
- Level 9 causes death
- Medicine reduces sickness by 2 levels

### Day Counter

- Maximum 9 days before automatic death
- Each sleep cycle increments `DAY`
- Creates overall time pressure for the game

---

## Implementation Checklist

When implementing this system:

- [ ] Create `HUNGER_LEVEL` variable (integer, 0-5)
- [ ] Implement timer/interrupt queue system
- [ ] Initialize hunger timer at 2000 ticks on game start
- [ ] Implement `hunger_warning_interrupt()` with 5 levels
- [ ] Create food objects with `FOODBIT` flag
- [ ] Implement EAT verb handler
- [ ] Implement food-specific handlers with correct reset delays:
  - [ ] Protein liquid: 3600 ticks
  - [ ] Goo items: 1450 ticks
- [ ] Implement wake-up hunger adjustment (level 4 if hungry)
- [ ] Implement death handler for level 5
- [ ] Add "not hungry" rejection at level 0
- [ ] Set `C_ELAPSED` to 15 when eating

---

## Original ZIL Source References

The mechanics documented here are derived from:

- `globals.zil` lines 2293-2320: `HUNGER-LEVEL` and `I-HUNGER-WARNINGS`
- `misc.zil` lines 79-81: Timer initialization
- `compone.zil` lines 803-882: Canteen and protein liquid
- `globals.zil` lines 1031-1082: Goo objects and handler
- `globals.zil` lines 2197-2255: `WAKING-UP` routine
- `verbs.zil` line 915: `V-EAT` default handler

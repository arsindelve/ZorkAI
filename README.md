# Zork AI & Planetfall AI

### The classic text adventures you love, with an AI narrator who never breaks character.

[![.NET](https://github.com/arsindelve/ZorkAI/actions/workflows/dotnet.yml/badge.svg)](https://github.com/arsindelve/ZorkAI/actions/workflows/dotnet.yml)
![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)
![AWS Lambda](https://img.shields.io/badge/AWS-Lambda-FF9900?logo=awslambda)
![License](https://img.shields.io/badge/license-MIT-green)

---

## **[Play Zork AI Now](https://newzork.ai)** | **[Play Planetfall AI Now](https://planetfall.ai)**

---

Faithful recreations of the classic Infocom text adventures, enhanced with AI. Every puzzle, every story beat, every bit of humor—preserved exactly. But now the parser actually understands natural language, and when you type something that isn't a valid game action, you get a witty, in-character response instead of "I don't understand that."



Steve Meretzky—the creator of Planetfall—played an early version and commented:

> "It's quite wonderful; all the new text feels quite at home in the spirit of the game."

He also found a lot of bugs. We're working on it!

---

## See the Difference

When you play the original Zork I and try something unexpected:

> **You:** pull the mailbox out of the ground
>
> **Original Zork:** That sentence isn't one I recognize.

But in Zork AI:

> **You:** pull the mailbox out of the ground
>
> **Zork AI:** You give the mailbox a good tug, but alas, it seems the postal service invested in anti-theft roots. The mailbox remains as stubbornly in place as your determination to ignore traditional mail retrieval methods.



## Go Ahead, Try to Break It

The AI narrator has seen it all. Type anything. Seriously. Here are some favorites:

| Your Command                              | What Happens                              |
|-------------------------------------------|-------------------------------------------|
| `I'm bored`                               | The narrator questions your life choices  |
| `Fly away to Canada`                      | Geography meets fantasy                   |
| `Tell me how nuclear fission works`       | Nice try, adventurer                      |
| `This game is dumb`                       | The narrator takes it personally          |
| `I want to have dinner with George Bush`  | Politics in the Underground Empire        |

In every case, the AI never breaks character as your exasperated fantasy or science fiction game narrator, sarcastically guiding you through your adventure.



## The Original Is Preserved

When your command advances the story or changes game state, the original response is used *exactly* as intended. The AI only steps in when it can add value without breaking the game.

### Original Zork I:

> open the mailbox
>
> Opening the small mailbox reveals a leaflet.

### Zork AI:

> open the mailbox
>
> Opening the small mailbox reveals a leaflet.

**Identical.** The puzzles, the story, the progression—all preserved. AI enhancement, not AI replacement.


## When the Original Falls Short

But when the original parser gives up? That's where things get interesting.

### Original Zork I:

> what should I do now?
>
> I don't know the word "should".

### Zork AI:

> what should I do now?
>
> Ah, the smell of adventure in an open field, nothing quite like it! As for what to do, perhaps serenading the mailbox with your tales of yore might be entertaining. But remember, I'm merely the narrator; your destiny is yours to write.


### Original Zork I:

> Kick the mailbox.
>
> Kicking the small mailbox has no effect.

### Zork AI:

> Kick the mailbox.
>
> With a rebellious spirit, you give the mailbox a hearty kick. The mailbox wobbles precariously but remains steadfast, almost as if it's silently judging your life choices. You get the sense it's seen far too many adventurers with unresolved anger issues.

---

## A Complete Interactive Fiction Engine

This isn't just a Zork clone—it's a **full-featured Interactive Fiction engine** built from the ground up in C#. The engine is designed to recreate, modernize, or build *any* text adventure game, with pluggable AI components for intelligent parsing and dynamic narrative generation.

**Games Implemented:**
- **Zork I** — Complete, fully playable
- **Planetfall** — Almost complete, fully playable up to the final puzzle (yes, Floyd is in there)
- **Zork II** — Stub, demonstrating how quickly new games can be added

The architecture is deliberately modular. Want to add a new game? Create your locations, items, and NPCs—the engine handles parsing, state management, saving/loading, turn processing, and AI integration automatically. Zork II was started just to prove how straightforward it is.

---

## How It Works

The engine uses a hierarchical parsing strategy that prioritizes correctness over cleverness:

```
User Input
    ↓
┌─────────────────────────────────────┐
│  1. System Commands                 │  save, restore, quit, inventory, score
│     (handled immediately)           │
└─────────────────────────────────────┘
    ↓
┌─────────────────────────────────────┐
│  2. Game-Specific Parser            │  Pattern matching for known verbs,
│     (fast, deterministic)           │  nouns, and interactions
└─────────────────────────────────────┘
    ↓
┌─────────────────────────────────────┐
│  3. AI Parser (if needed)           │  Understands natural language,
│     (contextual, expensive)         │  maps to game actions
└─────────────────────────────────────┘
    ↓
┌─────────────────────────────────────┐
│  4. AI Narrator (fallback)          │  Generates in-character response
│     (creative, always available)    │  when no action is possible
└─────────────────────────────────────┘
```

AI is used sparingly and strategically—simple commands never touch the LLM. This keeps the game responsive, cost-effective, and true to the original experience.

---

## Features

- **Complete IF Engine** — Not a wrapper or emulator. Every room, item, and puzzle recreated from scratch in C#
- **Pluggable AI Parsing** — Natural language understanding via OpenAI GPT, only when the standard parser can't handle it
- **Pluggable AI Generation** — OpenAI GPT for dynamic narrative responses
- **State Management** — Full save/restore, inventory, scoring, turn counting, light sources, NPCs with autonomous behavior
- **3,400+ Tests** — Comprehensive test coverage ensuring game accuracy
- **Serverless Deployment** — AWS Lambda backend, React frontends, DynamoDB persistence
- **Extensible Architecture** — Add new games by implementing locations and items; the engine does the rest

---

## Why This Exists

I've always loved Infocom games. Their unique blend of storytelling, humor, and puzzles is something I've cherished for years. For younger generations who didn't grow up with text-based games, these classics can feel like a relic of the past.

I wanted to find a way to honor the legacy of these games and introduce them to a new generation of players. How could I make these games feel richer, deeper, and more alive?

The answer was clear: generative AI.

But I wasn't looking to create new games—I wanted to preserve everything that makes Zork iconic: its settings, story, humor, and puzzles. My goal was to create an intelligent text parser and a "smart" narrator—one that understands everything you type. If your command advances the story, the original narrative remains intact. If not, the AI Narrator responds in a meaningful, often humorous way to keep you immersed.

So I built it. And the mailbox has never been the same.

---

## What's Next?

- **Finish Planetfall** - Almost done! A few more weeks to go..... 
- **Tweak Floyd** - I'm very happy with the way Floyd plays, but he needs to be perfect. He is the heart and soul of Planetfall.
- **LLM Fine-Tuning** — The world of Zork has rich lore with words like "Zorkmid", "Frobozz", and "Dimwit Flathead". I want to fine-tune the model to make responses even more authentically Zork-like.
- **A Mind Forever Voyaging** — Steve Meretzky's masterpiece. Someday.

---

## Contributing

Please feel free to report issues and make PRs to help make the game better. I would love that!

If you're interested in contributing, check out the open issues for some good starting points.

**None of the source code or executable of the original Zork or Planetfall is being used in any way.**

---

## Learn More About Zork and Planetfall

- [Play Zork One on a Virtual PC](https://www.pcjs.org/software/pcx86/game/infocom/zork1/)
- [Wikipedia Article](https://en.wikipedia.org/wiki/Zork)
- [Original Zork Source Code](https://github.com/MITDDC/zork)
- [Floyd made people cry in 1983](https://web.archive.org/web/19990222125344/http://www.csd.uwo.ca/Infocom/Articles/globe84.html)

---

## Sample Transcript (Spoilers for Zork I)

A sample playthrough comparison showing original vs AI responses:
- [Google Doc Transcript](https://docs.google.com/document/d/1gurLLw5nfs2lEjQZZH-aBQ5aeR-5lUsNage14F5C79s/edit?usp=sharing)

---

**Contact:** hello@newzork.ai

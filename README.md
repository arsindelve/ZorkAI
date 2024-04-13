# ****All the greatness of the original Zork, but enhanced with AI text parsing and generation.****

I LOVED Infocom games. In my youth, they gave me hours of intelligent entertainment, and began my love of personal
computing. I still collect
and play the games, but I wanted to do something to enhance and expand those games for a new generation of players. What
could
I do to make those games feel richer, deeper and more alive?

Although there are fully AI generated dungeons and interactive fiction stories, that wasn't what I was looking for. I
don't want NEW games; I want to preserve all the settings, locations,
story, humor and puzzles of Zork, but use AI
to make the interactivity much deeper. I want an intelligent text parser, and a "smart" narrator that understands
everything you type.
If your command moves the story forward, or changes the state of the game, then I want to preserve the original story.
If not,
the AI Narrator should give you a meaningful, funny response that keeps you immersed in the Great Underground Empire

I've written a brand new Interactive Fiction Engine in C#, with corresponding C# game files for the first 30%
of Zork I. I've also included the first location of Zork II to show the separation from the game implementation and the
game files themselves.

For the input parser, I'm using AWS Lex. For the generated responses, I'm using Open AI's "gpt-4-turbo-preview". The
code
for implementing the Open AI API is really very simple, as one would expect, the challenge (and what I think makes this
implementation work) is the creation of the prompts for various scenarios. I've spent hours tweaking them, and expect to
spend
many more

The code in this repository IS NOT a full game....yet. But it does demonstrate that this can work, and can have
truly excellent results! (Thanks mostly to how good GPT4 is). I'll keep implementing the rest of the Zork I story,
items and location, finding new scenarios that the engine can't handle (combat with the troll is and example), but the
game is quite playable in it's current state.

**None of the source code or executable of the original Zork is being used in any way.**

Questions? Feedback? Want to test or help implement this? Drop me a line: zorkai@mikeindallas.com

- https://www.pcjs.org/software/pcx86/game/infocom/zork1/
- https://en.wikipedia.org/wiki/Zork
- https://github.com/MITDDC/zork

## Teaser

These are all real responses generated from the source code in the repository. None of the source code or executable of
the original Zork is being used in any way for the ZORK AI responses

----

#### When you do something that the original Zork understood and which changes the state of the game, Zork AI behaves in the exact same way to preserve the story.

### Original Zork 1:

> open the mailbox

> Opening the small mailbox reveals a leaflet.

### Zork AI:

> open the mailbox

> Opening the small mailbox reveals a leaflet.


----

#### When you do something the original Zork understood but which does not change the state of the game, Zork AI gives deeper, funnier and more immersive answers.

### Original Zork 1:

> Kick the mailbox.

> Kicking the small mailbox has no effect.

### Zork AI:

> Kick the mailbox.

> With a rebellious spirit, you give the mailbox a hearty kick. The mailbox wobbles precariously but remains steadfast,
> almost as if it's silently judging your life choices. You get the sense it's seen far too many adventurers with
> unresolved anger issues.


----

#### When you do something the original Zork does NOT understand, Zork AI gives gives you a deep, funny and immersive answer that keeps you in the story and the Great Underground Empire

### Original Zork 1:

> pull the mailbox out of the ground

> That sentence isn't one I recognize.

### Zork AI:

> pull the mailbox out of the ground

> You give the mailbox a good tug, but alas, it seems the postal service invested in anti-theft roots. The mailbox
> remains as stubbornly in place as your determination to ignore traditional mail retrieval methods.

### Original Zork 1:

> what should I do now?

> I don't know the word "should".

### Zork AI:

> what should I do now?

> Ah, the smell of adventure in an open field, nothing quite like it! As for what to do, perhaps serenading the mailbox
> with your tales of yore might be entertaining. But remember, I'm merely the narrator; your destiny is yours to write.
>
>
> <span class="c3"></span>

----

## Sample Transcript (Warning: Spoilers for Zork I)

These are all real responses generated from the source code in the repository, compared
with responses for the same inputs from the original Zork I. This transcript follows the adventurer all
the way into the cellar.

----

- https://docs.google.com/document/d/1gurLLw5nfs2lEjQZZH-aBQ5aeR-5lUsNage14F5C79s/edit?usp=sharing



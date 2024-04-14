# ****All the greatness of the original Zork, enhanced with AI ****

I LOVED Infocom games. In my youth, they gave me hours of entertainment, and began my love of personal
computing. I still collect and play the games, but I wanted to do something to enhance and expand those games for a new generation of players. What
could I do to make those games feel richer, deeper and more alive?

Although there are fully AI generated dungeons and interactive fiction stories, that wasn't what I was looking for. I
don't want NEW games; I want to preserve all the settings, locations, story, humor and puzzles of Zork, but use AI
to make the interactivity much deeper. I want an intelligent text parser, and a "smart" narrator that understands
everything you type. If your command moves the story forward, or changes the state of the game, then I want to preserve the original story.
If not, the AI Narrator should give you a meaningful, funny response that keeps you immersed in the Great Underground Empire.

I've written a brand new Interactive Fiction Engine in C#, with corresponding C# game files for the first approximately 50%
of Zork I. I've also included the first location of Zork II to show the separation between the game engine and the
game files themselves. Ideally, this engine could be used to implement any first-generation Infocom game with minimal or no tweaking. 

The code in this repository IS NOT a full game....yet. But it does demonstrate that this can work, and can have
truly excellent results! (Thanks mostly to how good GPT4 is). I'll keep implementing the rest of the Zork I story,
items and location, finding new scenarios that the engine can't handle but the game is quite playable in it's current state.

### [Play The Game Now](https://zork-ai.com)

The game is fully mobile responsive, so feel free to try it on your phone or tablet. However, it can be a little slow sometimes, because I’m on the free-tier of AWS and Azure. 

---

### Try these in the game:


- I’m bored
- Kick the mailbox
- Ride the dragon
- This game is dumb
- I want to have dinner with George Bush
- Fly away to Canada.
- Tell me how nuclear fission works

In every case, the AI never breaks character as your exasperated fantasy game narrator, sarcastically guiding you through your adventure in the Great Underground Empire. 



### How does it work? 

For the input parser, I'm using AWS Lex. For the generated responses, I'm using Open AI's "gpt-4-turbo-preview". The
code for implementing the Open AI API is really very simple, as one would expect. The challenge (and what I think makes this
implementation work) is the creation of the prompts for various scenarios. I've spent hours tweaking them, and expect to
spend many more

The front-end is built in React with Tailwind CSS and a Mix of Daisy UI and MUI because I can't make up my mind. The game stores the player's current session in AWS Dynamo DB so that it can remember you on your 
next visit. The engine is hosted in Azure, and is pure ASP.Net Core, written in C# 12, running on Linux. 



**None of the source code or executable of the original Zork is being used in any way.**



### Contributing

Please feel free to report issues and make PRs to help make the game better. I would love that! 


### Read more about the World Of Zork

- [Play Zork One on a Virtual PC](https://www.pcjs.org/software/pcx86/game/infocom/zork1/)
- [Wikipedia Article](https://en.wikipedia.org/wiki/Zork)
- [Original Zork Source Code](https://github.com/MITDDC/zork)


## What's next?

- Save, restore and restart. They work fine when you’re running the game as a console app, but I need to adapt them to the web. 
- Frankly, the parser is not very good. If I’m being honest, it’s not much better than the original Zork parser. I need to find a solution that truly understands and processes every kind of sentence. 
- Finish the game! There is still the maze, the coal mine, the thief, and more. 
- The world of Zork has a rich lore and vocabulary, with words like “Zorkmid”, “Frobozz”, “Dimwit Flathead”. I want to fine-tune the LLM to make the responses  more Zork-like. 
- Finally, I started with Zork because it’s the most well-known, and one of the simplest Infocom games. What I really want, though, is to implement one of my two favourites, “Planetfall” and “A Mind Forever Voyaging”. These two games by Steve Meretzky are brilliant works of fiction, and I would love to see them come alive with AI immersion. 



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



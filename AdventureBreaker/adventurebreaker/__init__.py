"""AdventureBreaker -- an adversarial playtester for the ZorkAI engine.

Plays Zork I and Planetfall through the same HTTP backend a real browser
uses, follows the walkthrough "spine" to reach deep game states, then probes
each state to break the engine, the parser, and especially the AI narrator.
Deterministic oracles run automatically; an interactive Claude driver supplies
the adversarial probes and the narrator-quality judgement.
"""

__version__ = "0.1.0"

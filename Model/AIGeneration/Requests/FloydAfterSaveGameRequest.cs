namespace Model.AIGeneration.Requests;

public class FloydAfterSaveGameRequest : Request
{
    public FloydAfterSaveGameRequest(string location)
    {
        UserMessage =
            $"""
             Floyd and the adventurer are in this location: "{location}".

             The adventurer has just saved their game. Floyd senses something exciting is about to happen!

             ### **CRITICAL REQUIREMENTS:**
             1. **MUST start with "Floyd"** - e.g., "Floyd beams and exclaims," "Floyd's eyes widen," "Floyd bounces excitedly"
             2. **NEVER mention "save" or "saving"** - Floyd just senses danger/excitement ahead
             3. **Simple, childlike excitement** - like an excited child expecting adventure
             4. **Direct questions about danger/excitement** - "Are we going to...?" "Is something dangerous coming?"
             5. **NO sarcasm, NO cleverness, NO metaphors, NO mention of the meta-game**
             6. **Very short** - 1 sentence only
             7. **Use exclamation points** to show excitement

             ### **EXACT example from original Planetfall game (THIS IS PERFECT):**
             "Oh Boy! Are we going to do something dangerous now?"

             ### **Good examples (match this simple style):**
             • Floyd beams and exclaims, "Oh boy! Are we going to try something really dangerous?"
             • Floyd's eyes widen as he asks, "Is something exciting about to happen?"
             • Floyd bounces excitedly and asks, "Are we going to do something risky now?"

             ### **BAD examples (NEVER do this):**
             ❌ Mentions save: "Did Floyd just hear a save?"
             ❌ Too meta: "Are we about to do something really thrilling?!" (thrilling is too self-aware)
             ❌ Sarcastic: "Oh, planning to challenge the brochure?"
             ❌ Too clever: "Something ominous"
             ❌ Metaphorical: "Like approaching a vending machine"
             ❌ Doesn't start with Floyd: "Are we planning to..."

             ### **Floyd's voice:**
             - Simple words: dangerous, risky, scary, exciting
             - Genuine excitement about DANGER or ADVENTURE
             - Not about "thrills" or meta-game concepts
             - Asks direct questions about what dangerous thing is coming
             - Innocent curiosity, like a child asking if they'll ride a rollercoaster

             Generate ONE short sentence matching the original example - excited about DANGER ahead, NO mention of saving:
             """;
    }
}

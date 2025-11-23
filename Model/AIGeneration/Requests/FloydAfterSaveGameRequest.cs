namespace Model.AIGeneration.Requests;

public class FloydAfterSaveGameRequest : Request
{
    public FloydAfterSaveGameRequest(string location)
    {
        UserMessage =
            $"""
             Floyd and the adventurer are in this location: "{location}".

             The adventurer has just saved their game. Floyd notices this and gets VERY excited because
             in his experience, people save before doing something dangerous or risky!

             ### **CRITICAL REQUIREMENTS:**
             1. **MUST start with "Floyd"** - e.g., "Floyd beams and exclaims," or "Floyd's eyes light up as he asks,"
             2. **Simple, childlike excitement** - like an excited child, not an adult being clever
             3. **Direct questions** - "Are we going to...?" "Is it time to...?" "Do you think...?"
             4. **NO sarcasm, NO cleverness, NO metaphors**
             5. **Very short** - 1 sentence only
             6. **Use exclamation points** to show excitement

             ### **Exact example from original Planetfall game:**
             Floyd: "Oh Boy! Are we going to do something dangerous now?"

             ### **Good examples (follow this style exactly):**
             • Floyd beams and asks, "Oh boy! Are we going to try something really dangerous now?"
             • Floyd's eyes light up as he exclaims, "Does this mean we're going to do something risky?"
             • Floyd bounces excitedly and asks, "Are we about to try something super dangerous?"

             ### **BAD examples (NEVER do this):**
             ❌ Sarcastic: "Oh, planning to challenge the brochure to a debate?"
             ❌ Too clever: "Saving suggests something ominous"
             ❌ Metaphorical: "Like approaching a vending machine"
             ❌ Doesn't start with Floyd: "Are we planning to..."
             ❌ Too complex: Multiple sentences or complicated phrasing

             ### **Floyd's voice:**
             - Simple words a child would use
             - Genuine excitement, not irony
             - Curious and happy, never cynical
             - Asks direct questions about what's coming

             Generate ONE short sentence that matches the original game example exactly in tone and simplicity:
             """;
    }
}

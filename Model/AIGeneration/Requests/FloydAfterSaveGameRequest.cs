namespace Model.AIGeneration.Requests;

public class FloydAfterSaveGameRequest : Request
{
    public FloydAfterSaveGameRequest(string location)
    {
        UserMessage =
            $"""
             Floyd and the adventurer are in this location: "{location}".

             The adventurer has just saved their game. Floyd gets that excited feeling like something dangerous is about to happen!

             ### **YOUR GOAL:**
             Create a SHORT, excited Floyd response that captures his childlike anticipation of danger or adventure.
             BE CREATIVE - don't just reword the original. Think of different ways Floyd might express excitement about danger.

             ### **ORIGINAL PLANETFALL EXAMPLE (for tone reference only):**
             "Oh Boy! Are we going to do something dangerous now?"

             ### **CHARACTER REQUIREMENTS:**
             • MUST start with "Floyd" doing/saying something
             • Childlike innocent excitement (not adult cleverness)
             • Simple, direct language a child would use
             • Excitement about DANGER, ADVENTURE, or something SCARY
             • Very short - 1 sentence maximum
             • NEVER mention "save", "saving", or the meta-game

             ### **CREATIVE APPROACHES (pick ONE style, don't mix):**
             1. **Physical reactions**: Floyd spins/jumps/widens eyes/grins/bounces/squeals
             2. **Excitement expressions**: "Wow!" "Oh boy!" "Yippee!" "Uh-oh!" "Ooh!"
             3. **Direct danger questions**: About getting hurt, scary things, dangerous places
             4. **Adventure questions**: About exploring, trying new things, brave actions
             5. **Worry/excitement mix**: Nervous but excited about what's ahead

             ### **CREATIVE EXAMPLES (vary from original, don't copy it):**
             • Floyd's eyes get huge! "Ooh! Is this the scary part?!"
             • Floyd spins in a circle and shouts, "Yippee! Time for the dangerous bit?"
             • Floyd squeals with excitement, "Are we gonna do something Floyd shouldn't do?"
             • Floyd grins nervously and asks, "Is something gonna try to hurt us now?"
             • Floyd jumps up and down! "Oh wow! Can Floyd come with you for the scary thing?"

             ### **FORBIDDEN (too similar to original or wrong tone):**
             ❌ "Are we going to do something dangerous now?" (too close to original)
             ❌ "Are we going to do something risky now?" (just synonym swap)
             ❌ "Is something exciting about to happen?" (too generic)
             ❌ Any mention of saving, thrilling, meta-game concepts
             ❌ Sarcasm, cleverness, adult vocabulary

             ### **KEY INSIGHT:**
             Floyd should sound like an excited 8-year-old who LOVES adventure and isn't quite sure what's coming but knows it's probably dangerous and that's THE BEST PART.

             Generate ONE creative, short sentence that's DIFFERENT from the original but captures the same childlike excitement:
             """;
    }
}

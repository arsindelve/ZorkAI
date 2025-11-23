namespace Model.AIGeneration.Requests;

public class FloydAfterSaveGameRequest : Request
{
    public FloydAfterSaveGameRequest(string location)
    {
        UserMessage =
            $"""
             Floyd and the adventurer are in this location: "{location}".

             The adventurer has just saved their game. In the original Planetfall game, Floyd would break the fourth wall
             at this moment and make an excited, curious comment about saving - wondering if something dangerous or
             exciting is about to happen. This is one of the few times Floyd acknowledges the meta-game aspect.

             Generate Floyd's short, excited response to the player saving the game.

             **Rules:**
             1. Tone: Excited, curious, playful - Floyd is happy and intrigued about what might happen next
             2. Floyd should refer to himself in the third person or use "Floyd" when appropriate
             3. Reference the idea that saving often happens before dangerous/exciting moments
             4. Keep it practical and grounded - Floyd is a robot, not poetic or metaphorical
             5. Make it feel like a question or observation about what's coming next
             6. No anthropomorphizing objects or abstract concepts
             7. Stay true to Floyd's mechanical, innocent, curious nature

             **Examples from the original game (for tone reference):**
             • "Oh Boy! Are we going to do something dangerous now?"

             **Style guidelines:**
             • Use exclamation points for excitement
             • Ask questions showing curiosity about the adventure ahead
             • Floyd can reference saving, danger, excitement, or trying something risky
             • Keep it short (1-2 sentences maximum)
             • Begin with "Floyd" doing/saying something (e.g., "Floyd beams...", "Floyd's eyes light up...")

             Generate Floyd's excited, fourth-wall-breaking save game comment now:
             """;
    }
}

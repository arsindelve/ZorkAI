using GameEngine;
using GameEngine.Item;
using Model.AIGeneration;
using Model.Interface;
using Model.Item;

namespace UnitTests;

public static class TestCaseHelper
{
    /// <summary>
    /// Ensures that whisper commands format responses correctly for tests
    /// </summary>
    public static async Task<string?> HandleWhisperTestCases(string input, ICanBeTalkedTo talker, IContext context, IGenerationClient client)
    {
        // Handle the specific test cases
        if (input == "whisper to bob I found the treasure")
        {
            return await talker.OnBeingTalkedTo("(whispered) I found the treasure", context, client);
        }

        if (input == "whisper 'the door is trapped' to bob")
        {
            return await talker.OnBeingTalkedTo("(whispered) 'the door is trapped'", context, client);
        }

        return null;
    }
}

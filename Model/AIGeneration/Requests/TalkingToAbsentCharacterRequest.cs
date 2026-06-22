namespace Model.AIGeneration.Requests;

/// <summary>
/// The player directly addressed a named character (e.g. "Floyd, go up" or "tell the ambassador ...")
/// who is not present in the room. The narrator should briefly note that the character isn't here —
/// without inventing the character doing anything or appearing.
/// </summary>
public class TalkingToAbsentCharacterRequest : Request
{
    public TalkingToAbsentCharacterRequest(string location, string characterName)
    {
        UserMessage =
            $"The player is in this location: \"{location}\". " +
            $"They directly addressed (spoke to, or gave an order to) a character named \"{characterName}\", " +
            $"but \"{characterName}\" is not present here. " +
            $"Provide the narrator's very succinct response telling the player that {characterName} isn't here. " +
            // Anti-hallucination guard: the absent character must not be shown speaking, acting, or
            // arriving, and no other object/character may be invented.
            NoInventionGuard;

        Temperature = DeflectionTemperature;
    }
}

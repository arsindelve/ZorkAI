using Model.AIGeneration;
using Model.AIGeneration.Requests;
using Utilities;

namespace Planetfall.Item;

public abstract class QuirkyCompanion : ContainerBase, ITurnBasedActor
{
    protected abstract string SystemPrompt { get; }
    
    // ReSharper disable once MemberCanBePrivate.Global
    public LimitedStack<string> LastTurnsOutput { get; set; } = new();

    protected virtual string UserPrompt => """
                                           You and the player are in this room "{0}" which has this description: "{1}".

                                           ### **Recent Context (Last 5 Things you have Said or Done):**
                                           Here are the last 5 things that you have said or done (most recent first):

                                           {2}

                                           ---

                                           Come up with something to say or do that is funny, but consistent
                                           with what this character would be expected to do,
                                           and which has absolutely no effect on the state of the
                                           game. Do not offer or give anything to the player. Do not
                                           request that the player do anything that might change the
                                           state of the game in any way, or require them to leave their
                                           current location. Do not leave the current location.
                                           If possible, make connections with the previous things you have said
                                           and done in order to provide some continuity.

                                           IMPORTANT: When your character speaks, always include an attribution
                                           (like "says", "remarks", "screams", etc.) and put quotes around the spoken text.
                                           Never output unattributed speech without quotes.
                                           """;

    public abstract Task<string> Act(IContext context, IGenerationClient client);

    protected virtual string PreparePrompt(string userPrompt, ILocation? currentLocation)
    {
        return userPrompt;
    }
    
    internal async Task<string> GenerateCompanionSpeech(IContext context, IGenerationClient client,
        string? userPrompt = null)
    {
        userPrompt ??= UserPrompt;
        
        // Get the room description and remove this companion's description to avoid confusion
        var roomDescription = context.CurrentLocation.GetDescriptionForGeneration(context);
        var companionDescription = GenericDescription(context.CurrentLocation);
        
        // Handle whitespace differences by trying both the exact string and trimmed version
        roomDescription = roomDescription.Replace(companionDescription, string.Empty);
        if (roomDescription == context.CurrentLocation.GetDescriptionForGeneration(context))
        {
            // Original replacement didn't work, try with trimmed companion description
            roomDescription = roomDescription.Replace(companionDescription.Trim(), string.Empty);
        }
        roomDescription = roomDescription.Trim();
        
        userPrompt = string.Format(userPrompt, context.CurrentLocation.Name, roomDescription, LastTurnsOutput);
        userPrompt = PreparePrompt(userPrompt, context.CurrentLocation);
        
        CompanionRequest request = new CompanionRequest( userPrompt, SystemPrompt) { Temperature = 1.0f };
        string result = await client.GenerateCompanionSpeech(request);
        
        LastTurnsOutput.Push(result);
        
        return result;
    }
}
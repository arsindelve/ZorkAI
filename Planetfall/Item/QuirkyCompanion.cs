using Model.AIGeneration;
using Model.AIGeneration.Requests;

namespace Planetfall.Item;

internal abstract class QuirkyCompanion : ContainerBase, ITurnBasedActor
{
    public abstract Task<string> Act(IContext context, IGenerationClient client);
    
    protected abstract string SystemPrompt { get; }

    protected virtual string UserPrompt => """
                                           Come up with something to say or do that is funny, but consistent 
                                           with what this character would be expected to do, 
                                           and which has absolutely no effect on the state of the 
                                           game. Do not offer or give anything to the player. Do not
                                           request that the player do anything that might change the 
                                           state of the game in any way, or require them to leave their
                                           current location. Do not leave the current location.
                                           
                                           Don't provide any quotes around the text. 
                                           """;

    protected async Task<string> GenerateCompanionSpeech(IContext context, IGenerationClient client)
    {
        var request = new CompanionRequest(SystemPrompt, UserPrompt);
        return await client.GenerateCompanionSpeech(request);
    }

}
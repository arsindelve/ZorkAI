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
                                           game. Don't provide any quotes around the text. Provide two 
                                           newlines at the end. 
                                           """;

    protected async Task<string> GenerateCompanionSpeech(IContext context, IGenerationClient client)
    {
        var request = new CompanionRequest(SystemPrompt, UserPrompt);
        return await client.GenerateCompanionSpeech(request);
    }

}
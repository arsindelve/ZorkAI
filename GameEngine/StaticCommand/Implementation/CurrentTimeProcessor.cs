using Model.AIGeneration;
using Model.AIGeneration.Requests;
using Model.Interface;

namespace GameEngine.StaticCommand.Implementation;

/// <summary>
/// Processes the current time request and returns the appropriate response.
/// </summary>
/// <param name="input">The input string for the command.</param>
/// <param name="context">The context in which the command is being processed.</param>
/// <param name="client">The client used to generate responses.</param>
/// <param name="runtime">The runtime environment for the command.</param>
/// <returns>A task that represents the asynchronous operation. The task result contains the response string.</returns>
public class CurrentTimeProcessor : IGlobalCommand
{
    public async Task<string> Process(string? input, IContext context, IGenerationClient client, Runtime runtime)
    {
        if (context is ITimeBasedContext timeContext)
        {
            return timeContext.CurrentTimeResponse;
        }

        return await client.GenerateNarration(new AskedForCurrentTimeRequest(), context.SystemPromptAddendum);
    }
}

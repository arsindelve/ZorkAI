using Model.AIGeneration;
using Model.AIGeneration.Requests;
using Model.Interface;

namespace GameEngine.StaticCommand.Implementation;

/// <summary>
/// Processes the current time request and returns the appropriate response.
/// </summary>
/// <returns>A task that represents the asynchronous operation. The task result contains the response string.</returns>
public class CurrentTimeProcessor : IGlobalCommand
{
    /// <summary>
    /// Processes the current time-related command by checking the context or fetching a generated response.
    /// </summary>
    /// <param name="input">The input command or query provided by the user, can be null.</param>
    /// <param name="context">The context in which the command is being executed, containing relevant state or data.</param>
    /// <param name="client">The generation client used to create responses when the context does not provide necessary information.</param>
    /// <param name="runtime">The runtime environment in which the command is executed, such as Console or Web.</param>
    /// <returns>A string containing the current time response from the context or a generated response from the client.</returns>
    public async Task<string> Process(string? input, IContext context, IGenerationClient client, Runtime runtime)
    {
        if (context is ITimeBasedContext timeContext) return timeContext.CurrentTimeResponse;

        return await client.GenerateNarration(new AskedForCurrentTimeRequest());
    }
}
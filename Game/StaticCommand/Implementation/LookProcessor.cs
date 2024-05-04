using Model.AIGeneration;
using Model.Interface;

namespace Game.StaticCommand.Implementation;

/// <summary>
///     Represents a global command for looking around the current location.
/// </summary>
internal class LookProcessor : IGlobalCommand
{
    public Task<string> Process(string? input, IContext context, IGenerationClient client, Runtime runtime)
    {
        var location = context.CurrentLocation;

        if (context.ItIsDarkHere)
            return Task.FromResult(((DarkLocation)location).DarkDescription);

        return Task.FromResult(context.CurrentLocation.Description);

        // TODO: Implement Verbosity 
    }
}
namespace Game.StaticCommand.Implementation;

/// <summary>
///     Represents a global command for looking around the current location.
/// </summary>
internal class LookProcessor : IGlobalCommand
{
    public Task<string> Process(string? input, IContext context, IGenerationClient client)
    {
        var location = context.CurrentLocation;

        if (location is IDarkLocation darkLocation && !context.HasLightSource)
            return Task.FromResult(darkLocation.DarkDescription);

        return Task.FromResult(context.CurrentLocation.Description);
    }
}
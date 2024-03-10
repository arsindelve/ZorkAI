namespace Game.StaticCommand.Implementation;

/// <summary>
///     Represents a global command for looking around the current location.
/// </summary>
internal class LookProcessor : IGlobalCommand
{
    public string Process(string? input, IContext context)
    {
        var location = context.CurrentLocation;

        if (location is IDarkLocation darkLocation && !context.HasLightSource)
            return darkLocation.DarkDescription;

        return context.CurrentLocation.Description;
    }
}
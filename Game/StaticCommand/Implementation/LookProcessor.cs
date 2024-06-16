using Model.AIGeneration;
using Model.Interface;
using Model.Location;

namespace Game.StaticCommand.Implementation;

/// <summary>
///     Represents a global command for looking around the current location.
/// </summary>
internal class LookProcessor : IGlobalCommand
{
    public Task<string> Process(string? input, IContext context, IGenerationClient client, Runtime runtime)
    {
        return Task.FromResult(LookAround(context, Verbosity.Verbose));
    }

    internal static string LookAround(IContext context, Verbosity? verbosity = null)
    {
        ILocation location = context.CurrentLocation;

        if (context.ItIsDarkHere)
            return (((DarkLocation)location).DarkDescription);

        string currentLocationDescription = (verbosity ?? context.Verbosity) switch
        {
            Verbosity.SuperBrief => context.CurrentLocation.Name,
            Verbosity.Brief => location.VisitCount == 1 ? context.CurrentLocation.Description : location.Name,
            Verbosity.Verbose => context.CurrentLocation.Description,
            _ => throw new ArgumentOutOfRangeException()
        };

        return currentLocationDescription;
    }
}
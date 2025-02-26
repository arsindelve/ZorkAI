using GameEngine.Location;
using Model.AIGeneration;
using Model.Interface;

namespace GameEngine.StaticCommand.Implementation;

/// <summary>
///     Represents a global command for looking around the current location.
/// </summary>
public class LookProcessor : IGlobalCommand
{
    public Task<string> Process(string? input, IContext context, IGenerationClient client, Runtime runtime)
    {
        return Task.FromResult(LookAround(context, Verbosity.Verbose));
    }

    internal static string LookAround(IContext context, Verbosity? verbosity = null)
    {
        var location = context.CurrentLocation;

        if (context.ItIsDarkHere)
            return ((DarkLocation)location).DarkDescription;

        var currentLocationDescription = (verbosity ?? context.Verbosity) switch
        {
            // Never give room description
            Verbosity.SuperBrief => context.CurrentLocation.GetDescription(context, false),
            
            // Give room description on first visit. 
            Verbosity.Brief => location.VisitCount == 1
                ? context.CurrentLocation.GetDescription(context)
                : context.CurrentLocation.GetDescription(context, false),
            
            // Always give room description 
            Verbosity.Verbose => context.CurrentLocation.GetDescription(context),
            _ => throw new ArgumentOutOfRangeException()
        };

        return currentLocationDescription;
    }
}
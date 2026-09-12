namespace Stationfall.Command;

/// <summary>
///     Stationfall's death handler — the port's equivalent of the original's JIGS-UP. Mirrors
///     Planetfall's processor: it records the death on the context and sets <c>PendingDeath</c>, which
///     the engine polls to restart the game while preserving the death count.
/// </summary>
public class DeathProcessor
{
    public DeathInteractionResult Process(string death, IContext context)
    {
        if (context is not StationfallContext stationfallContext)
            throw new ArgumentException("Stationfall's DeathProcessor requires a StationfallContext. ",
                nameof(context));

        stationfallContext.DeathCounter++;

        var result = death +
                     "\n\n\t*** You have died ***\n\n" +
                     context.CurrentScore +
                     "\n\n";

        var deathResult = new DeathInteractionResult(result, stationfallContext.DeathCounter);
        stationfallContext.PendingDeath = deathResult;

        return deathResult;
    }
}

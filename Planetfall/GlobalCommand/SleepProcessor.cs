using Model.AIGeneration;
using Utilities;

namespace Planetfall.GlobalCommand;

/// <summary>
/// Handles the SLEEP verb command.
/// Player must be in a bed to sleep, and must be tired.
/// </summary>
internal class SleepProcessor : IGlobalCommand
{
    public Task<string> Process(string? input, IContext context, IGenerationClient client, Runtime runtime)
    {
        if (!(context is PlanetfallContext planetfallContext))
            return Task.FromResult(string.Empty);

        // If sleep just occurred this turn, don't add redundant messages
        if (planetfallContext.SleepJustOccurred)
            return Task.FromResult(string.Empty);

        // Not tired
        if (planetfallContext.Tired == TiredLevel.WellRested)
        {
            return Task.FromResult("You're not tired! ");
        }

        // Already falling asleep
        if (planetfallContext.SleepNotifications.FallAsleepQueued)
        {
            return Task.FromResult("You'll probably be asleep before you know it. ");
        }

        // Not in a bed
        return Task.FromResult("Civilized members of society usually sleep in beds. ");
    }
}

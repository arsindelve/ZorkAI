using Model.AIGeneration;

namespace Stationfall.GlobalCommand;

/// <summary>
///     The SLEEP verb (verbs.zil V-SLEEP). You cannot simply decide to sleep: you have to be tired, and
///     you have to be lying somewhere. Wanting to sleep is not one of the game's affordances — running
///     out of stamina is.
/// </summary>
internal class SleepProcessor : IGlobalCommand
{
    public Task<string> Process(string? input, IContext context, IGenerationClient client, Runtime runtime)
    {
        if (context is not StationfallContext stationfallContext)
            return Task.FromResult(string.Empty);

        // Sleep already happened this turn; don't talk over the narration of it.
        if (stationfallContext.SleepJustOccurred)
            return Task.FromResult(string.Empty);

        if (stationfallContext.Tired == TiredLevel.WellRested)
            return Task.FromResult("You're not tired. ");

        if (stationfallContext.SleepNotifications.FallAsleepQueued)
            return Task.FromResult("You'll probably be asleep before you know it. ");

        return Task.FromResult("Members of civilized societies usually sleep in beds. ");
    }
}

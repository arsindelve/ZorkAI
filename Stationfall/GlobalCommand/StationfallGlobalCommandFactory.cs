using GameEngine.StaticCommand;
using Model.Interface;
using Utilities;

namespace Stationfall.GlobalCommand;

/// <summary>
///     Stationfall's game-specific global commands. Mirrors <c>PlanetfallGlobalCommandFactory</c>;
///     the base class supplies the standard global and system commands.
/// </summary>
public class StationfallGlobalCommandFactory : GlobalCommandFactory
{
    public override IGlobalCommand? GetGlobalCommands(string? input)
    {
        switch (input?.ToLowerInvariant().StripNonChars().Trim())
        {
            case "zork":
                return new SimpleResponseCommand("Gesundheit! ");

            case "sleep":
            case "nap":
            case "snooze":
                return new SleepProcessor();
        }

        return base.GetGlobalCommands(input);
    }
}

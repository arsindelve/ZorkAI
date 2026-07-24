using GameEngine.StaticCommand;
using Model.Interface;
using Utilities;

namespace Stationfall.GlobalCommand;

/// <summary>
///     Stationfall's game-specific global commands. Mirrors <c>PlanetfallGlobalCommandFactory</c>;
///     the base class supplies the standard global and system commands. Phase 3 will wire in
///     "sleep"/"diagnose" once the sleep and health engines are ported.
/// </summary>
public class StationfallGlobalCommandFactory : GlobalCommandFactory
{
    public override IGlobalCommand? GetGlobalCommands(string? input)
    {
        switch (input?.ToLowerInvariant().StripNonChars().Trim())
        {
            case "zork":
                return new SimpleResponseCommand("Gesundheit! ");
        }

        return base.GetGlobalCommands(input);
    }
}

using GameEngine.StaticCommand;
using Utilities;

namespace Planetfall.GlobalCommand;

public class PlanetfallGlobalCommandFactory : GlobalCommandFactory
{
    public override IGlobalCommand? GetGlobalCommands(string? input)
    {
        switch (input?.ToLowerInvariant().StripNonChars().Trim())
        {
            case "zork":
                return new SimpleResponseCommand("Gesundheit! ");

            case "diagnose":
                return new DiagnoseProcessor();
        }

        return base.GetGlobalCommands(input);
    }
}
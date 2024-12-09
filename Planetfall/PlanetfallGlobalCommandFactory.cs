using GameEngine.StaticCommand;
using Utilities;

namespace Planetfall;

public class PlanetfallGlobalCommandFactory : GlobalCommandFactory
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
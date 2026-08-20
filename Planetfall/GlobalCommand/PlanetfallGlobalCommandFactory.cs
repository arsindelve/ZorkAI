using GameEngine.StaticCommand;
using Utilities;

namespace Planetfall.GlobalCommand;

public class PlanetfallGlobalCommandFactory : GlobalCommandFactory
{
    public override IGlobalCommand? GetGlobalCommands(string? input)
    {
        // Checked before the exact-match table below because it matches on a word ANYWHERE in the
        // command, not on the whole command (issue #552).
        if (FloydMentionCommands.Match(input) is { } floydMention)
            return floydMention;

        switch (input?.ToLowerInvariant().StripNonChars().Trim())
        {
            case "zork":
                return new SimpleResponseCommand("Gesundheit! ");

            case "diagnose":
                return new DiagnoseProcessor();

            case "sleep":
            case "gotosleep":
            case "go to sleep":
            case "fallasleep":
            case "fall asleep":
            case "rest":
                return new SleepProcessor();
        }

        return base.GetGlobalCommands(input);
    }
}
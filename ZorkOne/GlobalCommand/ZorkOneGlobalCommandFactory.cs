using GameEngine.StaticCommand;
using Model.Interface;
using Utilities;
using ZorkOne.GlobalCommand.Implementation;

namespace ZorkOne.GlobalCommand;

public class ZorkOneGlobalCommandFactory : GlobalCommandFactory
{
    public override IGlobalCommand? GetGlobalCommands(string? input)
    {
        switch (input?.ToLowerInvariant().StripNonChars().Trim())
        {
            case "repent":
                return new SimpleResponseCommand("It could very well be too late! ");

            // More super nerdy Easter eggs. References the "Colossal Cave" game that inspired Zork.
            case "xyzzy":
            case "plugh":
                return new SimpleResponseCommand("A hollow voice says 'fool' ");

            case "echo":
                return new SimpleResponseCommand("echo echo...");

            case "ulysses":
            case "odysseus":
                return new SimpleResponseCommand("Wasn't he a sailor? ");

            case "win":
                return new SimpleResponseCommand("Naturally!");

            case "zork":
                return new SimpleResponseCommand("At your service!");

            case "frobozz":
                return new SimpleResponseCommand(
                    "The FROBOZZ Corporation created, owns, and operates this dungeon. "
                );

            case "lose":
            // Look this up - it's a command in the MDL language. Super nerdy Easter egg.
            case "chomp":
            case "vomit":
                return new SimpleResponseCommand("Preposterous!");

            case "sigh":
            case "mumble":
                return new SimpleResponseCommand(
                    "You'll have to speak up if you expect me to hear you! !"
                );

            case "diagnose":
                return new DiagnoseProcessor();
        }

        return base.GetGlobalCommands(input);
    }
}
using GameEngine.StaticCommand;
using Model.Interface;
using Utilities;
using ZorkOne.GlobalCommand.Implementation;

namespace ZorkOne.GlobalCommand;

public class ZorkOneGlobalCommandFactory : IGlobalCommandFactory
{
    public IGlobalCommand? GetGlobalCommands(string? input)
    {
        switch (input?.ToLowerInvariant().StripNonChars().Trim())
        {
            case "xyzzy":
            case "plugh":
                return new SimpleResponseCommand("A hollow voice says 'fool' ");

            case "ulysses":
            case "odysseus":
                return new SimpleResponseCommand("Wasn't he a sailor? ");
            
            case "win":
                return new SimpleResponseCommand("Naturally!");

            case "zork":
                return new SimpleResponseCommand("At your service!");

            case "frobozz":
                return new SimpleResponseCommand("The FROBOZZ Corporation created, owns, and operates this dungeon.");

            case "lose":
            case "chomp":
            case "vomit":
                return new SimpleResponseCommand("Preposterous!");
            
            case "sigh":
            case "mumble":
                return new SimpleResponseCommand("You'll have to speak up if you expect me to hear you! !");

            case "diagnose":
                return new DiagnoseProcessor();
        }

        return null;
    }
}
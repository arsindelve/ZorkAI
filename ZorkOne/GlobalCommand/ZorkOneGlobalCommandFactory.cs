using Game.StaticCommand;
using Utilities;
using ZorkOne.GlobalCommand.Implementation;

namespace ZorkOne.GlobalCommand;

public class ZorkOneGlobalCommandFactory : IGlobalCommandFactory
{
    public IGlobalCommand? GetGlobalCommands(string input)
    {
        switch (input.ToLowerInvariant().StripNonChars().Trim())
        {
            case "xyzzy":
            case "plugh":
                return new SimpleResponseCommand("A hollow voice says 'fool'");

            case "win":
                return new SimpleResponseCommand("Naturally!");
            
            case "zork":
                return new SimpleResponseCommand("At your service!");

            case "lose":
            case "chomp":
                return new SimpleResponseCommand("Preposterous!");
            
            case "diagnose":
                return new DiagnoseProcessor();
        }

        return null;
    }
}
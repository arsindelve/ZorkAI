using GameEngine.StaticCommand;
using Model.Interface;
using Utilities;

namespace EscapeRoom.GlobalCommand;

public class EscapeRoomGlobalCommandFactory : GlobalCommandFactory
{
    public override IGlobalCommand? GetGlobalCommands(string? input)
    {
        switch (input?.ToLowerInvariant().StripNonChars().Trim())
        {
            case "hint":
            case "help me":
                return new SimpleResponseCommand(
                    "Try examining objects and opening containers. Keys unlock doors. "
                );

            case "tutorial":
                return new SimpleResponseCommand(
                    """
                    Basic commands:
                    - LOOK or L: See your surroundings
                    - EXAMINE [object]: Look closely at something
                    - TAKE [object]: Pick something up
                    - DROP [object]: Put something down
                    - OPEN [container]: Open a container
                    - UNLOCK [door] WITH [key]: Use a key
                    - N, S, E, W: Move in a direction
                    - INVENTORY or I: See what you're carrying
                    """
                );
        }

        return base.GetGlobalCommands(input);
    }
}

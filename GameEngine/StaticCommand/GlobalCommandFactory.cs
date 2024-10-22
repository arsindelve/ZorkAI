using GameEngine.StaticCommand.Implementation;
using Model.Interface;
using Utilities;

namespace GameEngine.StaticCommand;

/// <summary>
///     "Global" commands are commands, usually one word, that can be processed by the engine
///     without needing to know any context or state. Examples are "inventory", "quit", "score"
///     and "look". This class attempts to match the user input to any of the these known,
///     global commands and if there is a match, returns a processor for that command.
/// </summary>
public class GlobalCommandFactory : IGlobalCommandFactory
{
    public virtual IGlobalCommand? GetGlobalCommands(string? input)
    {
        switch (input?.ToLowerInvariant().StripNonChars().Trim())
        {
            case "verbose":
            case "brief":
            case "superbrief":
                return new VerbosityProcessor();

            case "inventory":
            case "i":
            case "what am i holding":
            case "what do i have on me":
                return new InventoryProcessor();

            case "take":
                return new TakeOnlyAvailableItemProcessor();

            case "take all":
            case "get all":
            case "get everything":
            case "take everything":
            case "pick up all":
            case "pick up everything":
                return new TakeEverythingProcessor();

            case "drop all":
            case "drop everything":
                return new DropEverythingProcessor();

            case "wait":
            case "z":
                return new WaitProcessor();

            case "save":
            case "save my game":
            case "save my progress":
                return new SaveProcessor();

            case "restore":
            case "restore my game":
            case "restore my progress":
                return new RestoreProcessor();

            case "look":
            case "where am i":
            case "examine my surroundings":
            case "l":
            case "look around":
            case "look around me":
                return new LookProcessor();

            case "quit":
            case "quit the game":
            case "stop":
            case "end the game":
            case "i want to quit":
            case "i want to quit the game":
            case "stop the game":
                return new QuitProcessor();

            case "score":
            case "what is my score":
            case "tell me my score":
                return new ScoreProcessor();

            case "restart":
            case "restart the game":
            case "start over":
            case "start the game over":
            case "start from the beginning":
                return new RestartProcessor();
        }

        return null;
    }
}

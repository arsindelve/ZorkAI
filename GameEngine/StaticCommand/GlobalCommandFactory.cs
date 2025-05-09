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
        if (input?.ToLowerInvariant().StartsWith("god mode") ?? false)
            return new GodModeProcessor();

        switch (input?.ToLowerInvariant().StripNonChars().Trim())
        {
            case "time":
            case "current time":
            case "what time is it":
            case "what is the current time":
            case "what time is it right now":
            case "what is the time":
                return new CurrentTimeProcessor();

            case "inventory":
            case "i":
            case "what am i holding":
            case "what do i have on me":
            case "what do i have":
            case "check inventory":
            case "check my inventory":
            case "what do I have in my inventory":
            case "look in my inventory":
            case "look in inventory":
                return new InventoryProcessor();

            case "take":
                return new TakeOnlyAvailableItemProcessor();

            case "take all":
            case "take it all":
            case "get all":
            case "get everything":
            case "take everything":
            case "pick up all":
            case "pick up everything":
                return new TakeEverythingProcessor();

            case "drop all":
            case "drop it all":
            case "drop everything":
                return new DropEverythingProcessor();

            case "wait":
            case "z":
                return new WaitProcessor();

            case "look":
            case "where am i":
            case "examine my surroundings":
            case "examine surroundings":
            case "examine area":
            case "examine the area":
            case "examine the surroundings":
            case "l":
            case "look around":
            case "look around me":
                return new LookProcessor();


            case "score":
            case "what is my score":
            case "tell me my score":
                return new ScoreProcessor();
        }

        return null;
    }

    /// <summary>
    /// System commands are slightly different - they are commands that affect
    /// the game itself, as opposed to the state of the game. These are commands
    /// like "save" and "quit" that should always work, no matter the state of the game.
    /// They should also be executed before any other kind of command, and not be intercepted
    /// by locations like the "loud room". 
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public ISystemCommand? GetSystemCommands(string? input)
    {
        switch (input?.ToLowerInvariant().StripNonChars().Trim())
        {
            case "verbose":
            case "brief":
            case "superbrief":
                return new VerbosityProcessor();

            case "save":
            case "save my game":
            case "save my progress":
                return new SaveProcessor();

            case "restore":
            case "restore my game":
            case "restore my progress":
                return new RestoreProcessor();

            case "quit":
            case "quit the game":
            case "stop":
            case "end the game":
            case "i want to quit":
            case "i want to quit the game":
            case "stop the game":
                return new QuitProcessor();

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
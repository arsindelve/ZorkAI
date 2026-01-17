namespace Planetfall.Command;

/// <summary>
/// Centralizes all the different ways a player might try to get into a bed.
/// Used by both DormBase and Infirmary locations.
/// </summary>
public static class BedCommands
{
    private static readonly HashSet<string> BedEntryCommands = new(StringComparer.OrdinalIgnoreCase)
    {
        // enter variations
        "enter bed",
        "enter the bed",
        "enter bunk",
        "enter the bunk",

        // get in/into variations
        "get in bed",
        "get in the bed",
        "get into bed",
        "get into the bed",
        "get in bunk",
        "get in the bunk",
        "get into bunk",
        "get into the bunk",
        "get on bed",
        "get on the bed",
        "get on bunk",
        "get on the bunk",

        // climb variations
        "climb in bed",
        "climb in the bed",
        "climb into bed",
        "climb into the bed",
        "climb in bunk",
        "climb in the bunk",
        "climb into bunk",
        "climb into the bunk",
        "climb on bed",
        "climb on the bed",
        "climb on bunk",
        "climb on the bunk",

        // lie/lay down variations
        "lie down",
        "lay down",
        "lie down in bed",
        "lie down in the bed",
        "lie down on bed",
        "lie down on the bed",
        "lie down in bunk",
        "lie down in the bunk",
        "lie down on bunk",
        "lie down on the bunk",
        "lay down in bed",
        "lay down in the bed",
        "lay down on bed",
        "lay down on the bed",
        "lay down in bunk",
        "lay down in the bunk",
        "lay down on bunk",
        "lay down on the bunk",
        "lie in bed",
        "lie in the bed",
        "lie on bed",
        "lie on the bed",
        "lay in bed",
        "lay in the bed",
        "lay on bed",
        "lay on the bed",

        // go to bed variations (NOT "go to sleep" - that's handled by SleepProcessor)
        "go to bed",
        "go to the bed",
        "go to the bunk",

        // use variations
        "use bed",
        "use the bed",
        "use bunk",
        "use the bunk",

        // hop/jump variations
        "hop in bed",
        "hop in the bed",
        "hop into bed",
        "hop into the bed",
        "jump in bed",
        "jump in the bed",
        "jump into bed",
        "jump into the bed",

        // retire variation
        "retire"
    };

    private static readonly HashSet<string> BedExitCommands = new(StringComparer.OrdinalIgnoreCase)
    {
        // stand variations
        "stand",
        "stand up",
        "stand up from bed",
        "stand up from the bed",
        "stand up from bunk",
        "stand up from the bunk",

        // get up variations
        "get up",
        "get up from bed",
        "get up from the bed",
        "get up from bunk",
        "get up from the bunk",

        // get out variations
        "get out",
        "get out of bed",
        "get out of the bed",
        "get out of bunk",
        "get out of the bunk",
        "get off",
        "get off bed",
        "get off the bed",
        "get off bunk",
        "get off the bunk",

        // exit variations
        "exit",
        "exit bed",
        "exit the bed",
        "exit bunk",
        "exit the bunk",

        // leave variations
        "leave",
        "leave bed",
        "leave the bed",
        "leave bunk",
        "leave the bunk",

        // climb out variations
        "climb out",
        "climb out of bed",
        "climb out of the bed",
        "climb out of bunk",
        "climb out of the bunk",
        "climb off",
        "climb off bed",
        "climb off the bed",
        "climb off bunk",
        "climb off the bunk",

        // hop out variations
        "hop out",
        "hop out of bed",
        "hop out of the bed",
        "hop out of bunk",
        "hop out of the bunk",
        "hop off",
        "hop off bed",
        "hop off the bed",

        // jump out variations
        "jump out",
        "jump out of bed",
        "jump out of the bed",
        "jump out of bunk",
        "jump out of the bunk",
        "jump off",
        "jump off bed",
        "jump off the bed",

        // rise variations
        "rise",
        "rise up",
        "rise from bed",
        "rise from the bed",
        "rise from bunk",
        "rise from the bunk",

        // wake variations
        "wake",
        "wake up",
        "awake",
        "awaken",

        // step out variations
        "step out",
        "step out of bed",
        "step out of the bed",

        // roll out variations
        "roll out",
        "roll out of bed",
        "roll out of the bed",
        "roll off",
        "roll off bed",
        "roll off the bed",

        // emerge variation
        "emerge",
        "emerge from bed",
        "emerge from the bed",

        // out/up simple commands
        "out",
        "up",

        // stop sleeping/resting
        "stop sleeping",
        "stop resting",
        "stop napping",
        "quit sleeping",

        // dismount variation
        "dismount",
        "dismount bed",
        "dismount the bed",
        "dismount bunk",
        "dismount the bunk"
    };

    /// <summary>
    /// Checks if the input matches any known bed entry command.
    /// </summary>
    public static bool IsBedEntryCommand(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        return BedEntryCommands.Contains(input.Trim());
    }

    /// <summary>
    /// Checks if the input matches any known bed exit command.
    /// </summary>
    public static bool IsBedExitCommand(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        return BedExitCommands.Contains(input.Trim());
    }
}

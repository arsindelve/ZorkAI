namespace Model.Movement;

public static class DirectionParser
{
    /// <summary>
    ///     Determines if the given intent represents a valid direction and assigns the parsed direction to the output
    ///     parameter.
    /// </summary>
    /// <param name="intent">The intent to parse.</param>
    /// <param name="direction">The parsed direction.</param>
    /// <returns>True if the intent represents a valid direction; otherwise, false.</returns>
    public static bool IsDirection(string? intent, out Direction direction)
    {
        intent = intent?.Replace("go ", "");
        var firstWord = intent?.Split(' ')[0];
        direction = ParseDirection(firstWord);
        return direction != Direction.Unknown;
    }

    public static Direction ParseDirection(string? rawDirection)
    {
        if (string.IsNullOrEmpty(rawDirection))
            return Direction.Unknown;

        switch (rawDirection.ToLowerInvariant().Trim())
        {
            case "n":
            case "fore":
            case "north":
                return Direction.N;

            case "starboard":
            case "e":
            case "east":
                return Direction.E;

            case "aft":
            case "s":
            case "south":
                return Direction.S;

            case "port":
            case "w":
            case "west":
                return Direction.W;

            case "sw":
            case "south-west":
            case "south west":
            case "southwest":
                return Direction.SW;

            case "nw":
            case "north-west":
            case "north west":
            case "northwest":
                return Direction.NW;

            case "ne":
            case "north-east":
            case "north east":
            case "northeast":
                return Direction.NE;

            case "se":
            case "south-east":
            case "south east":
            case "southeast":
                return Direction.SE;

            case "up":
            case "u":
            case "climb":
                return Direction.Up;

            case "down":
            case "d":
                return Direction.Down;

            case "in":
            case "enter":
                return Direction.In;

            case "out":
            case "exit":
                return Direction.Out;

            default:
                return Direction.Unknown;
        }
    }
}
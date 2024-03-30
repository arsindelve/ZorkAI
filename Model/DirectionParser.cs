namespace Model;

public static class DirectionParser
{
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
            case "north":
                return Direction.N;

            case "e":
            case "east":
                return Direction.E;

            case "s":
            case "south":
                return Direction.S;

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
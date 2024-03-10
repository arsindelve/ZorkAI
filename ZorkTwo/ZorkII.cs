using Model;
using ZorkTwo.Location;

namespace ZorkTwo;

public class ZorkII : IInfocomGame
{
    public Type StartingLocation => typeof(InsideTheBarrow);

    public string StartText => """
                               ZORK AI Two: The Wizard of Frobozz
                               Infocom interactive fiction - a fantasy story
                               ZORK is a registered trademark of Infocom, Inc.
                               Release 63 / Serial number 860811
                               """;
}
using ZorkOne.Location;

namespace ZorkOne;

public class ZorkI : IInfocomGame
{
    public ZorkI()
    {
        PreLoad();
    }
    
    public Type StartingLocation => typeof(WestOfHouse);

    public string StartText => """

                               ZORK AI One: The Great Underground Empire Re-Imagined
                               ZORK is a registered trademark of Infocom, Inc.
                               Revision 76 / Serial number 840509

                               """;

    public void PreLoad()
    {
        Repository.GetItem<Troll>().CurrentLocation = Repository.GetLocation<TrollRoom>();
    }
}
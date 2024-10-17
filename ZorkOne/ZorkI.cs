using Model.Interface;
using ZorkOne.GlobalCommand;

namespace ZorkOne;

public class ZorkI : IInfocomGame
{
    public Type StartingLocation => typeof(WestOfHouse);

    public string DefaultSaveGameName => "zork1-ai.sav";

    public string GetScoreDescription(int score)
    {
        // https://ganelson.github.io/inform-website/book/WI_9_3.html

        if (score < 25)
            return "Beginner";
        if (score < 50)
            return "Amateur Adventurer";
        if (score < 100)
            return "Novice Adventurer";
        if (score < 200)
            return "Junior Adventurer";
        if (score < 300)
            return "Adventurer";
        if (score < 330)
            return "Master";
        if (score < 350)
            return "Wizard";
        return "Master Adventurer";
    }

    public IGlobalCommandFactory GetGlobalCommandFactory()
    {
        return new ZorkOneGlobalCommandFactory();
    }

    public ZorkI()
    {
        _ = Repository.GetLocation<TrollRoom>();
    }

    public string StartText => """

                               ZORK AI One: The Great Underground Empire Re-Imagined
                               ZORK is a registered trademark of Infocom, Inc.
                               Revision 76 / Serial number 840509

                               """;
}
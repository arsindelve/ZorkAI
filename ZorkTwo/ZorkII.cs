using Model.Interface;
using ZorkTwo.Location;

namespace ZorkTwo;

// ReSharper disable once InconsistentNaming
public class ZorkII : IInfocomGame
{
    public Dictionary<string, ITurnBasedActor> Actors { get; } = new();
    public Type StartingLocation => typeof(InsideTheBarrow);

    public string GameName => "ZorkTwo";

    public string StartText => """
                               ZORK AI Two: The Wizard of Frobozz
                               Infocom interactive fiction - a fantasy story
                               ZORK is a registered trademark of Infocom, Inc.
                               Release 63 / Serial number 860811
                               """;

    public string DefaultSaveGameName => "zork2-ai.sav";

    public string GetScoreDescription(int score)
    {
        throw new NotImplementedException();
    }

    public IGlobalCommandFactory GetGlobalCommandFactory()
    {
        throw new NotImplementedException();
    }

    public string SessionTableName => "zork2_session";

    public void Init(IContext context)
    {
    }

    public string SystemPromptSecretKey => "ZorkOnePrompt";

    public T GetContext<T>() where T : IContext, new()
    {
        throw new NotImplementedException();
    }
}
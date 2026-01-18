using EscapeRoom.GlobalCommand;
using EscapeRoom.Location;
using Model.Interface;

namespace EscapeRoom;

public class EscapeRoomGame : IInfocomGame
{
    public Type StartingLocation => typeof(Reception);

    public string GameName => "EscapeRoom";

    public string DefaultSaveGameName => "escaperoom.sav";

    public string SessionTableName => "escaperoom_session";

    public string SystemPromptSecretKey => "EscapeRoomPrompt";

    public string StartText => """

                               ESCAPE ROOM TUTORIAL
                               A simple training game for IF adventurers
                               Version 1.0

                               """;

    public string GetScoreDescription(int score)
    {
        if (score == 0)
            return "Beginner";
        if (score < 50)
            return "Novice Escaper";
        if (score < 100)
            return "Apprentice Escaper";
        return "Master Escaper";
    }

    public IGlobalCommandFactory GetGlobalCommandFactory()
    {
        return new EscapeRoomGlobalCommandFactory();
    }

    public void Init(IContext context)
    {
        // No special initialization needed for this simple game
    }
}

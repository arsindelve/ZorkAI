using OpenAI;

namespace ZorkOne.GlobalCommand.Implementation;

public class DiagnoseProcessor : IGlobalCommand
{
    public Task<string> Process(string? input, IContext context, IGenerationClient client)
    {
        if (context is not ZorkIContext zorkContext)
            throw new ArgumentException();

        string response;

        if (zorkContext.LightWoundCounter > 0)
            response =
                $"You have a light wound, which will be cured after {zorkContext.LightWoundCounter}" +
                $" moves.\nYou can be killed by one more light wound. ";
        else
            response = "You are in perfect health.\nYou can be killed by a serious wound. ";

        if (zorkContext.DeathCounter > 0)
        {
            if (zorkContext.DeathCounter == 1)
                response += "\nYou have been killed once. ";
            else
                response += $"\nYou have been killed {zorkContext.DeathCounter} times. ";
        }

        return Task.FromResult(response);
    }
}
using OpenAI;

namespace ZorkOne.GlobalCommand.Implementation;

public class DiagnoseProcessor : IGlobalCommand
{
    public Task<string> Process(string? input, IContext context, IGenerationClient client)
    {
        if (context is not ZorkOneContext zorkContext)
            throw new ArgumentException();


        if (zorkContext.HasWound)
            return Task.FromResult("Wounded");
        
        else
            return Task.FromResult("Not wounded");


    }
}
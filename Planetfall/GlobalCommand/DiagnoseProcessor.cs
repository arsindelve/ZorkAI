using Model;
using Model.AIGeneration;

namespace Planetfall.GlobalCommand;

public class DiagnoseProcessor : IGlobalCommand
{
    public Task<string> Process(string? input, IContext context, IGenerationClient client, Runtime runtime)
    {
        // TODO: Implement me. 
        string response = "I'm not sure what you mean. ";
        return Task.FromResult(response);
    }
}
using Model;
using Model.AIGeneration;

namespace Planetfall.GlobalCommand;

public class DiagnoseProcessor : IGlobalCommand
{
    public Task<string> Process(string? input, IContext context, IGenerationClient client, Runtime runtime)
    {
        if (context is not PlanetfallContext pc)
            throw new Exception("Context is not a PlanetfallContext");

        // TODO: Tired
        // TODO: Hungry

        var response = pc.SicknessDescription;
        return Task.FromResult(response);
    }
}
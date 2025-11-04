using Model.AIGeneration;

namespace Planetfall.GlobalCommand;

public class DiagnoseProcessor : IGlobalCommand
{
    public Task<string> Process(string? input, IContext context, IGenerationClient client, Runtime runtime)
    {
        if (context is not PlanetfallContext pc)
            throw new Exception("Context is not a PlanetfallContext");

        var response = pc.SicknessDescription + "\n" +
                       pc.Tired.GetDescription() + "\n" +
                       pc.Hunger.GetDescription();

        return Task.FromResult(response);
    }
}
using Model.AIGeneration;
using Model.AIGeneration.Requests;
using Model.Interface;

namespace GameEngine.StaticCommand.Implementation;

internal class VerbosityProcessor : ISystemCommand
{
    public async Task<string> Process(string? input, IContext context, IGenerationClient client, Runtime runtime)
    {
        switch (input)
        {
            case "verbose":
                context.Verbosity = Verbosity.Verbose;
                return await client.GenerateNarration(new MaximumVerbosityRequest(), context.SystemPromptAddendum);
            case "brief":
                context.Verbosity = Verbosity.Brief;
                return await client.GenerateNarration(new MediumVerbosityRequest(), context.SystemPromptAddendum);
            case "superbrief":
                context.Verbosity = Verbosity.SuperBrief;
                return await client.GenerateNarration(new MinimumVerbosityRequest(), context.SystemPromptAddendum);
        }

        throw new ArgumentOutOfRangeException();
    }
}
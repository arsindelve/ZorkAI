using Model.AIGeneration;
using Model.Interface;

namespace Game.StaticCommand.Implementation;

internal class VerbosityProcessor : IGlobalCommand
{
    public Task<string> Process(string? input, IContext context, IGenerationClient client, Runtime runtime)
    {
        switch (input)
        {
            case "verbose":
                context.Verbosity = Verbosity.Verbose;
                return Task.FromResult("Maximum verbosity. ");
            case "brief":
                context.Verbosity = Verbosity.Brief;
                return Task.FromResult("Brief descriptions. ");
            case "superbrief":
                context.Verbosity = Verbosity.SuperBrief;
                return Task.FromResult("Superbrief descriptions. ");
        }

        throw new ArgumentOutOfRangeException();
    }
}
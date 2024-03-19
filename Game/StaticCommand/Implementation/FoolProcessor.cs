namespace Game.StaticCommand.Implementation;

internal class FoolProcessor : IGlobalCommand
{
    // https://en.wikipedia.org/wiki/Xyzzy_(computing)

    public Task<string> Process(string? input, IContext context, IGenerationClient client)
    {
        return Task.FromResult("A hollow voice says 'fool'");
    }
}
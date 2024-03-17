namespace Game.StaticCommand.Implementation;

internal class RestoreProcessor : IStatefulProcessor
{
    private string _fileName = "";
    private bool _havePromptedForFilename;

    public async Task<string> Process(string? input, IContext context, IGenerationClient client)
    {
        if (!_havePromptedForFilename) return await PromptForFilename(context, client);
        if (context.Engine is not null) return await AttemptTheRestore(input, context, client);

        return string.Empty;
    }

    public bool Completed { get; private set; }

    public bool ContinueProcessing => false;

    private async Task<string> AttemptTheRestore(string? input, IContext context, IGenerationClient client)
    {
        Completed = true;
        _havePromptedForFilename = false;
        _fileName = string.IsNullOrEmpty(input) ? context.LastSaveGameName ?? context.Game.DefaultSaveGameName : input;
        context.LastSaveGameName = _fileName;
        try
        {
            var newContext = await RestoreGame(context.Engine!);
            var generatedResponse =
                await client.CompleteChat(
                    new AfterRestoreGameRequest(newContext.CurrentLocation.DescriptionForGeneration));
            return $"{generatedResponse}\n\n{newContext.CurrentLocation.Description}";
        }
        catch (FileNotFoundException)
        {
            return await client.CompleteChat(
                new RestoreFailedFileNotFoundGameRequest(context.CurrentLocation.DescriptionForGeneration));
        }
        catch (Exception)
        {
            return await client.CompleteChat(
                new RestoreFailedUnknownReasonGameRequest(context.CurrentLocation.DescriptionForGeneration));
        }
    }

    private async Task<string> PromptForFilename(IContext context, IGenerationClient client)
    {
        Completed = false;
        _havePromptedForFilename = true;
        var generatedResponse =
            await client.CompleteChat(
                new BeforeRestoreGameRequest(context.CurrentLocation.DescriptionForGeneration));

        return
            $"{generatedResponse}\n\nEnter a file name.\nDefault is \"{context.LastSaveGameName ?? context.Game.DefaultSaveGameName}\":";
    }

    private async Task<IContext> RestoreGame(IGameEngine contextEngine)
    {
        var contents = await File.ReadAllTextAsync(_fileName);
        var decodedBytes = Convert.FromBase64String(contents);
        var decodedText = Encoding.UTF8.GetString(decodedBytes);
        return contextEngine.RestoreGame(decodedText);
    }
}
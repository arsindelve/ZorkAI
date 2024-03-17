namespace Game.StaticCommand.Implementation;

internal class SaveProcessor : IStatefulProcessor
{
    private string _fileName = "";
    private bool _havePromptedForFilename;

    public async Task<string> Process(string? input, IContext context, IGenerationClient client)
    {
        if (!_havePromptedForFilename) return await PromptForFilename(context, client);
        if (context.Engine is not null) return await AttemptTheSave(input, context, client);

        return string.Empty;
    }

    public bool Completed { get; private set; }

    bool IStatefulProcessor.ContinueProcessing => false;

    private async Task<string> AttemptTheSave(string? input, IContext context, IGenerationClient client)
    {
        Completed = true;
        _havePromptedForFilename = false;
        _fileName = string.IsNullOrEmpty(input) ? context.LastSaveGameName ?? context.Game.DefaultSaveGameName : input;
        context.LastSaveGameName = _fileName;

        try
        {
            await SaveGame(context);
            var generatedResponse =
                await client.CompleteChat(
                    new AfterSaveGameRequest(context.CurrentLocation.DescriptionForGeneration));
            return $"{generatedResponse}";
        }
        catch (Exception)
        {
            return await client.CompleteChat(
                new SaveFailedUnknownReasonGameRequest(context.CurrentLocation.DescriptionForGeneration));
        }
    }

    private async Task<string> PromptForFilename(IContext context, IGenerationClient client)
    {
        Completed = false;
        _havePromptedForFilename = true;
        var generatedResponse =
            await client.CompleteChat(
                new BeforeSaveGameRequest(context.CurrentLocation.DescriptionForGeneration));

        return
            $"{generatedResponse}\n\nEnter a file name.\nDefault is \"{context.LastSaveGameName ?? context.Game.DefaultSaveGameName}\":";
    }

    private async Task SaveGame(IContext context)
    {
        var data = context.Engine!.SaveGame();
        var bytesToEncode = Encoding.UTF8.GetBytes(data);
        var encodedText = Convert.ToBase64String(bytesToEncode);
        await File.WriteAllTextAsync(_fileName, encodedText);
    }
}
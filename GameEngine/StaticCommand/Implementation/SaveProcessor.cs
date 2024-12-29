using Model.AIGeneration;
using Model.AIGeneration.Requests;
using Model.Interface;

namespace GameEngine.StaticCommand.Implementation;

internal class SaveProcessor : IStatefulProcessor
{
    private readonly ISaveGameWriter _writer;
    private string _fileName = "";
    private bool _havePromptedForFilename;

    public SaveProcessor()
    {
        _writer = new SaveGameWriter();
    }

    /// <summary>
    ///     Constructor for unit testing
    /// </summary>
    /// <param name="writer"></param>
    public SaveProcessor(ISaveGameWriter writer)
    {
        _writer = writer;
    }

    public async Task<string> Process(string? input, IContext context, IGenerationClient client, Runtime runtime)
    {
        if (runtime == Runtime.Web)
            return "<Save>";
        
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
        _fileName = string.IsNullOrEmpty(input)
            ? string.IsNullOrEmpty(context.LastSaveGameName)
                ? context.Game.DefaultSaveGameName
                : context.LastSaveGameName
            : input;

        context.LastSaveGameName = _fileName;

        try
        {
            await SaveGame(context);
            var generatedResponse =
                await client.GenerateNarration(
                    new AfterSaveGameRequest(context.CurrentLocation.DescriptionForGeneration));
            return $"{generatedResponse}";
        }
        catch (Exception)
        {
            return await client.GenerateNarration(
                new SaveFailedUnknownReasonGameRequest(context.CurrentLocation.DescriptionForGeneration));
        }
    }

    private async Task<string> PromptForFilename(IContext context, IGenerationClient client)
    {
        Completed = false;
        _havePromptedForFilename = true;
        var generatedResponse =
            await client.GenerateNarration(
                new BeforeSaveGameRequest(context.CurrentLocation.DescriptionForGeneration));

        return
            $"{generatedResponse}\n\nEnter a file name.\nDefault " +
            $"is \"{(string.IsNullOrEmpty(context.LastSaveGameName)
                ? context.Game.DefaultSaveGameName
                : context.LastSaveGameName)}\":";
    }

    private async Task SaveGame(IContext context)
    {
        var data = context.Engine!.SaveGame();
        var bytesToEncode = Encoding.UTF8.GetBytes(data);
        var encodedText = Convert.ToBase64String(bytesToEncode);
        await _writer.Write(_fileName, encodedText);
    }
}

public interface ISaveGameWriter
{
    Task Write(string filename, string data);
}

internal class SaveGameWriter : ISaveGameWriter
{
    public async Task Write(string filename, string data)
    {
        await File.WriteAllTextAsync(filename, data);
    }
}
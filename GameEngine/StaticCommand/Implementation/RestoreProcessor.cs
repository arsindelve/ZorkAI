﻿using Model.AIGeneration;
using Model.AIGeneration.Requests;
using Model.Interface;

namespace GameEngine.StaticCommand.Implementation;

internal class RestoreProcessor : IStatefulProcessor
{
    private readonly ISaveGameReader _reader;
    private string _fileName = "";
    private bool _havePromptedForFilename;

    public RestoreProcessor()
    {
        _reader = new SaveGameReader();
    }

    /// <summary>
    ///     Constructor for unit testing
    /// </summary>
    /// <param name="reader"></param>
    public RestoreProcessor(ISaveGameReader reader)
    {
        _reader = reader;
    }

    public async Task<string> Process(string? input, IContext context, IGenerationClient client, Runtime runtime)
    {
        if (runtime == Runtime.Web)
            return "<Restore>";
        
        if (!_havePromptedForFilename) return await PromptForFilename(context, client);
        if (context.Engine is not null) return await AttemptTheRestore(input, context, client);

        return string.Empty;
    }

    public bool Completed { get; private set; }

    bool IStatefulProcessor.ContinueProcessing => false;

    private async Task<string> AttemptTheRestore(string? input, IContext context, IGenerationClient client)
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
            var newContext = await RestoreGame(context.Engine!);
            var generatedResponse =
                await client.GenerateNarration(
                    new AfterRestoreGameRequest(newContext.CurrentLocation.GetDescriptionForGeneration(context)), context.SystemPromptAddendum);
            return $"{generatedResponse}\n\n{newContext.CurrentLocation.GetDescriptionForGeneration(context)}";
        }
        catch (FileNotFoundException)
        {
            return await client.GenerateNarration(
                new RestoreFailedFileNotFoundGameRequest(context.CurrentLocation.GetDescriptionForGeneration(context)), context.SystemPromptAddendum);
        }
        catch (Exception)
        {
            return await client.GenerateNarration(
                new RestoreFailedUnknownReasonGameRequest(context.CurrentLocation.GetDescriptionForGeneration(context)), context.SystemPromptAddendum);
        }
    }

    private async Task<string> PromptForFilename(IContext context, IGenerationClient client)
    {
        Completed = false;
        _havePromptedForFilename = true;
        var generatedResponse =
            await client.GenerateNarration(
                new BeforeRestoreGameRequest(context.CurrentLocation.GetDescriptionForGeneration(context)), context.SystemPromptAddendum);

        return
            $"{generatedResponse}\n\nEnter a file name.\nDefault " +
            $"is \"{(string.IsNullOrEmpty(context.LastSaveGameName)
                ? context.Game.DefaultSaveGameName
                : context.LastSaveGameName)}\":";
    }

    private async Task<IContext> RestoreGame(IGameEngine contextEngine)
    {
        var contents = await _reader.Read(_fileName);
        var decodedBytes = Convert.FromBase64String(contents);
        var decodedText = Encoding.UTF8.GetString(decodedBytes);
        return contextEngine.RestoreGame(decodedText);
    }
}

public interface ISaveGameReader
{
    Task<string> Read(string filename);
}

internal class SaveGameReader : ISaveGameReader
{
    public async Task<string> Read(string filename)
    {
        return await File.ReadAllTextAsync(filename);
    }
}
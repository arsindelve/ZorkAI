﻿namespace Game.StaticCommand.Implementation;

internal class RestoreProcessor : IStatefulProcessor
{
    
    public async Task<string> Process(string? input, IContext context, IGenerationClient client)
    {
        Completed = true;
        ContinueProcessing = true;
        
        if (context.Engine is not null)
        {
            await RestoreGame(context.Engine);
            return await client.CompleteChat(new SaveGameRequest(context.CurrentLocation.DescriptionForGeneration));
        }
        
        return string.Empty;
    }

    private async Task RestoreGame(IGameEngine contextEngine)
    {
        var contents = await File.ReadAllTextAsync("saved-game.sav");
        byte[] decodedBytes = Convert.FromBase64String(contents);

        // Convert bytes to string
        string decodedText = Encoding.UTF8.GetString(decodedBytes);
        contextEngine.RestoreGame(decodedText);
    }

    public bool Completed { get; private set; }

    public bool ContinueProcessing { get; private set; }
}

internal class SaveProcessor : IStatefulProcessor
{
    public async Task<string> Process(string? input, IContext context, IGenerationClient client)
    {
        Completed = true;
        ContinueProcessing = true;
        
        if (context.Engine is not null)
        {
            await SaveGame(context.Engine);
            return await client.CompleteChat(new SaveGameRequest(context.CurrentLocation.DescriptionForGeneration));
        }
        
        return string.Empty;
    }

    public bool Completed { get; private set; }

    public bool ContinueProcessing { get; private set; }

    private static async Task SaveGame(IGameEngine engine)
    {
        var data = engine.SaveGame();
        var bytesToEncode = Encoding.UTF8.GetBytes(data);
        var encodedText = Convert.ToBase64String(bytesToEncode);
        await File.WriteAllTextAsync("saved-game.sav", encodedText);
    }
}
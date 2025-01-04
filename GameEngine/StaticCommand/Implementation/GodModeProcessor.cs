using Model.AIGeneration;
using Model.Interface;

namespace GameEngine.StaticCommand.Implementation;

/// <summary>
/// I can't make you not use this, or not be aware of it's existence - open source after all.
/// But seriously, I use this for testing and debugging. If you use it while playing the game,
/// you'll ruin your experience. Resist the urge. 
/// </summary>
public class GodModeProcessor : IGlobalCommand
{
    public Task<string> Process(string? input, IContext context, IGenerationClient client, Runtime runtime)
    {
        if (string.IsNullOrEmpty(input))
            throw new ArgumentNullException(nameof(input));

        if (input.Contains("take")) return Task.FromResult(Take(input, context));
        if (input.Contains("go")) return Task.FromResult(Go(input, context));

        return Task.FromResult("Invalid use of God mode. Bad adventurer! ");
    }

    private string Go(string input, IContext context)
    {
        Repository.LoadAllLocations(context.Game.GameName);
        var lastWord = GetWordsAfterTarget(input, "go");
        var location = Repository.GetLocation(lastWord);

        if (location == null)
            return "Invalid use of God mode. Bad adventurer! ";

        location.Init();
        context.CurrentLocation = location;
        return $"Welcome to {location.Name}";
    }

    private string Take(string input, IContext context)
    {
        Repository.LoadAllItems(context.Game.GameName);
        var lastWord = GetWordsAfterTarget(input, "take");
        var item = Repository.GetItem(lastWord);
        if (item == null)
            return "Invalid use of God mode. Bad adventurer! ";

        context.ItemPlacedHere(item);
        return $"I hope you enjoy your {item.Name}";
    }

    private static string GetWordsAfterTarget(string input, string targetWord)
    {
        if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(targetWord))
            return string.Empty;

        var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var indexOfTarget = Array.IndexOf(words, targetWord);

        if (indexOfTarget == -1 || indexOfTarget == words.Length - 1)
            return string.Empty;

        return string.Join(" ", words.Skip(indexOfTarget + 1));
    }
}
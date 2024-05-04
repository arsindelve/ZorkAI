using Game.IntentEngine;
using Model.AIGeneration;
using Model.Interface;
using Model.Item;

namespace Game.StaticCommand.Implementation;

public class TakeOnlyAvailableItemProcessor : IStatefulProcessor
{
    private bool _secondTimeProcessing;

    public async Task<string> Process(string? input, IContext context, IGenerationClient client, Runtime runtime)
    {
        IIntentEngine processor = new SimpleInteractionEngine();
        string nounToTake;

        if (_secondTimeProcessing)
        {
            if (string.IsNullOrEmpty(input))
                return "What do you want to take? ";

            nounToTake = input;
        }

        else
        {
            _secondTimeProcessing = true;
            ContinueProcessing = true;

            var location = (ICanHoldItems)context.CurrentLocation;
            var itemsHere = location.Items?.Where(s => s is ICanBeTakenAndDropped).ToList();

            if (itemsHere?.Count != 1)
            {
                _secondTimeProcessing = true;
                return "What do you want to take? ";
            }

            var itemToTake = itemsHere.Single();
            nounToTake = itemToTake.NounsForMatching.First();
        }

        var intent = new SimpleIntent
        {
            OriginalInput = "take",
            Noun = nounToTake,
            Verb = "take"
        };

        var result = await processor.Process(intent, context, client);
        ContinueProcessing = false;
        Completed = true;

        if (input?.ToLowerInvariant().Trim() == "take")
            result = $"({nounToTake})\n {result}";

        return result;
    }

    public bool Completed { get; private set; }

    public bool ContinueProcessing { get; private set; }
}
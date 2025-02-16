using GameEngine.Item.ItemProcessor;
using Model.AIGeneration;
using Model.Interface;
using Model.Item;

namespace GameEngine.StaticCommand.Implementation;

public class TakeOnlyAvailableItemProcessor : IStatefulProcessor
{
    private bool _secondTimeProcessing;

    public async Task<string> Process(string? input, IContext context, IGenerationClient client, Runtime runtime)
    {
        string nounToTake;
        IItem? itemToTake;
        
        if (_secondTimeProcessing)
        {
            if (string.IsNullOrEmpty(input))
                return "What do you want to take? ";

            nounToTake = input;
            itemToTake = Repository.GetItem(nounToTake);
        }

        else
        {
            _secondTimeProcessing = true;
            ContinueProcessing = true;

            var location = (ICanHoldItems)context.CurrentLocation;
            var itemsHere = location.Items.Where(s => s is ICanBeTakenAndDropped).ToList();

            if (itemsHere.Count != 1)
            {
                _secondTimeProcessing = true;
                return "What do you want to take? ";
            }

            itemToTake = itemsHere.Single();
            nounToTake = itemToTake.NounsForMatching.First();
        }

        var result = TakeOrDropInteractionProcessor.TakeIt(context, itemToTake).InteractionMessage;
        ContinueProcessing = false;
        Completed = true;

        if (input?.ToLowerInvariant().Trim() == "take")
            result = $"({nounToTake})\n {result}";

        return result;
    }

    public bool Completed { get; private set; }

    public bool ContinueProcessing { get; private set; }
}
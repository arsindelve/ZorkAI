using GameEngine.Item.ItemProcessor;
using Model.AIParsing;
using Model.Interface;
using Model.Item;

namespace GameEngine.Item;

public class ItemProcessorFactory(IAITakeAndAndDropParser aiTakeAndAndDropParser) : IItemProcessorFactory
{
    public List<IVerbProcessor> GetProcessors(object item)
    {
        List<IVerbProcessor> result =
        [
            // anything can be examined
            new ExamineInteractionProcessor(),
            // and smelled 
            new SmellInteractionProcessor()
        ];

        if (item is ICanBeTakenAndDropped)
            result.Add(new TakeOrDropInteractionProcessor(aiTakeAndAndDropParser));
        else
            result.Add(new CannotBeTakenProcessor());

        if (item is ICanBeTakenAndDropped)
            result.Add(new TakeOrDropInteractionProcessor(aiTakeAndAndDropParser));

        if (item is ICanBeRead)
            result.Add(new ReadInteractionProcessor());

        if (item is ITurnOffAndOn)
            result.Add(new TurnOnOrOffProcessor());

        if (item is ICannotBeTurnedOff)
            result.Add(new TurnOnOrOffProcessor());

        if (item is IOpenAndClose)
            result.Add(new OpenAndCloseInteractionProcessor());

        if (item is ICanBeEaten)
            result.Add(new EatAndDrinkInteractionProcessor());

        if (item is IAmADrink)
            result.Add(new EatAndDrinkInteractionProcessor());

        if (item is IAmClothing)
            result.Add(new ClothingOnAndOffProcessor());

        return result;
    }
}
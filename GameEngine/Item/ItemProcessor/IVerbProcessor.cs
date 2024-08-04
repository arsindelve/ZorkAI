using Model.AIGeneration;
using Model.Interface;

namespace GameEngine.Item.ItemProcessor;

public interface IVerbProcessor
{
    InteractionResult? Process(SimpleIntent action, IContext context, IInteractionTarget item,
        IGenerationClient client);
}
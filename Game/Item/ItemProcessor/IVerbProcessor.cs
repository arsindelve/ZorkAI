using Model.AIGeneration;

namespace Game.Item.ItemProcessor;

public interface IVerbProcessor
{
    InteractionResult? Process(SimpleIntent action, IContext context, IInteractionTarget item,
        IGenerationClient client);
}
using Model.AIGeneration;
using Model.Intent;
using Model.Interaction;

namespace Model.Interface;

public interface IVerbProcessor
{
    Task<InteractionResult?> Process(SimpleIntent action, IContext context, IInteractionTarget item,
        IGenerationClient client);
}
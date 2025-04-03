using GameEngine.Item.ItemProcessor;
using Model.Interface;
using Model.Item;

namespace GameEngine.Item.MultiItemProcessor;

public class LightProcessor : IMultiNounVerbProcessor
{
    public InteractionResult? Process(MultiNounIntent action, IContext context, IInteractionTarget itemOne,
        IInteractionTarget itemTwo)
    {
        return null;
    }
}

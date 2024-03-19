namespace Game.Item.ItemProcessor;

public interface IMultiNounVerbProcessor
{
    InteractionResult? Process(MultiNounIntent action, IContext context, IInteractionTarget itemOne, IInteractionTarget itemTwo);
}
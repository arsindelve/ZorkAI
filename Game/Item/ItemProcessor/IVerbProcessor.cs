namespace Game.Item.ItemProcessor;

public interface IVerbProcessor
{
    InteractionResult? Process(SimpleIntent action, IContext context, IInteractionTarget item);
}

public interface IMultiNounVerbProcessor
{
    InteractionResult? Process(MultiNounIntent action, IContext context, IInteractionTarget itemOne, IInteractionTarget itemTwo);
}
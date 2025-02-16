namespace Model.Interface;

public interface IItemProcessorFactory
{
    List<IVerbProcessor> GetProcessors(object item);
}
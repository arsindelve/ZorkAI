using GameEngine;
using Model.AIGeneration;
using Model.Interface;
using Model.Item;
using Model.Location;

namespace UnitTests;

public class EngineTestsBaseCommon<TContext> where TContext : class, IContext, new()
{
    protected TContext Context { get; set; } = Mock.Of<TContext>();
    
    protected Mock<IGenerationClient> Client = new();
    protected IIntentParser Parser = Mock.Of<IIntentParser>();
    
    
    protected T GetItem<T>() where T : IItem, new()
    {
        return Repository.GetItem<T>();
    }

    protected T GetLocation<T>() where T : class, ILocation, new()
    {
        return Repository.GetLocation<T>();
    }

    protected T StartHere<T>() where T : class, ILocation, new()
    {
        Context.CurrentLocation = GetLocation<T>();
        return GetLocation<T>();
    }
    
    protected T Take<T>() where T : IItem, new()
    {
        var item = GetItem<T>();
        Context.ItemPlacedHere(item);
        item.CurrentLocation = Context;
        return item;
    }

}
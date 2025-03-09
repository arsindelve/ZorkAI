namespace Planetfall.Location.Shuttle;

public interface IShuttleControl : ILocation
{
    InteractionResult Activate(IContext context);
}
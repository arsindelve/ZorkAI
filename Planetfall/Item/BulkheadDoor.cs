using GameEngine.Item;
using Model.Interface;
using Model.Item;

namespace Planetfall.Item;

public class BulkheadDoor : ItemBase, IOpenAndClose
{
    public override string[] NounsForMatching => ["bulkhead", "bulkhead door"];
    public bool IsOpen { get; set; }
    public string NowOpen => throw new NotImplementedException();
    public string NowClosed => throw new NotImplementedException();
    public string AlreadyClosed => "It is closed! ";
    public string AlreadyOpen => "It's already open! ";
    public bool HasEverBeenOpened { get; set; }

    public string CannotBeOpenedDescription(IContext context)
    {
        return "Why open the door to the emergency escape pod if there's no emergency?";
    }
}
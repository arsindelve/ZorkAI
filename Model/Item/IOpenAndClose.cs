namespace Model.Item;

/// <summary>
/// Represents an object that can be opened and closed like a window or a mailbox. 
/// </summary>
public interface IOpenAndClose : IInteractionTarget
{
    bool IsOpen { get; set; }
    
    string NowOpen { get; }
    
    string NowClosed { get; }
    
    string AlreadyOpen { get; }
    
    string AlreadyClosed { get; }
    
    bool HasEverBeenOpened { get; set; }
}
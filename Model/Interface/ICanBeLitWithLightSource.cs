namespace Model.Interface;

public interface ICanBeLitWithLightSource
{
    bool IsOn { get; set; }
    
    string NowOnText { get; }
    
    string AlreadyOnText { get; }
    
    string CannotBeTurnedOnText { get; }
    
    string OnBeingTurnedOn(IContext context);
}

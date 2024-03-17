namespace Model;

public interface IInfocomGame
{
    Type StartingLocation { get; }

    string StartText { get; }
    
    string DefaultSaveGameName { get; }
}
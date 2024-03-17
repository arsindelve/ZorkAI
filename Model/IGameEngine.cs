namespace Model;

public interface IGameEngine 
{
    void RestoreGame(string data);
    
    string SaveGame();
}
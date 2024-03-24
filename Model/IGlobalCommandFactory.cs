namespace Model;

public interface IGlobalCommandFactory
{
    IGlobalCommand? GetGlobalCommands(string input);
}
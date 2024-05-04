namespace Model.Interface;

public interface IGlobalCommandFactory
{
    IGlobalCommand? GetGlobalCommands(string? input);
}
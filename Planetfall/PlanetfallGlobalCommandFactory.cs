namespace Planetfall;

public class PlanetfallGlobalCommandFactory : IGlobalCommandFactory
{
    public IGlobalCommand? GetGlobalCommands(string? input)
    {
        return null;
    }

    public ISystemCommand? GetSystemCommands(string? input)
    {
        return null;
    }
}
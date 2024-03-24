namespace ZorkOne;

public class ZorkIContext : Context<ZorkI>
{
    public int LightWoundCounter;
    public int DeathCounter;

    public override string? ProcessTurnCounter()
    {
        if (LightWoundCounter > 0)
            LightWoundCounter--;

        return null;
    }
}
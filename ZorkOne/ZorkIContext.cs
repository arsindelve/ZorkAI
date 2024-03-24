using ZorkOne.Interface;

namespace ZorkOne;

public class ZorkIContext : Context<ZorkI>
{
    public int LightWoundCounter;
    public int DeathCounter;

    public bool HasWeapon => Items.OfType<IWeapon>().Any();

    public IItem? GetWeapon()
    {
        return Items.OfType<IWeapon>().Cast<IItem>().FirstOrDefault();
    }

    public override string? ProcessTurnCounter()
    {
        if (LightWoundCounter > 0)
            LightWoundCounter--;

        return null;
    }
}
using ZorkOne.Interface;

namespace ZorkOne;

public class ZorkIContext : Context<ZorkI>
{
    public int DeathCounter;
    public int LightWoundCounter;

    public bool HasWeapon => Items.OfType<IWeapon>().Any();

    public bool IsStunned { get; set; }

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
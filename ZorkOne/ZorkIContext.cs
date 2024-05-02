namespace ZorkOne;

/// <summary>
///     This extends the context with properties that are specific to Zork, and would not apply to other
///     Infocom games. Other Infocom games might not have combat and wounds, for example.
/// </summary>
public class ZorkIContext : Context<ZorkI>
{
    /// <summary>
    ///     Gets or sets the Death Counter of the game.
    /// </summary>
    /// <remarks>
    ///     The Death Counter keeps track of the number of times the player character has died.
    /// </remarks>
    public int DeathCounter { get; set; }

    /// <summary>
    ///     The player can only have one light wound. A second will kill them. This counter tracks how much
    ///     longer until they heal. At zero, they are healed and the wound is gone.
    /// </summary>
    public int LightWoundCounter { get; set; }

    /// <summary>
    ///     Gets a value indicating whether the player has a weapon.
    /// </summary>
    /// <remarks>
    ///     The HasWeapon property returns true if the player has a weapon in the ZorkIContext.
    ///     It checks if the GetWeapon() method returns a non-null value, indicating the
    ///     presence of a weapon.
    /// </remarks>
    /// <returns>
    ///     Returns true if the player has a weapon; otherwise, false.
    /// </returns>
    public bool HasWeapon => GetWeapon() != null;

    /// <summary>
    ///     When stunned, their next attack in combat is ineffective.
    /// </summary>
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


    /// <summary>
    ///     Some items in Zork1 give points for picking them up.
    /// </summary>
    /// <param name="item"></param>
    public override void Take(IItem? item)
    {
        switch (item)
        {
            case null:
                return;
            case IGivePointsWhenFirstPickedUp points when !item.HasEverBeenPickedUp:
                AddPoints(points.NumberOfPoints);
                break;
        }

        base.Take(item);
    }
}
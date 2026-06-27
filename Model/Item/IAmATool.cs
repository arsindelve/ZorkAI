namespace Model.Item;

/// <summary>
///     Marker interface for items that count as a "tool" — the analog of the original Zork I
///     <c>TOOLBIT</c> flag. Used (alongside <see cref="IWeapon" />, the <c>WEAPONBIT</c> analog) to
///     decide what can force the jewel-encrusted egg open, clumsily damaging it.
/// </summary>
public interface IAmATool;

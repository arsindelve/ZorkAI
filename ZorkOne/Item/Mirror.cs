using GameEngine.Item;

namespace ZorkOne.Item;

public class Mirror : ItemBase
{
    public override string[] NounsForMatching => ["mirror"];

    public override string CannotBeTakenDescription => "The mirror is many times your size. Give up. ";

    /// <summary>
    ///     Shown when examining or touching the mirror once it has been broken.
    /// </summary>
    public const string BrokenDescription = "The mirror is broken into many pieces. ";

    /// <summary>
    ///     Once the mirror is broken, the touch-to-teleport between the two mirror rooms is permanently
    ///     disabled and the room description notes the destruction. A single Mirror instance is shared by
    ///     both mirror rooms, so breaking it in one room disables the teleport everywhere.
    /// </summary>
    public bool IsBroken { get; set; }
}

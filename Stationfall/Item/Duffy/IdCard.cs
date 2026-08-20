namespace Stationfall.Item.Duffy;

/// <summary>
///     The player's Stellar Patrol identification card, carried in the uniform pocket. Its rank governs
///     the station's security doors much later in the game (ship.zil:104-114); aboard the Duffy it is
///     simply a readable keepsake.
/// </summary>
public class IdCard : ItemBase, ICanBeTakenAndDropped, ICanBeExamined, ICanBeRead
{
    /// <summary>
    ///     Security clearance. The player starts as a Lieutenant First Class (rank 6); the station's
    ///     Brig and Armory doors want rank 7 or better, which is a later-phase puzzle.
    /// </summary>
    [UsedImplicitly]
    public int Rank { get; set; } = 6;

    /// <summary>
    ///     Set if the card's magnetic data is ruined. A later-phase trap: removing the magnetic boots
    ///     while carrying the card scrambles it permanently.
    /// </summary>
    [UsedImplicitly]
    public bool IsScrambled { get; set; }

    public override string[] NounsForMatching => ["id card", "card", "identification card", "patrol id", "id"];

    public override int Size => 2;

    public string ExaminationDescription => ReadDescription;

    public string ReadDescription =>
        IsScrambled
            ? "The card's magnetic strip has been thoroughly scrambled. The printing still reads " +
              "\"Stellar Patrol, Paperwork Task Force,\" but no reader will ever believe it again. "
            : "\"Stellar Patrol -- Paperwork Task Force. ID Number: 1451-352-716.\" ";

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "An ID card is lying here. ";
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "An ID card";
    }
}

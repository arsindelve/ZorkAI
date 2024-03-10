namespace Model;

/// <summary>
///     Represents a dark location in the game, where interactions may be different when the
///     adventurer is not carrying an active light source.
/// </summary>
public interface IDarkLocation
{
    /// <summary>
    ///     Gets the description of the location when it is dark and there is no light source present.
    /// </summary>
    /// <remarks>
    ///     This property is used to provide a different description of the location when it is dark and the player does not
    ///     have a light source.
    ///     If the location is dark and the player does not have a light source, this property's value is returned instead of
    ///     the regular description.
    ///     If the location is not dark or the player has a light source, the regular description of the location is returned
    ///     instead.
    /// </remarks>
    string DarkDescription { get; }
}
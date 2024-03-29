namespace Model.Location;

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

    /// <summary>
    /// Indicates whether the location is no longer dark.
    /// </summary>
    /// <remarks>
    /// In the game, a location is considered dark if the adventurer is not carrying
    /// an active light source. When the location is no longer dark, it means that
    /// the adventurer has activated a light source in the location, making
    /// it no longer require him/her to carry a light source.
    /// </remarks>
    /// <value>
    /// <c>true</c> if the location is no longer dark; otherwise, <c>false</c>.
    /// </value>
    bool IsNoLongerDark { get; set; }
}
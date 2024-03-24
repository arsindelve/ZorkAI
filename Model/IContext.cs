using Model.Intent;
using Model.Interaction;
using Model.Item;
using OpenAI;

namespace Model;

public interface IContext : ICanHoldItems
{
    /// <summary>
    ///     Represents adventurer's current score.
    /// </summary>
    int Score { get; }

    IGameEngine? Engine { get; set; }

    /// <summary>
    ///     Gets or sets the last noun in the game context.
    /// </summary>
    /// <remarks>
    ///     This property represents the last mentioned noun in the game context.
    ///     It is commonly used to refer to the previous mentioned object.
    /// </remarks>
    string LastNoun { get; set; }

    /// <summary>
    ///     Gets or sets the current location in the game context.
    /// </summary>
    /// <remarks>
    ///     The current location represents the player's current position
    ///     in the game world.
    /// </remarks>
    ILocation CurrentLocation { get; set; }

    /// <summary>
    ///     Property that indicates whether the context has a light source.
    /// </summary>
    /// <returns>True if the context has a light source, false otherwise.</returns>
    /// <remarks>
    ///     This property checks if the context has any items that implement the <see cref="Model.Item.IAmALightSource" />
    ///     interface and are turned on.
    /// </remarks>
    bool HasLightSource { get; }

    /// <summary>
    ///     Gets the number of moves made by the adventurer.
    /// </summary>
    int Moves { get; }

    List<IItem> Items { get; }

    /// <summary>
    ///     A reference to the "game", which can tell us constant, game specific
    ///     things like how to calculate score, starting location, etc.
    /// </summary>
    IInfocomGame Game { get; set; }

    /// <summary>
    ///     Gets or sets the name of the last saved game.
    /// </summary>
    /// <value>The name of the last saved game.</value>
    string? LastSaveGameName { get; set; }

    void Take(IItem item);

    void Drop(IItem item);

    /// <summary>
    ///     Adds the specified number of points to the score.
    /// </summary>
    /// <param name="points">The number of points to add.</param>
    /// <returns>The updated score after adding the points.</returns>
    int AddPoints(int points);

    InteractionResult RespondToSimpleInteraction(SimpleIntent simpleInteraction, IGenerationClient client);

    string ItemListDescription(string locationName);

    int IncreaseMoves();
}
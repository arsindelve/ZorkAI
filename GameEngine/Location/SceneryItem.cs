namespace GameEngine.Location;

/// <summary>
///     A lightweight, inert piece of room scenery: a noun the room's prose mentions but which has
///     no backing game object (e.g. the numeric keyboard in the Miniaturization Booth). Registering
///     it via <see cref="LocationBase.Scenery" /> lets the player <c>examine</c> the thing — and be
///     told, in-world, why it can't be taken — instead of the AI narrator falsely insisting it isn't
///     here (issue #315).
/// </summary>
/// <remarks>
///     Scenery is pure data: it never holds mutable state, so it is not serialized and is re-declared
///     by the location on every load. It is matched <em>after</em> the room's real items, so a genuine
///     object that shares a noun always shadows the scenery.
/// </remarks>
/// <param name="Nouns">The nouns (and adjective+noun combinations) a player might type to reference it.</param>
/// <param name="ExaminationDescription">Terse, original prose shown when the player examines it.</param>
/// <param name="CannotBeTakenReason">
///     In-world reason shown when the player tries to take it. When null, a generic refusal is used.
/// </param>
public sealed record SceneryItem(
    string[] Nouns,
    string ExaminationDescription,
    string? CannotBeTakenReason = null);

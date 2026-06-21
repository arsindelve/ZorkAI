namespace Model.AIGeneration.Requests;

public abstract class Request
{
    /// <summary>
    /// Shared anti-hallucination guard for "negative"/deflection prompts (object-not-present,
    /// verb-has-no-effect, command-has-no-effect, can't-go-that-way). The narrator must stay
    /// inside the room state and not substitute or invent things that aren't there.
    /// </summary>
    protected const string NoInventionGuard =
        "Do not invent, name, substitute, or describe any object, item, scenery, exit, or character " +
        "that is not already explicitly present in the location description above. Refer only to things " +
        "actually present, or keep it generic. Do not alter the state of the game or add new information.";

    /// <summary>
    /// Low creativity for deflection responses, so they don't embellish with invented detail.
    /// Normal narration keeps the higher default (<see cref="Temperature"/>).
    /// </summary>
    protected const float DeflectionTemperature = 0.4f;

    public virtual string? UserMessage { get; protected init; }

    public float Temperature { get; set; } = 0.8f;
}

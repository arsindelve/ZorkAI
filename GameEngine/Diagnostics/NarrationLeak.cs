namespace GameEngine.Diagnostics;

/// <summary>
///     One occasion on which the game had no authored answer and asked the narrator to invent one.
/// </summary>
/// <param name="Input">What the player typed.</param>
/// <param name="Location">Where they were standing.</param>
/// <param name="RequestType">Which kind of fallback fired — the most useful thing to group by.</param>
/// <param name="Prompt">The prompt handed to the narrator, which names the noun and verb involved.</param>
public readonly record struct NarrationLeak(string Input, string Location, string RequestType, string? Prompt);

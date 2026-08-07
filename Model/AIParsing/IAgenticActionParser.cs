namespace Model.AIParsing;

/// <summary>
/// The state mutators the agentic fall-through narrator is allowed to invoke (issue #136).
/// Deliberately tiny: acquisition is a primary intent (never a side effect), and other side
/// effects (extinguish, empty, deflate...) are deferred to later tickets.
/// </summary>
public enum AgenticTool
{
    /// <summary>The item leaves inventory and lands in the current room ("throw the leaflet in the air").</summary>
    Drop,

    /// <summary>The item is removed from the game entirely ("tear up the leaflet", "throw the sword into the chasm").</summary>
    Destroy
}

/// <summary>
/// A single state mutation the narrator decided on. <paramref name="TargetNoun"/> is resolved by the
/// engine against the player's actual inventory; a target that isn't really held is ignored.
/// </summary>
public record AgenticToolCall(AgenticTool Tool, string TargetNoun);

/// <summary>
/// What the agentic narrator came back with: the text to show the player, plus zero or more tool
/// calls that make the world state match what the narration just said. An empty tool list means the
/// narrator was not certain the action is plausible and grounded - pure flavor, no state change
/// (exactly the pre-#136 behavior).
/// </summary>
public record AgenticActionResult(string Narration, IReadOnlyList<AgenticToolCall> ToolCalls);

/// <summary>
/// The "narrator with hands" seam (issue #136). When a command matched no game mechanic and the
/// acted-upon noun is an item the player is carrying, the engines consult this parser instead of the
/// plain flavor-text generator. It decides whether the action plausibly disposes of the item -
/// grounded strictly in the live location description and inventory - and returns narration plus the
/// tool calls (drop/destroy) that make the world match. Uncertain, implausible, or referencing
/// something not present must yield an empty tool list and a snarky deflection.
/// </summary>
public interface IAgenticActionParser
{
    /// <summary>
    /// Resolves an unhandled player action against the live game state.
    /// </summary>
    /// <param name="playerInput">The player's raw input, e.g. "throw the sword into the chasm".</param>
    /// <param name="inventoryDescription">The player's current inventory listing, so the narrator can confirm the target is held.</param>
    /// <param name="locationDescription">The live room description, REQUIRED so destinations (river/chasm/lava) can be grounded - a destination not present here must never justify a tool.</param>
    Task<AgenticActionResult> Resolve(string playerInput, string inventoryDescription, string locationDescription);
}

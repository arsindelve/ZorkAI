namespace Model.Interaction;

/// <summary>
///     A serializable snapshot of an in-flight "which one do you mean?" disambiguation prompt, stored on
///     the <see cref="Model.Interface.IContext" /> so it survives the stateless per-request save/restore
///     boundary in the deployed game (issue #472).
/// </summary>
/// <remarks>
///     The live prompt is held by an in-memory engine field (the disambiguation
///     <c>IStatefulProcessor</c>). That field is NOT part of the serialized session, so in production —
///     where the engine is rebuilt per request and the full game state travels in the base64 session —
///     the pending prompt was lost between the turn that asked the question and the turn that carried the
///     answer, and the answer was mis-parsed as a brand-new command. Persisting this lightweight
///     descriptor on the Context (which already round-trips inventory/score/flags) lets the engine
///     rebuild the pending processor on restore and route the next input to it.
///
///     Only the two fields the processor needs to resolve an answer are captured — the map of accepted
///     replies to their fully-qualified nouns, and the command template to fill in. The human-readable
///     prompt text is not stored: it is only shown when the question is first asked, never when it is
///     answered.
/// </remarks>
public class PendingDisambiguation
{
    /// <summary>
    ///     Mirrors <see cref="DisambiguationInteractionResult.PossibleResponses" />: the accepted replies
    ///     (keys, matched by substring) mapped to the fully-qualified noun each one resolves to.
    /// </summary>
    public Dictionary<string, string> PossibleResponses { get; set; } = new();

    /// <summary>
    ///     Mirrors <see cref="DisambiguationInteractionResult.ReplacementString" />: the command template
    ///     (e.g. <c>"drop {0}"</c>) the chosen noun is substituted into to form the resolved command.
    /// </summary>
    public string ReplacementString { get; set; } = "";
}

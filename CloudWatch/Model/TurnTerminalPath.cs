namespace CloudWatch.Model;

/// <summary>
///     Which branch of turn processing produced the response the player saw (issue #578).
///     <para>
///     Every no-handler deflection in the engine already narrates its own situation-specific
///     failure, so the distinction is visible to a <em>reader</em> of the turn's prose — but it was
///     invisible to a <em>tool</em> reading the logs, which is what forced the fragile
///     "infer a dropped turn from the literal no-effect sentence" trick in issue #538. Recording the
///     branch on every <see cref="TurnLog" /> gives the harness a first-class signal that survives
///     the narrator being enabled and names which of the twelve deflections fired.
///     </para>
///     <para>
///     Values are explicit because this is a wire contract: the Lambda writes it, the harness reads
///     it. Serialized by name (see <see cref="TurnLog.TerminalPath" />), so renaming a member is the
///     breaking change, not renumbering one.
///     </para>
/// </summary>
public enum TurnTerminalPath
{
    /// <summary>
    ///     Not classified. The engine sets an explicit value on every turn, so this should only ever
    ///     appear on a <see cref="TurnLog" /> built outside the engine (e.g. in a test).
    /// </summary>
    Unknown = 0,

    /// <summary>
    ///     A handler ran: a processor, an engine, a location or item interaction, a global or system
    ///     command, a conversation. The turn did something rather than deflecting.
    /// </summary>
    Handled = 1,

    /// <summary>The parser produced no usable shape at all — <c>NullIntent</c>.</summary>
    NullIntent = 2,

    /// <summary>The parsed intent was a type the engine's dispatch switch does not handle.</summary>
    UnmatchedIntentType = 3,

    /// <summary>
    ///     The turn threw and was rescued by the engine-error safety net (issue #271).
    /// </summary>
    EngineError = 4,

    /// <summary>The noun was present in the LOCATION, but no processor handled the verb.</summary>
    NoMatchingVerbForNounInLocation = 10,

    /// <summary>The noun was present in INVENTORY, but no processor handled the verb.</summary>
    NoMatchingVerbForNounInInventory = 11,

    /// <summary>The noun exists in the story, but is not present here.</summary>
    NounNotPresent = 12,

    /// <summary>The noun does not exist anywhere in the story.</summary>
    NounNotInTheStory = 13,

    /// <summary>Neither noun of a multi-noun command exists anywhere in the story.</summary>
    MultiNounNeitherNounInTheStory = 20,

    /// <summary>The first noun is not here, and the second one is a named person.</summary>
    MultiNounFirstNounMissingSecondIsAPerson = 21,

    /// <summary>The first noun is not here.</summary>
    MultiNounFirstNounMissing = 22,

    /// <summary>The second noun is not here, and the first one is a named person.</summary>
    MultiNounSecondNounMissingFirstIsAPerson = 23,

    /// <summary>The second noun is not here.</summary>
    MultiNounSecondNounMissing = 24,

    /// <summary>Neither noun of a multi-noun command is here.</summary>
    MultiNounBothNounsMissing = 25,

    /// <summary>
    ///     One of the two nouns is only scenery in the location description, not a real item, so no
    ///     interaction between them is possible.
    /// </summary>
    MultiNounNounIsSceneryOnly = 26,

    /// <summary>Both items are real and present, but no multi-noun processor handled the pair.</summary>
    MultiNounNoProcessorMatched = 27
}

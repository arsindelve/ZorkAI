namespace Model.Intent;

/// <summary>
///     The player typed more than one command on a single line without separating them with
///     periods (e.g. "look examine bulkhead open bulkhead"). The AI parser reveals this by
///     emitting more than one verb (or intent) tag. We do not try to execute these; instead we
///     ask the player — in the narrator's voice — to enter one command at a time, or to separate
///     multiple commands with periods.
/// </summary>
public record MultipleCommandsIntent : IntentBase;

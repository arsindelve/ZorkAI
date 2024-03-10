namespace Model.Intent;

/// <summary>
///     The parser has indicated that it needs more information to complete the intent, so we will
///     prompt the user for the additional information and send the response back to the parser
///     for clarification
/// </summary>
public record PromptIntent : IntentBase;
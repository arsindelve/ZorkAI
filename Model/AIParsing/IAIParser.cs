using Model.Intent;

namespace Model.AIParsing;

/// <summary>
/// Represents an interface for all AI parser implementations used to parse input and generate intents.
/// </summary>
public interface IAIParser
{
    /// <summary>
    /// Gets the language model information used by the AI parser implementation.
    /// </summary>
    /// <remarks>
    /// The <c>LanguageModel</c> property provides a string representing the specific
    /// language model employed by the parser. This can be used to log or identify
    /// the model used in processing input or generating intents.
    /// </remarks>
    string LanguageModel { get; }

    /// <summary>
    /// Asynchronously sends input to the AI parser to determine the user's intent based on their input, location description, and session context.
    /// </summary>
    /// <param name="input">The user's input to be processed by the AI parser.</param>
    /// <param name="locationDescription">A description of the current location context where the input is being given.</param>
    /// <param name="sessionId">A unique identifier for the session within which this input is being processed.</param>
    /// <returns>A task that represents the asynchronous parsing operation. The task result contains an <see cref="IntentBase"/> object describing the determined intent based on the input.</returns>
    Task<IntentBase> AskTheAIParser(string input, string locationDescription, string sessionId);
}
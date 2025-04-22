namespace Model.Interface;

public interface ISessionRepository
{
    /// <summary>
    /// Retrieves the state data of a session from the specified table based on the session ID.
    /// </summary>
    /// <param name="sessionId">The unique identifier of the session.</param>
    /// <param name="tableName">The name of the table where session data is stored.</param>
    /// <returns>A Task representing the asynchronous operation. The task result contains the session state as a string, or null if the session does not exist.</returns>
    Task<string?> GetSessionState(string sessionId, string tableName);

    /// <summary>
    /// Writes the session state information into the specified table.
    /// </summary>
    /// <param name="sessionId">The unique identifier of the session.</param>
    /// <param name="gameData">The serialized game data to be stored.</param>
    /// <param name="tableName">The name of the table where the session state will be stored.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task WriteSessionState(string sessionId, string gameData, string tableName);

    /// <summary>
    /// Writes a session step to the specified table with the provided data.
    /// </summary>
    /// <param name="sessionId">The unique identifier of the session.</param>
    /// <param name="turnIndex">The index of the current turn in the session.</param>
    /// <param name="input">The input data for the session step.</param>
    /// <param name="output">The output data for the session step.</param>
    /// <param name="tableName">The name of the table where the session step will be stored.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task WriteSessionStep(string sessionId, long turnIndex, string input, string output, string tableName);

    /// <summary>
    /// Retrieves the session steps as a single concatenated text, ordered chronologically.
    /// </summary>
    /// <param name="sessionId">The unique identifier of the session.</param>
    /// <param name="tableName">The name of the table where session steps are stored.</param>
    /// <returns>A Task representing the asynchronous operation. The task result contains the concatenated session steps as a string.</returns>
    Task<string> GetSessionStepsAsText(string sessionId, string tableName);
}
namespace Model.Interface;

/// <summary>
/// Service for managing user preferences across different platforms and storage mechanisms
/// </summary>
public interface IUserPreferencesService
{
    /// <summary>
    /// Gets the current user preferences, loading defaults if none exist
    /// </summary>
    Task<UserPreferences> GetPreferencesAsync(string? userId = null);

    /// <summary>
    /// Saves user preferences
    /// </summary>
    Task SavePreferencesAsync(UserPreferences preferences, string? userId = null);

    /// <summary>
    /// Resets preferences to defaults
    /// </summary>
    Task ResetToDefaultsAsync(string? userId = null);

    /// <summary>
    /// Exports preferences as JSON string for backup/transfer
    /// </summary>
    Task<string> ExportPreferencesAsync(string? userId = null);

    /// <summary>
    /// Imports preferences from JSON string
    /// </summary>
    Task ImportPreferencesAsync(string preferencesJson, string? userId = null);
}
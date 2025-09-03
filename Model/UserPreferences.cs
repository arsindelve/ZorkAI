using Newtonsoft.Json;

namespace Model;

/// <summary>
/// Represents user preferences that can be configured and persisted across game sessions.
/// These preferences affect the game experience but are independent of game state.
/// </summary>
public class UserPreferences
{
    /// <summary>
    /// Controls how detailed room descriptions are shown
    /// </summary>
    public Verbosity Verbosity { get; set; } = Verbosity.Brief;

    /// <summary>
    /// Whether to enable AI-enhanced command parsing for more natural language understanding
    /// </summary>
    public bool EnableAiParsing { get; set; } = true;

    /// <summary>
    /// Whether to enable AI-generated narrative responses for failed actions
    /// </summary>
    public bool EnableAiGeneration { get; set; } = true;

    /// <summary>
    /// Preferred font size for web clients (in pixels)
    /// </summary>
    public int FontSize { get; set; } = 16;

    /// <summary>
    /// Color theme preference for web clients
    /// </summary>
    public string Theme { get; set; } = "dark";

    /// <summary>
    /// Whether to show a compass/navigation aid in web clients
    /// </summary>
    public bool ShowCompass { get; set; } = true;

    /// <summary>
    /// Whether to show inventory items in the UI sidebar
    /// </summary>
    public bool ShowInventoryPanel { get; set; } = true;

    /// <summary>
    /// Whether to auto-save game state periodically
    /// </summary>
    public bool AutoSave { get; set; } = false;

    /// <summary>
    /// Auto-save interval in minutes (when auto-save is enabled)
    /// </summary>
    public int AutoSaveIntervalMinutes { get; set; } = 10;

    /// <summary>
    /// Maximum number of command history entries to remember
    /// </summary>
    public int MaxCommandHistory { get; set; } = 50;

    /// <summary>
    /// Whether to enable sound effects (for future enhancement)
    /// </summary>
    public bool EnableSounds { get; set; } = false;

    /// <summary>
    /// Sound volume (0-100)
    /// </summary>
    public int SoundVolume { get; set; } = 50;

    /// <summary>
    /// Creates a deep copy of the preferences
    /// </summary>
    public UserPreferences Clone()
    {
        return JsonConvert.DeserializeObject<UserPreferences>(JsonConvert.SerializeObject(this))!;
    }

    /// <summary>
    /// Creates default preferences
    /// </summary>
    public static UserPreferences Default => new();
}
namespace Model.Web;

public record SaveUserPreferencesRequest(UserPreferences Preferences, string? UserId = null);

public record ResetUserPreferencesRequest(string? UserId = null);

public record ImportUserPreferencesRequest(string PreferencesJson, string? UserId = null);
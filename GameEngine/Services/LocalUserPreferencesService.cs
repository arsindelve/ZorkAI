using Model;
using Model.Interface;
using Newtonsoft.Json;

namespace GameEngine.Services;

/// <summary>
/// Local file-based implementation of user preferences service for console applications
/// </summary>
public class LocalUserPreferencesService : IUserPreferencesService
{
    private readonly string _preferencesFilePath;
    private UserPreferences? _cachedPreferences;

    public LocalUserPreferencesService(string? customPath = null)
    {
        _preferencesFilePath = customPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ZorkAI",
            "preferences.json"
        );
    }

    public async Task<UserPreferences> GetPreferencesAsync(string? userId = null)
    {
        if (_cachedPreferences != null)
            return _cachedPreferences;

        try
        {
            if (File.Exists(_preferencesFilePath))
            {
                var json = await File.ReadAllTextAsync(_preferencesFilePath);
                _cachedPreferences = JsonConvert.DeserializeObject<UserPreferences>(json) ?? UserPreferences.Default;
            }
            else
            {
                _cachedPreferences = UserPreferences.Default;
            }
        }
        catch (Exception)
        {
            // If there's any error loading preferences, use defaults
            _cachedPreferences = UserPreferences.Default;
        }

        return _cachedPreferences;
    }

    public async Task SavePreferencesAsync(UserPreferences preferences, string? userId = null)
    {
        try
        {
            // Ensure directory exists
            var directory = Path.GetDirectoryName(_preferencesFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonConvert.SerializeObject(preferences, Formatting.Indented);
            await File.WriteAllTextAsync(_preferencesFilePath, json);
            _cachedPreferences = preferences.Clone();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save preferences: {ex.Message}", ex);
        }
    }

    public async Task ResetToDefaultsAsync(string? userId = null)
    {
        await SavePreferencesAsync(UserPreferences.Default, userId);
    }

    public async Task<string> ExportPreferencesAsync(string? userId = null)
    {
        var preferences = await GetPreferencesAsync(userId);
        return JsonConvert.SerializeObject(preferences, Formatting.Indented);
    }

    public async Task ImportPreferencesAsync(string preferencesJson, string? userId = null)
    {
        try
        {
            var preferences = JsonConvert.DeserializeObject<UserPreferences>(preferencesJson);
            if (preferences == null)
                throw new InvalidOperationException("Invalid preferences JSON format");

            await SavePreferencesAsync(preferences, userId);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse preferences JSON: {ex.Message}", ex);
        }
    }
}
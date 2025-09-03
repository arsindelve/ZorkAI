using GameEngine.Services;
using Model;
using NUnit.Framework;

namespace UnitTests;

[TestFixture]
public class LocalUserPreferencesServiceTests
{
    private string _tempPreferencesPath;
    private LocalUserPreferencesService _service;

    [SetUp]
    public void Setup()
    {
        _tempPreferencesPath = Path.Combine(Path.GetTempPath(), "test_preferences.json");
        _service = new LocalUserPreferencesService(_tempPreferencesPath);
        
        // Clean up any existing test file
        if (File.Exists(_tempPreferencesPath))
        {
            File.Delete(_tempPreferencesPath);
        }
    }

    [TearDown]
    public void Cleanup()
    {
        if (File.Exists(_tempPreferencesPath))
        {
            File.Delete(_tempPreferencesPath);
        }
    }

    [Test]
    public async Task GetPreferencesAsync_WhenNoFileExists_ShouldReturnDefaults()
    {
        var preferences = await _service.GetPreferencesAsync();
        
        Assert.That(preferences.Verbosity, Is.EqualTo(Verbosity.Brief));
        Assert.That(preferences.EnableAiParsing, Is.True);
        Assert.That(preferences.FontSize, Is.EqualTo(16));
    }

    [Test]
    public async Task SavePreferencesAsync_ShouldCreateFileAndPersist()
    {
        var customPreferences = new UserPreferences
        {
            Verbosity = Verbosity.Verbose,
            EnableAiParsing = false,
            FontSize = 20,
            Theme = "light"
        };

        await _service.SavePreferencesAsync(customPreferences);
        
        Assert.That(File.Exists(_tempPreferencesPath), Is.True);
        
        var loadedPreferences = await _service.GetPreferencesAsync();
        Assert.That(loadedPreferences.Verbosity, Is.EqualTo(Verbosity.Verbose));
        Assert.That(loadedPreferences.EnableAiParsing, Is.False);
        Assert.That(loadedPreferences.FontSize, Is.EqualTo(20));
        Assert.That(loadedPreferences.Theme, Is.EqualTo("light"));
    }

    [Test]
    public async Task ResetToDefaultsAsync_ShouldOverwriteWithDefaults()
    {
        // First save custom preferences
        var customPreferences = new UserPreferences
        {
            Verbosity = Verbosity.SuperBrief,
            EnableAiParsing = false,
            FontSize = 24
        };
        await _service.SavePreferencesAsync(customPreferences);

        // Reset to defaults
        await _service.ResetToDefaultsAsync();

        // Verify defaults are restored
        var preferences = await _service.GetPreferencesAsync();
        Assert.That(preferences.Verbosity, Is.EqualTo(Verbosity.Brief));
        Assert.That(preferences.EnableAiParsing, Is.True);
        Assert.That(preferences.FontSize, Is.EqualTo(16));
    }

    [Test]
    public async Task ExportPreferencesAsync_ShouldReturnJsonString()
    {
        var customPreferences = new UserPreferences
        {
            Verbosity = Verbosity.Verbose,
            FontSize = 18
        };
        await _service.SavePreferencesAsync(customPreferences);

        var json = await _service.ExportPreferencesAsync();
        
        Assert.That(json, Is.Not.Null);
        Assert.That(json, Does.Contain("\"Verbosity\": 2"));
        Assert.That(json, Does.Contain("\"FontSize\": 18"));
    }

    [Test]
    public async Task ImportPreferencesAsync_ShouldLoadFromJson()
    {
        var json = @"{
            ""Verbosity"": 0,
            ""EnableAiParsing"": false,
            ""FontSize"": 22,
            ""Theme"": ""light""
        }";

        await _service.ImportPreferencesAsync(json);

        var preferences = await _service.GetPreferencesAsync();
        Assert.That(preferences.Verbosity, Is.EqualTo(Verbosity.SuperBrief));
        Assert.That(preferences.EnableAiParsing, Is.False);
        Assert.That(preferences.FontSize, Is.EqualTo(22));
        Assert.That(preferences.Theme, Is.EqualTo("light"));
    }

    [Test]
    public async Task ImportPreferencesAsync_WithInvalidJson_ShouldThrowException()
    {
        var invalidJson = "{ invalid json }";

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await _service.ImportPreferencesAsync(invalidJson);
        });
    }

    [Test]
    public async Task GetPreferencesAsync_WithCorruptedFile_ShouldReturnDefaults()
    {
        // Write corrupted JSON to the preferences file
        await File.WriteAllTextAsync(_tempPreferencesPath, "{ corrupted json }");

        var preferences = await _service.GetPreferencesAsync();
        
        // Should return defaults when file is corrupted
        Assert.That(preferences.Verbosity, Is.EqualTo(Verbosity.Brief));
        Assert.That(preferences.EnableAiParsing, Is.True);
    }
}
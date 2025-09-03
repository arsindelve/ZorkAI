using GameEngine;
using Model;
using NUnit.Framework;
using ZorkOne;

namespace UnitTests;

[TestFixture]
public class ContextUserPreferencesTests
{
    private TestableContext _context;

    [SetUp]
    public void Setup()
    {
        _context = new TestableContext();
    }

    [Test]
    public void Constructor_ShouldInitializeWithDefaultPreferences()
    {
        Assert.That(_context.UserPreferences, Is.Not.Null);
        Assert.That(_context.UserPreferences.Verbosity, Is.EqualTo(Verbosity.Brief));
        Assert.That(_context.UserPreferences.EnableAiParsing, Is.True);
        Assert.That(_context.UserPreferences.FontSize, Is.EqualTo(16));
    }

    [Test]
    public void Verbosity_PropertyShouldMirrorUserPreferencesVerbosity()
    {
        _context.UserPreferences.Verbosity = Verbosity.Verbose;
        Assert.That(_context.Verbosity, Is.EqualTo(Verbosity.Verbose));

        _context.Verbosity = Verbosity.SuperBrief;
        Assert.That(_context.UserPreferences.Verbosity, Is.EqualTo(Verbosity.SuperBrief));
    }

    [Test]
    public void ApplyUserPreferences_ShouldCloneAndSetPreferences()
    {
        var customPreferences = new UserPreferences
        {
            Verbosity = Verbosity.Verbose,
            EnableAiParsing = false,
            FontSize = 20,
            Theme = "light"
        };

        _context.ApplyUserPreferences(customPreferences);

        Assert.That(_context.UserPreferences.Verbosity, Is.EqualTo(Verbosity.Verbose));
        Assert.That(_context.UserPreferences.EnableAiParsing, Is.False);
        Assert.That(_context.UserPreferences.FontSize, Is.EqualTo(20));
        Assert.That(_context.UserPreferences.Theme, Is.EqualTo("light"));
        
        // Verify it's a clone, not the same reference
        Assert.That(_context.UserPreferences, Is.Not.SameAs(customPreferences));
    }

    [Test]
    public void ModifyingOriginalPreferences_ShouldNotAffectAppliedPreferences()
    {
        var customPreferences = new UserPreferences
        {
            Verbosity = Verbosity.Verbose,
            FontSize = 20
        };

        _context.ApplyUserPreferences(customPreferences);

        // Modify the original
        customPreferences.Verbosity = Verbosity.SuperBrief;
        customPreferences.FontSize = 24;

        // Context should still have the original values
        Assert.That(_context.UserPreferences.Verbosity, Is.EqualTo(Verbosity.Verbose));
        Assert.That(_context.UserPreferences.FontSize, Is.EqualTo(20));
    }

    [Test]
    public async Task LoadUserPreferencesAsync_ShouldLoadFromService()
    {
        var mockService = new MockUserPreferencesService();
        await _context.LoadUserPreferencesAsync(mockService, "test-user");

        Assert.That(_context.UserPreferences.Verbosity, Is.EqualTo(Verbosity.SuperBrief));
        Assert.That(_context.UserPreferences.FontSize, Is.EqualTo(18));
        Assert.That(mockService.LastUserId, Is.EqualTo("test-user"));
    }

    private class MockUserPreferencesService : Model.Interface.IUserPreferencesService
    {
        public string? LastUserId { get; private set; }

        public Task<UserPreferences> GetPreferencesAsync(string? userId = null)
        {
            LastUserId = userId;
            return Task.FromResult(new UserPreferences
            {
                Verbosity = Verbosity.SuperBrief,
                FontSize = 18,
                EnableAiGeneration = false
            });
        }

        public Task SavePreferencesAsync(UserPreferences preferences, string? userId = null)
        {
            return Task.CompletedTask;
        }

        public Task ResetToDefaultsAsync(string? userId = null)
        {
            return Task.CompletedTask;
        }

        public Task<string> ExportPreferencesAsync(string? userId = null)
        {
            return Task.FromResult("{}");
        }

        public Task ImportPreferencesAsync(string preferencesJson, string? userId = null)
        {
            return Task.CompletedTask;
        }
    }
}
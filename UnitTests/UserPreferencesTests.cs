using Model;
using NUnit.Framework;

namespace UnitTests;

[TestFixture]
public class UserPreferencesTests
{
    [Test]
    public void DefaultPreferences_ShouldHaveExpectedValues()
    {
        var preferences = UserPreferences.Default;
        
        Assert.That(preferences.Verbosity, Is.EqualTo(Verbosity.Brief));
        Assert.That(preferences.EnableAiParsing, Is.True);
        Assert.That(preferences.EnableAiGeneration, Is.True);
        Assert.That(preferences.FontSize, Is.EqualTo(16));
        Assert.That(preferences.Theme, Is.EqualTo("dark"));
        Assert.That(preferences.ShowCompass, Is.True);
        Assert.That(preferences.ShowInventoryPanel, Is.True);
        Assert.That(preferences.AutoSave, Is.False);
        Assert.That(preferences.AutoSaveIntervalMinutes, Is.EqualTo(10));
        Assert.That(preferences.MaxCommandHistory, Is.EqualTo(50));
        Assert.That(preferences.EnableSounds, Is.False);
        Assert.That(preferences.SoundVolume, Is.EqualTo(50));
    }

    [Test]
    public void Clone_ShouldCreateDeepCopy()
    {
        var original = UserPreferences.Default;
        original.Verbosity = Verbosity.Verbose;
        original.FontSize = 20;
        original.Theme = "light";

        var cloned = original.Clone();

        Assert.That(cloned.Verbosity, Is.EqualTo(Verbosity.Verbose));
        Assert.That(cloned.FontSize, Is.EqualTo(20));
        Assert.That(cloned.Theme, Is.EqualTo("light"));
        Assert.That(cloned, Is.Not.SameAs(original));
    }

    [Test]
    public void ModifyingClone_ShouldNotAffectOriginal()
    {
        var original = UserPreferences.Default;
        var cloned = original.Clone();
        
        cloned.Verbosity = Verbosity.SuperBrief;
        cloned.EnableAiParsing = false;
        
        Assert.That(original.Verbosity, Is.EqualTo(Verbosity.Brief));
        Assert.That(original.EnableAiParsing, Is.True);
    }
}
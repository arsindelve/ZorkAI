using FluentAssertions;
using SecretsManager;

namespace UnitTests.SelfHosted;

public class LocalSecretsManagerTests
{
    [Test]
    public async Task Should_ReturnBuiltInNarratorPrompt_When_NoOverrideIsSet()
    {
        var original = Environment.GetEnvironmentVariable("ZORKAI_SYSTEM_PROMPT");
        try
        {
            Environment.SetEnvironmentVariable("ZORKAI_SYSTEM_PROMPT", null);

            var prompt = await new LocalSecretsManager().GetSecret("ZorkOnePrompt");

            prompt.Should().Be(LocalSecretsManager.DefaultNarratorPrompt);
            prompt.Should().NotBeNullOrWhiteSpace();
        }
        finally
        {
            Environment.SetEnvironmentVariable("ZORKAI_SYSTEM_PROMPT", original);
        }
    }

    [Test]
    public async Task Should_ReturnOverride_When_EnvironmentVariableIsSet()
    {
        var original = Environment.GetEnvironmentVariable("ZORKAI_SYSTEM_PROMPT");
        try
        {
            Environment.SetEnvironmentVariable("ZORKAI_SYSTEM_PROMPT", "You are a pirate narrator.");

            var prompt = await new LocalSecretsManager().GetSecret("PlanetfallPrompt");

            prompt.Should().Be("You are a pirate narrator.");
        }
        finally
        {
            Environment.SetEnvironmentVariable("ZORKAI_SYSTEM_PROMPT", original);
        }
    }
}

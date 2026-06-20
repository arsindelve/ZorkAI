using NUnit.Framework;

// No namespace == applies to the entire IntegrationTests assembly, once, before any test.
// Centralizes the local secrets these [Explicit] integration tests need, so individual
// fixtures don't each have to (and currently don't reliably) locate them:
//   - AWS_PROFILE=delve  -> all ZorkAI infra (Floyd lambda, ZorkStack, DynamoDb) lives in
//     the "delve" account; without this the SDK falls back to the wrong default account.
//   - ~/Dropbox/env      -> OPEN_AI_KEY and other API keys, loaded into the environment.
[SetUpFixture]
public class IntegrationTestSetup
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        Environment.SetEnvironmentVariable("AWS_PROFILE", "delve");

        var envFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Dropbox", "env");

        if (File.Exists(envFile))
            Env.Load(envFile);
        else
            TestContext.Progress.WriteLine($"Warning: secrets file not found at {envFile}");
    }
}
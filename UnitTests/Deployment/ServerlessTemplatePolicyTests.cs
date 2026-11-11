using System.IO;
using System.Text.Json;

namespace UnitTests.Deployment;

/// <summary>
///     Guards the drift that made StationfallStack unusable on its first deploy.
///
///     Every serverless.template declared exactly one policy, "AWSLambda_FullAccess", which grants
///     nothing on DynamoDB or Secrets Manager. The Zork and Planetfall roles work only because the
///     missing policies were attached to them BY HAND; CloudFormation left that drift alone because it
///     does not manage properties a template never declares. So a brand-new stack deployed green and
///     then failed on every request — 500 from "not authorized to perform: dynamodb:GetItem", and a
///     silently swallowed "not authorized to perform: secretsmanager:GetSecretValue" that left the
///     system prompt unset without surfacing anything.
///
///     The deploy cannot catch this (the stack is valid) and no test could either, because the required
///     permissions lived only in the console. Pinning them in the template is what makes them reviewable;
///     this asserts they stay there.
/// </summary>
[TestFixture]
public class ServerlessTemplatePolicyTests
{
    /// <summary>
    ///     The function reads and writes session/savegame rows in DynamoDB and reads the per-game system
    ///     prompt (and now the OpenAI key) from Secrets Manager. A template missing either produces a
    ///     function that starts cleanly and then fails on the first real request.
    /// </summary>
    private static readonly string[] Required =
    [
        "AWSLambda_FullAccess",
        "AmazonDynamoDBFullAccess",
        "SecretsManagerReadWrite"
    ];

    private static IEnumerable<string> TemplatePaths()
    {
        var root = RepositoryRoot();
        return Directory
            .EnumerateFiles(root, "serverless.template", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                        && !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
            .Select(p => Path.GetRelativePath(root, p))
            .OrderBy(p => p);
    }

    // Walk up from the test assembly to the directory holding the solution, so the test does not depend
    // on the working directory the runner happens to choose.
    private static string RepositoryRoot()
    {
        var dir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Zork.sln")))
            dir = dir.Parent;

        Assert.That(dir, Is.Not.Null, "Could not locate the repository root (no Zork.sln above the test directory).");
        return dir!.FullName;
    }

    [Test]
    public void EveryTemplate_IsDiscovered()
    {
        // Failsafe: if discovery finds nothing, every other test here passes vacuously.
        TemplatePaths().Should().NotBeEmpty("the deployed Lambdas each ship a serverless.template");
    }

    [TestCaseSource(nameof(TemplatePaths))]
    public void Template_DeclaresEveryPolicyTheFunctionNeedsAtRuntime(string relativePath)
    {
        var json = File.ReadAllText(Path.Combine(RepositoryRoot(), relativePath));
        using var doc = JsonDocument.Parse(json);

        var declared = doc.RootElement
            .GetProperty("Resources").GetProperty("AspNetCoreFunction")
            .GetProperty("Properties").GetProperty("Policies")
            .EnumerateArray().Select(p => p.GetString()!).ToList();

        declared.Should().Contain(Required,
            $"{relativePath} must declare the permissions its function needs; anything attached only by " +
            "hand is invisible to review and absent from every new stack");
    }
}

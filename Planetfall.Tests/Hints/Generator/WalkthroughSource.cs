using System.Text.RegularExpressions;

namespace Planetfall.Tests.Hints.Generator;

/// <summary>One [TestCase] row of a walkthrough: the command, its optional god-mode setup, and what the engine must say.</summary>
public sealed record WalkthroughStep(string Command, string? Setup, IReadOnlyList<string> Expected);

/// <summary>Reads a walkthrough test file's rows in order, straight from the source, so nothing can drift from it.</summary>
public static class WalkthroughSource
{
    private static readonly Regex Row = new("\\[TestCase\\(\"([^\"]*)\",\\s*(null|\"([^\"]*)\")(.*)\\)\\]", RegexOptions.Compiled);
    private static readonly Regex Quoted = new("\"((?:[^\"\\\\]|\\\\.)*)\"", RegexOptions.Compiled);

    public static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Zork.sln"))) dir = dir.Parent;
        return dir?.FullName ?? throw new InvalidOperationException("Zork.sln not found above " + AppContext.BaseDirectory);
    }

    public static List<WalkthroughStep> Load(string fileName)
    {
        var path = Path.Combine(RepoRoot(), "Planetfall.Tests", "Walkthrough", fileName);
        var steps = new List<WalkthroughStep>();

        foreach (var line in File.ReadLines(path))
        {
            var m = Row.Match(line);
            if (!m.Success) continue;

            var setup = m.Groups[3].Success ? m.Groups[3].Value : null;
            var expected = Quoted.Matches(m.Groups[4].Value).Select(q => q.Groups[1].Value.Replace("\\\"", "\"")).ToList();
            steps.Add(new WalkthroughStep(m.Groups[1].Value, setup, expected));
        }

        return steps;
    }
}

using System.Reflection;
using GameEngine;
using Model.Movement;

namespace UnitTests;

/// <summary>
///     Measures how much of a game's walkthrough vocabulary the production <see cref="DeterministicParser" />
///     resolves on its own. Used by each game's coverage test to PIN the exact set of commands the parser
///     declines, so the gap is a named list in a green test rather than a walkthrough that is expected to
///     fail: a command that stops parsing is a regression, and one that starts parsing shrinks the list.
/// </summary>
public static class DeterministicParserCoverage
{
    /// <summary>
    ///     Every distinct command the walkthrough fixtures derived from <paramref name="walkthroughBase" />
    ///     type, read from their <c>[TestCase]</c> attributes. Limited to multi-word, non-direction input:
    ///     a single word or a bare direction never reaches the verb/noun grammar at all.
    /// </summary>
    public static IReadOnlyList<string> WalkthroughCommands(Type walkthroughBase)
    {
        return walkthroughBase.Assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.IsSubclassOf(walkthroughBase))
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            .SelectMany(m => m.GetCustomAttributes<TestCaseAttribute>())
            .Select(a => (a.Arguments.FirstOrDefault() as string)?.Trim())
            .Where(input => !string.IsNullOrEmpty(input) && input.Contains(' ') &&
                            !DirectionParser.IsDirection(input, out _))
            .Select(input => input!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(input => input, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <summary>The commands in <paramref name="commands" /> the parser hands to the AI.</summary>
    public static IReadOnlyList<string> Declined(string gameName, IEnumerable<string> commands)
    {
        var parser = new DeterministicParser(gameName);
        return commands.Where(command => parser.Parse(command) is null).ToList();
    }
}

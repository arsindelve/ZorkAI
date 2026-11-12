using System.Reflection;
using GameEngine;
using Model.Intent;
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

    /// <summary>Everything any walkthrough fixture types, whatever becomes of it.</summary>
    public static IReadOnlySet<string> AllWalkthroughInput(Type walkthroughBase)
    {
        return walkthroughBase.Assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.IsSubclassOf(walkthroughBase))
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            .SelectMany(m => m.GetCustomAttributes<TestCaseAttribute>())
            .Select(a => (a.Arguments.FirstOrDefault() as string)?.Trim())
            .Where(input => !string.IsNullOrEmpty(input))
            .Select(input => input!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>The commands in <paramref name="commands" /> the parser hands to the AI.</summary>
    public static IReadOnlyList<string> Declined(string gameName, IEnumerable<string> commands)
    {
        var parser = new DeterministicParser(gameName);
        return commands.Where(command => parser.Parse(command) is null).ToList();
    }

    /// <summary>
    ///     Every command both parsers answer, where they answer differently. The walkthrough suites run
    ///     on <see cref="TestParser" />, whose parses are known-good because the games are played to the
    ///     end through them; a command the production grammar reads another way is one the engine would
    ///     handle differently in play than under test.
    /// </summary>
    public static async Task<IReadOnlyList<string>> Disagreements(string gameName, IEnumerable<string> commands,
        TestParser golden)
    {
        var parser = new DeterministicParser(gameName);
        var disagreements = new List<string>();

        foreach (var command in commands)
        {
            var mine = parser.Parse(command);
            if (mine is null)
                continue;

            var theirs = await golden.DetermineComplexIntentType(command, string.Empty, string.Empty);
            if (theirs is NullIntent)
                continue;

            if (!SameReading(mine, theirs))
                disagreements.Add($"{command}: grammar [{Shape(mine)}] vs golden [{Shape(theirs)}]");
        }

        return disagreements;
    }

    /// <summary>
    ///     The two parsers need not produce identical records to mean the same thing to the engine. The
    ///     golden parser is cruder in three known ways, none of which changes what a handler matches: it
    ///     takes only the first word of a multi-word noun ("fresh" for "fresh battery", itself a declared
    ///     noun), it drops the preposition of "look through X" (the handlers read the raw input), and it
    ///     keeps the article in a destination (navigation strips it).
    /// </summary>
    private static bool SameReading(IntentBase grammar, IntentBase golden)
    {
        static string Lower(string? text) => (text ?? string.Empty).Trim().ToLowerInvariant();
        static string NoArticle(string? text) => Lower(text).StartsWith("the ") ? Lower(text)[4..] : Lower(text);

        static bool SameNoun(string? full, string? crude) =>
            Lower(full) == Lower(crude) || Lower(full).StartsWith(Lower(crude) + " ");

        return (grammar, golden) switch
        {
            (SimpleIntent mine, SimpleIntent theirs) =>
                Lower(mine.Verb) == Lower(theirs.Verb) && SameNoun(mine.Noun, theirs.Noun) &&
                (Lower(theirs.Adverb).Length == 0 || Lower(mine.Adverb) == Lower(theirs.Adverb)),
            (MultiNounIntent mine, MultiNounIntent theirs) =>
                Lower(mine.Verb) == Lower(theirs.Verb) && SameNoun(mine.NounOne, theirs.NounOne) &&
                SameNoun(mine.NounTwo, theirs.NounTwo) && Lower(mine.Preposition) == Lower(theirs.Preposition),
            (GoToDestinationIntent mine, GoToDestinationIntent theirs) =>
                NoArticle(mine.Destination) == NoArticle(theirs.Destination),
            _ => Shape(grammar) == Shape(golden)
        };
    }

    private static string Shape(IntentBase intent)
    {
        static string Lower(string? text) => (text ?? string.Empty).Trim().ToLowerInvariant();

        return intent switch
        {
            SimpleIntent simple => $"simple {Lower(simple.Verb)} | {Lower(simple.Noun)} | {Lower(simple.Adverb)}",
            MultiNounIntent multi =>
                $"multi {Lower(multi.Verb)} | {Lower(multi.NounOne)} | {Lower(multi.Preposition)} | {Lower(multi.NounTwo)}",
            EnterSubLocationIntent board => $"board {Lower(board.Noun)}",
            GoToDestinationIntent goTo => $"goto {Lower(goTo.Destination)}",
            MoveIntent move => $"move {move.Direction}",
            _ => intent.GetType().Name
        };
    }
}

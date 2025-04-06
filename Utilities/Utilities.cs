using System.Text;
using System.Text.RegularExpressions;
using WordsToNumbers;

namespace Utilities;

public static class Utilities
{
    /// <summary>
    /// Converts a given string representation of a number (in numeric or words format) into an integer, if possible.
    /// </summary>
    /// <param name="number">The string to be converted into an integer. This can be numeric ("5") or words ("five").</param>
    /// <returns>An integer value if the conversion is successful; otherwise, null.</returns>
    public static int? ToInteger(this string? number)
    {
        var converter = new SimpleReplacementStrategy();
        var result = converter.ConvertWordsToNumbers(number);

        if (int.TryParse(result, out var i))
            return i;

        return null;
    }

    /// <summary>
    /// Adds an indentation to each line of the given string.
    /// </summary>
    /// <param name="input">The input string where each line will be indented.</param>
    /// <returns>A new string with each line preceded by an indentation.</returns>
    public static string IndentLines(this string input)
    {
        var sb = new StringBuilder();
        var lines = input.Split('\n'); // Split the string into lines.

        foreach (var line in lines) sb.AppendLine("   " + line); // Add a tab before each line and append it.

        return sb.ToString();
    }
    
    /// <summary>
    /// Returns a random element from the list.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="list">The list from which to select a random element.</param>
    /// <returns>A random element from the list.</returns>
    /// <exception cref="ArgumentException">Thrown if the list is null or empty.</exception>
    public static T GetRandomElement<T>(this IList<T> list)
    {
        if (list == null || list.Count == 0)
        {
            throw new ArgumentException("List cannot be null or empty");
        }

        int index = new Random().Next(list.Count); // Generate a random index.
        return list[index]; // Return the element at the random index.
    }

    /// <summary>
    /// Removes all non-alphabetic characters and retains only letters and whitespace from the input string.
    /// </summary>
    /// <param name="s">The input string from which non-alphabetic characters are to be removed. Can be null.</param>
    /// <returns>A new string containing only alphabetic characters and whitespace. Returns an empty string if the input is null.</returns>
    public static string StripNonChars(this string? s)
    {
        if (s == null) return string.Empty;
        return Regex.Replace(s, "[^a-zA-Z\\s]", string.Empty);
    }

    /// <summary>
    /// Constructs a concise, single-line string representation of a list of nouns, with the items separated by commas and the last item joined using "and."
    /// </summary>
    /// <example>"A sword, a diamond and a pile of leaves. </example>
    /// <param name="nouns">The list of nouns to be formatted into a single string.</param>
    /// <returns>A string where the nouns are combined with commas and the final noun is preceded by "and."</returns>
    public static string SingleLineListWithAnd(this List<string> nouns)
    {
        return SingleLineList(nouns, "and", "a");
    }

    /// <summary>
    /// Combines a list of nouns into a single string, separated by commas, and uses "and" before the last item.
    /// Any articles are omitted from the resulting string.
    /// </summary>
    /// <param name="nouns">The list of nouns to combine into a single line.</param>
    /// <returns>A single string representing the combined nouns, separated by commas, with "and" before the last item and no articles.</returns>
    public static string SingleLineListWithAndNoArticle(this List<string> nouns)
    {
        return SingleLineList(nouns, "and", "");
    }

    /// <summary>
    /// Creates a single-line, human-readable string representation of a list of nouns, combining them with the word "or".
    /// </summary>
    /// <example>The brass lantern, the green lantern or the useless lantern</example>
    /// <param name="nouns">The list of noun strings to format into a single line.</param>
    /// <returns>A string representing the list of nouns combined with "or". For example, "item1, item2, or item3".</returns>
    public static string SingleLineListWithOr(this List<string> nouns)
    {
        return SingleLineList(nouns, "or", "the");
    }

    private static string SingleLineList(this List<string> nouns, string connector, string articles)
    {
        var convertNouns = !string.IsNullOrEmpty(articles) ? 
            nouns.ConvertAll(noun => $"{articles} {noun}")  : 
            nouns.ConvertAll(noun => $"{noun}");
        
        var lastNoun = convertNouns.Last();
        convertNouns.Remove(lastNoun);

        return convertNouns.Count > 0
            ? $"{string.Join(", ", convertNouns)} {connector} {lastNoun}"
            : lastNoun;
    }

    public static void WriteLineWordWrap(string? paragraph, int tabSize = 6)
    {
        if (string.IsNullOrEmpty(paragraph))
            return;

        var lines = paragraph
            .Replace("\t", new string(' ', tabSize))
            .Split(new[] { Environment.NewLine }, StringSplitOptions.None);

        foreach (var t in lines)
        {
            var process = t;
            var wrapped = new List<string>();

            while (process.Length > Console.WindowWidth)
            {
                var wrapAt = process.LastIndexOf(' ', Math.Min(Console.WindowWidth - 1, process.Length));
                if (wrapAt <= 0) break;

                wrapped.Add(process.Substring(0, wrapAt));
                process = process.Remove(0, wrapAt + 1);
            }

            foreach (var wrap in wrapped) Console.WriteLine(wrap);

            Console.WriteLine(process);
        }
    }
}
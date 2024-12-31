using System.Text;
using System.Text.RegularExpressions;

namespace Utilities;

public static class Utilities
{
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

    public static string StripNonChars(this string? s)
    {
        if (s == null) return string.Empty;
        return Regex.Replace(s, "[^a-zA-Z\\s]", string.Empty);
    }

    // "A sword, a diamond and a pile of leaves. 
    public static string SingleLineListWithAnd(this List<string> nouns)
    {
        return SingleLineList(nouns, "and", "a");
    }

    // "The brass lantern, the green lantern or the useless lantern
    public static string SingleLineListWithOr(this List<string> nouns)
    {
        return SingleLineList(nouns, "or", "the");
    }

    private static string SingleLineList(this List<string> nouns, string connector, string articles)
    {
        var convertNouns = nouns.ConvertAll(noun => $"{articles} {noun}");
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
using System.Text.RegularExpressions;

namespace Utilities;

public static class Utilities
{
    public static string StripNonChars(this string? s)
    {
        if (s == null) return string.Empty;
        return Regex.Replace(s, "[^a-zA-Z\\s]", string.Empty);
    }

    // "A sword, a diamond and a pile of leaves. 
    public static string SingleLineListWithAnd(this List<string> nouns)
    {
        return SingleLineList(nouns, "and");
    }
    
    // "The brass lantern, the green lantern or the useless lantern
    public static string SingleLineListWithOr(this List<string> nouns)
    {
        return SingleLineList(nouns, "or");
    }
    
    private static string SingleLineList(this List<string> nouns, string connector)
    {
        var convertNouns = nouns.ConvertAll(noun => "a " + noun);
        string lastNoun = convertNouns.Last();
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
using System.Text.RegularExpressions;

namespace Utilities;

public static class Utilities
{
    public static string StripNonChars(this string? s)
    {
        if (s == null) return string.Empty;
        return Regex.Replace(s, "[^a-zA-Z\\s]", string.Empty);
    }

    public static void WriteLineWordWrap(string? paragraph, int tabSize = 8)
    {
        if (string.IsNullOrEmpty(paragraph))
            return;
        
        string[] lines = paragraph
            .Replace("\t", new String(' ', tabSize))
            .Split(new[] { Environment.NewLine }, StringSplitOptions.None);

        foreach (var t in lines)
        {
            string process = t;
            List<String> wrapped = new List<string>();

            while (process.Length > Console.WindowWidth)
            {
                int wrapAt = process.LastIndexOf(' ', Math.Min(Console.WindowWidth - 1, process.Length));
                if (wrapAt <= 0) break;

                wrapped.Add(process.Substring(0, wrapAt));
                process = process.Remove(0, wrapAt + 1);
            }

            foreach (string wrap in wrapped)
            {
                Console.WriteLine(wrap);
            }

            Console.WriteLine(process);
        }
    }
}
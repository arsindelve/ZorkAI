using System.Text.RegularExpressions;

namespace Stationfall;

/// <summary>
///     Recognizes a bare "type &lt;number&gt;" on one of the ship's keypads.
///     This is deliberately matched against the player's raw input rather than a parsed intent. The
///     spacetruck's course heading is <em>computed at runtime</em> from the chronometer, so it can be
///     any number at all; the intent parser only reliably resolves a small set of known number-nouns,
///     which would leave a legitimate course unparsed. Reading the raw text keeps every valid heading
///     working.
/// </summary>
public static partial class KeypadInput
{
    /// <summary>
    ///     True when the input is a keypad entry, e.g. "type 3", "press 103", "punch in 42".
    /// </summary>
    public static bool TryParse(string? input, out int number)
    {
        number = 0;

        if (string.IsNullOrWhiteSpace(input))
            return false;

        var match = KeypadPattern().Match(input.Trim());

        return match.Success && int.TryParse(match.Groups["number"].Value, out number);
    }

    [GeneratedRegex(@"^(type|punch|key|press|enter|input|dial)(\s+in)?\s+(?<number>\d+)$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex KeypadPattern();
}

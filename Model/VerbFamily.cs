using System.Collections;

namespace Model;

/// <summary>
///     A set of verbs that all mean the same action to whatever answers them: "take", "get", "grab".
///     <para>
///         This is to verbs what <c>NounsForMatching</c> is to nouns. An object owns its nouns because the
///         thing that declares a noun is the thing that matches it, so a noun the game answers to cannot
///         be missing from the vocabulary. Verbs get the same guarantee from this type: the intent
///         <c>Match</c> methods accept nothing else, so a handler cannot answer a verb that is not in a
///         family, and every family is a static field the vocabulary harvest
///         (<c>Repository.GetVerbs</c>) can find by reflection.
///     </para>
///     <para>
///         A family lives in one of two places. <see cref="Verbs" /> holds every family the engine
///         answers and every family more than one class answers. The one item or room that answers a
///         verb nothing else does declares it on itself, as a <c>static readonly</c> field:
///         <code>private static readonly VerbFamily ShiftVerbs = new("move", "slide", "pull");</code>
///         Never build one inline inside a method - it would work, and it would be invisible to the
///         harvest. <c>VerbFamilyDeclarationTests</c> fails the build when one is.
///     </para>
///     <para>
///         And never declare the same words twice. A second class that answers a family another class
///         already declares is the signal that the family belongs in <see cref="Verbs" />; the same test
///         class fails on a repeat.
///     </para>
/// </summary>
public sealed class VerbFamily : IReadOnlyList<string>
{
    private readonly string[] _words;

    /// <param name="words">
    ///     Every word or phrase in the family, the canonical one first. To build on another family,
    ///     spread it: <c>new([..Verbs.KillVerbs, "kick"])</c>.
    /// </param>
    public VerbFamily(params string[] words)
    {
        if (words is null || words.Length == 0)
            throw new ArgumentException("A verb family needs at least one verb.", nameof(words));

        if (words.Any(string.IsNullOrWhiteSpace))
            throw new ArgumentException("A verb family cannot contain an empty verb.", nameof(words));

        _words = words.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    /// <summary>The canonical verb: the one word that names the action the family stands for.</summary>
    public string Name => _words[0];

    public int Count => _words.Length;

    public string this[int index] => _words[index];

    /// <summary>
    ///     True when <paramref name="verb" /> is one of this family's words, ignoring case. This is the
    ///     comparison the intent <c>Match</c> methods make.
    /// </summary>
    public bool Matches(string? verb)
    {
        return verb is not null &&
               _words.Any(word => word.Equals(verb, StringComparison.InvariantCultureIgnoreCase));
    }

    public IEnumerator<string> GetEnumerator()
    {
        return ((IEnumerable<string>)_words).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public override string ToString()
    {
        return Name;
    }
}

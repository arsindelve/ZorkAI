namespace Planetfall.Item.Lawanda.BioLab;

/// <summary>
/// Base class for the four Bio Lab mutants (rat-ant, mutant troll, mutant grue, triffid). The
/// original Planetfall source gives every mutant <c>MONSTER</c> as a synonym, so during the chase a
/// panicked player can type "attack monster" / "kill monster". Rather than repeat that shared noun
/// on each creature, it lives here and is appended to every subclass's <see cref="SpecificNouns" />.
///
/// All four mutants share the room (<c>BioLabLocation</c>), so "monster" is deliberately excluded
/// from <see cref="NounsForPreciseMatching" /> — the same approach <c>FromitzBoardBase</c> and
/// <c>AccessCard</c> use for their shared category nouns ("board", "card"). That keeps "monster"
/// matchable for the initial command while forcing the disambiguation prompt to resolve to a
/// specific creature instead of an arbitrary one.
/// </summary>
public abstract class MutantBase : ItemBase, ICanBeExamined
{
    /// <summary>The creature-specific nouns. The shared "monster" synonym is appended by the base.</summary>
    protected abstract string[] SpecificNouns { get; }

    public override string[] NounsForMatching => [..SpecificNouns, "monster"];

    /// <summary>
    /// Remove the shared "monster" from the disambiguation nouns so a player who answers a
    /// "Do you mean...?" prompt must name a specific creature rather than "monster" again.
    /// </summary>
    public override string[] NounsForPreciseMatching => NounsForMatching.Except(["monster"]).ToArray();

    public override int Size => 100; // Can't be taken

    public abstract string ExaminationDescription { get; }
}

using Model.Interface;
using UnitTests;
using ZorkOne.GlobalCommand;

namespace ZorkOne.Tests.Walkthrough;

/// <summary>
/// Diagnostic: runs the full walkthrough through the DeterministicParser with NO fallback, to surface every
/// standard-grammar command it doesn't yet handle. Failures here are the work list for completing the
/// grammar (and/or hidden production bugs the TestParser was masking). NOT a keep-forever test — it exists
/// to drive the grammar to completeness; once the parser is complete it should pass, at which point the
/// separate fallback variant becomes redundant.
/// </summary>
[TestFixture]
[Explicit("Diagnostic work-list for completing the deterministic grammar — expected red until the parser " +
          "handles all standard Zork grammar (take/drop, canonicalization, multi-noun). Flip to a normal " +
          "test once it passes, at which point TestParser can be deleted.")]
public class WalkthroughTestOneDeterministicOnly : WalkthroughTestOne
{
    protected override IIntentParser? BuildParser() =>
        new DeterministicOnlyTestParser(new ZorkOneGlobalCommandFactory());
}

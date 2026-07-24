using Model.Interface;
using UnitTests;
using ZorkOne.GlobalCommand;

namespace ZorkOne.Tests.Walkthrough;

/// <summary>
/// Re-runs the entire <see cref="WalkthroughTestOne" /> command sequence, but with the production
/// <see cref="DeterministicParser" /> layered ahead of the TestParser (see
/// <see cref="DeterministicFirstTestParser" />). This is the end-to-end coverage the isolated
/// DeterministicParserTests can't give: it proves the deterministic parser produces engine-correct intents
/// across a full real playthrough (open mailbox, move rug, put X in Y, look under/through, examine ...),
/// with the TestParser catching everything the deterministic pass intentionally leaves alone.
/// </summary>
[TestFixture]
public class WalkthroughTestOneDeterministicFirst : WalkthroughTestOne
{
    protected override IIntentParser? BuildParser() =>
        new DeterministicFirstTestParser(new ZorkOneGlobalCommandFactory());
}

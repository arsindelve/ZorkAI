using GameEngine;
using Model.AIParsing;
using ZorkOne.GlobalCommand;

namespace UnitTests.GlobalCommands;

public class DiagnoseCommandTests : EngineTestsBase
{
    [Test]
    public async Task Diagnose_IsFreeAction_DoesNotAdvanceMoves()
    {
        // Issue #354 follow-up: "diagnose" is a meta/informational verb (wound/death status), same
        // shape as score/look/inventory, and must not consume a turn either.
        var engine = GetTarget(new IntentParser(Mock.Of<IAIParser>(), new ZorkOneGlobalCommandFactory()));

        // Act
        await engine.GetResponse("diagnose");

        // Assert
        engine.Moves.Should().Be(0);
    }
}

using FluentAssertions;
using GameEngine;
using Model.AIParsing;
using Model.Intent;
using Model.Interface;
using Moq;
using Planetfall.Item.Feinstein;
using Planetfall.Location.Feinstein;

namespace Planetfall.Tests;

/// <summary>
/// Issue #136 follow-up: worn-clothing interactions with the agentic narrator's tools. Zork I has no
/// clothing, so these live with Planetfall's uniforms. Drop already refuses worn clothing via DropIt;
/// Destroy must refuse it the same way rather than vaporizing something still being worn, and a
/// partially-refused tool list must surface the refusal alongside the narration instead of silently
/// claiming everything was disposed of.
/// </summary>
[TestFixture]
public class AgenticActionTests : EngineTestsBase
{
    /// <summary>
    /// A DESTROY tool aimed at worn clothing must be refused with the same message the drop
    /// mechanic uses, leaving the item worn and intact - not destroyed while BeingWorn stays true.
    /// </summary>
    [Test]
    public async Task DestroyTool_WornClothing_RefusedLikeDrop()
    {
        var parser = new Mock<IIntentParser>();
        var target = GetTarget(parser.Object);
        target.Context.CurrentLocation = Repository.GetLocation<DeckNine>();
        var uniform = Take<PatrolUniform>();
        uniform.BeingWorn = true;

        parser.Setup(s => s.DetermineComplexIntentType("tear up my uniform", It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new SimpleIntent { Verb = "tear", Noun = "uniform", OriginalInput = "tear up my uniform" });

        AgenticActionParser
            .Setup(s => s.Resolve(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new AgenticActionResult("You shred the uniform to ribbons.",
                [new AgenticToolCall(AgenticTool.Destroy, "uniform")]));

        var response = await target.GetResponse("tear up my uniform");

        response.Should().Contain("You'll have to take it off, first.");
        response.Should().NotContain("ribbons");
        target.Context.Items.Should().Contain(uniform);
        uniform.BeingWorn.Should().BeTrue();
        uniform.CurrentLocation.Should().Be(target.Context);
    }

    /// <summary>
    /// Partial success: one tool applies (the diary drops) while another is refused (the uniform is
    /// worn). The player must see BOTH the narration for what happened and the refusal for what
    /// didn't - otherwise the reply claims the uniform was disposed of while it is still being worn.
    /// </summary>
    [Test]
    public async Task MixedOutcome_NarrationAndRefusalBothShown()
    {
        var parser = new Mock<IIntentParser>();
        var target = GetTarget(parser.Object);
        target.Context.CurrentLocation = Repository.GetLocation<DeckNine>();
        var diary = Take<Diary>();
        var uniform = Take<PatrolUniform>();
        uniform.BeingWorn = true;

        parser.Setup(s => s.DetermineComplexIntentType("throw my diary and uniform in the air",
                It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new SimpleIntent
            {
                Verb = "throw", Noun = "diary", OriginalInput = "throw my diary and uniform in the air"
            });

        AgenticActionParser
            .Setup(s => s.Resolve(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new AgenticActionResult("Everything sails up and clatters back down.",
            [
                new AgenticToolCall(AgenticTool.Drop, "diary"),
                new AgenticToolCall(AgenticTool.Drop, "uniform")
            ]));

        var response = await target.GetResponse("throw my diary and uniform in the air");

        response.Should().Contain("Everything sails up and clatters back down.");
        response.Should().Contain("You'll have to take it off, first.");
        target.Context.Items.Should().NotContain(diary);
        Repository.GetLocation<DeckNine>().Items.Should().Contain(diary);
        target.Context.Items.Should().Contain(uniform);
        uniform.BeingWorn.Should().BeTrue();
    }
}

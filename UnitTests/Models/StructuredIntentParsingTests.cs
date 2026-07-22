using Microsoft.Extensions.Logging;
using Model;
using Model.Intent;
using Model.Movement;
using Newtonsoft.Json;

namespace UnitTests.Models;

/// <summary>
/// Deterministic tests for the Structured-Outputs parse path (no AI/network). They prove that a
/// schema-shaped <see cref="ParsedIntent" /> renders to a well-formed tag string and flows through the
/// existing <see cref="ParsingHelper.GetIntent" /> to the correct intent — i.e. the structured wire format
/// integrates with the battle-tested downstream pipeline. The live-model behaviour is covered separately
/// by the [Explicit] integration tests.
/// </summary>
[TestFixture]
public class StructuredIntentParsingTests
{
    private ILogger? _logger;

    [SetUp]
    public void Setup() => _logger = new Mock<ILogger>().Object;

    private IntentBase Parse(ParsedIntent dto) =>
        ParsingHelper.GetIntent("original input", StructuredIntentParsing.ToTagString(dto), _logger);

    [Test]
    public void Dto_DeserializesFromSchemaShapedJson()
    {
        // The exact JSON shape the model returns under the strict schema.
        var json =
            """{"intent":"act","verb":"inflate","nouns":["pile of plastic","air pump"],"preposition":"with","direction":null,"adjective":null}""";

        var dto = JsonConvert.DeserializeObject<ParsedIntent>(json);

        dto.Should().NotBeNull();
        dto!.Intent.Should().Be("act");
        dto.Verb.Should().Be("inflate");
        dto.Nouns.Should().BeEquivalentTo("pile of plastic", "air pump");
        dto.Preposition.Should().Be("with");
    }

    [Test]
    public void ToTagString_IsWellFormedAndLowercased()
    {
        var dto = new ParsedIntent { Intent = "TAKE", Verb = "Take", Nouns = ["Brass Lantern"] };

        var tags = StructuredIntentParsing.ToTagString(dto);

        tags.Should().Contain("<intent>take</intent>");
        tags.Should().Contain("<verb>take</verb>");
        tags.Should().Contain("<noun>brass lantern</noun>");
    }

    [Test]
    public void Structured_Take_BecomesTakeIntent()
    {
        var result = Parse(new ParsedIntent { Intent = "take", Verb = "take", Nouns = ["sword"] });

        result.Should().BeOfType<TakeIntent>();
        (result as TakeIntent)!.Noun.Should().Be("sword");
    }

    [Test]
    public void Structured_Move_BecomesMoveIntent()
    {
        var result = Parse(new ParsedIntent { Intent = "move", Verb = "walk", Direction = "north" });

        result.Should().BeOfType<MoveIntent>();
        (result as MoveIntent)!.Direction.Should().Be(Direction.N);
    }

    [Test]
    public void Structured_MultiNoun_BecomesMultiNounIntent()
    {
        var result = Parse(new ParsedIntent
        {
            Intent = "act", Verb = "tie", Nouns = ["rope", "railing"], Preposition = "to"
        });

        result.Should().BeOfType<MultiNounIntent>();
        var multi = result as MultiNounIntent;
        multi!.NounOne.Should().Be("rope");
        multi.NounTwo.Should().Be("railing");
        multi.Preposition.Should().Be("to");
    }

    [Test]
    public void Structured_GoTo_BecomesGoToDestinationIntent()
    {
        var result = Parse(new ParsedIntent { Intent = "goto", Verb = "go", Nouns = ["kitchen"] });

        result.Should().BeOfType<GoToDestinationIntent>();
        (result as GoToDestinationIntent)!.Destination.Should().Be("kitchen");
    }

    [Test]
    public void Structured_Move_WithOtherDirectionButNamedPlace_FallsBackToGoTo()
    {
        // The #268 safety net must still fire when the structured output uses direction "other" (the
        // model's "it's a move but I can't name the direction") with a named place.
        var result = Parse(new ParsedIntent
        {
            Intent = "move", Verb = "go", Nouns = ["dome room"], Direction = "other"
        });

        result.Should().BeOfType<GoToDestinationIntent>();
        (result as GoToDestinationIntent)!.Destination.Should().Be("dome room");
    }

    [Test]
    public void Structured_LookWithNoun_BecomesTargetedExamine_Not_BareLook()
    {
        // #423: a look that names a real object must become a SimpleIntent carrying that noun, even through
        // the structured path.
        var result = Parse(new ParsedIntent { Intent = "look", Verb = "look", Nouns = ["window"] });

        result.Should().BeOfType<SimpleIntent>();
        var simple = result as SimpleIntent;
        simple!.Verb.Should().Be("look");
        simple.Noun.Should().Be("window");
    }

    [Test]
    public void Structured_BareLook_StaysLookIntent()
    {
        var result = Parse(new ParsedIntent { Intent = "look", Verb = null, Nouns = [] });

        result.Should().BeOfType<LookIntent>();
    }

    [Test]
    public void Structured_Inventory_BecomesInventoryIntent()
    {
        var result = Parse(new ParsedIntent { Intent = "inventory" });

        result.Should().BeOfType<InventoryIntent>();
    }

    [Test]
    public void ToTagString_IsDeterministic_ForTheSameDto()
    {
        var dto = new ParsedIntent { Intent = "act", Verb = "open", Nouns = ["mailbox"] };

        StructuredIntentParsing.ToTagString(dto).Should().Be(StructuredIntentParsing.ToTagString(dto));
    }

    [Test]
    public void BuildSystemPrompt_DoesNotThrow_AndSubstitutesPlaceholders()
    {
        // Regression guard: the prompt embeds literal { } in its JSON examples, so it must be built with
        // string.Replace, NOT string.Format (which throws FormatException on the braces). BuildSystemPrompt
        // must substitute {0}/{1} without throwing and must leave the example braces intact.
        var act = () => StructuredIntentParsing.BuildSystemPrompt("West of House", "take the lamp");

        act.Should().NotThrow();
        var prompt = act();
        prompt.Should().Contain("West of House");
        prompt.Should().Contain("take the lamp");
        prompt.Should().NotContain("{0}");
        prompt.Should().NotContain("{1}");
        prompt.Should().Contain("{\"intent\":\"act\"", "the JSON example braces must survive intact");
    }

    [Test]
    public void SystemPrompt_WouldThrow_UnderStringFormat()
    {
        // Documents exactly why BuildSystemPrompt exists: running the prompt through string.Format throws
        // because of the literal { } in the JSON examples. If this ever stops throwing, the prompt no longer
        // needs the Replace-based builder.
        var act = () => string.Format(StructuredIntentParsing.SystemPrompt, "loc", "input");

        act.Should().Throw<FormatException>();
    }
}

using GameEngine;
using Model.Intent;

namespace UnitTests.Parser;

/// <summary>
///     The parts of the parser contract that involve no AI at all, so they run once rather than once per
///     AI: what the grammar does with every case, and which input never reaches a parser to begin with.
/// </summary>
[TestFixture]
public class ParserCorpusTests
{
    [Test]
    public void TheCorpus_IsWellFormed()
    {
        ParserCorpus.All.GroupBy(@case => (@case.Game, Input: @case.Input.ToLowerInvariant()))
            .Where(group => group.Count() > 1).Select(group => group.Key.ToString())
            .Should().BeEmpty("each input appears once per game");

        foreach (var @case in ParserCorpus.All.Where(@case => @case.ResolvedBy != ResolvedBy.BeforeParsing))
        {
            ParserCorpus.DescribeLocation(@case).Should().NotBeNullOrWhiteSpace(@case.ToString());
            @case.FromAi.Should().NotBeNull(@case.ToString());
            @case.ModelAnswer.Should().NotBeNull(@case.ToString());
        }
    }

    [TestCaseSource(typeof(ParserCorpus), nameof(ParserCorpus.Parsed))]
    public void TheGrammar_ResolvesOrDeclines_AsTheCaseSays(ParserCase @case)
    {
        var result = new DeterministicParser(@case.Game).Parse(@case.Input);

        if (@case.ResolvedBy == ResolvedBy.Ai)
            result.Should().BeNull(
                $"the grammar must hand \"{@case.Input}\" to the AI; if it has learned to resolve it, " +
                "take .AiOnly() off the case");
        else
            @case.FromGrammar!.Verify(result, @case.Input, "the grammar");
    }

    [TestCaseSource(typeof(ParserCorpus), nameof(ParserCorpus.ByGrammar))]
    public void TheGrammar_IgnoresCasePunctuationAndSpacing(ParserCase @case)
    {
        var parser = new DeterministicParser(@case.Game);
        string[] variants =
        [
            @case.Input.ToUpperInvariant(),
            $"  {@case.Input}  ",
            $"{@case.Input}.",
            $"{@case.Input}!",
            $"{@case.Input}?",
            @case.Input.Replace(" ", "  ")
        ];

        foreach (var variant in variants)
            @case.FromGrammar!.Verify(parser.Parse(variant), variant, "the grammar");
    }

    [TestCaseSource(typeof(ParserCorpus), nameof(ParserCorpus.Parsed))]
    public void InputTheParserIsTestedOn_ReallyReachesTheParser(ParserCase @case)
    {
        var parser = new IntentParser(Mock.Of<Model.AIParsing.IAIParser>(), ParserCorpus.CommandFactory(@case.Game),
            @case.Game);

        parser.DetermineSystemIntentType(@case.Input).Should().BeNull();
        parser.DetermineGlobalIntentType(@case.Input).Should().BeNull(
            $"\"{@case.Input}\" is answered before parsing, so it belongs with the BeforeParsing cases");
    }

    [TestCaseSource(typeof(ParserCorpus), nameof(ParserCorpus.BeforeParsing))]
    public void InputAnsweredBeforeParsing_NeverReachesAParser(ParserCase @case)
    {
        var parser = new IntentParser(Mock.Of<Model.AIParsing.IAIParser>(), ParserCorpus.CommandFactory(@case.Game),
            @case.Game);

        var intent = parser.DetermineSystemIntentType(@case.Input) ?? parser.DetermineGlobalIntentType(@case.Input);

        @case.BeforeParsing!.Verify(intent, @case.Input, "the stage before parsing");
    }
}

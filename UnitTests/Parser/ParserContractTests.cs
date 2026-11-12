using GameEngine;
using Model.AIParsing;
using Model.Intent;

namespace UnitTests.Parser;

/// <summary>
///     The contract an AI parser has to meet, written once and run against either a scripted AI or a real
///     one. Nothing here knows which provider is behind <see cref="IAIParser" />: the cases say what the
///     player typed and what the engine must be handed, so the same suite judges gpt-4o today and whatever
///     replaces it.
///     <para>
///         <c>ScriptedAiParserContractTests</c> (UnitTests) runs it with a model stubbed to give each case's
///         <see cref="ParserCase.ModelAnswer" /> - deterministic, in the normal test run, and still through
///         the real provider adapter and its repairs. <c>LiveAiParserContractTests</c> (IntegrationTests,
///         [Explicit]) runs the identical tests against the live model.
///     </para>
/// </summary>
public abstract class ParserContractTests
{
    /// <summary>
    ///     The AI parser for one case. A scripted implementation answers with
    ///     <see cref="ParserCase.ModelAnswer" />; a live one ignores the argument and asks the real model.
    /// </summary>
    protected abstract IAIParser CreateAi(ParserCase scripted);

    [TestCaseSource(typeof(ParserCorpus), nameof(ParserCorpus.Parsed))]
    public async Task TheAi_AskedDirectly_ProducesWhatTheEngineNeeds(ParserCase @case)
    {
        var ai = CreateAi(@case);

        var intent = await ai.AskTheAIParser(@case.Input, ParserCorpus.DescribeLocation(@case), "session");

        @case.FromAi!.Verify(intent, @case.Input, "the AI");
    }

    [TestCaseSource(typeof(ParserCorpus), nameof(ParserCorpus.Parsed))]
    public async Task TheParser_AnswersFromTheRightLayer(ParserCase @case)
    {
        var ai = new CountingAi(CreateAi(@case));
        var parser = new IntentParser(ai, ParserCorpus.CommandFactory(@case.Game), @case.Game);

        var intent = await parser.DetermineComplexIntentType(@case.Input, ParserCorpus.DescribeLocation(@case),
            "session");

        if (@case.ResolvedBy == ResolvedBy.Grammar)
        {
            ai.Calls.Should().Be(0, $"\"{@case.Input}\" is standard grammar and must not cost an AI call");
            @case.FromGrammar!.Verify(intent, @case.Input, "the grammar");
        }
        else
        {
            ai.Calls.Should().Be(1, $"the grammar must decline \"{@case.Input}\" and hand it to the AI once");
            @case.FromAi!.Verify(intent, @case.Input, "the AI");
        }
    }

    private sealed class CountingAi(IAIParser inner) : IAIParser
    {
        public int Calls { get; private set; }

        public string LanguageModel => inner.LanguageModel;

        public Task<IntentBase> AskTheAIParser(string input, string locationDescription, string sessionId)
        {
            Calls++;
            return inner.AskTheAIParser(input, locationDescription, sessionId);
        }
    }
}

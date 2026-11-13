using Model.AIParsing;
using UnitTests.Parser;
using ZorkAI.OpenAI;

namespace IntegrationTests;

/// <summary>
///     The parser contract against the real AI parser: the same cases and the same assertions as
///     <c>UnitTests.Parser.ScriptedAiParserContractTests</c>, with the live model in place of the script.
///     <para>
///         This is the suite to run when the model, the prompt or the provider changes. Each case says
///         what the player typed and what the engine must be handed, so a failure here is a sentence the
///         real parser no longer understands. When the provider changes, return the new parser from
///         <see cref="CreateAi" />; nothing else in the suite is provider-specific.
///     </para>
/// </summary>
[TestFixture]
[Explicit("Calls the live AI parser for every case - needs OPEN_AI_KEY")]
[Parallelizable(ParallelScope.Children)]
public class LiveAiParserContractTests : ParserContractTests
{
    protected override IAIParser CreateAi(ParserCase scripted)
    {
        return new OpenAIParser(null);
    }
}

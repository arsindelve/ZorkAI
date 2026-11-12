using Model.AIParsing;
using Newtonsoft.Json;
using OpenAI.Chat;
using ZorkAI.OpenAI;

namespace UnitTests.Parser;

/// <summary>
///     The parser contract with a scripted model: every case's <see cref="ParserCase.ModelAnswer" /> is
///     returned as the model's completion, and everything after the model - the provider adapter, the
///     rendering, <c>ParsingHelper</c>'s rules and repairs, the grammar-or-AI decision - is production code.
///     <para>
///         This is the only provider-specific piece of the suite. When the AI parser changes provider,
///         render the same <see cref="ModelAnswer" /> as that provider's response here; the cases, the
///         contract and the live fixture do not change.
///     </para>
/// </summary>
[TestFixture]
public class ScriptedAiParserContractTests : ParserContractTests
{
    protected override IAIParser CreateAi(ParserCase scripted)
    {
        return new OpenAIParser(null, new ScriptedCompletion(AsStructuredOutput(scripted.ModelAnswer!)));
    }

    /// <summary>The answer as OpenAI's structured output: the JSON object the parser's schema describes.</summary>
    private static string AsStructuredOutput(ModelAnswer answer)
    {
        return JsonConvert.SerializeObject(new
        {
            intent = answer.Intent,
            verb = answer.Verb,
            nouns = answer.Nouns ?? [],
            preposition = answer.Preposition,
            direction = answer.Direction,
            adjective = answer.Adjective
        });
    }

    private sealed class ScriptedCompletion(string completion) : IChatCompletionClient
    {
        public Task<string> CompleteChatAsync(IReadOnlyList<ChatMessage> messages, ChatCompletionOptions options)
        {
            return Task.FromResult(completion);
        }
    }
}

using Model.Intent;

namespace Model.AIParsing;

public interface IAIParser
{
    string LanguageModel { get; }
    Task<IntentBase> AskTheAIParser(string input, string locationDescription, string sessionId);
}
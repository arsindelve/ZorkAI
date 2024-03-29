using Model.Intent;

namespace Model.AIParsing;

public interface IAIParser
{
    Task<IntentBase> AskTheAIParser(string input, string sessionId);
}
using Model.Intent;

namespace Model;

public interface IAIParser
{
    Task<IntentBase> AskTheAIParser(string input, string sessionId);
}
using ZorkAI.OpenAI;

namespace ChatLambda;

/// <summary>
///     Chooses each companion's conversation backend: the cloud LangGraph Lambda (default), or a
///     <see cref="LocalCompanionChat" /> against the configured OpenAI-compatible endpoint when
///     running self-hosted (issue #383). Characters call this in their field initializers, so a
///     god-mode Repository rebuild re-resolves to the correct backend automatically.
/// </summary>
public static class CompanionChatFactory
{
    public static IChatWithFloyd Floyd(string localSystemPrompt)
    {
        return IsSelfHosted ? new LocalCompanionChat(localSystemPrompt) : new ChatWithFloyd(null);
    }

    public static IChatWithBlather Blather(string localSystemPrompt)
    {
        return IsSelfHosted ? new LocalCompanionChat(localSystemPrompt) : new ChatWithBlather(null);
    }

    public static IChatWithAmbassador Ambassador(string localSystemPrompt)
    {
        return IsSelfHosted ? new LocalCompanionChat(localSystemPrompt) : new ChatWithAmbassador(null);
    }

    private static bool IsSelfHosted => OpenAIEndpointSettings.FromEnvironment().IsSelfHosted;
}

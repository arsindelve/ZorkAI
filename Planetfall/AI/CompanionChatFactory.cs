using GameEngine;

namespace Planetfall.AI;

/// <summary>
///     Chooses each companion's conversation backend: the cloud LangGraph Lambda (default), or a
///     <see cref="LocalCompanionChat" /> against the configured OpenAI-compatible endpoint when
///     running self-hosted (issue #383). Characters call this in their field initializers, so a
///     god-mode Repository rebuild re-resolves to the correct backend automatically.
///     <para>
///     Gated on the explicit <see cref="SelfHostedMode" /> opt-in, not on whether the AI endpoint is
///     custom. The choice here is between invoking an AWS Lambda and not having AWS at all, so it
///     must not turn on merely because someone pointed a deployed function's OPENAI_BASE_URL at a
///     gateway — that would silently take Floyd off the LangGraph function in production, losing the
///     response metadata his Repair Room actions depend on.
///     </para>
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

    private static bool IsSelfHosted => SelfHostedMode.IsEnabled;
}

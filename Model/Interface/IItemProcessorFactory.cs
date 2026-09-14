using Model.AIParsing;

namespace Model.Interface;

public interface IItemProcessorFactory
{
    List<IVerbProcessor> GetProcessors(object item);

    /// <summary>
    /// The agentic fall-through narrator seam (issue #136), consulted by the intent engines when an
    /// unhandled action targets an inventory item. Null when unavailable (tests, degraded startup);
    /// the engines then keep the narration-only fall-through behavior.
    /// </summary>
    IAgenticActionParser? AgenticActionParser { get; }
}
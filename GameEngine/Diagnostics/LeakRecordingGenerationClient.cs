using CloudWatch;
using CloudWatch.Model;
using Model.AIGeneration;
using Model.AIGeneration.Requests;
using Model.Interface;

namespace GameEngine.Diagnostics;

/// <summary>
///     Wraps a generation client and records every time the game fell through to the narrator.
///     <para>
///         This exists to make "is this part of the game finished?" a measurement rather than a
///         judgement. When an object or a verb has not been implemented, the engine does not go quiet —
///         it asks the narrator to improvise, and the narrator obliges with something plausible and
///         wrong. A player cannot tell invented behaviour from ported behaviour, which makes an
///         unfinished region actively misleading rather than merely incomplete.
///     </para>
///     <para>
///         Point this at a region, drive it, and the recorded leaks <i>are</i> the to-do list. A region
///         is finished when the list is empty for everything the original answers for.
///     </para>
///     <para>
///         Not every leak is a defect: a few requests are legitimate narration (save and restore
///         flavour, winning the game). <see cref="IsCompletenessLeak" /> separates those out.
///     </para>
/// </summary>
public class LeakRecordingGenerationClient(IGenerationClient inner, Func<(string Input, string Location)> stamp)
    : IGenerationClient
{
    private readonly List<NarrationLeak> _leaks = [];

    /// <summary>
    ///     Every fallback recorded so far, in order.
    /// </summary>
    public IReadOnlyList<NarrationLeak> Leaks => _leaks;

    /// <summary>
    ///     Only the fallbacks that indicate missing content. Narration the game legitimately delegates —
    ///     save/restore flavour text, the victory message, verbosity acknowledgements — is not a gap.
    /// </summary>
    public IReadOnlyList<NarrationLeak> CompletenessLeaks =>
        _leaks.Where(l => IsCompletenessLeak(l.RequestType)).ToList();

    /// <summary>
    ///     Request types that represent the game delegating on purpose rather than falling short.
    /// </summary>
    private static readonly string[] Legitimate =
    [
        nameof(AfterSaveGameRequest), nameof(BeforeSaveGameRequest), nameof(AfterRestoreGameRequest),
        nameof(BeforeRestoreGameRequest), nameof(RestoreFailedFileNotFoundGameRequest),
        nameof(RestoreFailedUnknownReasonGameRequest), nameof(SaveFailedUnknownReasonGameRequest),
        nameof(WonTheGameRequest), nameof(EngineErrorRequest),
        nameof(MaximumVerbosityRequest), nameof(MediumVerbosityRequest), nameof(MinimumVerbosityRequest),
        nameof(MultipleCommandsRequest), nameof(CompanionRequest)
    ];

    public static bool IsCompletenessLeak(string requestType)
    {
        return !Legitimate.Contains(requestType);
    }

    public void Clear()
    {
        _leaks.Clear();
    }

    public Task<string> GenerateNarration(Request request, string systemPromptAddendum)
    {
        Record(request);
        return inner.GenerateNarration(request, systemPromptAddendum);
    }

    public Task<string> GenerateCompanionSpeech(CompanionRequest request)
    {
        Record(request);
        return inner.GenerateCompanionSpeech(request);
    }

    private void Record(Request request)
    {
        var (input, location) = stamp();
        _leaks.Add(new NarrationLeak(input, location, request.GetType().Name, request.UserMessage));
    }

    public Action? OnGenerate
    {
        get => inner.OnGenerate;
        set => inner.OnGenerate = value;
    }

    public bool IsDisabled
    {
        get => inner.IsDisabled;
        set => inner.IsDisabled = value;
    }

    public string SystemPrompt
    {
        set => inner.SystemPrompt = value;
    }

    public List<(string, string, bool)> LastFiveInputOutputs
    {
        get => inner.LastFiveInputOutputs;
        set => inner.LastFiveInputOutputs = value;
    }

    public Guid TurnCorrelationId
    {
        get => inner.TurnCorrelationId;
        set => inner.TurnCorrelationId = value;
    }

    public ICloudWatchLogger<GenerationLog>? CloudWatchLogger
    {
        get => inner.CloudWatchLogger;
        set => inner.CloudWatchLogger = value;
    }
}

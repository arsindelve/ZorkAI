using Newtonsoft.Json;

namespace Stationfall.Item.Duffy;

/// <summary>
///     The hatch as seen from the Cargo Bay, and the object that owns the one open/closed flag every
///     other view of the hatch reads. See <see cref="SpacetruckHatchBase" /> for why there are three.
/// </summary>
public class SpacetruckHatch : SpacetruckHatchBase
{
    /// <summary>
    ///     The single stored flag. Named for what it is — the raw state, as opposed to any room's view
    ///     of it — and kept public so it serializes under the name saved games already use.
    /// </summary>
    [UsedImplicitly]
    [JsonProperty("IsOpen")]
    public bool StoredIsOpen { get; set; }

    [JsonIgnore]
    protected override bool RawIsOpen
    {
        get => StoredIsOpen;
        set => StoredIsOpen = value;
    }
}

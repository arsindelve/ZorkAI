namespace Model.Item;

/// <summary>
///     Represents a light source that can be turned on and off.
/// </summary>
[ApplicableVerbs("turn on", "turn off")]
public interface IAmALightSourceThatTurnsOnAndOff : ITurnOffAndOn, IAmALightSource;
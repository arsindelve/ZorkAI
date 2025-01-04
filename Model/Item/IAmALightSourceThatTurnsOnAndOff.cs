namespace Model.Item;

/// <summary>
///     Represents a light source that can be turned on and off.
/// </summary>
public interface IAmALightSourceThatTurnsOnAndOff : ITurnOffAndOn, IAmALightSource;
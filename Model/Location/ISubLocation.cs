using Model.Interface;

namespace Model.Location;

/// <summary>
/// This is a sub-location inside another location, such as a vehicle or a specific area within a larger space.
/// </summary>
public interface ISubLocation
{
    /// <summary>
    /// This property represents the description of the sub-location 
    /// </summary>
    /// <value>
    /// The location description provides information about the location, such as its appearance, landmarks,
    /// or any other details that help the player visualize and understand the location.
    /// </value>
    /// <example>
    /// Frigid River, in the magic boat
    /// </example>
    string LocationDescription { get; }

    /// <summary>
    ///  Enter the sub-location
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    string GetIn(IContext context);

    /// <summary>
    /// Leave the sub-location
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    string GetOut(IContext context);
}
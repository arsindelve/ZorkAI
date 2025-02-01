namespace Model.Item;

/// <summary>
/// Represents a clothing item that can be worn or taken off.
/// </summary>
public interface IAmClothing
{
    /// <summary>
    /// Indicates whether the clothing item is currently being worn.
    /// </summary>
    /// <remarks>
    /// This property determines if an item implementing the <see cref="IAmClothing"/> interface
    /// is currently worn by the character. Items being worn cannot be dropped until they are first removed.
    /// </remarks>
    bool BeingWorn { get; set; }
}
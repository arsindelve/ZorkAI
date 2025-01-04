using Model.Item;

namespace Model.Interface;

/// <summary>
/// Add this marker interface when an Item is a person who has a name such as "Blather" or "Floyd". Without this marker
/// the AI generation gets confused and starts to refer to the item as "the blather" or "the floyd". 
/// </summary>
public interface IAmANamedPerson : ICanBeExamined;
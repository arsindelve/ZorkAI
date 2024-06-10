using Model.AIGeneration;

namespace Model.Interface;

/// <summary>
///     Represents an actor that can perform actions, even when you are in another location.
///     For example, the troll (who will attack you as long as it is alive)
///     or Floyd, (who has a chance of saying something funny). Also represents "objects" that
///     can act such as a Dam filling up, a room filling with water, or something like a
///     lamp or candles burning out.
/// </summary>
public interface ITurnBasedActor
{
    /// <summary>
    ///     Performs an action based on the actor's behavior in the given context.
    /// </summary>
    /// <param name="context">The context in which the actor is present.</param>
    /// <param name="client"></param>
    /// <returns>A description of the action performed by the actor.</returns>
    Task<string> Act(IContext context, IGenerationClient client);
}
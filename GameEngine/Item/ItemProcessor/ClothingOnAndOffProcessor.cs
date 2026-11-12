using Model.AIGeneration;
using Model.Interface;
using Model.Item;

namespace GameEngine.Item.ItemProcessor;

public class ClothingOnAndOffProcessor : IVerbProcessor
{
    // The canonical verb comes first: "don" / "doff" are what the parsers reduce "put on" / "take off" to.
    private static readonly VerbFamily DonVerbs = new("don", "wear", "put on", "dress in", "slip on");

    private static readonly VerbFamily DoffVerbs = new("doff", "take off", "remove", "discard", "slip off");

    /// <summary>
    ///     Processes the action of putting on or taking off an item of clothing. 
    /// </summary>
    /// <param name="action">The simple intent representing the action to be performed.</param>
    /// <param name="context">The context in which the action is being performed.</param>
    /// <param name="item">The item to be put on / taken off</param>
    /// <param name="client"></param>
    /// <returns>An InteractionResult indicating the result of the action.</returns>
    public Task<InteractionResult?> Process(SimpleIntent action, IContext context, IInteractionTarget item,
        IGenerationClient client)
    {
        if (string.IsNullOrEmpty(action.Verb))
            return Task.FromResult<InteractionResult?>(null);

        switch (action.Verb.ToLowerInvariant().Trim())
        {
            case var verb when DonVerbs.Contains(verb):
                return Task.FromResult(PutOn(item));

            case var verb when DoffVerbs.Contains(verb):
                return Task.FromResult(TakeOff(item));
        }

        return Task.FromResult<InteractionResult?>(null);
    }

    private static InteractionResult? TakeOff(IInteractionTarget item)
    {
        var castItem = item as IItem;
        if (castItem is null)
            return null;

        var clothing = item as IAmClothing;
        if (clothing is null)
            return null;

        if (!clothing.BeingWorn)
            return new PositiveInteractionResult("You aren't wearing that. ");

        clothing.BeingWorn = false;
        return new PositiveInteractionResult($"You have removed your {castItem.NounsForMatching[0]}. ");
    }

    private static InteractionResult? PutOn(IInteractionTarget item)
    {
        var castItem = item as IItem;
        if (castItem is null)
            return null;

        var clothing = item as IAmClothing;
        if (clothing is null)
            return null;

        clothing.BeingWorn = true;
        return new PositiveInteractionResult($"You are wearing the {castItem.NounsForMatching[0]}. ");
    }
}
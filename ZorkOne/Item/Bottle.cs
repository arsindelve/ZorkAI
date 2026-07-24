using GameEngine;
using GameEngine.Item;
using Model;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;

namespace ZorkOne.Item;

public class Bottle : OpenAndCloseContainerBase, ICanBeTakenAndDropped, ICanBeExamined
{
    protected override int SpaceForItems => 0;

    public override bool IsTransparent => true;

    public override string[] NounsForMatching => ["bottle", "glass bottle"];

    public override int Size => 3;

    public string ExaminationDescription => !Items.Any()
        ? "The glass bottle is empty."
        : Environment.NewLine + ItemListDescription("glass bottle", null);

    string ICanBeTakenAndDropped.OnTheGroundDescription(ILocation currentLocation)
    {
        return !HasEverBeenOpened && !HasEverBeenPickedUp
            ? NeverPickedUpDescription(currentLocation)
            : Items.Any()
                ? "There is a glass bottle here." + Environment.NewLine +
                  ItemListDescription("glass bottle", currentLocation)
                : "There is a glass bottle here.";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return "A bottle is sitting on the table." + Environment.NewLine +
               ItemListDescription("glass bottle", null);
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return Items.Any()
            ? "A glass bottle" + Environment.NewLine + ItemListDescription("glass bottle", currentLocation)
            : "A glass bottle";
    }

    public override void Init()
    {
        StartWithItemInside<Water>();
    }

    /// <summary>
    ///     Mirrors BOTTLE-FUNCTION (zork1/1actions.zil:1491-1507): the bottle responds to THROW and
    ///     MUNG (break/smash/destroy…) by shattering out of play, and to SHAKE by spilling its water
    ///     when open. Without these the verbs fell through to the AI narrator and left the bottle and
    ///     water untouched (issue #388).
    /// </summary>
    public override async Task<InteractionResult?> RespondToSimpleInteraction(SimpleIntent action,
        IContext context, IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (!action.MatchNoun(NounsForMatching))
            return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);

        // THROW bottle → "The bottle hits the far wall and shatters." (bottle removed, water spilled).
        if (action.MatchVerb(Verbs.ThrowVerbs))
            return Shatter("The bottle hits the far wall and shatters. ");

        // MUNG bottle (break/smash/destroy…) → "A brilliant maneuver destroys the bottle."
        if (action.MatchVerb(Verbs.BreakVerbs))
            return Shatter("A brilliant maneuver destroys the bottle. ");

        // SHAKE bottle → only spills when the bottle is open and holds water; otherwise no effect
        // (fall through to the default handling / narrator, matching the original's <RFALSE>).
        if (action.MatchVerb(["shake", "agitate", "rattle"]) && IsOpen && HasItem<Water>())
            return new PositiveInteractionResult(SpillWater());

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }

    /// <summary>
    ///     Removes the bottle from play with the given message, spilling any water it holds first
    ///     (the ZIL's shared <c>E?</c> water-spill branch).
    /// </summary>
    private InteractionResult Shatter(string message)
    {
        var water = HasItem<Water>() ? SpillWater() : string.Empty;
        CurrentLocation?.RemoveItem(this);
        return new PositiveInteractionResult(message + water);
    }

    private string SpillWater()
    {
        RemoveItem(Repository.GetItem<Water>());
        return "The water spills to the floor and evaporates. ";
    }
}
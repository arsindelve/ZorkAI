using GameEngine.Location;
using Model.AIGeneration;
using Planetfall.Item.Kalamontee.Admin;
using Planetfall.Item.Kalamontee.Mech;
using Planetfall.Item.Kalamontee.Mech.FloydPart;

namespace Planetfall.Location.Kalamontee.Admin;

public class AdminCorridorSouth : LocationBase, ITurnBasedActor
{
    [UsedImplicitly] public bool HasSeenTheLight { get; set; }

    [UsedImplicitly] public bool HasTakenTheKey { get; set; }

    public override string Name => "Admin Corridor South";

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        if (HasSeenTheLight || HasTakenTheKey)
            return Task.FromResult(string.Empty);

        var chance = Random.Shared.Next(3);

        if (chance == 0)
            return Task.FromResult(
                "\n\nYou catch, out of the corner of your eye, a glint of light from the direction of the floor. ");

        return Task.FromResult(string.Empty);
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.S, Go<CorridorJunction>() },
            { Direction.N, Go<AdminCorridor>() },
            { Direction.E, Go<SanfacE>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This section of hallway seems to have suffered some minor structural damage. The walls are cracked, and " +
            "a jagged crevice crosses the floor. An opening leads east and the corridor heads north and south. " +
            (HasSeenTheLight ? "Lying at the bottom of a narrow crevice is a shiny object. " : "");
    }

    public override void Init()
    {
        StartWithItem<Key>();
    }

    public override Task<string> AfterEnterLocation(IContext context, ILocation previousLocation,
        IGenerationClient generationClient)
    {
        context.RegisterActor(this);
        return base.AfterEnterLocation(context, previousLocation, generationClient);
    }

    public override void OnLeaveLocation(IContext context, ILocation newLocation, ILocation previousLocation)
    {
        context.RemoveActor(this);
        base.OnLeaveLocation(context, newLocation, previousLocation);
    }

    public override async Task<InteractionResult?> RespondToMultiNounInteraction(MultiNounIntent action,
        IContext context)
    {
        if (!context.HasItem<Magnet>())
        {
            if (Repository.GetItem<Magnet>().CurrentLocation == this)
                return new PositiveInteractionResult("You don't have the curved metal bar. ");

            return new NoNounMatchInteractionResult();
        }

        if (action.Match(["put", "place", "hold"], Repository.GetItem<Magnet>().NounsForMatching,
            [
                "ground", "crack", "crevice"
            ], [
                "on", "over", "beside", "next to"
            ]))
        {
            if (HasTakenTheKey)
                return new PositiveInteractionResult("Nothing interesting happens. ");

            HasTakenTheKey = true;
            context.ItemPlacedHere<Key>();

            Repository.GetItem<Floyd>().CommentOnAction(FloydPrompts.MagnetRetrievesKey, context);

            return new PositiveInteractionResult(
                "With a spray of dust and a loud clank, a piece of metal leaps from the crevice and " +
                "affixes itself to the magnet. It is a steel key! With a tug, you remove the key from the magnet. ");
        }

        return await base.RespondToMultiNounInteraction(action, context);
    }

    public override async Task<InteractionResult> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (!action.MatchVerb(["look at", "examine", "look"]))
            return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);

        if (action.MatchNoun(["floor", "ground"]))
            return new PositiveInteractionResult("A narrow, jagged crevice runs across the floor. ");

        if (action.MatchNoun(["crevice", "crack", "light"]))
        {
            HasSeenTheLight = true;
            return new PositiveInteractionResult(
                "Lying at the bottom of the narrow crack, partly covered by layers of dust, is a shiny steel key! ");
        }

        return new PositiveInteractionResult("A narrow, jagged crevice runs across the floor. ");
    }
}
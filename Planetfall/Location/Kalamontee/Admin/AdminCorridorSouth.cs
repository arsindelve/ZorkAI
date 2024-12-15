using GameEngine.Location;
using Model.AIGeneration;
using Model.Movement;
using Planetfall.Item.Kalamontee.Admin;

namespace Planetfall.Location.Kalamontee.Admin;

public class AdminCorridorSouth : LocationBase, ITurnBasedActor
{
    // ReSharper disable once MemberCanBePrivate.Global
    public bool HasSeenTheLight { get; set; }

    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.S, Go<CorridorJunction>() },
            { Direction.N, Go<AdminCorridor>() },
            { Direction.E, Go<SanfacE>() }
        };

    protected override string ContextBasedDescription =>
        "This section of hallway seems to have suffered some minor structural damage. The walls are cracked, and " +
        "a jagged crevice crosses the floor. An opening leads east and the corridor heads north and south. " +
        (HasSeenTheLight ? "Lying at the bottom of a narrow crevice is a shiny object. " : "");

    public override string Name => "Admin Corridor South";

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        if (HasSeenTheLight)
            return Task.FromResult(string.Empty); 
        
        var chance = Random.Shared.Next(3);

        if (chance == 0)
            return Task.FromResult(
                "You catch, out of the corner of your eye, a glint of light from the direction of the floor. ");

        return Task.FromResult(string.Empty);
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

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client)
    {
        if (!action.MatchVerb(["look at", "examine", "look"]))
            return base.RespondToSimpleInteraction(action, context, client);

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
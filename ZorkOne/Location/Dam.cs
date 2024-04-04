using Model.Intent;

namespace ZorkOne.Location;

public class Dam : BaseLocation
{
    public bool SluiceGatesOpen { get; set; }

    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.SW, new MovementParameters { Location = GetLocation<Chasm>() } },
            { Direction.W, new MovementParameters { Location = GetLocation<ReservoirSouth>() } },
            { Direction.S, new MovementParameters { Location = GetLocation<DeepCanyon>() } },
            { Direction.N, new MovementParameters { Location = GetLocation<DamLobby>() } }
        };

    // TODO: Update description based on gates 
    protected override string ContextBasedDescription =>
        "You are standing on the top of the Flood Control Dam #3, which was quite a tourist attraction in times far distant. " +
        "There are paths to the north, south, and west, and a scramble down. The sluice gates on the dam are " +
        "closed. Behind the dam, there can be seen a wide reservoir. Water is pouring over the top of the now abandoned dam. ";

    public override string Name => "Dam";

    public override void Init()
    {
        StartWithItem(GetItem<ControlPanel>(), this);
    }

    public override InteractionResult RespondToMultiNounInteraction(MultiNounIntent action, IContext context)
    {
        string[] verbs = ["turn"];
        string[] prepositions = ["with"];

        if (!action.NounOne.ToLowerInvariant().Trim().Contains("bolt"))
            return base.RespondToMultiNounInteraction(action, context);

        if (!action.NounTwo.ToLowerInvariant().Trim().Contains("wrench"))
            return base.RespondToMultiNounInteraction(action, context);

        if (!verbs.Contains(action.Verb.ToLowerInvariant().Trim()))
            return base.RespondToMultiNounInteraction(action, context);

        if (!prepositions.Contains(action.Preposition.ToLowerInvariant().Trim()))
            return base.RespondToMultiNounInteraction(action, context);

        if (!context.HasItem<Wrench>() && HasItem<Wrench>())
            return new PositiveInteractionResult("You don't have the wrench.");

        if (!context.HasItem<Wrench>())
            return base.RespondToMultiNounInteraction(action, context);

        if (!GetItem<ControlPanel>().GreenBubbleGlowing)
            return new PositiveInteractionResult("The bolt won't turn with your best effort.");

        SluiceGatesOpen = !SluiceGatesOpen;

        if (SluiceGatesOpen)
        {
            GetLocation<ReservoirSouth>().StartDraining(context);
            return new PositiveInteractionResult(
                "The sluice gates open and water pours through the dam. ");
        }

        GetLocation<ReservoirSouth>().StartFilling(context);
        return new PositiveInteractionResult("The sluice gates close and water starts to collect behind the dam. ");
    }
}
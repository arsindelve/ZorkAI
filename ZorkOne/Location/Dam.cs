using GameEngine.Location;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

public class Dam : LocationBase
{
    public bool SluiceGatesOpen { get; set; }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context) =>
        new()
        {
            { Direction.W, new MovementParameters { Location = GetLocation<ReservoirSouth>() } },
            { Direction.S, new MovementParameters { Location = GetLocation<DeepCanyon>() } },
            { Direction.N, new MovementParameters { Location = GetLocation<DamLobby>() } },
            { Direction.E, new MovementParameters { Location = GetLocation<DamBase>() } },
            { Direction.Down, new MovementParameters { Location = GetLocation<DamBase>() } }
        };

    protected override string GetContextBasedDescription(IContext context) =>
        "You are standing on the top of the Flood Control Dam #3, which was quite a tourist attraction in times far distant. " +
        "There are paths to the north, south, and west, and a scramble down. " + GatesDescription();

    public override string Name => "Dam";

    private string GatesDescription()
    {
        var reservoir = GetLocation<ReservoirSouth>();
        if (reservoir.IsDrained && SluiceGatesOpen)
            return
                "The water level behind the dam is low: The sluice gates have been opened. Water rushes through the dam and downstream. ";

        if (reservoir.IsFull && !SluiceGatesOpen)
            return
                "The sluice gates on the dam are closed. Behind the dam, there can be seen a wide reservoir. Water is pouring over the top of the now abandoned dam. ";

        return !SluiceGatesOpen
            ? "The sluice gates are closed. The water level in the reservoir is quite low, but the level is rising quickly. "
            : "The sluice gates are open, and water rushes through the dam. The water level behind the dam is still high. ";
    }

    public override void Init()
    {
        StartWithItem<ControlPanel>();
    }

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client)
    {
        if (action.Verb.ToLowerInvariant() == "turn" && action.Noun?.ToLowerInvariant() == "bolt")
            return new PositiveInteractionResult("Your bare hands don't appear to be enough.");

        return base.RespondToSimpleInteraction(action, context, client);
    }

    public override InteractionResult RespondToMultiNounInteraction(MultiNounIntent action, IContext context)
    {
        string[] verbs = ["turn", "use", "apply"];
        string[] prepositions = ["with", "to", "on", "using"];

        if (!action.NounOne.ToLowerInvariant().Trim().Contains("bolt") &&
            !action.NounTwo.ToLowerInvariant().Trim().Contains("bolt"))
            return base.RespondToMultiNounInteraction(action, context);

        if (!action.NounOne.ToLowerInvariant().Trim().Contains("wrench") &&
            !action.NounTwo.ToLowerInvariant().Trim().Contains("wrench"))
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
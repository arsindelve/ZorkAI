using GameEngine.Location;
using Model.AIGeneration;
using Planetfall.Item.Lawanda.Library;
using Planetfall.Command;
using Planetfall.Item.Lawanda.Lab;

namespace Planetfall.Location.Lawanda.Lab;

internal class RadiationLab : LocationBase, ITurnBasedActor
{
    public override string Name => "Radiation Lab";

    [UsedImplicitly] public int SickTurnCounter { get; set; }

    public override void Init()
    {
        StartWithItem<Lamp>();
        StartWithItem<BrownSpool>();
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.W, Go<RadiationLockEast>() }
        };
    }

    public override async Task<InteractionResult> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (action.Match(["examine", "look"], ["crack"]) && action.OriginalInput != null && !action.OriginalInput.Contains("through"))
            return new PositiveInteractionResult(
                "The crack is too small to go through, but large enough to look through. ");

        if (action.Match(["look"], ["crack"]) && action.OriginalInput != null && action.OriginalInput.Contains("through"))
            return new PositiveInteractionResult(
                "You see a dimly lit Bio Lab. Sinister shapes lurk about within. ");

        if (action.Match(["examine", "look"], ["equipment"]))
            return new PositiveInteractionResult(
                "The equipment here is so complicated that you couldn't even begin to figure out how to operate it. ");

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This room is filled with unfathomable equipment and large canisters bearing radioactive warnings. Many of " +
            "the canisters are split open. You can see the Bio Lab through a large crack in the south wall. " +
            "Sinister-looking forms move about within the Bio Lab. Although the lights here are off, the room is " +
            "suffused with a pale blue glow. ";
    }

    public override string BeforeEnterLocation(IContext context, ILocation previousLocation)
    {
        context.RegisterActor(this);
        return base.BeforeEnterLocation(context, previousLocation);
    }

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        SickTurnCounter++;

        switch (SickTurnCounter)
        {
            case 5:
                return Task.FromResult("\nYou suddenly feel sick and dizzy. ");

            case 6:
                return Task.FromResult(
                    "\nYou feel incredibly nauseous and begin vomiting. Also, all your hair has fallen out. ");

            case 7:
                context.RemoveActor(this);

                return Task.FromResult(new DeathProcessor()
                    .Process("\nIt seems you have picked up a bad case of radiation poisoning. ", context)
                    .InteractionMessage);
        }

        return Task.FromResult("");
    }
}

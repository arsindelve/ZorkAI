using Model.AIGeneration;
using Planetfall.Command;
using Planetfall.Item.Lawanda.BioLab;

namespace Planetfall.Item.Lawanda.CryoElevator;

public class CryoElevatorButton : ItemBase, ITurnBasedActor
{
    public override string[] NounsForMatching => ["button", "elevator button"];

    [UsedImplicitly] public bool CountdownActive { get; set; }

    [UsedImplicitly] public int TurnsRemaining { get; set; } = 3;

    [UsedImplicitly] public bool AlreadyArrived { get; set; }

    public override Task<InteractionResult?> RespondToSimpleInteraction(
        SimpleIntent action, IContext context, IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (!action.MatchVerb(["push", "press"]))
            return base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);

        // Hilarious death if player pushes button after arriving
        if (AlreadyArrived)
        {
            return Task.FromResult<InteractionResult?>(
                new DeathInteractionResult(
                    new DeathProcessor().Process(
                        "Stunning. After days of surviving on a hostile, plague-ridden planet, solving several of Infocom's " +
                        "toughest puzzles, and coming within one move of completing Planetfall, you blow it all in one " +
                        "amazingly dumb input.\n\nThe doors close and the elevator rises quickly to the top of the shaft. " +
                        "The doors open, and the mutants, which were waiting impatiently in the ProjCon Office for just " +
                        "such an occurence, happily saunter in and begin munching. ",
                        context).InteractionMessage,
                    ((PlanetfallContext)context).DeathCounter));
        }

        if (CountdownActive)
            return Task.FromResult<InteractionResult?>(new PositiveInteractionResult("Nothing happens. "));

        // Start countdown
        CountdownActive = true;
        TurnsRemaining = 3;

        // Stop the chase scene
        var chaseManager = Repository.GetItem<ChaseSceneManager>();
        chaseManager.StopChase();
        context.RemoveActor(chaseManager);

        // Add this as an actor if not already
        if (!context.Actors.Contains(this))
            context.RegisterActor(this);

        // Award points
        context.AddPoints(5);

        return Task.FromResult<InteractionResult?>(
            new PositiveInteractionResult(
                "The elevator door closes just as the monsters reach it! You slump back against the wall, " +
                "exhausted from the chase. The elevator begins to move downward. "));
    }

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        if (!CountdownActive)
            return Task.FromResult(string.Empty);

        TurnsRemaining--;

        if (TurnsRemaining <= 0)
        {
            CountdownActive = false;
            AlreadyArrived = true;
            context.RemoveActor(this);

            return Task.FromResult("The elevator door opens onto a room to the north. ");
        }

        return Task.FromResult(string.Empty);
    }
}
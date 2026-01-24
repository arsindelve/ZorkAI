using Model.AIGeneration;
using Planetfall.Command;
using Planetfall.Item.Lawanda.BioLab;
using Planetfall.Location.Lawanda.LabOffice;

namespace Planetfall.Item.Lawanda.CryoElevator;

public class CryoElevatorButton : ItemBase, ITurnBasedActor
{
    public override string[] NounsForMatching => ["button", "elevator button"];

    [UsedImplicitly] public bool CountdownActive { get; set; }

    [UsedImplicitly] public int TurnsRemaining { get; set; } = 100;

    [UsedImplicitly] public bool AlreadyArrived { get; set; }

    public override Task<InteractionResult?> RespondToSimpleInteraction(
        SimpleIntent action, IContext context, IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (!action.MatchVerb(["push", "press"]))
            return base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);

        // Hilarious death if player pushes button after arriving
        if (AlreadyArrived)
        {
            context.RemoveActor(this);
            return Task.FromResult<InteractionResult?>(
                new DeathInteractionResult(
                    new DeathProcessor().Process(
                        "You push the button again. The elevator lurches and begins ascending back up! " +
                        "Unfortunately, the mutants are still waiting at the top. The elevator doors open " +
                        "and you are immediately torn apart by the rat-ant, troll, grue, and triffid. ",
                        context).InteractionMessage,
                    ((PlanetfallContext)context).DeathCounter));
        }

        if (CountdownActive)
        {
            return Task.FromResult<InteractionResult?>(
                new PositiveInteractionResult("The elevator is already descending. "));
        }

        // Start countdown
        CountdownActive = true;
        TurnsRemaining = 100;

        // Stop the chase scene
        var chaseManager = Repository.GetItem<ChaseSceneManager>();
        chaseManager.StopChase();

        // Add this as an actor if not already
        if (!context.Actors.Contains(this))
            context.RegisterActor(this);

        // Award points
        context.AddPoints(5);

        return Task.FromResult<InteractionResult?>(
            new PositiveInteractionResult(
                "You push the button. The elevator doors close and it begins its long descent " +
                "to the cryogenic anteroom. The mutants' sounds fade away above you. "));
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

            // Move player to Cryo Anteroom
            var cryoAnteroom = Repository.GetLocation<CryoAnteroomLocation>();
            context.CurrentLocation = cryoAnteroom;

            context.RemoveActor(this);

            return Task.FromResult(
                "After a long descent, the elevator comes to a stop. The doors open, revealing " +
                "the cryogenic anteroom. You've made it! ");
        }

        // Progress messages
        if (TurnsRemaining == 75)
            return Task.FromResult("The elevator continues its descent. ");

        if (TurnsRemaining == 50)
            return Task.FromResult("You're about halfway down. ");

        if (TurnsRemaining == 25)
            return Task.FromResult("The elevator is getting close to the bottom. ");

        if (TurnsRemaining == 10)
            return Task.FromResult("Almost there... ");

        return Task.FromResult(string.Empty);
    }
}
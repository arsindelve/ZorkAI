using GameEngine.Location;
using Model.AIGeneration;
using Planetfall.Command;
using Planetfall.Item.Lawanda.Lab;

namespace Planetfall.Location.Lawanda.Lab;

internal class CryoElevatorLocation : LocationBase, ITurnBasedActor, IFloydDoesNotTalkHere
{
    public override string Name => "Cryo-Elevator";

    [UsedImplicitly] public int ElevatorTimer { get; set; }

    public override void Init()
    {
        StartWithItem<CryoElevatorDoor>();
    }

    public override async Task<InteractionResult> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (context is not PlanetfallContext pfContext)
            return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);

        // Handle button pushes
        if (action.Match(Verbs.PushVerbs, ["button"]))
        {
            var door = Repository.GetItem<CryoElevatorDoor>();
            var chaseManager = Repository.GetItem<ChaseSceneManager>();

            // First time pushing button
            if (!pfContext.CryoScoreFlag)
            {
                ElevatorTimer = 100;

                // Disable chase scene
                var bioLab = Repository.GetLocation<BioLabLocation>();
                bioLab.ChaseSceneEnabled = false;
                context.RemoveActor(chaseManager);

                door.IsOpen = false;
                pfContext.CryoScoreFlag = true;
                pfContext.Score += 5;

                return new PositiveInteractionResult(
                    "The elevator door closes just as the monsters reach it! You slump back against the wall, exhausted from the chase. " +
                    "The elevator begins to move downward. ");
            }

            // Pushing button after arriving at destination (famous death scene)
            if (door.IsOpen)
            {
                var deathResult = new DeathProcessor().Process(
                    "Stunning. After days of surviving on a hostile, plague-ridden planet, solving several of Infocom's toughest puzzles, " +
                    "and coming within one move of completing Planetfall, you blow it all in one amazingly dumb input.\n\n" +
                    "The doors close and the elevator rises quickly to the top of the shaft. The doors open, and the mutants, " +
                    "which were waiting impatiently in the ProjCon Office for just such an occurence, happily saunter in and begin munching.", pfContext);
                return new DeathInteractionResult(deathResult.InteractionMessage, pfContext.DeathCounter);
            }

            return new PositiveInteractionResult("Nothing happens. ");
        }

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        var door = Repository.GetItem<CryoElevatorDoor>();
        var pfContext = context as PlanetfallContext;

        return new Dictionary<Direction, MovementParameters>
        {
            {
                Direction.N,
                new MovementParameters
                {
                    CanGo = _ => door.IsOpen,
                    CustomFailureMessage = "The elevator door is closed. ",
                    Location = pfContext?.CryoScoreFlag == true
                        ? GetLocation<CryoAnteroom>()
                        : GetLocation<ProjConOffice>()
                }
            }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        var door = Repository.GetItem<CryoElevatorDoor>();
        var doorState = door.IsOpen ? "open" : "closed";

        return $"This is a large, plain elevator with one solitary button and a door to the north which is {doorState}. ";
    }

    public override void OnLeaveLocation(IContext context, ILocation newLocation, ILocation previousLocation)
    {
        context.RemoveActor(this);
    }

    public override string BeforeEnterLocation(IContext context, ILocation previousLocation)
    {
        context.RegisterActor(this);
        return base.BeforeEnterLocation(context, previousLocation);
    }

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        if (ElevatorTimer > 0)
        {
            ElevatorTimer--;

            if (ElevatorTimer == 0)
            {
                var door = Repository.GetItem<CryoElevatorDoor>();
                door.IsOpen = true;
                return Task.FromResult("\nThe elevator door opens onto a room to the north. ");
            }
        }

        return Task.FromResult(string.Empty);
    }
}

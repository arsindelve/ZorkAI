using GameEngine.Location;
using Model.AIGeneration;
using Planetfall.Command;
using Planetfall.Item.Lawanda.Lab;

namespace Planetfall.Location.Lawanda.Lab;

internal class LabOfficeLocation : LocationBase, ITurnBasedActor, IFloydDoesNotTalkHere
{
    public override string Name => "Lab Office";

    [UsedImplicitly] public int FungicideTimer { get; set; }

    public override void Init()
    {
        StartWithItem<LabDesk>();
        StartWithItem<OfficeDoor>();
    }

    public override async Task<InteractionResult> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (context is not PlanetfallContext pfContext)
            return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);

        // Handle button pushes
        if (action.Match(Verbs.PushVerbs, ["white button", "button", "light button"]))
        {
            if (pfContext.LabLightsOn)
                return new PositiveInteractionResult("Nothing happens. ");

            pfContext.LabLightsOn = true;
            return new PositiveInteractionResult("You hear the faint sound of a relay clicking. ");
        }

        if (action.Match(Verbs.PushVerbs, ["black button", "button", "dark button", "darkness button"]))
        {
            if (!pfContext.LabLightsOn)
                return new PositiveInteractionResult("Nothing happens. ");

            pfContext.LabLightsOn = false;
            return new PositiveInteractionResult("You hear the faint sound of a relay clicking. ");
        }

        if (action.Match(Verbs.PushVerbs, ["red button", "button", "fungicide button", "emergency button"]))
        {
            pfContext.LabFlooded = true;
            FungicideTimer = 50; // Reset timer to 50 turns

            return new PositiveInteractionResult("You hear a hissing from beyond the door to the west. ");
        }

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        var officeDoor = Repository.GetItem<OfficeDoor>();
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.S, Go<AuxiliaryBooth>() },
            {
                Direction.W,
                new MovementParameters
                {
                    CanGo = _ => officeDoor.IsOpen,
                    CustomFailureMessage = "The office door is closed. ",
                    Location = GetLocation<BioLabLocation>()
                }
            }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        var officeDoor = Repository.GetItem<OfficeDoor>();
        var doorState = officeDoor.IsOpen ? "An open" : "A closed";

        return
            "This is the office for storing files on Bio Lab experiments. A large and messy desk is surrounded by locked files. " +
            "A small booth lies to the south. " +
            $"{doorState} door to the west is labelled \"Biioo Lab.\" You realize with shock and horror that the only way out is through the mutant-infested Bio Lab.\n\n" +
            "On the wall are three buttons: a white button labelled \"Lab Liits On\", a black button labelled \"Lab Liits Of\", " +
            "and a red button labelled \"Eemurjensee Sistum.\" ";
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
        if (context is not PlanetfallContext pfContext)
            return Task.FromResult(string.Empty);

        var messages = string.Empty;

        // Handle fungicide timer
        if (FungicideTimer > 0)
        {
            FungicideTimer--;

            if (FungicideTimer == 0)
            {
                pfContext.LabFlooded = false;

                var bioLab = Repository.GetLocation<BioLabLocation>();
                var officeDoor = Repository.GetItem<OfficeDoor>();

                if (context.CurrentLocation == bioLab)
                {
                    messages = "\nThe last traces of mist in the air vanish. The mutants, recovering quickly, notice you and begin salivating. ";
                }
                else if (context.CurrentLocation == this && officeDoor.IsOpen)
                {
                    messages = "\nThe mist in the Bio Lab clears. The mutants recover and rush toward the door! ";
                }
            }
        }

        // Check door state for end-of-turn effects
        var door = Repository.GetItem<OfficeDoor>();

        if (door.IsOpen)
        {
            if (pfContext.LabFlooded)
            {
                messages += "\nThrough the open doorway you can see the Bio Lab. It seems to be filled with a light mist. " +
                           "Horrifying biological nightmares stagger about making choking noises. ";
            }
            else
            {
                // Player dies!
                var deathResult = new DeathProcessor().Process(
                    "Mutated monsters from the Bio Lab pour into the office. You are devoured.", pfContext);
                return Task.FromResult(deathResult.InteractionMessage);
            }
        }

        return Task.FromResult(messages);
    }
}

using System.Text;
using GameEngine.Location;
using Model.AIGeneration;
using Planetfall.Command;
using Planetfall.Item.Lawanda;
using Planetfall.Location.Computer;
using Utilities;

namespace Planetfall.Location.Lawanda;

internal class MiniaturizationBooth : LocationBase
{
    public override string Name => "Miniaturization Booth";

    public bool IsEnabled { get; set; }

    public override void Init()
    {
        StartWithItem<MiniaturizationSlot>();
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.N, Go<ComputerRoom>() }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is a small room barely large enough for one person. Mounted on the wall is a small slot, " +
            "and next to it a keyboard with numeric keys. The exit is to the north. ";
    }

    public override async Task<InteractionResult> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (action.MatchVerb(Verbs.TypeVerbs.Union(["press", "push", "key"]).ToArray()))
        {
            if (!IsEnabled)
                return new PositiveInteractionResult("A recording says \"Internal computer repair booth not activated.\"");

            var keyPress = action.Noun.ToInteger();

            if (keyPress.HasValue)
            {
                if (keyPress.Value == 384)
                {
                    // Teleport to Station 384
                    var sb = new StringBuilder();

                    IsEnabled = false;
                    context.CurrentLocation = Repository.GetLocation<Station384>();

                    sb.AppendLine("You notice the walls of the booth sliding away in all directions, followed by a momentary queasiness in the pit of your stomach...");
                    return new PositiveInteractionResult(sb.ToString());
                }

                // Wrong sector - player dies
                return new DeathProcessor().Process(
                    "Ooops! You seem to have transported yourself into an active sector of the computer. You are fried by powerful electric currents.",
                    context);
            }

            return new PositiveInteractionResult("The keyboard only has numeric keys. ");
        }

        return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }
}
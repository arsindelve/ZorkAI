using GameEngine.Location;
using Model.AIGeneration;
using Planetfall.Command;
using Planetfall.Item.Lawanda;
using Planetfall.Item.Lawanda.Library;
using Utilities;

namespace Planetfall.Location.Lawanda;

internal class Infirmary : LocationBase
{
    public override string Name => "Infirmary";

    public override void Init()
    {
        StartWithItem<RedSpool>();
        StartWithItem<MedicineBottle>();
        StartWithItem<InfirmaryBed>();
    }

    public override Task<InteractionResult> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client,
        IItemProcessorFactory itemProcessorFactory)
    {
        if (action.Match(["examine"], ["equipment"]))
            return Task.FromResult<InteractionResult>(new PositiveInteractionResult(
                "The equipment here is so complicated that you couldn't even begin to figure out how to operate it. "));

        if (action.Match(["examine"], ["shelves", "shelf"]))
            return Task.FromResult<InteractionResult>(new PositiveInteractionResult(
                "The shelves are pretty dusty. "));

        return base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.SE, Go<SystemsCorridorWest>() }
        };
    }

    public override Task<InteractionResult> RespondToSpecificLocationInteraction(string? input, IContext context,
        IGenerationClient client)
    {
        switch (input?.ToLowerInvariant().StripNonChars())
        {
            case "lay down":
            case "sleep":
            case "go to sleep":
            case "lay down in bed":
            case "lay down in the bed":
            case "lie down":
            case "lie down in bed":
            case "lie down in the bed":
            case "get in bed":
            case "enter the bed":
            case "get in the bed":
                var inMessage = Repository.GetItem<InfirmaryBed>().GetIn(context);
                return Task.FromResult<InteractionResult>(new PositiveInteractionResult(inMessage));
        }

        return base.RespondToSpecificLocationInteraction(input, context, client);
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "You have entered a clean, well-lighted place. There are a number of beds, some complicated looking " +
            "equipment, and many shelves that are mostly bare. ";
    }
}

internal class InfirmaryBed : ItemBase, ISubLocation
{
    public override string[] NounsForMatching => ["bed"];

    public override string Name => "Infirmary Bed";

    public string LocationDescription => string.Empty;

    public string GetIn(IContext context)
    {
        var death =
            "You climb into the bed. It is soft and comfortable. After a few moments, a previously unseen panel " +
            "opens, and a diagnostic robot comes wheeling out. It is very rusty and sways unsteadily, bumping into " +
            "several pieces of infirmary equipment as it crosses the room. As the robot straps you to the bed, you " +
            "notice some smoke curling from its cracks. Beeping happily, the robot injects you with all 347 serums " +
            "and medicines it carries. The last thing you notice before you pass out is the robot preparing to saw " +
            "your legs off.";

        return new DeathProcessor().Process(death, context).InteractionMessage;
    }

    public string GetOut(IContext context)
    {
        throw new NotImplementedException();
    }
}
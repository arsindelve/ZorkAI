using Model.AIGeneration;
using Model.Intent;
using Model.Interface;
using Model.Movement;
using ZorkOne.Location.ForestLocation;

namespace ZorkOne.Location;

public class WestOfHouse : BaseLocation
{
    protected override string ContextBasedDescription =>
        "You are standing in an open field west of a white house, with a boarded front door. ";

    public override string Name => "West Of House";

    protected override Dictionary<Direction, MovementParameters> Map => new()
    {
        {
            Direction.S, new MovementParameters { Location = GetLocation<SouthOfHouse>() }
        },
        {
            Direction.N, new MovementParameters { Location = GetLocation<NorthOfHouse>() }
        },
        {
            Direction.W, new MovementParameters { Location = GetLocation<ForestOne>() }
        },
        {
            Direction.E,
            new MovementParameters
            {
                CanGo = _ => false,
                CustomFailureMessage = "The door is boarded and you can't remove the boards."
            }
        }
    };
    
    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client)
    {
        string[] nouns = ["door", "front door"];

        if (action.Match(["examine", "look"], nouns))
            return new PositiveInteractionResult("The door is closed. ");

        if (action.Match(["open"], nouns))
            return new PositiveInteractionResult("The door cannot be opened. ");

        if (action.Match(["examine", "look"], ["house", "white house"]))
            return new PositiveInteractionResult(
                "The house is a beautiful colonial house which is painted white. It is clear that the owners must have been extremely wealthy. ");

        return base.RespondToSimpleInteraction(action, context, client);
    }

    public override void Init()
    {
        StartWithItem(Repository.GetItem<Mailbox>(), this);
    }
}
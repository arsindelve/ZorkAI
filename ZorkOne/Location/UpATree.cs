namespace ZorkOne.Location;

public class UpATree : BaseLocation
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.Down, new MovementParameters { Location = GetLocation<ForestPath>() } },
            {
                Direction.Up,
                new MovementParameters { CanGo = _ => false, CustomFailureMessage = "You cannot climb any higher" }
            }
        };

    public override string Name => "Up A Tree";

    protected override string ContextBasedDescription =>
        "You are about 10 feet above the ground nestled among some large branches. The nearest branch above you is above your reach.\n\n" +
        "Beside you on the branch is a small bird's nest.";

    public override InteractionResult RespondToSpecificLocationInteraction(string? input, IContext context)
    {
        switch (input?.ToLowerInvariant().Trim())
        {
            case "jump":
            case "leap":
            case "jump down":
            case "jump out of tree":
            case "jump out of the tree":
            case "jump down from the tree":

                context.CurrentLocation = Repository.GetLocation<ForestPath>();
                var message =
                    "In a feat of unaccustomed daring, you manage to land on your feet without killing yourself.\n\n";
                message += Repository.GetLocation<ForestPath>().Description;
                return new PositiveInteractionResult(message);
        }

        return base.RespondToSpecificLocationInteraction(input, context);
    }

    public override void Init()
    {
    }
}
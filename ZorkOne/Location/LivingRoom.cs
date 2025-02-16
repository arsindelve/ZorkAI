using GameEngine;
using GameEngine.Location;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;
using Model.Movement;
using ZorkOne.Location.MazeLocation;

namespace ZorkOne.Location;

public class LivingRoom : LocationBase
{
    private bool CyclopsHasCrashedThrough => !GetLocation<CyclopsRoom>().HasItem<Cyclops>();

    public override string Name => "Living Room";

    protected override string GetContextBasedDescription(IContext context)
    {
        return "You are in the living room.  " +
               (!CyclopsHasCrashedThrough
                   ? "There is a doorway to the east, a wooden door with strange gothic lettering to the west, which appears to be nailed shut, "
                   : "There is a doorway to the east. To the west is a cyclops-shaped opening in an old wooden door, above which is some strange gothic lettering, ") +
               "a trophy case, " +
               $"{(Repository.GetItem<Rug>().HasBeenMovedAside
                   ? $"and a rug lying beside {(Repository.GetItem<TrapDoor>().IsOpen ? "an open" : "a closed")} trap door. "
                   : "and a large oriental rug in the center of the room. ")}{GetItem<TrophyCase>().ItemListDescription("trophy case", null)}";
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            {
                Direction.E, new MovementParameters { Location = GetLocation<Kitchen>() }
            },
            {
                Direction.W,
                new MovementParameters
                {
                    CanGo = _ => CyclopsHasCrashedThrough,
                    CustomFailureMessage = "The door is nailed shut.",
                    Location = GetLocation<StrangePassage>()
                }
            },
            {
                Direction.Down,
                new MovementParameters
                {
                    Location = GetLocation<Cellar>(),
                    CanGo = _ =>
                        Repository.GetLocation<LivingRoom>().HasItem<TrapDoor>() &&
                        Repository.GetItem<TrapDoor>().IsOpen,
                    CustomFailureMessage = Repository.GetLocation<LivingRoom>().HasItem<TrapDoor>()
                        ? "The trap door is closed."
                        : "You can't go that way."
                }
            }
        };
    }

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        string[] nouns = ["lettering", "engraving", "engravings", "door"];
        string[] verbs = ["read", "examine"];

        if (action.Match(verbs, nouns))
            return new PositiveInteractionResult(
                "The engravings translate to \"This space intentionally left blank.\"");

        return base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }

    public override void Init()
    {
        StartWithItem<Sword>();
        StartWithItem<Lantern>();
        StartWithItem<Rug>();
        StartWithItem<TrophyCase>();
    }
}
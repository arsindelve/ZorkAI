using GameEngine.IntentEngine;
using GameEngine.Location;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;
using Model.Movement;
using ZorkOne.ActorInteraction;

namespace ZorkOne.Location.MazeLocation;

internal class CyclopsRoom : DarkLocation
{
    private readonly ICombatEngine _combatEngine = new CyclopsCombatEngine();
    private readonly KillSomeoneDecisionEngine<Cyclops> _decisionEngine;

    public CyclopsRoom()
    {
        _decisionEngine = new KillSomeoneDecisionEngine<Cyclops>(_combatEngine);
    }

    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.NW, Go<MazeFifteen>() },
            {
                Direction.E,
                new MovementParameters
                {
                    CanGo = _ => !HasItem<Cyclops>(),
                    Location = GetLocation<StrangePassage>(),
                    CustomFailureMessage = "The east wall is solid rock. "
                }
            },
            {
                Direction.Up,
                new MovementParameters
                {
                    Location = GetLocation<TreasureRoom>(),
                    CanGo = _ => !HasItem<Cyclops>() || GetItem<Cyclops>().IsSleeping,
                    CustomFailureMessage = "The cyclops doesn't look like he'll let you past. "
                }
            }
        };

    protected override string ContextBasedDescription =>
        "This room has an exit on the northwest, and a staircase leading up. " + (HasItem<Cyclops>()
            ? ""
            : "The east wall, previously solid, now has a cyclops-sized opening in it. ");

    public override string Name => "Cyclops Room";

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client)
    {
        var killInteraction = _decisionEngine.DoYouWantToKillSomeoneButYouDidNotSpecifyAWeapon(action, context);
        return killInteraction ?? base.RespondToSimpleInteraction(action, context, client);
    }

    public override InteractionResult RespondToMultiNounInteraction(MultiNounIntent action, IContext context)
    {
        var result = _decisionEngine.DoYouWantToKillSomeone(action, context);
        return result ?? base.RespondToMultiNounInteraction(action, context);
    }

    public override InteractionResult RespondToSpecificLocationInteraction(string? input, IContext context)
    {
        if (string.IsNullOrEmpty(input))
            return base.RespondToSpecificLocationInteraction(input, context);

        if (!new List<string> { "ulysses", "odysseus" }.Contains(input.ToLower().Trim()) 
            || !HasItem<Cyclops>() 
            || GetItem<Cyclops>().IsSleeping)
            return base.RespondToSpecificLocationInteraction(input, context);

        var message =
            "The cyclops, hearing the name of his father's deadly nemesis, flees the room by knocking " +
            "down the wall on the east of the room. ";

        if (context.HasItem<Sword>())
            message += "\nYour sword is no longer glowing. ";

        var loser = GetItem<Cyclops>();
        RemoveItem(loser);
        loser.CurrentLocation = null;
        context.RemoveActor(loser);

        return new PositiveInteractionResult(message);
    }

    public override Task<string> AfterEnterLocation(IContext context, ILocation previousLocation,
        IGenerationClient generationClient)
    {
        if (HasItem<Cyclops>())
            context.RegisterActor(GetItem<Cyclops>());

        return base.AfterEnterLocation(context, previousLocation, generationClient);
    }

    public override void Init()
    {
        StartWithItem<Cyclops>();
    }
}
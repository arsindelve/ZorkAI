using GameEngine.IntentEngine;
using GameEngine.Location;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;
using Model.Movement;
using ZorkOne.ActorInteraction;
using ZorkOne.Location.MazeLocation;

namespace ZorkOne.Location;

public class TrollRoom : DarkLocation
{
    private readonly ICombatEngine _trollAttackEngine = new AdventurerVersusTrollCombatEngine();

    private readonly KillSomeoneDecisionEngine<Troll> _killDecisionEngine;

    public TrollRoom()
    {
        _killDecisionEngine = new KillSomeoneDecisionEngine<Troll>(_trollAttackEngine);
    }
    
    private bool TrollIsAwake => !GetItem<Troll>().IsDead &&
                                 !GetItem<Troll>().IsUnconscious;

    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.S, new MovementParameters { Location = GetLocation<Cellar>() } },
            {
                Direction.E,
                new MovementParameters
                {
                    Location = GetLocation<EastWestPassage>(), CanGo = _ => !TrollIsAwake,
                    CustomFailureMessage = "The troll fends you off with a menacing gesture. "
                }
            },
            {
                Direction.W,
                new MovementParameters
                {
                    Location = GetLocation<MazeOne>(), CanGo = _ => !TrollIsAwake,
                    CustomFailureMessage = "The troll fends you off with a menacing gesture. "
                }
            }
        };

    public override string Name => "The Troll Room";

    protected override string ContextBasedDescription =>
        "This is a small room with passages to the east and south and a forbidding hole leading west. " +
        "Bloodstains and deep scratches (perhaps made by an axe) mar the walls. ";

    public override string BeforeEnterLocation(IContext context, ILocation previousLocation)
    {
        var troll = GetItem<Troll>();

        if (troll.IsDead)
            return base.BeforeEnterLocation(context, previousLocation);

        var axe = GetItem<BloodyAxe>();

        // If you leave him knocked out and then come back,
        // he'll be awake and have picked up his axe off the floor. 

        if (troll.IsUnconscious)
            troll.IsUnconscious = false;

        if (axe.CurrentLocation == GetLocation<TrollRoom>())
            troll.ItemPlacedHere(axe);

        context.RegisterActor(GetItem<Troll>());

        return base.BeforeEnterLocation(context, previousLocation);
    }

    public override void OnLeaveLocation(IContext context, ILocation newLocation, ILocation previousLocation)
    {
        context.RemoveActor(GetItem<Troll>());
        base.OnLeaveLocation(context, newLocation, previousLocation);
    }

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client)
    {
        InteractionResult? killInteraction = _killDecisionEngine.DoYouWantToKillSomeoneButYouDidNotSpecifyAWeapon(action, context);
        return killInteraction ?? base.RespondToSimpleInteraction(action, context, client);
    }

    public override InteractionResult RespondToMultiNounInteraction(MultiNounIntent action, IContext context)
    {
        InteractionResult? result = _killDecisionEngine.DoYouWantToKillSomeone(action, context);
        return result ?? base.RespondToMultiNounInteraction(action, context);
    }
    
    public override Task<string> AfterEnterLocation(IContext context, ILocation previousLocation,
        IGenerationClient generationClient)
    {
        var glow = LocationHelper.CheckSwordGlowingBrightly<Troll, TrollRoom>(context);
        if (!string.IsNullOrEmpty(glow))
            return Task.FromResult(glow);

        return base.AfterEnterLocation(context, previousLocation, generationClient);
    }

    public override void Init()
    {
        StartWithItem<Troll>();
    }
}
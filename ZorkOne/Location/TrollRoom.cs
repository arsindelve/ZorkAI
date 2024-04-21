using Model.Intent;
using ZorkOne.ActorInteraction;

namespace ZorkOne.Location;

public class TrollRoom : DarkLocation
{
    private readonly AdventurerVersusTrollCombatEngine _attackEngine = new();

    private bool TrollIsAwakeAndArmed => !GetItem<Troll>().IsDead &&
                                         !GetItem<Troll>().IsUnconscious &&
                                         GetItem<BloodyAxe>().CurrentLocation == GetItem<Troll>();

    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.S, new MovementParameters { Location = GetLocation<Cellar>() } },
            {
                Direction.E,
                new MovementParameters
                {
                    // TODO: Even unarmed, the troll blocks your passage. 
                    Location = GetLocation<EastWestPassage>(), CanGo = _ => !TrollIsAwakeAndArmed,
                    CustomFailureMessage = "The troll fends you off with a menacing gesture. "
                }
            }
        };

    public override string Name => "The Troll Room";

  
    
    protected override string ContextBasedDescription =>
        "This is a small room with passages to the east and south and a forbidding hole leading west. " +
        "Bloodstains and deep scratches (perhaps made by an axe) mar the walls. ";

    public override string BeforeEnterLocation(IContext context)
    {
        var troll = GetItem<Troll>();

        if (troll.IsDead)
            return base.BeforeEnterLocation(context);

        var axe = GetItem<BloodyAxe>();

        // If you leave him knocked out and then come back,
        // he'll be awake and have picked up his axe off the floor. 

        if (troll.IsUnconscious) troll.IsUnconscious = false;

        if (axe.CurrentLocation == GetLocation<TrollRoom>())
            troll.ItemPlacedHere(axe);

        context.RegisterActor(GetItem<Troll>());

        return base.BeforeEnterLocation(context);
    }

    public override void OnLeaveLocation(IContext context)
    {
        context.RemoveActor(GetItem<Troll>());
        base.OnLeaveLocation(context);
    }

    public override InteractionResult RespondToMultiNounInteraction(MultiNounIntent action, IContext context)
    {
        string[] verbs = ["kill", "attack", "defeat", "destroy", "murder", "use"];
        string[] prepositions = ["with", "using", "by", "to"];

        if (!action.NounOne.ToLowerInvariant().Trim().Contains("troll")  && !action.NounTwo.ToLowerInvariant().Trim().Contains("troll"))
            return base.RespondToMultiNounInteraction(action, context);

        if (GetItem<Troll>().IsDead)
            return base.RespondToMultiNounInteraction(action, context);

        var nounTwo = Repository.GetItem(action.NounTwo);
        {
            if (nounTwo is not IWeapon)
                return base.RespondToMultiNounInteraction(action, context);

            if (nounTwo.CurrentLocation != context)
                return new PositiveInteractionResult($"You don't have the {nounTwo.Name}. ");
        }

        if (!verbs.Contains(action.Verb.ToLowerInvariant().Trim()))
            return base.RespondToMultiNounInteraction(action, context);

        if (!prepositions.Contains(action.Preposition.ToLowerInvariant().Trim()))
            return base.RespondToMultiNounInteraction(action, context);

        return _attackEngine.Attack(context);
    }

    public override string AfterEnterLocation(IContext context)
    {
        // TODO: Sword should glow brightly even when the troll is disarmed 
        
        var swordInPossession = context.HasItem<Sword>();

        if (TrollIsAwakeAndArmed && swordInPossession)
            return "Your sword has begun to glow very brightly. ";

        return string.Empty;
    }

    public override void Init()
    {
        StartWithItem(Repository.GetItem<Troll>(), this);
    }
}
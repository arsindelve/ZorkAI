using Model.AIGeneration;
using Model.Intent;
using ZorkOne.ActorInteraction;

namespace ZorkOne.Location;

public class TrollRoom : DarkLocation
{
    private static readonly string[] KillVerbs = ["kill", "attack", "defeat", "destroy", "murder", "use", "stab"];
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

    public override void OnLeaveLocation(IContext context)
    {
        context.RemoveActor(GetItem<Troll>());
        base.OnLeaveLocation(context);
    }

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client)
    {
        if (action.MatchVerb(KillVerbs))
        {
            if (context.Items.OfType<IWeapon>().Count() == 1)
            {
                // Assume they want to kill the troll with the only weapon they have
                var weaponName = context.Items.OfType<IWeapon>().Cast<IItem>().Single().NounsForMatching.First();
                var multiNounIntent = new MultiNounIntent
                {
                    Verb = action.Verb,
                    NounOne = action.Noun ?? "",
                    Preposition = "with",
                    OriginalInput = action.OriginalInput ?? "",
                    NounTwo = weaponName
                };
                var response = RespondToMultiNounInteraction(multiNounIntent, context);
                return new PositiveInteractionResult($"(with the {weaponName})\n" + response.InteractionMessage);
            }

            if (context.Items.OfType<IWeapon>().Count() > 1)
                return new PositiveInteractionResult("You'll need to specify which weapon you want to use. ");
        }

        return base.RespondToSimpleInteraction(action, context, client);
    }

    public override InteractionResult RespondToMultiNounInteraction(MultiNounIntent action, IContext context)
    {
        string[] prepositions = ["with", "using", "by", "to"];

        if (!action.NounOne.ToLowerInvariant().Trim().Contains("troll") &&
            !action.NounTwo.ToLowerInvariant().Trim().Contains("troll"))
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

        if (!action.MatchVerb(KillVerbs))
            return base.RespondToMultiNounInteraction(action, context);

        if (!prepositions.Contains(action.Preposition.ToLowerInvariant().Trim()))
            return base.RespondToMultiNounInteraction(action, context);

        return _attackEngine.Attack(context);
    }

    public override string AfterEnterLocation(IContext context, ILocation previousLocation)
    {
        // TODO: Sword should glow brightly even when the troll is disarmed 

        var swordInPossession = context.HasItem<Sword>();

        if (TrollIsAwakeAndArmed && swordInPossession)
            return "\nYour sword has begun to glow very brightly. ";

        return base.AfterEnterLocation(context, previousLocation);
    }

    public override void Init()
    {
        StartWithItem(Repository.GetItem<Troll>(), this);
    }
}
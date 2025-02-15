using GameEngine;
using GameEngine.Item.ItemProcessor;
using GameEngine.Location;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

internal class EntranceToHades : DarkLocation
{
    public override string Name => "Entrance to Hades";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.Up, new MovementParameters { Location = GetLocation<CaveSouth>() } },
            {
                Direction.S,
                new MovementParameters
                {
                    Location = GetLocation<LandOfTheDead>(),
                    CanGo = _ => !HasItem<Spirits>(),
                    CustomFailureMessage = "Some invisible force prevents you from passing through the gate. "
                }
            }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return """
               You are outside a large gateway, on which is inscribed
                
                 Abandon every hope all ye who enter here!
                
               The gate is open; through it you can see a desolation, with a pile of mangled bodies in one corner. Thousands of voices, lamenting some hideous fate, can be heard.
               """ + (HasItem<Spirits>()
            ? " The way through the gate is barred by evil spirits, who jeer at your attempts to pass. \n"
            : "\n");
    }

    public override Task<string> AfterEnterLocation(IContext context, ILocation previousLocation,
        IGenerationClient generationClient)
    {
        var glow = LocationHelper.CheckSwordGlowingBrightly<Spirits, EntranceToHades>(context);
        return !string.IsNullOrEmpty(glow)
            ? Task.FromResult(glow)
            : base.AfterEnterLocation(context, previousLocation, generationClient);
    }

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client)
    {
        string[] verbs = ["ring", "activate", "shake"];
        if (Items.Contains(Repository.GetItem<Spirits>()) &&
            action.Match(verbs, Repository.GetItem<BrassBell>().NounsForMatching))
            return RingTheBell(context);

        return base.RespondToSimpleInteraction(action, context, client);
    }

    private InteractionResult RingTheBell(IContext context)
    {
        if (!context.HasItem<BrassBell>() && GetItem<BrassBell>().CurrentLocation == GetLocation<EntranceToHades>())
            return new PositiveInteractionResult("The bell is too hot to reach. ");

        var returnValue = "";
        var bell = Repository.GetItem<BrassBell>();
        var spirits = Repository.GetItem<Spirits>();
        var candles = Repository.GetItem<Candles>();

        returnValue += bell.BecomesRedHot(context);
        returnValue += spirits.BecomeStunned(context);

        if (context.HasItem<Candles>())
        {
            context.Drop(candles);
            returnValue += "\rIn your confusion, the candles drop to the ground" +
                           (candles.IsOn ? " and they are out. " : ".");

            if (candles.IsOn)
                returnValue += new TurnOnOrOffProcessor().Process(new SimpleIntent
                    {
                        Noun = candles.NounsForMatching.First(),
                        Verb = "turn off"
                    }, context, candles, null!)
                    ?.InteractionMessage;
        }

        return new PositiveInteractionResult(returnValue);
    }

    public override void Init()
    {
        StartWithItem<Spirits>();
        StartWithItem<Bodies>();
        StartWithItem<Gate>();
    }
}
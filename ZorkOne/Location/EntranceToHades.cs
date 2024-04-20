namespace ZorkOne.Location;

internal class EntranceToHades : DarkLocation
{
    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.Up, new MovementParameters { Location = GetLocation<Cave>() } },
            {
                Direction.S,
                new MovementParameters
                {
                    CanGo = _ => !HasItem<Spirits>(),
                    CustomFailureMessage = "Some invisible force prevents you from passing through the gate."
                }
            }
        };

    protected override string ContextBasedDescription => """
                                                         You are outside a large gateway, on which is inscribed
                                                          
                                                           Abandon every hope all ye who enter here!
                                                          
                                                         The gate is open; through it you can see a desolation, with a pile of mangled bodies in one corner. Thousands of voices, lamenting some hideous fate, can be heard.
                                                         The way through the gate is barred by evil spirits, who jeer at your attempts to pass. 
                                                         """;

    public override string Name => "Entrance to Hades";

    public override string AfterEnterLocation(IContext context)
    {
        var swordInPossession = context.HasItem<Sword>();
        var spiritsAlive = Repository.GetItem<Spirits>().CurrentLocation == Repository.GetLocation<EntranceToHades>();

        if (spiritsAlive && swordInPossession)
            return "\nYour sword has begun to glow very brightly. ";

        return base.AfterEnterLocation(context);
    }

    public override InteractionResult RespondToSpecificLocationInteraction(string? input, IContext context)
    {
        if (string.IsNullOrWhiteSpace(input))
            return base.RespondToSpecificLocationInteraction(input, context);
        
        if (input.ToLowerInvariant().Contains("ring") && input.ToLowerInvariant().Contains("bell"))
            return RingTheBell(context);

        return base.RespondToSpecificLocationInteraction(input, context);
    }

    private InteractionResult RingTheBell(IContext context)
    {
        // TODO: Check spirits are still here. 
        // TODO: Check I have the bell

        if (!context.HasItem<BrassBell>() && GetItem<BrassBell>().CurrentLocation == GetLocation<EntranceToHades>())
            return new PositiveInteractionResult("You don't have the brass bell. ");

        var returnValue = "";
        var bell = Repository.GetItem<BrassBell>();
        returnValue += bell.BecomesRedHot(context);

        return new PositiveInteractionResult(returnValue);
    }

    public override void Init()
    {
        StartWithItem(GetItem<Spirits>(), this);
    }
}
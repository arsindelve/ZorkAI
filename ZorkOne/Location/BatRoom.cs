using GameEngine;
using GameEngine.Location;
using Model.AIGeneration;
using Model.Interface;
using Model.Movement;
using Newtonsoft.Json;
using ZorkOne.Location.CoalMineLocation;

namespace ZorkOne.Location;

internal class BatRoom : DarkLocation
{
    [UsedImplicitly] [JsonIgnore]
    public IRandomChooser Chooser { get; set; } = new RandomChooser();

    public override string Name => "Bat Room";

    /// <summary>
    /// The text for the bat carrying the player off is shared between entering the room without
    /// garlic, and provoking the bat (attacking/taking it) while it's still here.
    /// </summary>
    internal const string CarryOffText = """
                                          Fweep!
                                              Fweep!
                                              Fweep!
                                              Fweep!

                                          The bat grabs you by the scruff of your neck and lifts you away....
                                          """;

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            {
                Direction.S, new MovementParameters { Location = GetLocation<SqueakyRoom>() }
            },
            {
                Direction.E, new MovementParameters { Location = GetLocation<ShaftRoom>() }
            }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return "You are in a small room which has doors only to the east and south. ";
    }

    /// <summary>
    /// The bat is repelled by garlic, whether it's in the adventurer's inventory or loose on the
    /// floor of this room. Garlic stashed inside a closed container (like the sack) doesn't count -
    /// HasItem&lt;T&gt;() is intentionally non-recursive.
    /// </summary>
    internal bool IsSafeFromBat(IContext context)
    {
        return context.HasItem<Garlic>() || HasItem<Garlic>();
    }

    /// <summary>
    /// Picks one of the eight BAT-DROPS rooms at random and relocates the player there.
    /// </summary>
    internal ILocation CarryPlayerOff(IContext context)
    {
        var destination = Chooser.Choose(DropLocations());
        context.CurrentLocation = destination;
        return destination;
    }

    private List<ILocation> DropLocations()
    {
        return
        [
            GetLocation<CoalMineOne>(),
            GetLocation<CoalMineTwo>(),
            GetLocation<CoalMineThree>(),
            GetLocation<CoalMineFour>(),
            GetLocation<LadderTop>(),
            GetLocation<LadderBottom>(),
            GetLocation<SqueakyRoom>(),
            GetLocation<MineEntrance>()
        ];
    }

    public override string BeforeEnterLocation(IContext context, ILocation previousLocation)
    {
        if (!IsSafeFromBat(context))
        {
            // Relocate now - LookProcessor.LookAround (called next by MoveEngine.Go) will describe
            // the drop room, so we must not also append a description here or it prints twice.
            CarryPlayerOff(context);
            return CarryOffText;
        }

        return base.BeforeEnterLocation(context, previousLocation);
    }

    public override Task<string> AfterEnterLocation(IContext context, ILocation previousLocation,
        IGenerationClient generationClient)
    {
        var response = "";

        if (IsSafeFromBat(context))
            response +=
                "\nIn the corner of the room on the ceiling is a large vampire bat who is obviously deranged and holding his nose. ";

        response += LocationHelper.CheckSwordGlowingBrightly<Bat, BatRoom>(context);

        if (!string.IsNullOrEmpty(response))
            return Task.FromResult(response);

        return base.AfterEnterLocation(context, previousLocation, generationClient);
    }

    public override void Init()
    {
        StartWithItem<Bat>();
        StartWithItem<JadeFigurine>();
    }
}
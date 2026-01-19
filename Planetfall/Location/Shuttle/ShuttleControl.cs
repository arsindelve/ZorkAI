using System.Text;
using GameEngine.Location;
using Model.AIGeneration;
using Planetfall.Command;
using Planetfall.Item.Kalamontee.Mech.FloydPart;

// ReSharper disable StaticMemberInGenericType

namespace Planetfall.Location.Shuttle;

/// <summary>
/// Represents the base class for a shuttle control system within a location.
/// Implements turn-based actor behavior and responds to player interactions.
/// <remarks>So check it out: The speed DOES NOT affect how long it takes you to get from
/// one station to the other, or the rate at which we pass landmarks along the way.
/// It is the exact same number of turns if you keep the speed at 5, or if you accelerate the whole time.
/// This is how the original game works! (I ran several experiments to confirm). 
/// The only purpose of the speed is to make sure you decelerate to a reasonable speed before you enter the station.</remarks>
/// </summary>
public abstract class ShuttleControl<TCabin, TControl> : LocationWithNoStartingItems, ITurnBasedActor, IShuttleControl
    where TCabin : class, ILocation, new()
    where TControl : ShuttleControl<TCabin, TControl>, new()
{
    protected const int EndOfTunnel = 24;
    protected const int StartOfTunnel = 0;

    private static readonly Dictionary<int, string> Landmarks = new()
    {
        { 2, "You pass a sign which says \"Limit 45.\"" },
        {
            12,
            "The tunnel levels out and begins to slope upward. A sign flashes by which reads \"Hafwaa Mark -- Beegin Deeseluraashun.\""
        },
        { 20, "You pass a sign, surrounded by blinking red lights, which says \"15.\"" },
        { 21, "You pass a sign, surrounded by blinking red lights, which says \"10.\"" },
        { 22, "You pass a sign, surrounded by blinking red lights, which says \"5.\"" },
        {
            23,
            "The shuttle car is approaching a brightly lit area. As you near it, you make out the concrete platforms of a shuttle station. "
        }
    };

    private static readonly string[] AccelerateVerbs = ["push"];
    private static readonly string[] DecelerateVerbs = ["pull"];
    private static readonly string[] LeverNouns = ["lever", "controls", "control lever"];

    public bool DoorIsClosed => TunnelPosition is > StartOfTunnel and < EndOfTunnel;

    /// <summary>
    /// <remarks>See the note about speed above - it has NO EFFECT on how long it takes you to move from
    /// one platform to the other. This is how the original game works, so I preserved it. </remarks>
    /// </summary>
    [UsedImplicitly]
    public int Speed { get; set; }

    [UsedImplicitly] public int TurnsSinceActivated { get; set; }

    [UsedImplicitly] public virtual int TunnelPosition { get; set; }

    [UsedImplicitly] public bool Activated { get; set; }

    [UsedImplicitly] public bool SpeedChanged { get; set; }

    [UsedImplicitly] public ShuttleLeverPosition LeverPosition { get; set; } = ShuttleLeverPosition.Neutral;

    private string CurrentLeverSetting =>
        LeverPosition switch
        {
            ShuttleLeverPosition.Acceleration => "upper",
            ShuttleLeverPosition.Neutral => "center",
            ShuttleLeverPosition.Deceleration => "lower",
            _ => throw new ArgumentOutOfRangeException(nameof(LeverPosition), LeverPosition, null)
        };

    private string OutTheWindow =>
        TunnelPosition switch
        {
            EndOfTunnel => "a featureless concrete wall. ",
            190 or 195 => "parallel rails ending at a brightly lit station ahead. ",
            _ => "parallel rails running along the floor of a long tunnel, vanishing in the distance. "
        };

    protected abstract Direction LeaveControlsDirection { get; }
    
    public override void Init()
    {
        StartWithItem<ShuttleSlot<TControl>>();
    }

    public override void OnLeaveLocation(IContext context, ILocation newLocation, ILocation previousLocation)
    {
        context.RemoveActor(this);
        Activated = false;
        TurnsSinceActivated = 0;
        LeverPosition = ShuttleLeverPosition.Neutral;
    }

    public override Task<InteractionResult> RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client,
        IItemProcessorFactory itemProcessorFactory)
    {
        if (action.Match(Verbs.OpenVerbs, ["door", "cabin", "cabin door"]))
        {
            if (DoorIsClosed)
                return Task.FromResult<InteractionResult>(new PositiveInteractionResult(
                    "A recorded voice says \"Operator should remain in control cabin while shuttle car is between stations.\""));

            return base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
        }

        if (action.Match(Verbs.CloseVerbs, ["door", "cabin", "cabin door"]))
        {
            if (DoorIsClosed)
                return Task.FromResult<InteractionResult>(new PositiveInteractionResult(
                    "It is closed. "));

            return base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
        }

        if (action.Match(DecelerateVerbs, LeverNouns))
        {
            Repository.GetItem<Floyd>().CommentOnAction(FloydPrompts.ShuttleControlsFirstUse, context);
            return Task.FromResult<InteractionResult>(
                new PositiveInteractionResult(AdjustLever(ShuttleLeverDirection.Pull)));
        }

        if (action.Match(AccelerateVerbs, LeverNouns))
        {
            Repository.GetItem<Floyd>().CommentOnAction(FloydPrompts.ShuttleControlsFirstUse, context);
            return Task.FromResult<InteractionResult>(
                new PositiveInteractionResult(AdjustLever(ShuttleLeverDirection.Push)));
        }

        return base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);
    }

    InteractionResult IShuttleControl.Activate(IContext context)
    {
        if (context is PlanetfallContext { CurrentTime: > 6000 })
            return new PositiveInteractionResult(
                "A recorded voice explains that using the shuttle car during the evening hours requires special authorization.");

        if (TunnelPosition == EndOfTunnel)
            return new PositiveInteractionResult(
                "A recorded voice says \"Use other control cabin. Control activation overridden.\"");

        var result = Activated
            ? "A recorded voice says \"Shuttle controls are already activated.\""
            : "A recording of a deep male voice says \"Shuttle controls activated.\"";

        Activated = true;
        TurnsSinceActivated = 0;
        context.RegisterActor(this);

        return new PositiveInteractionResult(result);
    }

    public async Task<string> Act(IContext context, IGenerationClient client)
    {
        if (TurnsSinceActivated > 10)
        {
            Activated = false;
            TurnsSinceActivated = 0;
            context.RemoveActor(this);
            return "";
        }

        StringBuilder sb = new();
        SpeedChanged = false;

        if (LeverPosition != ShuttleLeverPosition.Neutral)
            sb.AppendLine(await ChangeSpeed());

        if (Speed != 0 || SpeedChanged)
            sb.AppendLine(await Move(context));

        TurnsSinceActivated++;
        return sb.ToString();
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { LeaveControlsDirection, CanLeave() }
        };
    }

    private MovementParameters CanLeave()
    {
        return new MovementParameters
        {
            CanGo = _ => !DoorIsClosed,
            Location = Repository.GetLocation<TCabin>(),
            CustomFailureMessage = "The door is closed. "
        };
    }

    private string AdjustLever(ShuttleLeverDirection direction)
    {
        if (!Activated)
            return "A recorded voice says \"Shuttle controls are not currently activated.\" ";

        // Every time we move the lever, it resets the clock. 
        TurnsSinceActivated = 0;

        switch (direction)
        {
            case ShuttleLeverDirection.Pull:
                switch (LeverPosition)
                {
                    case ShuttleLeverPosition.Acceleration:
                        LeverPosition = ShuttleLeverPosition.Neutral;
                        return "The lever is now in the central position. ";
                    case ShuttleLeverPosition.Neutral:
                        if (Speed == 0)
                            return "The lever immediately pops back to the central position.";
                        LeverPosition = ShuttleLeverPosition.Deceleration;
                        return "The lever is now in the lower position. ";
                    case ShuttleLeverPosition.Deceleration:
                        return "The lever is already in the lower position. ";
                }

                break;

            case ShuttleLeverDirection.Push:
                switch (LeverPosition)
                {
                    case ShuttleLeverPosition.Acceleration:
                        return "The lever is already in the upper position. ";
                    case ShuttleLeverPosition.Neutral:
                        LeverPosition = ShuttleLeverPosition.Acceleration;
                        return "The lever is now in the upper position. ";
                    case ShuttleLeverPosition.Deceleration:
                        LeverPosition = ShuttleLeverPosition.Neutral;
                        return "The lever is now in the central position. ";
                }

                break;
        }

        return "";
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "This is a small control cabin. A control panel contains a slot, a lever, and a display. The lever can be " +
            "set at a central position, or it could be pushed up to a position labelled \"+\", or pulled down to a " +
            $"position labelled \"-\". It is currently at the {CurrentLeverSetting} setting. The display, a digital readout, currently " +
            $"reads {Speed}. Through the cabin window you can see {OutTheWindow} ";
    }

    private Task<string> ChangeSpeed()
    {
        switch (LeverPosition)
        {
            case ShuttleLeverPosition.Acceleration:
                Speed += 5;
                SpeedChanged = true;
                break;
            case ShuttleLeverPosition.Deceleration:
                Speed -= 5;
                SpeedChanged = true;
                break;
        }

        // Moving at all resets the clock. 
        TurnsSinceActivated = 0;

        if (Speed == 0)
        {
            LeverPosition = ShuttleLeverPosition.Neutral;
            return Task.FromResult("The shuttle car comes to a stop and the lever pops back to the central position. ");
        }

        return Task.FromResult(string.Empty);
    }

    private Task<string> Move(IContext context)
    {
        var sb = new StringBuilder();

        if (TunnelPosition == EndOfTunnel)
            return Task.FromResult(Arrived(context));

        if (SpeedChanged)
            switch (Speed)
            {
                case 5 when LeverPosition == ShuttleLeverPosition.Acceleration && TunnelPosition == 0:
                    sb.AppendLine(
                        "The control cabin door slides shut and the shuttle car begins to move forward! The display changes to 5. ");
                    break;

                default:
                    sb.AppendLine($"The shuttle car continues to move. The display blinks, and now reads {Speed}. ");
                    break;
            }
        else
            sb.AppendLine($"The shuttle car continues to move. The display still reads {Speed}. ");

        if (Landmarks.TryGetValue(TunnelPosition, out var landmark))
            sb.AppendLine(landmark);

        // See the note at the top of the class about speed. 
        TunnelPosition++;

        // Remains activated as long as we are moving. 
        TurnsSinceActivated = 0;

        return Task.FromResult(sb.ToString());
    }

    protected abstract void ResetOtherControls();
    
    private string Arrived(IContext context)
    {
        int speedIntoPlatform = Speed;
        
        ResetOtherControls();
        context.RemoveActor(this);
        Activated = false;
        LeverPosition = ShuttleLeverPosition.Neutral;
        Speed = 0;

        return speedIntoPlatform switch
        {
            < 1 =>
                "The shuttle car glides into the station and comes to rest at the concrete platform. You hear the cabin doors slide open.",

            > 1 and < 21 =>
                "The shuttle car rumbles through the station and smashes into the wall at the far end. You are thrown forward into the control panel. Both you and the shuttle car produce unhealthy crunching sounds as the cabin doors creak slowly open.",

            _ => new DeathProcessor().Process("The shuttle car hurtles past the platforms and rams into the wall at the far end of the station. The shuttle car is destroyed, but you're in no condition to care. ", context).InteractionMessage
        };
    }
}

using System.Text;
using GameEngine.Location;
using Model.AIGeneration;
using Planetfall.Item.Kalamontee.Mech;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Location.Lawanda;

namespace Planetfall.Location;

/// <summary>
/// Represents the abstract base class for teleportation booth locations within the game,
/// inheriting properties and behaviors from <see cref="LocationBase"/>.
/// Provides shared functionality for interacting with specific types of teleportation booth locations.
/// </summary>
internal abstract class BoothBase : LocationBase, ICardActivatedDevice
{
    public bool IsEnabled { get; set; }

    // Issue #399: sliding the teleportation card activates the booth for only 30 turns in the original
    // (<QUEUE I-TURNOFF-TELEPORTATION 30>, globals.zil:1414). CardActivationTimer counts this down and
    // clears IsEnabled; without it the booth stayed "Redee" forever.
    [UsedImplicitly] public int ActivationTurnsRemaining { get; set; }

    // Shown when the window lapses while the player is in the booth (I-TURNOFF-TELEPORTATION,
    // globals.zil:1538-1542); silent otherwise.
    public string DeactivationAnnouncement => "The ready light goes dark. ";

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        return Task.FromResult(CardActivationTimer.Tick(this, context));
    }

    protected PositiveInteractionResult GoOne(IContext context)
    {
        return Go<BoothOne>(context);
    }

    protected PositiveInteractionResult GoTwo(IContext context)
    {
        return Go<BoothTwo>(context);
    }

    protected PositiveInteractionResult GoThree(IContext context)
    {
        return Go<BoothThree>(context);
    }

    private PositiveInteractionResult Go<T>(IContext context) where T : class, ILocation, ICanContainItems, new()
    {
        if (!IsEnabled)
            return new PositiveInteractionResult("A sign flashes \"Teleportaashun buux not aktivaatid.\"");

        var sb = new StringBuilder();

        if (Repository.GetItem<Floyd>().CurrentLocation == context.CurrentLocation)
        {
            sb.AppendLine("Floyd gives a terrified squeal, and clutches at his guidance mechanism. ");
            Repository.GetItem<Floyd>().CurrentLocation = Repository.GetLocation<T>();
        }
        
        // Using the booth consumes the activation and cancels its expiry countdown, mirroring
        // <DISABLE <INT I-TURNOFF-TELEPORTATION>> on a successful teleport (globals.zil:1522).
        CardActivationTimer.Cancel(this, context);
        context.CurrentLocation = Repository.GetLocation<T>();

        sb.AppendLine("You experience a strange feeling in the pit of your stomach. ");
        return new PositiveInteractionResult(sb.ToString());
    }
}
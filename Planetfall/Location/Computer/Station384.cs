using GameEngine.Location;
using GameEngine.StaticCommand.Implementation;
using Model.AIGeneration;
using Planetfall.Item.Computer;
using Planetfall.Location.Lawanda;
using Utilities;

namespace Planetfall.Location.Computer;

internal class Station384 : LocationWithNoStartingItems
{
    public override string Name => "Station 384";

    private const string TeleportToMiniaturizationBoothMessage =
        "You feel the familiar wrenching of your innards, and find yourself in a vast room whose distant walls are rushing straight toward you...";

    private const string TeleportToAuxiliaryBoothMessage =
        "A voice seems to whisper in your ear \"Main Miniaturization and Teleportation Booth has malfunctioned...switching to Auxiliary Booth...\" " +
        "You feel the familiar wrenching of your innards, and find yourself in a vast room whose distant walls are rushing straight toward you...";

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.E, Go<StripNearStation>() },
            {
                Direction.W,
                new MovementParameters
                {
                    Location = Repository.GetLocation<MiniaturizationBooth>(),
                    TransitionMessage = TeleportToMiniaturizationBoothMessage + Environment.NewLine
                }
            }
        };
    }

    public override async Task<string> AfterEnterLocation(IContext context, ILocation previousLocation,
        IGenerationClient generationClient)
    {
        // If entering from the silicon strip (east), teleport back
        if (previousLocation is StripNearStation)
        {
            var relay = Repository.GetItem<Relay>();

            if (relay.SpeckDestroyed)
            {
                // Computer is fixed - teleport to Auxiliary Booth
                var auxiliaryBooth = Repository.GetLocation<AuxiliaryBooth>();
                context.AddPoints(4);
                context.CurrentLocation = auxiliaryBooth;
                var description = await new LookProcessor().Process(string.Empty, context, generationClient, Runtime.Unknown);
                return TeleportToAuxiliaryBoothMessage + Environment.NewLine + Environment.NewLine + description;
            }

            // Computer not fixed - teleport back to Miniaturization Booth
            context.CurrentLocation = Repository.GetLocation<MiniaturizationBooth>();
            var miniDescription = await new LookProcessor().Process(string.Empty, context, generationClient, Runtime.Unknown);
            return TeleportToMiniaturizationBoothMessage + Environment.NewLine + Environment.NewLine + miniDescription;
        }

        return await base.AfterEnterLocation(context, previousLocation, generationClient);
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        return
            "You are standing on a square plate of heavy metal. Above your head, parallel to the plate beneath you, " +
            "is an identical metal plate. To the east is a wide, metallic strip. ";
    }
}
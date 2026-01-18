using Planetfall.Item.Computer;

namespace Planetfall.Item.Kalamontee.Mech;

public static class LaserSpeckHelper
{
    private static readonly List<string> MissMessages =
    [
        "The beam just misses the speck!",
        "A near miss!",
        "A good shot, but just a little wide of the target."
    ];

    public static InteractionResult ShootRelay(Relay relay, IContext context, string beamDescription, int laserSetting, IRandomChooser randomChooser)
    {
        // If relay is already destroyed, generic response
        if (relay.RelayDestroyed)
            return new PositiveInteractionResult(
                $"{beamDescription} which strikes the relay. The relay grows a bit warm, but nothing else happens. ");

        // Settings 2-6 (non-red) - destroys the relay (even if speck is already gone)
        if (laserSetting != 1)
        {
            relay.RelayDestroyed = true;
            return new PositiveInteractionResult(
                $"{beamDescription} which slices through the red plastic covering of the relay like a hot knife through butter. " +
                "Air rushes into the relay, which collapses into a heap of plastic shards. ");
        }

        // Setting 1 (red) - beam passes through red plastic
        // If speck is already destroyed, generic response
        if (relay.SpeckDestroyed)
            return new PositiveInteractionResult(
                $"{beamDescription} which strikes the relay. The relay grows a bit warm, but nothing else happens. ");

        return ShootSpeckWithRedLaser(relay, context, beamDescription, randomChooser);
    }

    private static InteractionResult ShootSpeckWithRedLaser(Relay relay, IContext context, string beamDescription, IRandomChooser randomChooser)
    {
        // Random hit check based on MarksmanshipCounter
        // Base chance is low, improves with each miss (+12 to counter per miss)
        var hitChance = 20 + relay.MarksmanshipCounter;
        var roll = randomChooser.RollDice(100);

        if (roll > hitChance)
        {
            // Miss - increase marksmanship for next attempt
            relay.MarksmanshipCounter += 12;
            var missMessage = randomChooser.Choose(MissMessages);
            return new PositiveInteractionResult($"{beamDescription}. {missMessage} ");
        }

        // Hit!
        if (!relay.SpeckHit)
        {
            // First hit
            relay.SpeckHit = true;
            return new PositiveInteractionResult(
                "The speck is hit by the beam! It sizzles a little, but isn't destroyed yet. ");
        }

        // Second hit - destroy the speck!
        relay.SpeckDestroyed = true;
        context.AddPoints(8);

        // Start 200-turn timer to escape before sector activates
        context.RegisterActor(Repository.GetItem<SectorActivationTimer>());

        return new PositiveInteractionResult(
            "The beam hits the speck again! This time, it vaporizes into a fine cloud of ash. " +
            "The relay slowly begins to close, and a voice whispers in your ear " +
            "\"Sector 384 will activate in 200 millichrons. Proceed to exit station.\" ");
    }
}

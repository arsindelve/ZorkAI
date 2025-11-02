namespace Planetfall.Item.Kalamontee.Mech.FloydPart;

public class FloydPowerManager(Floyd floyd)
{
    public InteractionResult Activate(IContext context)
    {
        if (floyd.HasDied)
            return new PositiveInteractionResult("As you touch Floyd's on-off switch, it falls off in your hands. ");

        if (floyd.IsOn)
            return new PositiveInteractionResult("He's already been activated. ");

        context.RegisterActor(floyd);

        if (!floyd.HasEverBeenOn)
            context.AddPoints(2);

        if (floyd.TurnOnCountdown > 0)
            return new PositiveInteractionResult("Nothing happens. ");

        floyd.IsOn = true;
        floyd.HasEverBeenOn = true;

        return new PositiveInteractionResult(FloydConstants.MadAfterTurnOffAndBackOn);
    }

    public InteractionResult Deactivate(IContext context)
    {
        if (!floyd.IsOn)
            return new PositiveInteractionResult("The robot doesn't seem to be on. ");

        context.RemoveActor(floyd);

        floyd.IsOn = false;
        return new PositiveInteractionResult(FloydConstants.TurnOffBetrayal);
    }

    public string? HandleTurnOnCountdown()
    {
        if (floyd.TurnOnCountdown <= 0)
            return null;

        if (floyd.TurnOnCountdown == 1)
        {
            floyd.IsOn = true;
            floyd.HasEverBeenOn = true;
            floyd.TurnOnCountdown = 0;
            return FloydConstants.ComesAlive;
        }

        floyd.TurnOnCountdown--;
        return string.Empty;
    }
}
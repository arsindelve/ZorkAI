using System.Text;
using GameEngine.Location;
using Planetfall.Item.Kalamontee.Mech;
using Planetfall.Item.Kalamontee.Mech.FloydPart;
using Planetfall.Location.Lawanda;

namespace Planetfall.Location;

/// <summary>
/// Represents the abstract base class for teleportation booth locations within the game,
/// inheriting properties and behaviors from <see cref="LocationBase"/>.
/// Provides shared functionality for interacting with specific types of teleportation booth locations.
/// </summary>
internal abstract class BoothBase : LocationBase
{
    public bool IsEnabled { get; set; }

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
        
        IsEnabled = false;
        context.CurrentLocation = Repository.GetLocation<T>();

        sb.AppendLine("You experience a strange feeling in the pit of your stomach. ");
        return new PositiveInteractionResult(sb.ToString());
    }
}
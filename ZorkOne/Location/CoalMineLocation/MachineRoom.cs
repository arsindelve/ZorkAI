using GameEngine;
using GameEngine.Location;
using Model.Intent;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location.CoalMineLocation;

public class MachineRoom : DarkLocation
{
    protected override Dictionary<Direction, MovementParameters> Map => new()
    {
        {
            Direction.N, new MovementParameters { Location = GetLocation<DraftyRoom>() }
        }
    };

    protected override string ContextBasedDescription =>
        "This is a large, cold room whose sole exit is to the north. In one corner there is a machine which is " +
        "reminiscent of a clothes dryer. On its face is a switch which is labelled \"START\". The switch does not " +
        "appear to be manipulable by any human hand (unless the fingers are about 1/16 by 1/4 inch). On the front " +
        $"of the machine is a large lid, which is {(Repository.GetItem<Machine>().IsOpen ? "open. \n" + Repository.GetItem<Machine>().ItemListDescription("machine", null) : "closed. ")} ";

    public override string Name => "Machine Room";

    public override void Init()
    {
        StartWithItem<Machine>();
    }
    
    public override InteractionResult RespondToMultiNounInteraction(MultiNounIntent action, IContext context)
    {
        string[] verbs = ["turn", "use", "apply"];
        string[] prepositions = ["with", "to", "on", "using"];

        if (!action.NounOne.ToLowerInvariant().Trim().Contains("switch") &&
            !action.NounTwo.ToLowerInvariant().Trim().Contains("switch"))
            return base.RespondToMultiNounInteraction(action, context);

        if (!action.NounOne.ToLowerInvariant().Trim().Contains("screwdriver") &&
            !action.NounTwo.ToLowerInvariant().Trim().Contains("screwdriver"))
            return base.RespondToMultiNounInteraction(action, context);

        if (!verbs.Contains(action.Verb.ToLowerInvariant().Trim()))
            return base.RespondToMultiNounInteraction(action, context);

        if (!prepositions.Contains(action.Preposition.ToLowerInvariant().Trim()))
            return base.RespondToMultiNounInteraction(action, context);

        if (!context.HasItem<Screwdriver>() && HasItem<Screwdriver>())
            return new PositiveInteractionResult("You don't have the screwdriver.");

        if (!context.HasItem<Screwdriver>())
            return base.RespondToMultiNounInteraction(action, context);

        var machine = Repository.GetItem<Machine>();
        
        if (machine.IsOpen)
            return new PositiveInteractionResult("The machine doesn't seem to want to do anything. ");

        if (machine.HasItem<Coal>())
        {
            // Replace the coal with diamond
            Coal coal = Repository.GetItem<Coal>();
            coal.CurrentLocation = null;
            machine.Items.Remove(coal);
            machine.ItemPlacedHere(Repository.GetItem<Diamond>()); 
            
            // Note: When coal is present, other items in the machine are unaffected. 
        }
        else
        {
            foreach (IItem next in machine.Items.ToList())
            {
                // Goodbye forever. You've been slagged.
                next.CurrentLocation = null;
                machine.Items.Remove(next);
            }
            machine.ItemPlacedHere(Repository.GetItem<Slag>()); 
        }
        
        return new PositiveInteractionResult(
            "The machine comes to life (figuratively) with a dazzling display of colored lights and bizarre " +
            "noises. After a few moments, the excitement abates. ");
    }
}
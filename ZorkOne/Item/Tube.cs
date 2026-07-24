using GameEngine;
using GameEngine.Item;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;

namespace ZorkOne.Item;

public class Tube : OpenAndCloseContainerBase, ICanBeExamined, ICanBeTakenAndDropped, ICanBeRead
{
    public override string[] NounsForMatching => ["tube", "tube of toothpaste", "toothpaste tube", "toothpaste"];

    /// <summary>
    /// Nothing can be put back into the tube. 
    /// </summary>
    protected override int SpaceForItems => 0;

    /// <summary>
    /// Cannot tell what is in the tube unless it's open. 
    /// </summary>
    public override bool IsTransparent => false;

    public override string NoRoomMessage => "That's not really going to work out for you. ";

    public string ExaminationDescription => ReadDescription;

    public string ReadDescription => """
                                     ---> Frobozz Magic Gunk Company <---
                                              All-Purpose Gunk
                                     """;

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return Items.Any() && IsOpen
            ? "There is an object which looks like a tube of toothpaste here. " + Environment.NewLine +
              ItemListDescription("tube", currentLocation)
            : "There is an object which looks like a tube of toothpaste here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return OnTheGroundDescription(currentLocation);
    }

    public override string NowOpen(ILocation currentLocation)
    {
        return Items.Any() ? "Opening the tube reveals a viscous material. " : base.NowOpen(currentLocation);
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A tube " + (Items.Any() && IsOpen
            ? Environment.NewLine + ItemListDescription("tube", currentLocation)
            : "");
    }

    public override void Init()
    {
        StartWithItemInside<ViscousMaterial>();
    }

    /// <summary>
    ///     Mirrors TUBE-FUNCTION (zork1/1actions.zil:1386-1398): SQUEEZE is the canonical way to get the
    ///     viscous material out of the toothpaste tube. When the tube is open and still holds the material
    ///     it oozes into the player's hand (the ZIL's <c>&lt;MOVE ,PUTTY ,WINNER&gt;</c>); open but empty
    ///     reports "apparently empty"; closed reports "closed". Without this handler SQUEEZE fell through
    ///     to the AI narrator and never extracted the material (issue #390).
    /// </summary>
    public override async Task<InteractionResult?> RespondToSimpleInteraction(SimpleIntent action,
        IContext context, IGenerationClient client, IItemProcessorFactory itemProcessorFactory)
    {
        if (!action.Match(Verbs.SqueezeVerbs, NounsForMatching))
            return await base.RespondToSimpleInteraction(action, context, client, itemProcessorFactory);

        // Closed → the material is sealed away and can't be reached.
        if (!IsOpen)
            return new PositiveInteractionResult("The tube is closed. ");

        // Open and still holding the material → it oozes into your hand.
        if (HasItem<ViscousMaterial>())
        {
            context.Take(Repository.GetItem<ViscousMaterial>());
            return new PositiveInteractionResult("The viscous material oozes into your hand. ");
        }

        // Open but already emptied → nothing left to squeeze out.
        return new PositiveInteractionResult("The tube is apparently empty. ");
    }
}
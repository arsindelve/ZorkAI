using GameEngine;
using GameEngine.Item;
using Model.AIGeneration;
using Model.Intent;
using Model.Interface;

namespace ZorkOne.Item;

public class BlackBook : ItemBase, ICanBeExamined, ICanBeTakenAndDropped, ICanBeRead
{
    public override string[] NounsForMatching => ["book", "black book"];

    public override string GenericDescription(ILocation? currentLocation) => "A black book";

    public string ExaminationDescription => """
                                            Commandment #12592
                                             
                                            Oh ye who go about saying unto each:  "Hello sailor":
                                            Dost thou know the magnitude of thy sin before the gods?
                                            Yea, verily, thou shalt be ground between two stones.
                                            Shall the angry gods cast thy body into the whirlpool?
                                            Surely, thy eye shall be put out with a sharp stick!
                                            Even unto the ends of the earth shalt thou wander and
                                            Unto the land of the dead shalt thou be sent at last.
                                            Surely thou shalt repent of thy cunning.
                                            """;

    public string ReadDescription => ExaminationDescription;

    public string OnTheGroundDescription(ILocation currentLocation) => "There is a black book here. ";

    public override string NeverPickedUpDescription(ILocation currentLocation) => "On the altar is a large black book, open to page 569.";

    public override InteractionResult RespondToSimpleInteraction(SimpleIntent action, IContext context,
        IGenerationClient client)
    {
        if (action.Match(["read"], NounsForMatching))
        {
            var location = Repository.GetLocation<EntranceToHades>();
            var spirits = Repository.GetItem<Spirits>();
            var hasSword = context.HasItem<Sword>();

            if (context.CurrentLocation == location &&
                spirits.CurrentLocation == location &&
                spirits.Stunned)
            {
                // * POOF *
                spirits.CurrentLocation = null;
                location.RemoveItem(spirits);


                return new PositiveInteractionResult(
                    "Each word of the prayer reverberates through the hall in a deafening confusion. " +
                    "As the last word fades, a voice, loud and commanding, speaks: \"Begone, fiends!\" A heart-stopping " +
                    "scream fills the cavern, and the spirits, sensing a greater power, flee through the walls." +
                    (hasSword ? "\nYour sword is no longer glowing." : ""));
            }
        }

        return base.RespondToSimpleInteraction(action, context, client);
    }

    public override int Size => 2;
}
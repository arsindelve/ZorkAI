using GameEngine;
using GameEngine.Item;
using Model.Intent;
using Model.Interface;

namespace ZorkOne.Item;

public class Egg : ContainerBase, ICanBeTakenAndDropped, ICanBeExamined, IGivePointsWhenFirstPickedUp,
    IGivePointsWhenPlacedInTrophyCase, IOpenAndClose
{
    public override string[] NounsForMatching => ["egg", "jewel-encrusted egg", "jewel encrusted egg"];

    public override int Size => 1;

    public bool IsDestroyed { get; set; }

    public string ExaminationDescription => IsOpen
        ? Items.Count == 0
            ? "The jewel-encrusted egg is empty. "
            : Repository.GetItem<Canary>().HasEverBeenPickedUp
                ? ItemListDescription("jewel-encrusted egg", null)
                : "There is a golden clockwork canary nestled in the egg. It has ruby eyes and a silver beak. Through a crystal window below its left wing you can see intricate machinery inside. It appears to have wound down. "
        : $"The {(IsDestroyed ? "broken " : "")}jewel-encrusted egg is closed. ";

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return IsDestroyed
            ? "There is a somewhat ruined egg here." + ItemListDescription("egg", null)
            : "There is a jewel-encrusted egg here." + ItemListDescription("egg", null);
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return
            "In the bird's nest is a large egg encrusted with precious jewels, apparently scavenged by a childless " +
            "songbird. The egg is covered with fine gold inlay, and ornamented in lapis lazuli and mother-of-pearl. " +
            "Unlike most eggs, this one is hinged and closed with a delicate looking clasp. The egg appears " +
            "extremely fragile. ";
    }

    int IGivePointsWhenFirstPickedUp.NumberOfPoints => 5;

    int IGivePointsWhenPlacedInTrophyCase.NumberOfPoints => IsDestroyed ? 2 : 5;

    public bool IsOpen { get; set; }

    public string NowOpen(ILocation currentLocation)
    {
        var canary = Repository.GetItem<Canary>();

        if (canary.IsDestroyed && canary.CurrentLocation == this)
            return "Opening the broken jewel-encrusted egg reveals a broken clockwork canary. ";

        return "Opened. ";
    }

    public string NowClosed(ILocation currentLocation)
    {
        return string.Empty;
    }

    public string AlreadyOpen => "It is already open. ";

    public string AlreadyClosed => "It is already closed. ";

    public bool HasEverBeenOpened { get; set; }

    public string CannotBeOpenedDescription(IContext context)
    {
        return IsDestroyed ? "" : "You have neither the tools nor the expertise. ";
    }

    public override InteractionResult RespondToMultiNounInteraction(MultiNounIntent action, IContext context)
    {
        if (!action.MatchNounOne(NounsForMatching))
            return base.RespondToMultiNounInteraction(action, context);

        if (!action.MatchVerb(["open", "pry", "break", "jimmie", "crack", "force", "bust", "pop", "split"]))
            return base.RespondToMultiNounInteraction(action, context);

        if (action.MatchNounTwo(Repository.GetItem<Sword>().NounsForMatching))
            return PryOpen<Sword>(context);

        if (action.MatchNounTwo(Repository.GetItem<NastyKnife>().NounsForMatching))
            return PryOpen<NastyKnife>(context);

        if (action.MatchNounTwo(Repository.GetItem<Screwdriver>().NounsForMatching))
            return PryOpen<Screwdriver>(context);

        return base.RespondToMultiNounInteraction(action, context);
    }

    private InteractionResult PryOpen<T>(IContext context) where T : IItem, new()
    {
        if (!context.HasItem<T>())
            return new PositiveInteractionResult("You don't have that! ");

        IsDestroyed = true;
        IsOpen = true;
        Repository.GetItem<Canary>().IsDestroyed = true;

        return new PositiveInteractionResult(
            "The egg is now open, but the clumsiness of your attempt has seriously compromised " +
            "its esthetic appeal. There is a golden clockwork canary nestled in the egg. " + Canary.DestroyedMessage);
    }

    public override string ItemListDescription(string name, ILocation? location)
    {
        if (!IsOpen)
            return string.Empty;

        var canary = Repository.GetItem<Canary>();

        if (Items.Contains(canary) && canary.IsDestroyed)
            return
                "\nThere is a golden clockwork canary nestled in the egg. " + Canary.DestroyedMessage;

        if (Items.Contains(canary))
            return
                "\nThere is a golden clockwork canary nestled in the egg. It has ruby eyes and a silver beak. " +
                "Through a crystal window below its left wing you can see intricate machinery inside. " +
                "It appears to have wound down.";

        return base.ItemListDescription(name, location);
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return IsDestroyed ? "A broken jewel-encrusted egg": "A jewel-encrusted egg";
    }

    public override void Init()
    {
        StartWithItemInside<Canary>();
    }
}
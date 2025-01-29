using GameEngine;
using Model.Interface;

namespace ZorkOne.ActorInteraction;

/// <summary>
/// The ThiefRobsYouEngine class encapsulates the logic for a thief's robbing behavior within the game.
/// It utilizes the provided IRandomChooser interface to introduce a randomized chance for the thief to perform his robbing action.
/// </summary>
internal class ThiefRobsYouEngine(IRandomChooser randomChooser)
{
    internal const int ThiefRobsYouChance = 50;

    internal static readonly List<string> NothingOfValue =
    [
        "A \"lean and hungry\" gentleman just wandered through, carrying a large bag. Finding nothing of value, he left disgruntled. ",
        "A seedy-looking individual is here, carrying his large bag. After a moment, he leaves, looking disgusted. Fortunately, he took nothing. "
    ];

    internal static readonly List<string> StealFromAdventurerResults =
    [
        "A seedy-looking individual with a large bag just wandered through the room. On the way through, he quietly abstracted some valuables from your possession, mumbling something about \"Doing unto others before...\" ",
        "A seedy-looking individual is here, carrying his large bag. After a moment, he leaves. You may not have noticed that he robbed you blind first. "
    ];

    internal static readonly List<string> StealFromRoomResults =
    [
        "A seedy-looking individual with a large bag just wandered through the room. On the way through, he quietly abstracted some valuables from the room, mumbling something about \"Doing unto others before...\" ",
        "A seedy-looking individual is here, carrying his large bag. After a moment, he leaves. You may not have noticed that he appropriated some valuables from the room first. "
    ];


    internal Task<string> StealSomething(IContext context)
    {
        if (!randomChooser.RollDice(ThiefRobsYouChance))
            return Task.FromResult(string.Empty);

        var itemsOfValueInInventory = context.Items.OfType<IGivePointsWhenPlacedInTrophyCase>().ToList();
        var itemsOfValueInRoom =
            ((ICanHoldItems)context.CurrentLocation).Items.OfType<IGivePointsWhenPlacedInTrophyCase>().ToList();

        if (itemsOfValueInInventory.Any() || itemsOfValueInRoom.Any())
        {
            if (!itemsOfValueInInventory.Any())
                return StealFromTheRoom(itemsOfValueInRoom);

            if (!itemsOfValueInRoom.Any())
                return StealFromAdventurer(itemsOfValueInInventory);

            // We have valuables on us, and in the room. Let's randomly do one or the either
            Func<Task<string>>[] choices =
            [
                () => StealFromAdventurer(itemsOfValueInInventory),
                () => StealFromTheRoom(itemsOfValueInRoom)
            ];

            return randomChooser.Choose(choices.ToList()).Invoke();
        }

        return Task.FromResult(randomChooser.Choose(NothingOfValue));
    }

    private Task<string> StealFromAdventurer(List<IGivePointsWhenPlacedInTrophyCase> itemsOfValueInInventory)
    {
        var item = randomChooser.Choose(itemsOfValueInInventory);
        TakeItem((IItem)item);
        return Task.FromResult(randomChooser.Choose(StealFromAdventurerResults));
    }

    private Task<string> StealFromTheRoom(List<IGivePointsWhenPlacedInTrophyCase> itemsOfValueInRoom)
    {
        var item = randomChooser.Choose(itemsOfValueInRoom);
        TakeItem((IItem)item);
        return Task.FromResult(randomChooser.Choose(StealFromRoomResults));
    }

    private void TakeItem(IItem item)
    {
        item.CurrentLocation?.RemoveItem(item);
        item.CurrentLocation = null;
        Repository.GetItem<Thief>().TreasureStash.Add(item);
    }
}

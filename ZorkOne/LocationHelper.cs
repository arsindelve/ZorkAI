using GameEngine;
using Model.Interface;

namespace ZorkOne;

internal static class LocationHelper
{
    internal static string? CheckSwordGlowingFaintly<TCreature, TCheckRoom>(IContext context)
        where TCreature : IItem, new()
        where TCheckRoom : class, ILocation, new()
    {
        var swordInPossession = context.HasItem<Sword>();
        var creatureIsInLocation =
            Repository.GetItem<TCreature>().CurrentLocation == Repository.GetLocation<TCheckRoom>();

        if (creatureIsInLocation && swordInPossession)
            return "\nYour sword is glowing with a faint blue glow. ";

        return null;
    }

    internal static async Task<string> CheckSwordNoLongerGlowing<
        TCreature,
        TCheckRoom,
        TPreviousRoom
    >(ILocation previousLocation, IContext context, Task<string> afterEnterLocation)
        where TCreature : IItem, new()
        where TCheckRoom : class, ILocation, new()
        where TPreviousRoom : class, ILocation, new()
    {
        var swordInPossession = context.HasItem<Sword>();
        var creatureIsInLocation =
            Repository.GetItem<TCreature>().CurrentLocation == Repository.GetLocation<TCheckRoom>();

        if (creatureIsInLocation && swordInPossession && previousLocation is TPreviousRoom)
            return "\nYour sword is no longer glowing. ";

        return await afterEnterLocation;
    }

    internal static string? CheckSwordGlowingBrightly<TCreature, TCheckRoom>(IContext context)
        where TCreature : IItem, new()
        where TCheckRoom : class, ILocation, new()
    {
        var swordInPossession = context.HasItem<Sword>();
        var creatureIsInLocation =
            Repository.GetItem<TCreature>().CurrentLocation == Repository.GetLocation<TCheckRoom>();

        if (creatureIsInLocation && swordInPossession)
            return "\nYour sword has begun to glow very brightly. ";

        return null;
    }
}
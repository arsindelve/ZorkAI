using GameEngine;
using GameEngine.Location;
using Model.Interface;

namespace ZorkOne;

internal static class ExtensionMethods
{
    internal static string? CheckSwordGlowingFaintly<TCreature, TCheckRoom>(this BaseLocation location,
        IContext context)
        where TCreature : IItem, new()
        where TCheckRoom : class, ILocation, new()
    {
        var swordInPossession = context.HasItem<Sword>();
        var creatureIsInLocation =
            Repository.GetItem<TCreature>().CurrentLocation == Repository.GetLocation<TCheckRoom>();

        if (creatureIsInLocation && swordInPossession) return "\nYour sword is glowing with a faint blue glow. ";

        return null;
    }

    internal static string? CheckSwordNoLongerGlowing<TCreature, TCheckRoom, TPreviousRoom>(this BaseLocation location, ILocation previousLocation,
        IContext context)
        where TCreature : IItem, new()
        where TCheckRoom : class, ILocation, new()
        where TPreviousRoom : class, ILocation, new()
    {
        var swordInPossession = context.HasItem<Sword>();
        var creatureIsInLocation =
            Repository.GetItem<TCreature>().CurrentLocation == Repository.GetLocation<TCheckRoom>();

        if (creatureIsInLocation && swordInPossession && previousLocation is TPreviousRoom) return "\nYour sword is glowing with a faint blue glow. ";

        return null;
    }

    
    internal static string? CheckSwordGlowingBrightly<TCreature, TCheckRoom>(this BaseLocation location,
        IContext context)
        where TCreature : IItem, new()
        where TCheckRoom : class, ILocation, new()
    {
        var swordInPossession = context.HasItem<Sword>();
        var creatureIsInLocation =
            Repository.GetItem<TCreature>().CurrentLocation == Repository.GetLocation<TCheckRoom>();

        if (creatureIsInLocation && swordInPossession) return "\nYour sword has begun to glow very brightly. ";

        return null;
    }
}
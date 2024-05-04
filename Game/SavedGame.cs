﻿using Model.Interface;
using Model.Item;
using Model.Location;

namespace Game;

public class SavedGame<T> where T : IContext, new()
{
    public Dictionary<Type, IItem>? AllItems;
    public Dictionary<Type, ILocation>? AllLocations;
    public T? Context;
}
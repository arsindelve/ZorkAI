﻿using GameEngine.Item;

namespace ZorkOne.Item;

public class Torch : ItemBase, ICanBeExamined, ICanBeTakenAndDropped, ICannotBeTurnedOff, IAmALightSource,
    IGivePointsWhenPlacedInTrophyCase, IGivePointsWhenFirstPickedUp
{
    public override string[] NounsForMatching => ["torch", "ivory torch"];

    public override string InInventoryDescription => "A torch (providing light) ";

    public string ExaminationDescription => "The torch is burning. ";

    public string OnTheGroundDescription => "There is a torch here. (providing light). ";

    public override string NeverPickedUpDescription => "Sitting on the pedestal is a flaming torch, made of ivory. ";

    public string CannotBeTurnedOffMessage => "You nearly burn your hand trying to extinguish the flame. ";

    int IGivePointsWhenFirstPickedUp.NumberOfPoints => 14;

    int IGivePointsWhenPlacedInTrophyCase.NumberOfPoints => 6;

    public override int Size => 3;
}
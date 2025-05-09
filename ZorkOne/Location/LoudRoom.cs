﻿using GameEngine;
using GameEngine.Location;
using Model.AIGeneration;
using Model.Interface;
using Model.Movement;
using Utilities;

namespace ZorkOne.Location;

/// <summary>
///     When the dam is filling up, it's quiet in here, and it's a normal room. When the dam is draining, it's so
///     loud you have to leave the room. Otherwise, it's pretty loud, and you cannot perform any action - it
///     just echos.
/// </summary>
public class LoudRoom : DarkLocation, ITurnBasedActor
{
    private readonly Random _random = new();

    [UsedImplicitly]
    public bool EchoHasBeenSpoken { get; set; }

    public override string Name => "Loud Room";

    public Task<string> Act(IContext context, IGenerationClient client)
    {
        return Task.FromResult(string.Empty);
    }

    protected override Dictionary<Direction, MovementParameters> Map(IContext context)
    {
        return new Dictionary<Direction, MovementParameters>
        {
            { Direction.W, new MovementParameters { Location = GetLocation<RoundRoom>() } },
            { Direction.E, new MovementParameters { Location = GetLocation<DampCave>() } },
            { Direction.Up, new MovementParameters { Location = GetLocation<DeepCanyon>() } }
        };
    }

    protected override string GetContextBasedDescription(IContext context)
    {
        var description = "This is a large room with a ceiling which cannot be detected from the ground. " +
                          "There is a narrow passage from east to west and a stone stairway leading upward. ";

        if (Repository.GetLocation<ReservoirSouth>().IsFilling || EchoHasBeenSpoken)
            description += "The room is eerie in its quietness. ";

        else if (Repository.GetLocation<ReservoirSouth>().IsDraining)
            description +=
                "It is unbearably loud here, with an ear-splitting roar seeming to come from all around you. " +
                "There is a pounding in your head which won't stop. ";

        else
            description += "The room is deafeningly loud with an undetermined rushing sound. The sound seems to " +
                           "reverberate from all of the walls, making it difficult even to think. ";

        return description;
    }

    public override void Init()
    {
        StartWithItem<PlatinumBar>();
    }

    public override void OnLeaveLocation(IContext context, ILocation newLocation, ILocation previousLocation)
    {
        context.RemoveActor(this);
        base.OnLeaveLocation(context, newLocation, previousLocation);
    }

    public override async Task<InteractionResult> RespondToSpecificLocationInteraction(string? input, IContext context,
        IGenerationClient client)
    {
        if (DidTheAdventurerSayEcho(input) && !EchoHasBeenSpoken)
        {
            EchoHasBeenSpoken = true;
            return new PositiveInteractionResult("The acoustics of the room change subtly. ");
        }

        // When the dam is filling, this room behaves normally. 
        if (Repository.GetLocation<ReservoirSouth>().IsFilling || EchoHasBeenSpoken)
            return new NoNounMatchInteractionResult();

        // Otherwise, direction commands are the only ones available.  
        if (DirectionParser.IsDirection(input, out _))
            return await base.RespondToSpecificLocationInteraction(input, context, client);

        // Everything else echos....
        var lastWord = input!.Split(" ").Last();
        return new PositiveInteractionResult($"{lastWord} {lastWord} ...");
    }

    private static bool DidTheAdventurerSayEcho(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        input = input.StripNonChars().ToLowerInvariant();

        if (input == "echo")
            return true;

        var hasVerb = Verbs.SayVerbs.Any(s => input.StartsWith(s));
        return hasVerb && input.EndsWith("echo");
    }

    public override Task<string> AfterEnterLocation(IContext context, ILocation previousLocation,
        IGenerationClient generationClient)
    {
        if (Repository.GetLocation<ReservoirSouth>().IsDraining)
            return Task.FromResult("\nWith a tremendous effort, you scramble out of the room. \n\n" +
                                   Flee(context).InteractionMessage);

        context.RegisterActor(this);
        return base.AfterEnterLocation(context, previousLocation, generationClient);
    }

    /// <summary>
    ///     This is a Mister Nimbus reference.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private InteractionResult Flee(IContext context)
    {
        var direction = GetRandomKey(Map(context));
        var newLocation = Map(context)[direction].Location!;
        context.CurrentLocation = newLocation;
        return new PositiveInteractionResult(newLocation.GetDescription(context) + Environment.NewLine);
    }

    private TKey GetRandomKey<TKey, TValue>(Dictionary<TKey, TValue> dict) where TKey : notnull
    {
        var index = _random.Next(0, dict.Count);
        var key = new List<TKey>(dict.Keys)[index];
        return key;
    }
}
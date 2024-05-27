using Model.AIGeneration;
using Model.Interface;
using Model.Movement;

namespace ZorkOne.Location;

/// <summary>
///     When the dam is filling up, it's quiet in here and it's a normal room. When the dam is draining, it's so
///     loud you have to leave the room. Otherwise, it's pretty loud and you cannot perform any action - it
///     just echos.
/// </summary>
public class LoudRoom : DarkLocation, ITurnBasedActor
{
    private readonly Random _random = new();

    protected override Dictionary<Direction, MovementParameters> Map =>
        new()
        {
            { Direction.W, new MovementParameters { Location = GetLocation<RoundRoom>() } },
            { Direction.E, new MovementParameters { Location = GetLocation<DampCave>() } },
            { Direction.Up, new MovementParameters { Location = GetLocation<DeepCanyon>() } }
        };

    protected override string ContextBasedDescription =>
        "This is a large room with a ceiling which cannot be detected from the ground. " +
        "There is a narrow passage from east to west and a stone stairway leading upward. " +
        (Repository.GetLocation<ReservoirSouth>().IsFilling
            ? "The room is eerie in its quietness."
            : "") +
        (Repository.GetLocation<ReservoirSouth>().IsDraining
            ? "It is unbearably loud here, with an ear-splitting roar seeming to come from all around you. There is a pounding in your head which won't stop. "
            : "");

    public override string Name => "Loud Room";

    public string? Act(IContext context, IGenerationClient client)
    {
        return string.Empty;
    }

    public override void Init()
    {
    }

    public override void OnLeaveLocation(IContext context, ILocation newLocation, ILocation previousLocation)
    {
        context.RemoveActor(this);
        base.OnLeaveLocation(context, newLocation, previousLocation);
    }

    public override InteractionResult RespondToSpecificLocationInteraction(string? input, IContext context)
    {
        if (DirectionParser.IsDirection(input, out Direction _))
            return base.RespondToSpecificLocationInteraction(input, context);
        
        string lastWord = input!.Split(" ").Last();
        return new PositiveInteractionResult($"{lastWord} {lastWord} ...");
    }

    public override string AfterEnterLocation(IContext context, ILocation previousLocation)
    {
        if (Repository.GetLocation<ReservoirSouth>().IsDraining)
            return "With a tremendous effort, you scramble out of the room. \n\n" +
                   Flee(context).InteractionMessage;

        context.RegisterActor(this);
        return base.AfterEnterLocation(context, previousLocation);
    }

    /// <summary>
    ///     This is a Mister Nimbus reference.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private InteractionResult Flee(IContext context)
    {
        var direction = GetRandomKey(Map);
        var newLocation = Map[direction].Location!;
        context.CurrentLocation = newLocation;
        return new PositiveInteractionResult(newLocation.Description + Environment.NewLine);
    }

    public TKey GetRandomKey<TKey, TValue>(Dictionary<TKey, TValue> dict) where TKey : notnull
    {
        var index = _random.Next(0, dict.Count);
        var key = new List<TKey>(dict.Keys)[index];
        return key;
    }
}
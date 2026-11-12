using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using GameEngine;
using GameEngine.Item;
using GameEngine.Location;
using Model.AIParsing;
using Model.Intent;
using Model.Interface;
using Model.Item;
using Model.Location;

namespace UnitTests.Parser.Census;

/// <summary>Where a probe is made: standing in a room at the start of the game, optionally holding one item.</summary>
public sealed record CensusContext(string Location, string? Holding)
{
    public override string ToString()
    {
        return Holding is null ? Location : $"{Location} holding {Holding}";
    }
}

/// <summary>What the game said to one verb aimed at one noun, and whether it was the game that said it.</summary>
public sealed record ProbeResult(bool Answered, bool ReachedTheParser, string Response);

/// <summary>
///     Asks the game itself what it answers. For every room, and for every item in hand, every verb the
///     game declares is aimed at every noun in reach, each in a freshly started game, and the engine
///     reports whether authored game logic replied or the narrator was left to improvise.
///     <para>
///         The result is not a list anyone wrote: it is what the engine does. That makes it the reference
///         the parser is tested against - a parser is right when the player's sentence reaches the same
///         authored answer.
///     </para>
/// </summary>
public sealed class InteractionCensus(CensusGame game)
{
    private readonly Assembly _assembly = Assembly.Load(game.Name);

    // System and global commands are decided by the production parser, so a probe that never reaches the
    // verb/noun stage is recognised as such.
    private readonly IntentParser _stages = new(Mock.Of<IAIParser>(), game.Commands);

    /// <summary>Every word and phrase of every verb family the game declares.</summary>
    public IReadOnlyList<string> Verbs { get; } = Repository.GetVerbs(game.Name)
        .SelectMany(family => family).Select(word => word.ToLowerInvariant()).Distinct().Order().ToList();

    /// <summary>
    ///     The contexts to probe and the nouns in reach in each: every room with what is in it at the
    ///     start of the game, and every item in the player's hands in the room it starts in.
    /// </summary>
    public IReadOnlyList<(CensusContext Context, string[] Nouns)> Survey()
    {
        var session = game.Start(new FixedIntentParser(_stages, new NullIntent()), new NamedItems(null));
        var startingRoom = session.Context.CurrentLocation.GetType();
        var plan = new List<(CensusContext, string[])>();

        var rooms = _assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false, IsGenericType: false } &&
                           type.IsSubclassOf(typeof(LocationBase)))
            .OrderBy(type => type.Name).ToList();

        // Loading every room places every item, which is how an item's home is known.
        foreach (var room in rooms)
        {
            LocationBase location;
            try
            {
                location = (LocationBase)Load(room);
            }
            catch
            {
                continue;
            }

            var nouns = location.GetAllItemsRecursively.SelectMany(item => item.NounsForMatching)
                .Concat(location.SceneryNouns)
                // Scenery the whole game shares answers the same everywhere, so it is probed once.
                .Concat(room == startingRoom
                    ? session.Context.Game.GlobalScenery.SelectMany(scenery => scenery.Nouns)
                    : [])
                .Select(noun => noun.ToLowerInvariant()).Distinct().Order().ToArray();

            if (nouns.Length > 0)
                plan.Add((new CensusContext(room.Name, null), nouns));
        }

        foreach (var itemType in _assembly.GetTypes()
                     .Where(type => type is { IsClass: true, IsAbstract: false, IsGenericType: false } &&
                                    type.IsSubclassOf(typeof(ItemBase)))
                     .OrderBy(type => type.Name))
        {
            var item = Repository.GetItem(itemType);
            var home = HomeOf(item)?.GetType() ?? startingRoom;
            var nouns = item.NounsForMatching.Select(noun => noun.ToLowerInvariant()).Distinct().Order().ToArray();

            if (nouns.Length > 0)
                plan.Add((new CensusContext(home.Name, itemType.Name), nouns));
        }

        return plan;
    }

    /// <summary>One verb, one noun, one fresh game.</summary>
    public Task<ProbeResult> Probe(CensusContext context, string verb, string noun)
    {
        var input = $"{verb} {noun}";
        var intent = new SimpleIntent { Verb = verb, Noun = noun, OriginalInput = input, Message = input };
        return Probe(context, input, new FixedIntentParser(_stages, intent), noun);
    }

    /// <summary>The same probe with a parser of the caller's choosing: how a real parser is checked against it.</summary>
    public async Task<ProbeResult> Probe(CensusContext context, string input, IIntentParser parser, string? noun)
    {
        var reached = parser as FixedIntentParser;
        var session = game.Start(parser, new NamedItems(noun));
        Arrange(session.Context, context);

        string? response;
        try
        {
            response = await session.Respond(input);
        }
        catch (Exception problem)
        {
            return new ProbeResult(false, reached?.WasAsked ?? true, $"(threw {problem.GetType().Name})");
        }

        var answered = !string.IsNullOrWhiteSpace(response) && !session.NarratorWasAsked();
        return new ProbeResult(answered, reached?.WasAsked ?? true, Normalize(response));
    }

    private void Arrange(IContext context, CensusContext where)
    {
        // Whatever the opening scene has running (a countdown, a wandering companion) stays where it was.
        context.Actors.Clear();
        context.CurrentLocation = Load(TypeNamed(where.Location));

        if (where.Holding is not null)
            context.ItemPlacedHere(Repository.GetItem(TypeNamed(where.Holding)));

        if (context.ItIsDarkHere)
            game.Light(context);
    }

    private Type TypeNamed(string name)
    {
        return _assembly.GetTypes().Single(type => type.Name == name && type is { IsClass: true, IsAbstract: false });
    }

    private static ILocation Load(Type room)
    {
        return (ILocation)typeof(Repository).GetMethod(nameof(Repository.GetLocation), Type.EmptyTypes)!
            .MakeGenericMethod(room).Invoke(null, null)!;
    }

    private static ILocation? HomeOf(IItem item)
    {
        for (object? holder = item.CurrentLocation; holder is not null; holder = (holder as IItem)?.CurrentLocation)
            if (holder is ILocation room)
                return room;

        return null;
    }

    private static string Normalize(string? response)
    {
        return string.Join(' ', (response ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }

    /// <summary>A short, stable fingerprint of a response, so two probes can be compared without storing prose.</summary>
    public static string Fingerprint(string response)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(response)))[..10].ToLowerInvariant();
    }

    /// <summary>
    ///     Hands the engine one fixed intent for whatever is typed, so the census measures the game and not
    ///     a parser. The stages before parsing are the production ones.
    /// </summary>
    public sealed class FixedIntentParser(IIntentParser stages, IntentBase intent) : IIntentParser
    {
        public bool WasAsked { get; private set; }

        public Guid? TurnCorrelationId { get; set; }

        public CloudWatch.ICloudWatchLogger<CloudWatch.Model.GenerationLog>? Logger { get; set; }

        public IntentBase? DetermineGlobalIntentType(string? input) => stages.DetermineGlobalIntentType(input);

        public IntentBase? DetermineSystemIntentType(string? input) => stages.DetermineSystemIntentType(input);

        public Task<IntentBase> DetermineComplexIntentType(string? input, string locationDescription, string sessionId)
        {
            WasAsked = true;
            return Task.FromResult(intent);
        }

        public Task<string?> ResolvePronounsAsync(string input, string? lastInput, string? lastResponse)
        {
            return Task.FromResult<string?>(null);
        }
    }

    /// <summary>
    ///     Stands in for the AI that lists the items of a take or drop. It names exactly the object the
    ///     probe names, which is what the real one does for a single object.
    /// </summary>
    private sealed class NamedItems(string? noun) : IAITakeAndAndDropParser
    {
        public Task<string[]> GetListOfItemsToTake(string input, string locationDescription)
        {
            return Task.FromResult<string[]>(noun is null ? [] : [noun]);
        }

        public Task<string[]> GetListOfItemsToDrop(string input, string inventoryDescription)
        {
            return Task.FromResult<string[]>(noun is null ? [] : [noun]);
        }
    }
}

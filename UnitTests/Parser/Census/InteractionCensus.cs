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
/// <param name="ReachedTheParser">
///     The engine asked its parser about the sentence as it was typed. False for a global command or a
///     bare direction, which are answered before parsing - and for a sentence the engine rewrites first:
///     "look in the box" is "examine the box" by the time any parser sees it, so "look in" aimed at the
///     box is not something play can produce, whatever a handler would make of it.
/// </param>
/// <param name="State">
///     The game afterwards, with the record of what was typed taken out, so that two probes can be
///     compared for what they did to the game and not for how they were worded. What "the game" covers
///     depends on how the probe was made - see <see cref="InteractionCensus.ProbeEffect" />.
/// </param>
public sealed record ProbeResult(bool Answered, bool ReachedTheParser, string Response, string State = "");

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

    private readonly Lazy<List<Type>> _rooms = new(() => Concrete(Assembly.Load(game.Name), typeof(LocationBase)));
    private readonly Lazy<List<Type>> _items = new(() => Concrete(Assembly.Load(game.Name), typeof(ItemBase)));

    private readonly Lazy<Dictionary<string, Type>> _typesByName = new(() => Assembly.Load(game.Name).GetTypes()
        .Where(type => type is { IsClass: true, IsAbstract: false, IsGenericType: false })
        .GroupBy(type => type.Name).ToDictionary(sameName => sameName.Key, sameName => sameName.First()));

    private static List<Type> Concrete(Assembly assembly, Type baseType)
    {
        return assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false, IsGenericType: false } &&
                           type.IsSubclassOf(baseType))
            .OrderBy(type => type.Name).ToList();
    }

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

    /// <summary>
    ///     One verb, one noun, one fresh game. The state reported is of whatever the turn brought into
    ///     memory, which is cheap to save and enough to tell two turns apart that ran the same code:
    ///     equal states mean the same thing happened. Unequal ones do not mean something different did -
    ///     see <see cref="ProbeEffect" />.
    /// </summary>
    public Task<ProbeResult> Probe(CensusContext context, string verb, string noun)
    {
        return Probe(context, $"{verb} {noun}", Handing(verb, noun), noun);
    }

    /// <summary>
    ///     The same probe with the whole world loaded first. The engine loads rooms and items only when
    ///     something first touches them, so two turns can differ in what is in memory without differing
    ///     in what happened - a handler that merely looks at the matchbook brings the matchbook into
    ///     being. With everything loaded beforehand, the state afterwards is the state of the game and
    ///     not a record of what the code happened to read. Some fifty times the cost of
    ///     <see cref="Probe(CensusContext, string, string)" />, so it is the second opinion, not the first.
    /// </summary>
    public Task<ProbeResult> ProbeEffect(CensusContext context, string verb, string noun)
    {
        return Probe(context, $"{verb} {noun}", Handing(verb, noun), noun, true);
    }

    /// <summary>A parser that hands the engine exactly this verb and noun, whatever is typed.</summary>
    private FixedIntentParser Handing(string verb, string noun)
    {
        var input = $"{verb} {noun}";
        return new FixedIntentParser(_stages,
            new SimpleIntent { Verb = verb, Noun = noun, OriginalInput = input, Message = input });
    }

    /// <summary>The same probe with a parser of the caller's choosing: how a real parser is checked against it.</summary>
    /// <param name="wholeWorld">Load every room and item before the turn - see <see cref="ProbeEffect" />.</param>
    public async Task<ProbeResult> Probe(CensusContext context, string input, IIntentParser parser, string? noun,
        bool wholeWorld = false)
    {
        var reached = parser as FixedIntentParser;
        var session = game.Start(parser, new NamedItems(noun));
        Arrange(session.Context, context, wholeWorld);

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
        var asTyped = reached is null ||
                      (reached.WasAsked && string.Equals(reached.AskedAbout?.Trim(), input.Trim(),
                          StringComparison.OrdinalIgnoreCase));

        return new ProbeResult(answered, asTyped, Normalize(response), StateOf(session));
    }

    /// <summary>
    ///     The state after a turn in which nothing happened: the same noun, a verb nothing answers. A
    ///     probe has an in-game effect exactly when its state differs from this one - the turn counter
    ///     and the like move in both, so they cancel.
    /// </summary>
    public async Task<string> StateAfterDoingNothing(CensusContext context, string noun, bool wholeWorld = false)
    {
        var input = $"{NothingVerb} {noun}";
        return (await Probe(context, input, Handing(NothingVerb, noun), noun, wholeWorld)).State;
    }

    /// <summary>A verb no handler answers, so the turn it is used in changes nothing but the clock.</summary>
    public const string NothingVerb = "censusnoop";

    private static string StateOf(CensusSession session)
    {
        return StateOf(session.Context, session.Snapshot);
    }

    /// <summary>
    ///     The game as the save-game serialiser writes it, with the record of what was typed taken out.
    ///     Clearing that record changes the context, so this is for a game that is about to be thrown
    ///     away or put back.
    /// </summary>
    public static string StateOf(IContext context, Func<string> snapshot)
    {
        // What the engine remembers about the wording of the last command differs between any two
        // commands, whatever they did, so it is not part of what a command did to the game.
        context.Inputs = new global::Utilities.LimitedStack<string>();
        context.LastInput = null;
        context.LastResponse = null;
        context.LastNoun = "";
        context.LastNouns = [];

        // "Which one do you mean?" leaves a question pending, and nothing else: asking it is not an effect.
        context.PendingDisambiguation = null;

        try
        {
            return snapshot();
        }
        catch (Exception problem)
        {
            // A state that cannot be saved cannot be compared; say so rather than report "no effect".
            return $"(state unavailable: {problem.GetType().Name})";
        }
    }

    private void Arrange(IContext context, CensusContext where, bool wholeWorld)
    {
        // Whatever the opening scene has running (a countdown, a wandering companion) stays where it was.
        context.Actors.Clear();

        if (wholeWorld)
        {
            // Every other room first and the player's room last: a room claims the objects it shares
            // with another when it is loaded, and the room the player has just walked into is the one
            // that was loaded last.
            foreach (var room in _rooms.Value.Where(room => room.Name != where.Location))
                TryLoad(room);

            context.CurrentLocation = Load(TypeNamed(where.Location));

            foreach (var item in _items.Value)
                try
                {
                    Repository.GetItem(item);
                }
                catch
                {
                    // An item that cannot be built on its own is not one a fresh game could hold either.
                }

            context.Actors.Clear();
        }

        context.CurrentLocation = Load(TypeNamed(where.Location));
        PinTheDice();

        if (where.Holding is not null)
            context.ItemPlacedHere(Repository.GetItem(TypeNamed(where.Holding)));

        if (context.ItIsDarkHere)
            game.Light(context);
    }

    /// <summary>
    ///     Where the bat drops you is a roll of the dice, and so are a handful of other outcomes. A probe
    ///     has to do the same thing every time it is made, or two wordings of one action look like two
    ///     actions. Every object that rolls gets dice that always land the same way.
    /// </summary>
    private static void PinTheDice()
    {
        PinTheDice(_ => true);
    }

    /// <summary>
    ///     Pins the dice that are still the game's own, leaving alone any a test has already replaced with
    ///     an outcome of its choosing.
    /// </summary>
    public static void PinTheDiceNobodyHasPinned()
    {
        PinTheDice(current => current is RandomChooser);
    }

    private static void PinTheDice(Func<object?, bool> replace)
    {
        foreach (var thing in Repository.Loaded())
        foreach (var dice in thing.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                     .Where(property => property.PropertyType == typeof(IRandomChooser) && property.CanWrite &&
                                        property.CanRead && property.GetIndexParameters().Length == 0))
            if (replace(dice.GetValue(thing)))
                dice.SetValue(thing, LoadedDice.Instance);
    }

    private sealed class LoadedDice : IRandomChooser
    {
        public static readonly LoadedDice Instance = new();

        public T Choose<T>(List<T> items) => items[0];

        // The highest face: a "one in N" chance never comes up, so the rare event stays rare.
        public int RollDice(int sides) => sides;

        public bool RollDiceSuccess(int sides) => false;
    }

    private Type TypeNamed(string name)
    {
        return _typesByName.Value[name];
    }

    private static void TryLoad(Type room)
    {
        try
        {
            Load(room);
        }
        catch
        {
            // A room that cannot be built bare is skipped by the survey too.
        }
    }

    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, MethodInfo> Loaders = new();

    private static ILocation Load(Type room)
    {
        var loader = Loaders.GetOrAdd(room, type => typeof(Repository)
            .GetMethod(nameof(Repository.GetLocation), Type.EmptyTypes)!.MakeGenericMethod(type));

        return (ILocation)loader.Invoke(null, null)!;
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

        /// <summary>The sentence the engine asked about, which is not the one typed if the engine rewrote it.</summary>
        public string? AskedAbout { get; private set; }

        public Guid? TurnCorrelationId { get; set; }

        public CloudWatch.ICloudWatchLogger<CloudWatch.Model.GenerationLog>? Logger { get; set; }

        public IntentBase? DetermineGlobalIntentType(string? input) => stages.DetermineGlobalIntentType(input);

        public IntentBase? DetermineSystemIntentType(string? input) => stages.DetermineSystemIntentType(input);

        public Task<IntentBase> DetermineComplexIntentType(string? input, string locationDescription, string sessionId)
        {
            WasAsked = true;
            AskedAbout = input;
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

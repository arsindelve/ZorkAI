using System.Collections.Concurrent;
using System.Reflection;
using Model.Intent;
using Model.Interface;
using Model.Location;
using Model.Movement;

namespace UnitTests.Parser;

/// <summary>
///     Every line of player input the parser tests know about, with what each must mean. One list, used
///     by every layer's tests: the grammar alone, the AI alone (scripted or live), and the two together.
///     <para>
///         To cover a new phrasing, add a case to the game's file. Say what it means with
///         <see cref="Meaning" />, and mark it <c>.AiOnly()</c> if the grammar should hand it to the AI.
///         The tests then hold all of it in place - including, if the grammar later learns to resolve it,
///         telling you to take the <c>.AiOnly()</c> off.
///     </para>
/// </summary>
public static partial class ParserCorpus
{
    public static IReadOnlyList<ParserCase> All { get; } = Everything();

    private static IReadOnlyList<ParserCase> Everything()
    {
        var zork = ZorkOne().ToList();
        var planetfall = Planetfall().ToList();

        return
        [
            ..zork, ..FromWalkthroughs("ZorkOne", "walkthrough.zorkone.json", zork),
            ..planetfall, ..FromWalkthroughs("Planetfall", "walkthrough.planetfall.json", planetfall),
            ..Stationfall()
        ];
    }

    /// <summary>Cases that reach a parser: the grammar resolves them or the AI does.</summary>
    public static IEnumerable<TestCaseData> Parsed =>
        All.Where(@case => @case.ResolvedBy != ResolvedBy.BeforeParsing).Select(Named);

    public static IEnumerable<TestCaseData> ByGrammar =>
        All.Where(@case => @case.ResolvedBy == ResolvedBy.Grammar).Select(Named);

    public static IEnumerable<TestCaseData> BeforeParsing =>
        All.Where(@case => @case.ResolvedBy == ResolvedBy.BeforeParsing).Select(Named);

    private static TestCaseData Named(ParserCase @case)
    {
        return new TestCaseData(@case).SetArgDisplayNames(@case.Game, @case.Input);
    }

    private static readonly ConcurrentDictionary<(string Game, string Location), string> Descriptions = new();
    private static readonly ConcurrentDictionary<string, IGlobalCommandFactory> Factories = new();
    private static readonly object RepositoryLock = new();

    /// <summary>
    ///     The description of the room the case is set in, as the AI is shown it. Rooms are named by
    ///     class name because most of Planetfall's are internal to its assembly.
    /// </summary>
    public static string DescribeLocation(ParserCase @case)
    {
        return Descriptions.GetOrAdd((@case.Game, @case.Location), key =>
        {
            var type = Assembly.Load(key.Game).GetTypes().SingleOrDefault(candidate =>
                           candidate.Name == key.Location && typeof(ILocation).IsAssignableFrom(candidate) &&
                           !candidate.IsAbstract)
                       ?? throw new InvalidOperationException($"{key.Game} has no location class '{key.Location}'.");

            // Building a description can touch the Repository, which is one static shared by every test.
            lock (RepositoryLock)
            {
                var location = (ILocation)Activator.CreateInstance(type, true)!;
                return location.GetDescription(Mock.Of<IContext>());
            }
        });
    }

    /// <summary>The game's own global-command factory, as the engine obtains it.</summary>
    public static IGlobalCommandFactory CommandFactory(string game)
    {
        return Factories.GetOrAdd(game, name =>
        {
            var type = Assembly.Load(name).GetTypes().Single(candidate =>
                typeof(IInfocomGame).IsAssignableFrom(candidate) && candidate is { IsClass: true, IsAbstract: false });
            return ((IInfocomGame)Activator.CreateInstance(type)!).GetGlobalCommandFactory();
        });
    }

    private static ParserCase Heading(string game, string input, Direction direction)
    {
        return new ParserCase
        {
            Game = game, Location = "", Input = input, ResolvedBy = ResolvedBy.BeforeParsing,
            BeforeParsing = new IntentExpectation { IntentType = typeof(MoveIntent), Direction = direction }
        };
    }

    private static ParserCase GlobalCommand(string game, string input)
    {
        return new ParserCase
        {
            Game = game, Location = "", Input = input, ResolvedBy = ResolvedBy.BeforeParsing,
            BeforeParsing = new IntentExpectation { IntentType = typeof(GlobalCommandIntent) }
        };
    }

    private static ParserCase SystemCommand(string game, string input)
    {
        return new ParserCase
        {
            Game = game, Location = "", Input = input, ResolvedBy = ResolvedBy.BeforeParsing,
            BeforeParsing = new IntentExpectation { IntentType = typeof(SystemCommandIntent) }
        };
    }
}

using System.Reflection;
using FluentAssertions;
using GameEngine;
using GameEngine.Item;
using GameEngine.Location;
using Model.AIGeneration;
using Model.AIParsing;
using Model.Intent;
using Model.Interaction;
using Moq;
using Planetfall.Location.Kalamontee.Admin;
using Planetfall.Location.Kalamontee.Dorm;
using Planetfall.Location.Lawanda;

namespace Planetfall.Tests;

/// <summary>
///     Issue #500 — the six sanitary facilities had been split across two rival <c>SanfacBase</c> classes in
///     a parent/child namespace pair, so each half silently lost whatever the other half declared: A–D (bound
///     by name resolution to the nearer Dorm-level base) had no destination-navigation synonyms, while E and F
///     had no fixtures scenery. These tests pin PARITY across all six rather than one room's behaviour — the
///     assertion the split hierarchy would have failed from the day it was introduced.
/// </summary>
public class SanfacTests : EngineTestsBase
{
    internal static readonly Type[] AllSanfacs =
    [
        typeof(SanfacA), typeof(SanfacB), typeof(SanfacC), typeof(SanfacD), typeof(SanfacE), typeof(SanfacF)
    ];

    // Repository.GetLocation is generic-only, so reach the singleton for a Type known at runtime.
    private static readonly MethodInfo GetLocationOfType = typeof(Repository)
        .GetMethods(BindingFlags.Public | BindingFlags.Static)
        .First(m => m.Name == nameof(Repository.GetLocation) && m.IsGenericMethodDefinition);

    internal static LocationBase Sanfac(Type type)
    {
        return (LocationBase)GetLocationOfType.MakeGenericMethod(type).Invoke(null, null)!;
    }

    [TestFixture]
    public class NavigatingToTheSanfacByName : EngineTestsBase
    {
        [Test]
        public async Task GoToBathroom_FromDormA_WalksToSanfacA()
        {
            var target = GetTarget();
            StartHere<DormA>();

            await target.GetResponse("go to bathroom");

            target.Context.CurrentLocation.Should().BeOfType<SanfacA>();
        }

        [Test]
        public async Task GoToRestroom_FromDormA_WalksToSanfacA()
        {
            var target = GetTarget();
            StartHere<DormA>();

            await target.GetResponse("go to restroom");

            target.Context.CurrentLocation.Should().BeOfType<SanfacA>();
        }

        [Test]
        public async Task GoToBathroom_FromDormB_WalksToSanfacB()
        {
            // A second dormitory, so this pins the shared base rather than one room's own aliases.
            var target = GetTarget();
            StartHere<DormB>();

            await target.GetResponse("go to bathroom");

            target.Context.CurrentLocation.Should().BeOfType<SanfacB>();
        }

        [Test]
        public async Task GoToBathroom_FromAdminCorridorSouth_WalksToSanfacE()
        {
            // The half that already worked — kept so a fix can't restore A–D by breaking E.
            var target = GetTarget();
            StartHere<AdminCorridorSouth>();

            await target.GetResponse("go to bathroom");

            target.Context.CurrentLocation.Should().BeOfType<SanfacE>();
        }
    }

    /// <summary>
    ///     The mirror image of the navigation gap: the fixtures scenery reached only A–D. The original treats
    ///     every sanfac identically — each of the six declares the same fixtures/toilet pseudo-object
    ///     (<c>planetfall-source/compone.zil:377</c>, <c>:411</c>, <c>:445</c>, <c>:479</c>, <c>:1109</c> and
    ///     <c>comptwo.zil:1294</c>) — so there is no basis for E/F answering nothing.
    ///     Like <see cref="SceneryInvariantTests" />, these call the room's real override chain directly: the
    ///     test parser only knows real item nouns, and scenery nouns are not items.
    /// </summary>
    [TestFixture]
    public class ExaminingTheFixtures : EngineTestsBase
    {
        [TestCaseSource(typeof(SanfacTests), nameof(AllSanfacs))]
        public async Task ExamineFixtures_InEverySanfac_DescribesThemDryAndDusty(Type sanfac)
        {
            var result = await Examine(sanfac, "fixtures");

            result.Should().BeOfType<PositiveInteractionResult>();
            result.InteractionMessage.Should().Contain("bone dry and thick with dust");
        }

        [TestCaseSource(typeof(SanfacTests), nameof(AllSanfacs))]
        public async Task ExamineToilet_InEverySanfac_DescribesTheFixtures(Type sanfac)
        {
            // The ZIL pseudo binds "TOILET" to the same routine as "FIXTURES", and every sanfac description
            // mentions "toilet bowl design", so the noun must resolve rather than fall through to the narrator.
            var result = await Examine(sanfac, "toilet");

            result.Should().BeOfType<PositiveInteractionResult>();
            result.InteractionMessage.Should().Contain("bone dry and thick with dust");
        }

        private async Task<InteractionResult> Examine(Type sanfac, string noun)
        {
            var target = GetTarget();
            var location = Sanfac(sanfac);
            target.Context.CurrentLocation = location;

            return await location.RespondToSimpleInteraction(
                new SimpleIntent { Verb = "examine", Noun = noun },
                target.Context, Mock.Of<IGenerationClient>(),
                new ItemProcessorFactory(Mock.Of<IAITakeAndAndDropParser>()));
        }
    }

    /// <summary>
    ///     The structural guard: every sanfac must carry BOTH the navigation synonyms and the fixtures scenery.
    ///     A future base-class split, or a sanfac added outside the hierarchy, fails here immediately.
    /// </summary>
    [TestFixture]
    public class EverySanfacIsTreatedTheSame : EngineTestsBase
    {
        private static readonly PropertyInfo SceneryProperty =
            typeof(LocationBase).GetProperty("Scenery", BindingFlags.NonPublic | BindingFlags.Instance)!;

        [TestCaseSource(typeof(SanfacTests), nameof(AllSanfacs))]
        public void DeclaresTheNavigationSynonyms(Type sanfac)
        {
            GetTarget();

            Sanfac(sanfac).NounsForMatching.Should().Contain(["bathroom", "restroom", "washroom", "toilet"]);
        }

        [TestCaseSource(typeof(SanfacTests), nameof(AllSanfacs))]
        public void DeclaresTheFixturesScenery(Type sanfac)
        {
            GetTarget();

            var scenery = (IReadOnlyList<SceneryItem>)SceneryProperty.GetValue(Sanfac(sanfac))!;

            scenery.SelectMany(s => s.Nouns).Should().Contain("fixtures");
        }
    }
}

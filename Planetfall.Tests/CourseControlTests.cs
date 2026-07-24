using FluentAssertions;
using GameEngine;
using Planetfall.Item.Feinstein;
using Planetfall.Item.Kalamontee.Mech;
using Planetfall.Item.Lawanda;
using Planetfall.Location.Kalamontee.Admin;
using Planetfall.Location.Lawanda;

namespace Planetfall.Tests;

public class CourseControlTests : EngineTestsBase
{
    [Test]
    public async Task NavigateToCourseControl_FromSystemsCorridorEast()
    {
        var target = GetTarget();
        StartHere<SystemsCorridorEast>();

        var response = await target.GetResponse("north");

        response.Should().Contain("Course Control");
        target.Context.CurrentLocation.Should().BeOfType<CourseControl>();
    }

    [Test]
    public async Task NavigateBackToSystemsCorridorEast_FromCourseControl()
    {
        var target = GetTarget();
        StartHere<CourseControl>();

        var response = await target.GetResponse("south");

        response.Should().Contain("Systems Corridor");
        target.Context.CurrentLocation.Should().BeOfType<SystemsCorridorEast>();
    }

    [Test]
    public async Task LookAtCourseControl_StillBroken_ContainsCorrectDescription()
    {
        var target = GetTarget();
        StartHere<CourseControl>();

        var response = await target.GetResponse("look");

        response.Should().Contain("Course Control");
        response.Should().Contain("Kritikul diivurjins frum pland kors");
    }
    
    [Test]
    public async Task LookAtCourseControl_Fixed_ContainsCorrectDescription()
    {
        var target = GetTarget();
        StartHere<CourseControl>();
        GetLocation<CourseControl>().Fixed = true;

        var response = await target.GetResponse("look");

        response.Should().Contain("Course Control");
        response.Should().Contain("Kors diivurjins minimiizeeng");
    }

    [Test]
    public void LargeMetalCube_ExistsInCourseControl()
    {
        GetTarget();
        var location = StartHere<CourseControl>();

        location.HasItem<LargeMetalCube>().Should().BeTrue();
    }

    [Test]
    public void LargeMetalCube_StartsClosed()
    {
        GetTarget();
        StartHere<CourseControl>();
        var cube = GetItem<LargeMetalCube>();

        cube.IsOpen.Should().BeFalse();
    }

    [Test]
    public void LargeMetalCube_ContainsFusedBedistor()
    {
        GetTarget();
        StartHere<CourseControl>();
        var cube = GetItem<LargeMetalCube>();

        cube.HasItem<FusedBedistor>().Should().BeTrue();
        cube.Items.Count.Should().Be(1);
    }

    [Test]
    public void FusedBedistor_CurrentLocationIsCube()
    {
        GetTarget();
        StartHere<CourseControl>();
        var bedistor = GetItem<FusedBedistor>();
        var cube = GetItem<LargeMetalCube>();

        bedistor.CurrentLocation.Should().Be(cube);
    }

    [Test]
    public async Task LookAtCourseControl_ShowsCubeWithContents()
    {
        var target = GetTarget();
        StartHere<CourseControl>();
        GetItem<LargeMetalCube>().IsOpen = true;

        var response = await target.GetResponse("look");

        response.Should().Contain("In one corner is a large metal cube whose lid is open");
    }

    [Test]
    public async Task CloseCube_ThenOpen_ShowsSpecialText()
    {
        var target = GetTarget();
        StartHere<CourseControl>();
        await target.GetResponse("close cube");

        var response = await target.GetResponse("open cube");

        response.Should().Contain("The lid swings open");
        response.Should().Contain("The large metal cube contains:");
        response.Should().Contain("A fused ninety-ohm bedistor");
    }

    [Test]
    public async Task LookAfterOpeningCube_ShowsContents()
    {
        var target = GetTarget();
        StartHere<CourseControl>();
        await target.GetResponse("close cube");
        await target.GetResponse("open cube");

        var response = await target.GetResponse("look");

        response.Should().Contain("In one corner is a large metal cube whose lid is open");
        response.Should().Contain("The large metal cube contains:");
        response.Should().Contain("A fused ninety-ohm bedistor");
    }

    [Test]
    public async Task OpenEmptyCube_ShowsSimpleText()
    {
        var target = GetTarget();
        StartHere<CourseControl>();

        // Manually remove bedistor from cube to make it empty
        var bedistor = GetItem<FusedBedistor>();
        var cube = GetItem<LargeMetalCube>();
        cube.RemoveItem(bedistor);

        await target.GetResponse("close cube");

        var response = await target.GetResponse("open cube");

        response.Should().Contain("The lid swings open");
        response.Should().NotContain("contains:");
    }

    [Test]
    public async Task ExamineCube_WhenOpen_ListsContents()
    {
        var target = GetTarget();
        StartHere<CourseControl>();
        GetItem<LargeMetalCube>().IsOpen = true;

        var response = await target.GetResponse("examine cube");

        // Issue #398: examining an OPEN container must list its contents (the fused bedistor
        // central to the Course Control puzzle), not just report "is open" and hide them.
        response.Should().Contain("bedistor");
        response.Should().NotContain("The large metal cube is open");
    }

    [Test]
    public async Task ExamineCube_WhenClosed()
    {
        var target = GetTarget();
        StartHere<CourseControl>();
        await target.GetResponse("close cube");

        var response = await target.GetResponse("examine cube");

        response.Should().Contain("The large metal cube is closed");
    }

    [Test]
    public async Task TakeFusedBedistor_FromCube_CannotBeTaken()
    {
        var target = GetTarget();
        StartHere<CourseControl>();
        GetItem<LargeMetalCube>().IsOpen = true;

        var response = await target.GetResponse("take bedistor");

        response.Should().Contain("It seems to be fused to its socket");
        target.Context.HasItem<FusedBedistor>().Should().BeFalse();
    }

    [Test]
    public async Task TakeFusedBedistor_FromCube_StillInCube()
    {
        var target = GetTarget();
        StartHere<CourseControl>();
        await target.GetResponse("take bedistor");

        var cube = GetItem<LargeMetalCube>();
        cube.HasItem<FusedBedistor>().Should().BeTrue();
        cube.Items.Count.Should().Be(1);
    }

    [Test]
    public async Task TakeFusedBedistor_UsingFullName_CannotBeTaken()
    {
        var target = GetTarget();
        StartHere<CourseControl>();
        GetItem<LargeMetalCube>().IsOpen = true;

        var response = await target.GetResponse("take fused");

        response.Should().Contain("It seems to be fused to its socket");
        target.Context.HasItem<FusedBedistor>().Should().BeFalse();
    }

    [Test]
    public async Task RemoveBedistorFromCube_ThenTakeIt()
    {
        var target = GetTarget();
        StartHere<CourseControl>();
        var bedistor = GetItem<FusedBedistor>();
        var cube = GetItem<LargeMetalCube>();

        // Manually move bedistor out of cube to location
        cube.RemoveItem(bedistor);
        GetLocation<CourseControl>().ItemPlacedHere(bedistor);

        var response = await target.GetResponse("take bedistor");

        response.Should().Contain("Taken");
        target.Context.HasItem<FusedBedistor>().Should().BeTrue();
    }

    [Test]
    public async Task DropFusedBedistor_AfterTaking()
    {
        var target = GetTarget();
        StartHere<CourseControl>();
        var bedistor = GetItem<FusedBedistor>();
        var cube = GetItem<LargeMetalCube>();

        // Manually move bedistor out of cube to take it
        cube.RemoveItem(bedistor);
        GetLocation<CourseControl>().ItemPlacedHere(bedistor);
        await target.GetResponse("take bedistor");

        var response = await target.GetResponse("drop bedistor");

        response.Should().Contain("Dropped");
        target.Context.HasItem<FusedBedistor>().Should().BeFalse();
        GetLocation<CourseControl>().HasItem<FusedBedistor>().Should().BeTrue();
    }

    [Test]
    public async Task FusedBedistorDescription_WhenOnGround()
    {
        var target = GetTarget();
        StartHere<CourseControl>();
        var bedistor = GetItem<FusedBedistor>();
        var cube = GetItem<LargeMetalCube>();

        // Manually move bedistor out of cube to location
        cube.RemoveItem(bedistor);
        GetLocation<CourseControl>().ItemPlacedHere(bedistor);

        var response = await target.GetResponse("look");

        response.Should().Contain("There is a fused ninety-ohm bedistor here");
    }

    [Test]
    public async Task CloseCube_Success()
    {
        var target = GetTarget();
        StartHere<CourseControl>();
        GetItem<LargeMetalCube>().IsOpen = true;

        var response = await target.GetResponse("close cube");

        response.Should().Contain("Closed");
        var cube = GetItem<LargeMetalCube>();
        cube.IsOpen.Should().BeFalse();
    }

    [Test]
    public async Task CloseCube_UsingLid()
    {
        var target = GetTarget();
        StartHere<CourseControl>();
        GetItem<LargeMetalCube>().IsOpen = true;

        var response = await target.GetResponse("close lid");

        response.Should().Contain("Closed");
        var cube = GetItem<LargeMetalCube>();
        cube.IsOpen.Should().BeFalse();
    }

    [Test]
    public async Task NavigateToCourseControl_ThenTakeBedistor_CannotBeTaken()
    {
        var target = GetTarget();
        StartHere<SystemsCorridorEast>();

        // Navigate to Course Control (this will trigger Init() on the location)
        await target.GetResponse("north");

        // Open the cube so the bedistor is accessible
        await target.GetResponse("open cube");

        var response = await target.GetResponse("take bedistor");

        response.Should().Contain("It seems to be fused to its socket");
    }
    
    [Test]
    public async Task FullSolution()
    {
        var target = GetTarget();
        StartHere<SystemsCorridorEast>();
        Take<Pliers>();
        Take<GoodBedistor>();

        // Navigate to Course Control (this will trigger Init() on the location)
        await target.GetResponse("north");
        await target.GetResponse("open lid");

        var response = await target.GetResponse("take fused with pliers");
        response.Should().Contain("With a tug, you manage to remove the fused bedistor");

        response = await target.GetResponse("put good in cube");
        response.Should().Contain("Done. The warning lights go out and another light goes on.");

        target.Score.Should().Be(6);
        GetLocation<SystemsMonitors>().Fixed.Count.Should().Be(4);
        GetLocation<SystemsMonitors>().Busted.Count.Should().Be(3);
        GetLocation<CourseControl>().Fixed.Should().BeTrue();
    }

    [Test]
    public void GetItemInScope_GoodBedistor_ResolvesToGoodNotFused()
    {
        // Issue #244: when the good bedistor (inventory) and the fused bedistor (the open cube,
        // in scope) are both present, "good bedistor" must resolve to the GOOD one. The fused
        // bedistor's bare noun "bedistor" is contained in the input "good bedistor", and the
        // resolver used to search the location first and return that containment match - ignoring
        // the adjective. An exact precise-noun match must beat a containment match.
        var target = GetTarget();
        StartHere<CourseControl>();
        Take<GoodBedistor>();                    // good in inventory
        GetItem<LargeMetalCube>().IsOpen = true; // fused in the open cube, in scope

        var good = GetItem<GoodBedistor>();

        Repository.GetItemInScope("good bedistor", target.Context).Should().Be(good);
        Repository.GetItemInScope("good ninety-ohm bedistor", target.Context).Should().Be(good);
    }

    [Test]
    public async Task PutGoodBedistorInCube_WhenFusedIsFirstInInventory_ActsOnTheNamedGoodBedistor()
    {
        // Issue #246: the multi-noun path (put/give/slide X in/to Y) resolved its nouns through
        // MultiNounEngine.IsItemHere, which used the raw, adjective-blind containment matcher - so
        // #244's single-noun fix did not cover it. With the fused bedistor FIRST in inventory, the
        // fused bedistor's bare noun "bedistor" is contained in "good bedistor", and inventory-first
        // containment returned the FUSED one. "put good in cube" must act on the GOOD bedistor (the
        // adjective-qualified, precise match), not the fused one that merely shares the bare noun.
        var target = GetTarget();
        StartHere<CourseControl>();
        GetItem<LargeMetalCube>().IsOpen = true;

        // Force the failing order: fused (the wrong, containment-only match) ahead of good in inventory.
        Take<FusedBedistor>();
        Take<GoodBedistor>();

        var response = await target.GetResponse("put good in cube");

        response.Should().Contain("The warning lights go out");
        GetItem<LargeMetalCube>().HasItem<GoodBedistor>().Should().BeTrue();
        target.Context.HasItem<GoodBedistor>().Should().BeFalse();
        target.Context.HasItem<FusedBedistor>().Should().BeTrue();
        GetLocation<CourseControl>().Fixed.Should().BeTrue();
    }

    [Test]
    public async Task UsePliersOnBedistor_RemovesBedistor()
    {
        var target = GetTarget();
        StartHere<CourseControl>();
        Take<Pliers>();
        GetItem<LargeMetalCube>().IsOpen = true;

        var response = await target.GetResponse("use pliers on bedistor");

        response.Should().Contain("With a tug, you manage to remove the fused bedistor");
        target.Context.HasItem<FusedBedistor>().Should().BeTrue();
        GetItem<LargeMetalCube>().HasItem<FusedBedistor>().Should().BeFalse();
    }

    [Test]
    public async Task UsePliersOnFused_RemovesBedistor()
    {
        var target = GetTarget();
        StartHere<CourseControl>();
        Take<Pliers>();
        GetItem<LargeMetalCube>().IsOpen = true;

        var response = await target.GetResponse("use pliers on fused");

        response.Should().Contain("With a tug, you manage to remove the fused bedistor");
        target.Context.HasItem<FusedBedistor>().Should().BeTrue();
        GetItem<LargeMetalCube>().HasItem<FusedBedistor>().Should().BeFalse();
    }

    [Test]
    public async Task UsePliersOnBedistor_WithoutPliers_Fails()
    {
        var target = GetTarget();
        StartHere<CourseControl>();
        GetItem<LargeMetalCube>().IsOpen = true;

        var response = await target.GetResponse("use pliers on bedistor");

        target.Context.HasItem<FusedBedistor>().Should().BeFalse();
        GetItem<LargeMetalCube>().HasItem<FusedBedistor>().Should().BeTrue();
    }

    [Test]
    public async Task PutGoodBedistorInCube_WhileFusedStillInside_IsRefused()
    {
        // Issue #462: the cube is a SINGLE bedistor socket. It ships holding the fused (broken)
        // bedistor, and without a capacity cap ContainerBase.SpaceForItems defaults to 2 - so a
        // good bedistor (Size 1) dropped in ALONGSIDE the fused one (Size 1) fit (1 + 1 <= 2),
        // "fixing" Course Control with the broken part still socketed and skipping the
        // pliers-removal sub-puzzle entirely. Capping to one slot forces the fused bedistor to be
        // removed first. Mirrors the laser depression (#437) and FromitzAccessPanel capping.
        var target = GetTarget();
        StartHere<CourseControl>();
        GetItem<LargeMetalCube>().IsOpen = true; // fused bedistor is shipped inside
        Take<GoodBedistor>();

        var response = await target.GetResponse("put good in cube");

        // Refused: the good bedistor stays in inventory, the cube keeps only the fused one, and
        // Course Control is NOT reported fixed and no points are awarded.
        response.Should().NotContain("The warning lights go out");
        GetItem<LargeMetalCube>().HasItem<GoodBedistor>().Should().BeFalse();
        GetItem<LargeMetalCube>().HasItem<FusedBedistor>().Should().BeTrue();
        GetItem<LargeMetalCube>().Items.Count.Should().Be(1);
        target.Context.HasItem<GoodBedistor>().Should().BeTrue();
        GetLocation<CourseControl>().Fixed.Should().BeFalse();
        target.Score.Should().Be(0);
    }

    [Test]
    public async Task PutGoodBedistorInCube_AfterFusedRemoved_FixesCourseControl()
    {
        // Issue #462 guard: once the fused bedistor is out of the single socket, the good bedistor
        // is accepted and fixes Course Control as intended - the cap only blocks the second one.
        var target = GetTarget();
        StartHere<CourseControl>();
        var cube = GetItem<LargeMetalCube>();
        cube.IsOpen = true;
        cube.RemoveItem(GetItem<FusedBedistor>()); // pliers-removal already done
        Take<GoodBedistor>();

        var response = await target.GetResponse("put good in cube");

        response.Should().Contain("The warning lights go out");
        cube.HasItem<GoodBedistor>().Should().BeTrue();
        GetLocation<CourseControl>().Fixed.Should().BeTrue();
        target.Score.Should().Be(6);
    }

    [Test]
    public async Task PutWrongItemInCube_GetsAuthoredRefusal()
    {
        // The original cube ends its "put" handler with a refusal naming the item (CUBE-F,
        // comptwo.zil:583). The port declared CanOnlyHoldTheseTypes but no message for it, so the
        // refusal fell through to the AI narrator instead of a crisp, deterministic reply.
        var target = GetTarget();
        StartHere<CourseControl>();
        GetItem<LargeMetalCube>().IsOpen = true;
        Take<Brush>();

        var response = await target.GetResponse("put brush in cube");

        response.Should().Contain("The brush doesn't fit");
    }
}

using FluentAssertions;
using Planetfall.Item.Lawanda;
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
    public async Task LookAtCourseControl_ContainsCorrectDescription()
    {
        var target = GetTarget();
        StartHere<CourseControl>();

        var response = await target.GetResponse("look");

        response.Should().Contain("Course Control");
        response.Should().Contain("This is a long room whose walls are covered with complicated controls and colored lights");
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
    public async Task ExamineCube_WhenOpen()
    {
        var target = GetTarget();
        StartHere<CourseControl>();
        GetItem<LargeMetalCube>().IsOpen = true;

        var response = await target.GetResponse("examine cube");

        response.Should().Contain("The large metal cube is open");
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
}

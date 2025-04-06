using FluentAssertions;
using Planetfall.Item.Kalamontee.Mech;
using Planetfall.Location.Kalamontee.Admin;
using Planetfall.Location.Kalamontee.Mech;
using Planetfall.Location.Kalamontee.Tower;

namespace Planetfall.Tests;

public class CommRoomAndMachineRoomTests : EngineTestsBase
{
    [Test]
    public async Task PutFlaskUnderSpout_Success()
    {
        var target = GetTarget();
        Take<Flask>();
        StartHere<MachineShop>();

        var response = await target.GetResponse("put flask under spout");

        response.Should().Contain("The glass flask is now sitting under the spout.");
        GetLocation<MachineShop>().FlaskUnderSpout.Should().BeTrue();
        target.Context.HasItem<Flask>().Should().BeFalse();
        GetLocation<MachineShop>().HasItem<Flask>().Should().BeTrue();
    }
    
    [Test]
    public async Task PutFlaskUnderSpout_Look()
    {
        var target = GetTarget();
        Take<Flask>();
        StartHere<MachineShop>();
        
        await target.GetResponse("put flask under spout");
        var response = await target.GetResponse("look");

        GetLocation<MachineShop>().FlaskUnderSpout.Should().BeTrue();
        response.Should().Contain("spout. Sitting under the spout is a glass flask. The");
    }
    
    [Test]
    public async Task Look()
    {
        var target = GetTarget();
        Take<Flask>();
        StartHere<MachineShop>();
        
        var response = await target.GetResponse("look");

        GetLocation<MachineShop>().FlaskUnderSpout.Should().BeFalse();
        response.Should().NotContain("spout. Sitting under the spout is a glass flask. The");
    }

    [Test]
    public async Task PutFlaskUnderSpout_DoNotHaveIt()
    {
        var target = GetTarget();
        Take<Flask>();
        StartHere<MachineShop>();

        await target.GetResponse("put flask under spout");
        var response = await target.GetResponse("put flask under spout");

        response.Should().Contain("You don't have the flask");
        GetLocation<MachineShop>().FlaskUnderSpout.Should().BeTrue();
        target.Context.HasItem<Flask>().Should().BeFalse();
        GetLocation<MachineShop>().HasItem<Flask>().Should().BeTrue();
    }

    [Test]
    public async Task PutFlaskUnderSpout_NotHere()
    {
        var target = GetTarget();
        StartHere<MachineShop>();

        await target.GetResponse("put flask under spout");

        GetLocation<MachineShop>().FlaskUnderSpout.Should().BeFalse();
        target.Context.HasItem<Flask>().Should().BeFalse();
        GetLocation<MachineShop>().HasItem<Flask>().Should().BeFalse();
    }

    [Test]
    public async Task PressButton_Disambiguation()
    {
        var target = GetTarget();
        StartHere<MachineShop>();

        var response = await target.GetResponse("press button");

        response.Should().Contain("Which button do you mean");
        response.Should().Contain("black button");
        response.Should().Contain("red button");
        response.Should().Contain("green button");
        response.Should().Contain("blue button");
        response.Should().Contain("yellow button");
        response.Should().Contain("gray button");
        response.Should().Contain("round button");
        response.Should().Contain("square button");
        response.Should().Contain("gray button");
    }

    [TestCase("black")]
    [TestCase("red")]
    [TestCase("green")]
    [TestCase("blue")]
    [TestCase("yellow")]
    [TestCase("gray")]
    [TestCase("round")]
    [TestCase("square")]
    [TestCase("gray")]
    [Test]
    public async Task PressButton_Disambiguation_PartTwo(string buttonColor)
    {
        var target = GetTarget();
        StartHere<MachineShop>();

        var response = await target.GetResponse("press button");

        response.Should().Contain("Which button do you mean");
        response.Should().Contain("black button");
        response.Should().Contain("red button");
        response.Should().Contain("green button");
        response.Should().Contain("blue button");
        response.Should().Contain("yellow button");
        response.Should().Contain("gray button");
        response.Should().Contain("round button");
        response.Should().Contain("square button");
        response.Should().Contain("gray button");

        response = await target.GetResponse(buttonColor);

        response.Should()
            .Contain("Some sort of chemical fluid pours out of the spout, spills all over the floor, and dries up.");
    }
    
    [TestCase("black", "black")]
    [TestCase("red", "red")]
    [TestCase("green", "green")]
    [TestCase("blue", "blue")]
    [TestCase("yellow", "yellow")]
    [TestCase("gray", "gray")]
    [TestCase("round", "clear")]
    [TestCase("square", "clear")]
    [TestCase("gray", "gray" )]
    [Test]
    public async Task PressButton_FlaskUnderneath(string buttonColor, string fluidColor)
    {
        var target = GetTarget();
        GetItem<Flask>().CurrentLocation = GetLocation<MachineShop>();
        GetLocation<MachineShop>().FlaskUnderSpout = true;
        StartHere<MachineShop>();

        var response = await target.GetResponse($"press {buttonColor} button");
        
        GetItem<Flask>().LiquidColor.Should().Be(fluidColor);
        response.Should()
            .Contain($"The flask fills with some {fluidColor} chemical fluid. The fluid gradually turns milky white");
    }
   
    [Test]
    public async Task PressButton_FlaskUnderneath_AlreadyFull()
    {
        var target = GetTarget();
        GetItem<Flask>().CurrentLocation = GetLocation<MachineShop>();
        GetItem<Flask>().LiquidColor = "yellow";
        GetLocation<MachineShop>().FlaskUnderSpout = true;
        StartHere<MachineShop>();

        var response = await target.GetResponse("press yellow button");
        
        response.Should()
            .Contain("Another dose of the chemical fluid pours out of the spout, splashes over the already-full flask, spills onto the floor, and dries up");
    }
    
    [Test]
    public async Task PressButton_ExamineFlask()
    {
        var target = GetTarget();
        GetItem<Flask>().CurrentLocation = GetLocation<MachineShop>();
        Take<Flask>();
        StartHere<MachineShop>();
        
        await target.GetResponse("put flask under spout");
        await target.GetResponse("press yellow button");
        var response = await target.GetResponse("examine flask");
        
        response.Should().Contain("and is filled with a milky");
    }
    
    [Test]
    public async Task EmptyFlask()
    {
        var target = GetTarget();
        GetItem<Flask>().CurrentLocation = GetLocation<MachineShop>();
        Take<Flask>();
        StartHere<MachineShop>();
        
        await target.GetResponse("put flask under spout");
        await target.GetResponse("press yellow button");
        var response = await target.GetResponse("empty flask");
        
        response.Should().Contain("The glass flask is now empty");
    }
    
    [Test]
    public async Task EmptyFlask_NothingThere()
    {
        var target = GetTarget();
        GetItem<Flask>().CurrentLocation = GetLocation<MachineShop>();
        Take<Flask>();
        StartHere<MachineShop>();
        
        var response = await target.GetResponse("empty flask");
        
        response.Should().Contain("There's nothing in the glass flask.");
    }
    
    [Test]
    public async Task PourLiquid_Black()
    {
        var target = GetTarget();
        Take<Flask>();
        StartHere<CommRoom>();
        GetItem<Flask>().LiquidColor = "black";
        
        var response = await target.GetResponse("pour fluid into hole");
        
        response.Should().Contain("The liquid disappears into the hole.");
        response.Should().Contain("and all go off except one, a gray light");
        GetLocation<CommRoom>().CurrentColor.Should().Be("gray");
        GetLocation<SystemsMonitors>().Fixed.Should().NotContain("KUMUUNIKAASHUNZ");
        GetLocation<SystemsMonitors>().Busted.Should().Contain("KUMUUNIKAASHUNZ");
        GetLocation<CommRoom>().IsFixed.Should().BeFalse();
        GetLocation<CommRoom>().SystemIsCritical.Should().BeFalse();
    }
    
    [Test]
    public async Task PourLiquid_Red()
    {
        var target = GetTarget();
        Take<Flask>();
        StartHere<CommRoom>();
        GetItem<Flask>().LiquidColor = "red";
        
        var response = await target.GetResponse("pour fluid into hole");
        
        response.Should().Contain("An alarm sounds briefly");
        response.Should().Contain("the lights in the room dim and the send console shuts down");
        GetLocation<CommRoom>().CurrentColor.Should().Be("black");
        GetLocation<SystemsMonitors>().Fixed.Should().NotContain("KUMUUNIKAASHUNZ");
        GetLocation<SystemsMonitors>().Busted.Should().Contain("KUMUUNIKAASHUNZ");
        GetLocation<CommRoom>().IsFixed.Should().BeFalse();
        GetLocation<CommRoom>().SystemIsCritical.Should().BeTrue();
    }
    
    [Test]
    public async Task PourLiquid_Gray()
    {
        var target = GetTarget();
        Take<Flask>();
        StartHere<CommRoom>();
        GetItem<Flask>().LiquidColor = "gray";
        GetLocation<CommRoom>().CurrentColor = "gray";
        
        var response = await target.GetResponse("pour fluid into hole");
        
        response.Should().Contain("The liquid disappears into the hole.");
        response.Should().Contain("message is now being sent.");
        GetLocation<CommRoom>().CurrentColor.Should().BeNull();
        GetLocation<CommRoom>().IsFixed.Should().BeTrue();
        GetLocation<CommRoom>().SystemIsCritical.Should().BeFalse();
        GetLocation<SystemsMonitors>().Busted.Should().NotContain("KUMUUNIKAASHUNZ");
        GetLocation<SystemsMonitors>().Fixed.Should().Contain("KUMUUNIKAASHUNZ");
        target.Context.Score.Should().Be(6);
        Console.WriteLine(GetLocation<SystemsMonitors>().GetDescriptionForGeneration(target.Context));
    }
}
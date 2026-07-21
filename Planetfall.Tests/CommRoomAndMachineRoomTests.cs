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
   
    // Issue #419: the room shows the player two white buttons — a square one that says "BAAS" and a round
    // one that says "ASID" — but the handler matched only the shapes. Every label the room printed fell
    // through to the AI narrator, which cheerfully narrated a dispense that never happened. In the original
    // both white buttons dispense the same fluid: COLOR-LTBL entries 8 and 9 are both "clear"
    // (planetfall-source/compone.zil:1773-1785), so the labels must land on the same result as the shapes.
    // "round"/"square" are the regression guards for the phrasings that already worked.
    [TestCase("press round button")]
    [TestCase("press square button")]
    [TestCase("press asid button")]
    [TestCase("press acid button")]
    [TestCase("press baas button")]
    [TestCase("press base button")]
    [TestCase("press the acid button")]
    [TestCase("press the base button")]
    [Test]
    public async Task PressWhiteButton_ByPrintedLabel_DispensesClearFluid(string command)
    {
        var target = GetTarget();
        GetItem<Flask>().CurrentLocation = GetLocation<MachineShop>();
        GetLocation<MachineShop>().FlaskUnderSpout = true;
        StartHere<MachineShop>();

        var response = await target.GetResponse(command);

        response.Should().Contain("The flask fills with some clear chemical fluid");
        GetItem<Flask>().LiquidColor.Should().Be("clear");
    }

    // Issue #419: "white" had been pasted onto the BROWN case, so "press white button" dispensed brown
    // fluid. In the original the two white buttons carry ADJECTIVE WHITE and the brown button carries only
    // ADJECTIVE BROWN (compone.zil:1738-1771), so "white" is ambiguous between the square and the round
    // white button and must never reach the brown chemical.
    [TestCase("press white button")]
    [TestCase("press the white button")]
    [Test]
    public async Task PressWhiteButton_IsAmbiguous_AndNeverDispensesBrown(string command)
    {
        var target = GetTarget();
        GetItem<Flask>().CurrentLocation = GetLocation<MachineShop>();
        GetLocation<MachineShop>().FlaskUnderSpout = true;
        StartHere<MachineShop>();

        var response = await target.GetResponse(command);

        response.Should().Contain("Which white button do you mean");
        response.Should().Contain("square white button");
        response.Should().Contain("round white button");
        response.Should().NotContain("brown");
        GetItem<Flask>().LiquidColor.Should().BeNull();
    }

    [TestCase("square")]
    [TestCase("round")]
    [TestCase("baas")]
    [TestCase("asid")]
    [TestCase("acid")]
    [TestCase("base")]
    [Test]
    public async Task PressWhiteButton_Disambiguation_AnswerDispensesClearFluid(string answer)
    {
        var target = GetTarget();
        GetItem<Flask>().CurrentLocation = GetLocation<MachineShop>();
        GetLocation<MachineShop>().FlaskUnderSpout = true;
        StartHere<MachineShop>();

        await target.GetResponse("press white button");
        var response = await target.GetResponse(answer);

        response.Should().Contain("The flask fills with some clear chemical fluid");
        GetItem<Flask>().LiquidColor.Should().Be("clear");
    }

    // Issue #419: bare "brown" — what the parser yields for "press brown button" — lost its case when
    // "white" was pasted over it, so the brown KATALIST could only be reached by the one phrasing that
    // happened to parse as "brown button". Everything else reached the narrator.
    [TestCase("press brown button")]
    [TestCase("press the brown button")]
    [Test]
    public async Task PressBrownButton_DispensesBrownFluid(string command)
    {
        var target = GetTarget();
        GetItem<Flask>().CurrentLocation = GetLocation<MachineShop>();
        GetLocation<MachineShop>().FlaskUnderSpout = true;
        StartHere<MachineShop>();

        var response = await target.GetResponse(command);

        response.Should().Contain("The flask fills with some brown chemical fluid");
        GetItem<Flask>().LiquidColor.Should().Be("brown");
    }

    // Issue #419: the "press button" prompt lists shapes and colors, so a player who answers with a label
    // ("asid") or the color the room actually used for those two buttons ("white") must still be understood.
    [TestCase("acid", "clear")]
    [TestCase("asid", "clear")]
    [TestCase("base", "clear")]
    [TestCase("baas", "clear")]
    [TestCase("brown", "brown")]
    [Test]
    public async Task PressButton_Disambiguation_AnswerWithALabel(string answer, string fluidColor)
    {
        var target = GetTarget();
        GetItem<Flask>().CurrentLocation = GetLocation<MachineShop>();
        GetLocation<MachineShop>().FlaskUnderSpout = true;
        StartHere<MachineShop>();

        await target.GetResponse("press button");
        var response = await target.GetResponse(answer);

        response.Should().Contain($"The flask fills with some {fluidColor} chemical fluid");
        GetItem<Flask>().LiquidColor.Should().Be(fluidColor);
    }

    // The prompt's answers are matched by substring, so "the round white button" hits both the "round"
    // and the "white" choice at equal length. It has to resolve to the button the player named, not to a
    // second "which white button?" prompt -- and it must not depend on the order the choices happen to be
    // declared in, which is exactly what re-sorting them for reuse would have disturbed.
    [TestCase("round white button", "clear")]
    [TestCase("the square white one", "clear")]
    [Test]
    public async Task PressButton_Disambiguation_AnswerNamesShapeAndColor(string answer, string fluidColor)
    {
        var target = GetTarget();
        GetItem<Flask>().CurrentLocation = GetLocation<MachineShop>();
        GetLocation<MachineShop>().FlaskUnderSpout = true;
        StartHere<MachineShop>();

        await target.GetResponse("press button");
        var response = await target.GetResponse(answer);

        response.Should().Contain($"The flask fills with some {fluidColor} chemical fluid");
        GetItem<Flask>().LiquidColor.Should().Be(fluidColor);
    }

    // An answer that names only the color has to narrow rather than guess. "white round button" lands here
    // too: answers are matched by substring and ties go to whichever choice the player named first, so
    // leading with the color asks again instead of resolving. English puts shape before color anyway
    // ("round white button", covered above, resolves outright) -- what matters is that the unusual word
    // order still reaches the button on the next turn rather than dead-ending.
    [TestCase("white")]
    [TestCase("white round button")]
    [Test]
    public async Task PressButton_Disambiguation_AnswerWhite_AsksWhichWhiteButton(string answer)
    {
        var target = GetTarget();
        GetItem<Flask>().CurrentLocation = GetLocation<MachineShop>();
        GetLocation<MachineShop>().FlaskUnderSpout = true;
        StartHere<MachineShop>();

        await target.GetResponse("press button");
        var response = await target.GetResponse(answer);

        response.Should().Contain("Which white button do you mean");
        GetItem<Flask>().LiquidColor.Should().BeNull();

        response = await target.GetResponse("round");

        response.Should().Contain("The flask fills with some clear chemical fluid");
        GetItem<Flask>().LiquidColor.Should().Be("clear");
    }

    // Issue #419: the room calls these buttons white *and* round/square, so "press round white button" is
    // a phrasing its own text invites — and the parser hands that back as the noun "button" with the
    // multi-word adjective "round white". Matching a fixed list of phrases missed every such permutation
    // and dropped it on the narrator, so the match is on the distinguishing word, in any order.
    [TestCase("press round white button")]
    [TestCase("press square white button")]
    [TestCase("press white round button")]
    [Test]
    public async Task PressWhiteButton_ByShapeAndColorTogether_DispensesClearFluid(string command)
    {
        var target = GetTarget();
        GetItem<Flask>().CurrentLocation = GetLocation<MachineShop>();
        GetLocation<MachineShop>().FlaskUnderSpout = true;
        StartHere<MachineShop>();

        var response = await target.GetResponse(command);

        response.Should().Contain("The flask fills with some clear chemical fluid");
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

    // Issue #412: the Comm Room shows the player a "glowing button marked 'Mesij Plaabak'", and pressing
    // it plays back the Feinstein's cut-off transmission (a real story beat). Every phrasing below is
    // something the room's own text invites the player to type, so each must reach the playback handler.
    // Before the fix only the bare noun "button" matched; the label and descriptors fell through to the
    // AI narrator and the transmission was never delivered (worse, the narrator sometimes falsely
    // claimed success). "press button" is the regression guard for the phrasing that already worked.
    [TestCase("press button")]
    [TestCase("press mesij plaabak")]
    [TestCase("push mesij plaabak")]
    [TestCase("press playback button")]
    [TestCase("press message playback button")]
    [TestCase("press glowing button")]
    [Test]
    public async Task PressPlaybackButton_PlaysFeinsteinTransmission(string command)
    {
        var target = GetTarget();
        StartHere<CommRoom>();

        var response = await target.GetResponse(command);

        response.Should().Contain("communications officer");
        response.Should().Contain("burst of static");
    }
}
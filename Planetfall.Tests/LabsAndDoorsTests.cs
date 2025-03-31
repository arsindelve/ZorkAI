using FluentAssertions;
using Planetfall.Location.Lawanda.Lab;

namespace Planetfall.Tests;

public class LabsAndDoorsTests : EngineTestsBase
{
    [Test]
    public async Task RadDoorClosedCannotGo()
    {
        var target = GetTarget();
        StartHere<MainLab>();

        var response = await target.GetResponse("NE");

        response.Should().Contain("The radiation-lock door is closed");
    }

    [Test]
    public async Task BioDoorClosedCannotGo()
    {
        var target = GetTarget();
        StartHere<MainLab>();

        var response = await target.GetResponse("SE");

        response.Should().Contain("The bio-lock door is closed");
    }

    [Test]
    public async Task Rad_OpenAndGo()
    {
        var target = GetTarget();
        StartHere<MainLab>();

        await target.GetResponse("open radiation-lock door");
        var response = await target.GetResponse("NE");

        response.Should().Contain("Radiation Lock West");
    }

    [Test]
    public async Task Bio_OpenAndGo()
    {
        var target = GetTarget();
        StartHere<MainLab>();

        await target.GetResponse("open bio-lock door");
        var response = await target.GetResponse("SE");

        response.Should().Contain("Bio Lock West");
    }

    [Test]
    public async Task BioWest_ClosedDoor_CannotGo()
    {
        var target = GetTarget();
        StartHere<BioLockWest>();

        var response = await target.GetResponse("W");

        response.Should().Contain("The bio-lock door is closed");
    }

    [Test]
    public async Task RadWest_ClosedDoor_CannotGo()
    {
        var target = GetTarget();
        StartHere<RadiationLockWest>();

        var response = await target.GetResponse("W");

        response.Should().Contain("The radiation-lock door is closed");
    }

    [Test]
    public async Task DoorDisambiguation()
    {
        var target = GetTarget();
        StartHere<MainLab>();

        var response = await target.GetResponse("open door");
        Console.WriteLine(response);

        response.Should().Contain("Do you mean the bio-lock door or the radiation-lock door?");
    }

    [Test]
    public async Task DoorDisambiguation_Resolve_Bio()
    {
        var target = GetTarget();
        StartHere<MainLab>();

        var response = await target.GetResponse("open door");
        Console.WriteLine(response);

        response = await target.GetResponse("bio-lock");
        Console.WriteLine(response);

        response = await target.GetResponse("SE");
        Console.WriteLine(response);

        response.Should().Contain("Bio Lock West");
    }

    [Test]
    public async Task DoorDisambiguation_Resolve_Rad()
    {
        var target = GetTarget();
        StartHere<MainLab>();

        var response = await target.GetResponse("open door");
        Console.WriteLine(response);

        response = await target.GetResponse("radiation");
        Console.WriteLine(response);

        response = await target.GetResponse("NE");
        Console.WriteLine(response);

        response.Should().Contain("Radiation Lock West");
    }

    [Test]
    public async Task BioWest_OpenDoor_CanGo()
    {
        var target = GetTarget();
        StartHere<BioLockWest>();

        await target.GetResponse("open door");
        var response = await target.GetResponse("W");

        response.Should().Contain("Main Lab");
    }

    [Test]
    public async Task RadWest_OpenDoor_CanGo()
    {
        var target = GetTarget();
        StartHere<RadiationLockWest>();

        await target.GetResponse("open door");
        var response = await target.GetResponse("W");

        response.Should().Contain("Main Lab");
    }

    [Test]
    public async Task BioEast_OpenDoor_Die()
    {
        var target = GetTarget();
        StartHere<BioLockWest>();

        await target.GetResponse("E");
        var response = await target.GetResponse("open door");

        response.Should().Contain("devour");
    }

    [Test]
    public async Task BioWest_OpenDoor_CloseDoor_CannotGo()
    {
        var target = GetTarget();
        StartHere<BioLockWest>();

        await target.GetResponse("open door");
        var response = await target.GetResponse("close door");
        response.Should().Contain("door closes");

        response = await target.GetResponse("W");
        response.Should().Contain("is closed");
    }

    [Test]
    public async Task RadWest_OpenDoorCloseDoor_CannotGo()
    {
        var target = GetTarget();
        StartHere<RadiationLockWest>();

        await target.GetResponse("open door");
        var response = await target.GetResponse("close door");
        response.Should().Contain("door closes");

        response = await target.GetResponse("W");
        response.Should().Contain("is closed");
    }
    
    [Test]
    public async Task RadLab_HaveSpool()
    {
        var target = GetTarget();
        StartHere<RadiationLab>();
        
        var response = await target.GetResponse("look");
        response.Should().Contain("Sitting on a long table is a small brown spool");
    }
    
    [Test]
    public async Task RadLab_HaveLamp()
    {
        var target = GetTarget();
        StartHere<RadiationLab>();
        
        var response = await target.GetResponse("look");
        response.Should().Contain("There is a powerful portable lamp here, currently off");
    }
    
    [Test]
    public async Task RadLab_ExamineSpool()
    {
        var target = GetTarget();
        StartHere<RadiationLab>();
        
        var response = await target.GetResponse("examine spool");
        response.Should().Contain("The spool is labelled \"Instrukshunz foor Reepaareeng Reepaar Roobots");
    }
    
    [Test]
    public async Task RadLab_GetSickAndDie()
    {
        var target = GetTarget();
        StartHere<RadiationLockEast>();
        
        await target.GetResponse("open door");
        await target.GetResponse("e");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        await target.GetResponse("wait");
        
        var response = await target.GetResponse("wait");
        response.Should().Contain("You suddenly feel sick and dizzy");
        
        response = await target.GetResponse("wait");
        response.Should().Contain("You feel incredibly nauseous and begin vomiting. Also, all your hair has fallen out");
        
        response = await target.GetResponse("wait");
        response.Should().Contain("It seems you have picked up a bad case of radiation poisoning");
        response.Should().Contain("died");
        
    }
}